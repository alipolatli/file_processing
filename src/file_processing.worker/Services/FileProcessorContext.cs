using file_processing.worker.Services.Abstractions;

namespace file_processing.worker.Services;

public sealed class FileProcessorContext(IServiceProvider serviceProvider) : IFileProcessorContext
{
    public async Task<(Stream Stream, string ContentType)> ProcessAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        var fileProcessor = serviceProvider.GetRequiredKeyedService<IFileProcessor>(contentType);

        Stream str = await fileProcessor.ProcessAsync(stream, cancellationToken);
        return (str, contentType);
    }
}