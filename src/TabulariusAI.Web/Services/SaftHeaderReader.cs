using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>
/// Reads SAF-T (PT) header information and structural metrics using secure streaming XML parsing.
/// </summary>
public sealed class SaftHeaderReader : ISaftHeaderReader
{
    static SaftHeaderReader()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <inheritdoc />
    public async Task<SaftHeaderViewModel> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            CloseInput = false
        };

        try
        {
            using var reader = XmlReader.Create(stream, settings);
            var model = new SaftHeaderViewModel();
            var rootValidated = false;

            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (!rootValidated)
                {
                    ValidateRoot(reader);
                    rootValidated = true;
                    continue;
                }

                switch (reader.LocalName)
                {
                    case "AuditFileVersion": model.SaftVersion = await reader.ReadElementContentAsStringAsync(); break;
                    case "TaxRegistrationNumber" when string.IsNullOrEmpty(model.TaxRegistrationNumber): model.TaxRegistrationNumber = await reader.ReadElementContentAsStringAsync(); break;
                    case "CompanyName" when string.IsNullOrEmpty(model.CompanyName): model.CompanyName = await reader.ReadElementContentAsStringAsync(); break;
                    case "FiscalYear": model.FiscalYear = await reader.ReadElementContentAsStringAsync(); break;
                    case "StartDate": model.StartDate = await reader.ReadElementContentAsStringAsync(); break;
                    case "EndDate": model.EndDate = await reader.ReadElementContentAsStringAsync(); break;
                    case "ProductID": model.ProductId = await reader.ReadElementContentAsStringAsync(); break;
                    case "ProductVersion": model.ProductVersion = await reader.ReadElementContentAsStringAsync(); break;
                    case "Account":
                        var node = await XNode.ReadFromAsync(reader, cancellationToken);
                        if (node is XElement accountElement)
                        {
                            model.Accounts.Add(ParseAccount(accountElement));
                            model.AccountCount = model.Accounts.Count;
                        }
                        break;
                    case "Customer": model.CustomerCount++; break;
                    case "Supplier": model.SupplierCount++; break;
                    case "Product": model.ProductCount++; break;
                    case "Transaction": model.TransactionCount++; break;
                    case "Invoice": model.SalesInvoiceCount++; break;
                    case "StockMovement": model.MovementOfGoodsCount++; break;
                    case "WorkDocument": model.WorkingDocumentCount++; break;
                    case "Payment": model.PaymentCount++; break;
                }
            }

            ValidateRequiredHeader(model);
            return model;
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException($"Não foi possível ler o XML SAF-T (PT): {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Validates that the document root identifies a Portuguese SAF-T namespace.
    /// </summary>
    /// <param name="reader">The XML reader positioned at the document root.</param>
    /// <exception cref="InvalidDataException">Thrown when the root or namespace is unsupported.</exception>
    private static void ValidateRoot(XmlReader reader)
    {
        if (!string.Equals(reader.LocalName, "AuditFile", StringComparison.Ordinal) ||
            !reader.NamespaceURI.StartsWith("urn:OECD:StandardAuditFile-Tax:PT_", StringComparison.Ordinal))
        {
            throw new InvalidDataException("O ficheiro XML não contém uma estrutura SAF-T (PT) reconhecida.");
        }
    }

    /// <summary>
    /// Maps one general ledger account element into the import model while preserving source values.
    /// </summary>
    /// <param name="element">The source SAF-T (PT) account element.</param>
    /// <returns>The parsed account.</returns>
    private static SaftAccountViewModel ParseAccount(XElement element)
    {
        var ns = element.Name.Namespace;
        return new SaftAccountViewModel
        {
            AccountId = GetRequiredValue(element, ns + "AccountID"),
            Description = GetRequiredValue(element, ns + "AccountDescription"),
            OpeningDebitBalance = ParseDecimal(element, ns + "OpeningDebitBalance"),
            OpeningCreditBalance = ParseDecimal(element, ns + "OpeningCreditBalance"),
            ClosingDebitBalance = ParseDecimal(element, ns + "ClosingDebitBalance"),
            ClosingCreditBalance = ParseDecimal(element, ns + "ClosingCreditBalance"),
            TaxonomyReference = element.Element(ns + "TaxonomyReference")?.Value
        };
    }

    /// <summary>
    /// Reads a required source element value.
    /// </summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="name">The required child element name.</param>
    /// <returns>The source value.</returns>
    /// <exception cref="InvalidDataException">Thrown when the source element is absent or empty.</exception>
    private static string GetRequiredValue(XElement parent, XName name)
    {
        var value = parent.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"O SAF-T (PT) contém uma conta sem o campo obrigatório {name.LocalName}.");
        }
        return value;
    }

    /// <summary>
    /// Parses a required SAF-T decimal value using invariant culture.
    /// </summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="name">The decimal child element name.</param>
    /// <returns>The parsed decimal value.</returns>
    /// <exception cref="InvalidDataException">Thrown when the value is missing or invalid.</exception>
    private static decimal ParseDecimal(XElement parent, XName name)
    {
        var source = GetRequiredValue(parent, name);
        if (!decimal.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidDataException($"O SAF-T (PT) contém um valor inválido em {name.LocalName}.");
        }
        return value;
    }

    /// <summary>
    /// Validates the minimum required header values after the document has been streamed.
    /// </summary>
    /// <param name="model">The parsed SAF-T (PT) summary.</param>
    /// <exception cref="InvalidDataException">Thrown when required header data is missing.</exception>
    private static void ValidateRequiredHeader(SaftHeaderViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SaftVersion)) throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório AuditFileVersion.");
        if (string.IsNullOrWhiteSpace(model.CompanyName)) throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório CompanyName.");
        if (string.IsNullOrWhiteSpace(model.TaxRegistrationNumber)) throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório TaxRegistrationNumber.");
    }
}
