namespace DocumentAnalysis.Infrastructure.Tasks;

public interface IImagePreprocessor
{
    Task<string> PreprocessAsync(string inputPath, CancellationToken cancellationToken = default);

    Task<string> CropHeaderAsync(string inputPath, CancellationToken cancellationToken = default);
}
