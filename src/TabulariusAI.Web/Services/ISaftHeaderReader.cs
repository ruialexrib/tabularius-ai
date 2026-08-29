using TabulariusAI.Web.Models;

namespace TabulariusAI.Web.Services;

/// <summary>
/// Defines operations for validating a SAF-T XML stream and reading its header information.
/// </summary>
public interface ISaftHeaderReader
{
    /// <summary>
    /// Reads and validates the header of a SAF-T XML stream.
    /// </summary>
    /// <param name="stream">The readable XML stream containing the SAF-T document.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The header information extracted from the SAF-T document.</returns>
    /// <exception cref="InvalidDataException">Thrown when the XML is invalid or does not contain a recognizable SAF-T header.</exception>
    Task<SaftHeaderViewModel> ReadAsync(Stream stream, CancellationToken cancellationToken = default);
}
