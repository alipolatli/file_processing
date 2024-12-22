namespace file_processing.worker.Services.Abstractions;

public interface IFileProcessor
{
    Task<Stream> ProcessAsync(Stream stream, CancellationToken cancellationToken = default);
}