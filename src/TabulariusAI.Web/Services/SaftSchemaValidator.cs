using System.Xml;
using System.Xml.Schema;

namespace TabulariusAI.Web.Services;

/// <summary>
/// Validates SAF-T (PT) 1.04_01 documents against the repository copy of the official schema.
/// </summary>
public sealed class SaftSchemaValidator(IWebHostEnvironment environment) : ISaftSchemaValidator
{
    private const string SupportedNamespace = "urn:OECD:StandardAuditFile-Tax:PT_1.04_01";
    private readonly string schemaPath = Path.Combine(environment.ContentRootPath, "assets", "schema", "saftpt1.04_01.xsd");

    /// <inheritdoc />
    public async Task ValidateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!File.Exists(schemaPath))
        {
            throw new InvalidOperationException("O schema oficial SAF-T (PT) 1.04_01 não está disponível na aplicação.");
        }

        var schemas = new XmlSchemaSet { XmlResolver = null };
        var schemaSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
        using (var schemaReader = XmlReader.Create(schemaPath, schemaSettings))
        {
            schemas.Add(SupportedNamespace, schemaReader);
        }

        var errors = new List<string>();
        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            Schemas = schemas,
            ValidationType = ValidationType.Schema,
            ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
        };
        settings.ValidationEventHandler += (_, eventArgs) => errors.Add(FormatValidationError(eventArgs));

        try
        {
            using var reader = XmlReader.Create(stream, settings);
            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException($"O ficheiro SAF-T (PT) não é XML válido: {exception.Message}", exception);
        }
        catch (XmlSchemaException exception)
        {
            throw new InvalidDataException($"O ficheiro SAF-T (PT) não é válido segundo o XSD oficial 1.04_01: {exception.Message}", exception);
        }

        if (errors.Count > 0)
        {
            var details = string.Join("; ", errors.Take(5));
            if (errors.Count > 5)
            {
                details += $"; e mais {errors.Count - 5} erro(s)";
            }

            throw new InvalidDataException($"O ficheiro SAF-T (PT) não é válido segundo o XSD oficial 1.04_01. {details}");
        }
    }

    /// <summary>
    /// Formats a schema validation event into a concise user-facing diagnostic.
    /// </summary>
    /// <param name="eventArgs">The schema validation event.</param>
    /// <returns>A concise validation diagnostic including line information when available.</returns>
    private static string FormatValidationError(ValidationEventArgs eventArgs)
    {
        var exception = eventArgs.Exception;
        return exception is not null && exception.LineNumber > 0
            ? $"linha {exception.LineNumber}, posição {exception.LinePosition}: {eventArgs.Message}"
            : eventArgs.Message;
    }
}
