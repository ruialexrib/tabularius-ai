using System.Text;
using System.Xml;
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
                    if (!string.Equals(reader.LocalName, "AuditFile", StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("O ficheiro XML não contém uma estrutura SAF-T (PT) reconhecida.");
                    }

                    rootValidated = true;
                    continue;
                }

                switch (reader.LocalName)
                {
                    case "AuditFileVersion":
                        model.SaftVersion = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "TaxRegistrationNumber" when string.IsNullOrEmpty(model.TaxRegistrationNumber):
                        model.TaxRegistrationNumber = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "CompanyName" when string.IsNullOrEmpty(model.CompanyName):
                        model.CompanyName = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "FiscalYear":
                        model.FiscalYear = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "StartDate":
                        model.StartDate = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "EndDate":
                        model.EndDate = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "ProductID":
                        model.ProductId = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "ProductVersion":
                        model.ProductVersion = await reader.ReadElementContentAsStringAsync();
                        break;
                    case "Account":
                        model.AccountCount++;
                        break;
                    case "Customer":
                        model.CustomerCount++;
                        break;
                    case "Supplier":
                        model.SupplierCount++;
                        break;
                    case "Product":
                        model.ProductCount++;
                        break;
                    case "Transaction":
                        model.TransactionCount++;
                        break;
                    case "Invoice":
                        model.SalesInvoiceCount++;
                        break;
                    case "StockMovement":
                        model.MovementOfGoodsCount++;
                        break;
                    case "WorkDocument":
                        model.WorkingDocumentCount++;
                        break;
                    case "Payment":
                        model.PaymentCount++;
                        break;
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
    /// Validates the minimum required header values after the document has been streamed.
    /// </summary>
    /// <param name="model">The parsed SAF-T (PT) summary.</param>
    /// <exception cref="InvalidDataException">Thrown when required header data is missing.</exception>
    private static void ValidateRequiredHeader(SaftHeaderViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SaftVersion))
        {
            throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório AuditFileVersion.");
        }

        if (string.IsNullOrWhiteSpace(model.CompanyName))
        {
            throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório CompanyName.");
        }

        if (string.IsNullOrWhiteSpace(model.TaxRegistrationNumber))
        {
            throw new InvalidDataException("O cabeçalho SAF-T (PT) não contém o campo obrigatório TaxRegistrationNumber.");
        }
    }
}
