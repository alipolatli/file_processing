namespace file_processing_helper.Services.Abstractions;

public interface IFileProcessor
{
    Task<Stream> ProcessAsync(Stream stream, CancellationToken cancellationToken = default);
}