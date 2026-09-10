using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DocumentAnalysis.Infrastructure.Tasks;

public class ImagePreprocessor
{
    private const int Scale = 2;

    public async Task<string> PreprocessAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        using var sourceImage = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken);
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"preprocessed-{Guid.NewGuid()}.png");

        using var processedImage = ProcessImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
        await processedImage.SaveAsPngAsync(outputPath, cancellationToken);

        return outputPath;
    }

    public async Task<string> CropHeaderAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        using var sourceImage = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken);
        var cropX = sourceImage.Width / 2;
        var cropWidth = sourceImage.Width - cropX;
        var cropHeight = Math.Max(1, (int)(sourceImage.Height * 0.22));

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"preprocessed-header-{Guid.NewGuid()}.png");

        using var processedImage = ProcessImage(sourceImage, new Rectangle(cropX, 0, cropWidth, cropHeight));
        await processedImage.SaveAsPngAsync(outputPath, cancellationToken);

        return outputPath;
    }

    private static Image<Rgba32> ProcessImage(Image<Rgba32> sourceImage, Rectangle cropArea)
    {
        var image = sourceImage.Clone(context => context
            .Crop(cropArea)
            .Resize(cropArea.Width * Scale, cropArea.Height * Scale, KnownResamplers.Lanczos3));

        image.Metadata.HorizontalResolution = 300;
        image.Metadata.VerticalResolution = 300;
        image.Metadata.ResolutionUnits = PixelResolutionUnit.PixelsPerInch;

        ApplyThreshold(image);
        return image;
    }

    private static void ApplyThreshold(Image<Rgba32> image)
    {
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    var alpha = pixel.A;
                    var red = BlendWithWhite(pixel.R, alpha);
                    var green = BlendWithWhite(pixel.G, alpha);
                    var blue = BlendWithWhite(pixel.B, alpha);

                    var gray = (int)((red * 0.299) + (green * 0.587) + (blue * 0.114));
                    var normalized = Math.Clamp((gray - 128) * 2 + 128, 0, 255);
                    var binary = normalized > 170 ? (byte)255 : (byte)0;

                    row[x] = new Rgba32(binary, binary, binary, 255);
                }
            }
        });
    }

    private static byte BlendWithWhite(byte channel, byte alpha)
    {
        return (byte)((channel * alpha + 255 * (255 - alpha)) / 255);
    }
}
