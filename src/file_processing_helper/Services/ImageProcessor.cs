using file_processing_helper.Services.Abstractions;

namespace file_processing_helper.Services;

public sealed class ImageProcessor : IFileProcessor
{
    public async Task<Stream> ProcessAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        //some proccessings.

        return await Task.FromResult(stream);
    }
}