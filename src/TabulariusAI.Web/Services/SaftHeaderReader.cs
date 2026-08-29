using System.Xml;
using System.Xml.Linq;
using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>
/// Reads SAF-T header information from XML documents using secure XML parser settings.
/// </summary>
public sealed class SaftHeaderReader : ISaftHeaderReader
{
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
            var document = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            var root = document.Root;

            if (root is null || !string.Equals(root.Name.LocalName, "AuditFile", StringComparison.Ordinal))
            {
                throw new InvalidDataException("O ficheiro XML não contém uma estrutura SAF-T reconhecida.");
            }

            var header = root.Elements().FirstOrDefault(element => element.Name.LocalName == "Header")
                ?? throw new InvalidDataException("O ficheiro SAF-T não contém a secção Header.");

            var companyName = GetRequiredValue(header, "CompanyName");
            var taxRegistrationNumber = GetRequiredValue(header, "TaxRegistrationNumber");

            return new SaftHeaderViewModel
            {
                SaftVersion = root.Name.NamespaceName,
                CompanyName = companyName,
                TaxRegistrationNumber = taxRegistrationNumber,
                FiscalYear = GetValue(header, "FiscalYear"),
                StartDate = GetValue(header, "StartDate"),
                EndDate = GetValue(header, "EndDate"),
                ProductId = GetValue(header, "ProductID"),
                ProductVersion = GetValue(header, "ProductVersion")
            };
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException("O ficheiro selecionado não contém XML válido.", exception);
        }
    }

    /// <summary>
    /// Gets the trimmed value of a direct child element by its local XML name.
    /// </summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="localName">The local name of the child element.</param>
    /// <returns>The child value, or an empty string when the element is absent.</returns>
    private static string GetValue(XElement parent, string localName)
    {
        return parent.Elements()
            .FirstOrDefault(element => string.Equals(element.Name.LocalName, localName, StringComparison.Ordinal))?
            .Value.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets a required direct child value and rejects a missing or empty element.
    /// </summary>
    /// <param name="parent">The parent XML element.</param>
    /// <param name="localName">The local name of the required child element.</param>
    /// <returns>The non-empty child value.</returns>
    /// <exception cref="InvalidDataException">Thrown when the required element is missing or empty.</exception>
    private static string GetRequiredValue(XElement parent, string localName)
    {
        var value = GetValue(parent, localName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"O cabeçalho SAF-T não contém o campo obrigatório {localName}.");
        }

        return value;
    }
}
