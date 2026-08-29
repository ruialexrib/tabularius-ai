namespace TabulariusAI.Web.Services;

/// <summary>
/// Validates SAF-T (PT) XML documents against the supported official XSD schema.
/// </summary>
public interface ISaftSchemaValidator
{
    /// <summary>
    /// Validates a SAF-T (PT) XML stream against the supported official schema.
    /// </summary>
    /// <param name="stream">The SAF-T (PT) XML stream to validate.</param>
    /// <param name="cancellationToken">A token used to cancel the validation operation.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    /// <exception cref="InvalidDataException">Thrown when the document does not conform to the supported XSD.</exception>
    Task ValidateAsync(Stream stream, CancellationToken cancellationToken = default);
}
