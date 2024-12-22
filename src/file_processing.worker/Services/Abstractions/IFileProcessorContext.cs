namespace file_processing.worker.Services.Abstractions;

public interface IFileProcessorContext
{
    Task<(Stream Stream, string ContentType)> ProcessAsync(Stream stream, string contentType, CancellationToken cancellationToken = default);
}
