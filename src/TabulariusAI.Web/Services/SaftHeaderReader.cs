using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>
/// Reads SAF-T (PT) header information and master-file data using secure streaming XML parsing.
/// </summary>
public sealed class SaftHeaderReader : ISaftHeaderReader
{
    static SaftHeaderReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <inheritdoc />
    public async Task<SaftHeaderViewModel> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var settings = new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, IgnoreComments = true, IgnoreWhitespace = true, CloseInput = false };
        try
        {
            using var reader = XmlReader.Create(stream, settings);
            var model = new SaftHeaderViewModel();
            var rootValidated = false;
            var advanceReader = true;

            while (true)
            {
                if (advanceReader && !await reader.ReadAsync()) break;
                advanceReader = true;
                cancellationToken.ThrowIfCancellationRequested();
                if (reader.NodeType != XmlNodeType.Element) continue;
                if (!rootValidated) { ValidateRoot(reader); rootValidated = true; continue; }

                switch (reader.LocalName)
                {
                    case "Header":
                        await ParseElementAsync(reader, cancellationToken, element => ParseHeader(element, model));
                        advanceReader = false;
                        break;
                    case "MasterFiles":
                        await ParseElementAsync(reader, cancellationToken, element => ParseMasterFiles(element, model));
                        advanceReader = false;
                        break;
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
        catch (XmlException exception) { throw new InvalidDataException($"Não foi possível ler o XML SAF-T (PT): {exception.Message}", exception); }
    }

    /// <summary>Reads the current XML element as an isolated subtree and applies a mapper.</summary>
    /// <param name="reader">The reader positioned on the source element.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <param name="map">The mapping operation applied to the parsed element.</param>
    /// <returns>A task representing the asynchronous read operation.</returns>
    private static async Task ParseElementAsync(XmlReader reader, CancellationToken cancellationToken, Action<XElement> map)
    {
        var node = await XNode.ReadFromAsync(reader, cancellationToken);
        if (node is XElement element) map(element);
    }

    /// <summary>Validates that the document root identifies a Portuguese SAF-T namespace.</summary>
    /// <param name="reader">The XML reader positioned at the document root.</param>
    /// <exception cref="InvalidDataException">Thrown when the root or namespace is unsupported.</exception>
    private static void ValidateRoot(XmlReader reader)
    {
        if (!string.Equals(reader.LocalName, "AuditFile", StringComparison.Ordinal) || !reader.NamespaceURI.StartsWith("urn:OECD:StandardAuditFile-Tax:PT_", StringComparison.Ordinal))
            throw new InvalidDataException("O ficheiro XML não contém uma estrutura SAF-T (PT) reconhecida.");
    }

    /// <summary>Maps the SAF-T header subtree into the import model without advancing over adjacent fields.</summary>
    /// <param name="element">The source header element.</param>
    /// <param name="model">The import model to populate.</param>
    private static void ParseHeader(XElement element, SaftHeaderViewModel model)
    {
        var ns = element.Name.Namespace;
        model.SaftVersion = GetOptionalValue(element, ns + "AuditFileVersion");
        model.TaxRegistrationNumber = GetOptionalValue(element, ns + "TaxRegistrationNumber");
        model.CompanyName = GetOptionalValue(element, ns + "CompanyName");
        model.FiscalYear = GetOptionalValue(element, ns + "FiscalYear");
        model.StartDate = GetOptionalValue(element, ns + "StartDate");
        model.EndDate = GetOptionalValue(element, ns + "EndDate");
        model.ProductId = GetOptionalValue(element, ns + "ProductID");
        model.ProductVersion = GetOptionalValue(element, ns + "ProductVersion");
    }

    /// <summary>Maps the SAF-T master-files subtree into the import model using exact namespace-aware element names.</summary>
    /// <param name="element">The source MasterFiles element.</param>
    /// <param name="model">The import model to populate.</param>
    private static void ParseMasterFiles(XElement element, SaftHeaderViewModel model)
    {
        var ns = element.Name.Namespace;
        var generalLedgerAccounts = element.Element(ns + "GeneralLedgerAccounts");
        if (generalLedgerAccounts is not null)
        {
            foreach (var account in generalLedgerAccounts.Elements(ns + "Account")) model.Accounts.Add(ParseAccount(account));
        }
        foreach (var customer in element.Elements(ns + "Customer")) model.Customers.Add(ParseParty(customer, "CustomerID", "CustomerTaxID"));
        foreach (var supplier in element.Elements(ns + "Supplier")) model.Suppliers.Add(ParseParty(supplier, "SupplierID", "SupplierTaxID"));
        model.AccountCount = model.Accounts.Count;
        model.CustomerCount = model.Customers.Count;
        model.SupplierCount = model.Suppliers.Count;
        model.ProductCount = element.Elements(ns + "Product").Count();
    }

    /// <summary>Reads an optional source element value while preserving the non-null model contract.</summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="name">The child element name.</param>
    /// <returns>The source value, or an empty string when the element is absent.</returns>
    private static string GetOptionalValue(XElement parent, XName name) => parent.Element(name)?.Value ?? string.Empty;

    /// <summary>Maps one general ledger account element into the import model.</summary>
    /// <param name="element">The source account element.</param>
    /// <returns>The parsed account.</returns>
    private static SaftAccountViewModel ParseAccount(XElement element)
    {
        var ns = element.Name.Namespace;
        return new SaftAccountViewModel { AccountId = GetRequiredValue(element, ns + "AccountID"), Description = GetRequiredValue(element, ns + "AccountDescription"), OpeningDebitBalance = ParseDecimal(element, ns + "OpeningDebitBalance"), OpeningCreditBalance = ParseDecimal(element, ns + "OpeningCreditBalance"), ClosingDebitBalance = ParseDecimal(element, ns + "ClosingDebitBalance"), ClosingCreditBalance = ParseDecimal(element, ns + "ClosingCreditBalance"), TaxonomyReference = element.Element(ns + "TaxonomyReference")?.Value };
    }

    /// <summary>Maps one customer or supplier master-file element into a common import model.</summary>
    /// <param name="element">The source party element.</param>
    /// <param name="idElementName">The source identifier element name.</param>
    /// <param name="taxElementName">The source tax identifier element name.</param>
    /// <returns>The parsed party.</returns>
    private static SaftPartyViewModel ParseParty(XElement element, string idElementName, string taxElementName)
    {
        var ns = element.Name.Namespace;
        return new SaftPartyViewModel { PartyId = GetRequiredValue(element, ns + idElementName), AccountId = GetRequiredValue(element, ns + "AccountID"), TaxId = GetRequiredValue(element, ns + taxElementName), CompanyName = GetRequiredValue(element, ns + "CompanyName") };
    }

    /// <summary>Reads a required source element value.</summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="name">The required child element name.</param>
    /// <returns>The source value.</returns>
    /// <exception cref="InvalidDataException">Thrown when the source element is absent or empty.</exception>
    private static string GetRequiredValue(XElement parent, XName name)
    {
        var value = parent.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"O SAF-T (PT) contém um registo sem o campo obrigatório {name.LocalName}.");
        return value;
    }

    /// <summary>Parses a required SAF-T decimal value using invariant culture.</summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="name">The decimal child element name.</param>
    /// <returns>The parsed decimal value.</returns>
    /// <exception cref="InvalidDataException">Thrown when the value is missing or invalid.</exception>
    private static decimal ParseDecimal(XElement parent, XName name)
    {
        var source = GetRequiredValue(parent, name);
        if (!decimal.TryParse(source, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)) throw new InvalidDataException($"O SAF-T (PT) contém um valor inválido em {name.LocalName}.");
        return value;
    }

    /// <summary>Validates the minimum required header values after the document has been streamed.</summary>
    /// <param name="model">The parsed SAF-T (PT) summary.</param>
    /// <exception cref="InvalidDataException">Thrown when required header data is missing.</exception>
    private static void ValidateRequiredHeader(SaftHeaderViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SaftVersion)) throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório AuditFileVersion.");
        if (string.IsNullOrWhiteSpace(model.CompanyName)) throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório CompanyName.");
        if (string.IsNullOrWhiteSpace(model.TaxRegistrationNumber)) throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório TaxRegistrationNumber.");
    }
}
