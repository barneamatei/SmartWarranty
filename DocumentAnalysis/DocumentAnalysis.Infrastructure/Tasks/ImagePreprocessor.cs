using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DocumentAnalysis.Infrastructure.Tasks;

public class ImagePreprocessor
{
    public Task<string> PreprocessAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        using var sourceImage = Image.Load<Rgba32>(inputPath);
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"preprocessed-{Guid.NewGuid()}.png");

        using var processedImage = ProcessImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
        processedImage.SaveAsPng(outputPath);
        return Task.FromResult(outputPath);
    }

    public Task<string> CropHeaderAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        using var sourceImage = Image.Load<Rgba32>(inputPath);
        var cropX = sourceImage.Width / 2;
        var cropY = 0;
        var cropWidth = sourceImage.Width - cropX;
        var cropHeight = (int)(sourceImage.Height * 0.22);

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"preprocessed-header-{Guid.NewGuid()}.png");

        using var processedImage = ProcessImage(sourceImage, new Rectangle(cropX, cropY, cropWidth, cropHeight));
        processedImage.SaveAsPng(outputPath);
        return Task.FromResult(outputPath);
    }

    private static Image<Rgba32> ProcessImage(Image<Rgba32> sourceImage, Rectangle cropRectangle)
    {
        var scale = 2;
        var width = cropRectangle.Width * scale;
        var height = cropRectangle.Height * scale;

        var processedImage = sourceImage.Clone(context => context
            .Crop(cropRectangle)
            .Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Stretch,
                Sampler = KnownResamplers.Bicubic
            })
            .BackgroundColor(Color.White));

        processedImage.Metadata.HorizontalResolution = 300;
        processedImage.Metadata.VerticalResolution = 300;

        processedImage.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];

                    var gray = (int)((pixel.R * 0.299) + (pixel.G * 0.587) + (pixel.B * 0.114));
                    var normalized = Math.Clamp((gray - 128) * 2 + 128, 0, 255);
                    var binary = normalized > 170 ? (byte)255 : (byte)0;

                    row[x] = new Rgba32(binary, binary, binary);
                }
            }
        });

        return processedImage;
    }
}
