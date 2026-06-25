using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DocumentAnalysis.Infrastructure.Tasks;

public class ImagePreprocessor : IImagePreprocessor
{
    public Task<string> PreprocessAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            return Task.FromResult(inputPath);

        using var sourceImage = Image.FromFile(inputPath);
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"preprocessed-{Guid.NewGuid()}.png");

        using var processedBitmap = ProcessImage(sourceImage, sourceImage.Width, sourceImage.Height, 0, 0);
        processedBitmap.Save(outputPath, ImageFormat.Png);
        return Task.FromResult(outputPath);
    }

    public Task<string> CropHeaderAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            return Task.FromResult(inputPath);

        using var sourceImage = Image.FromFile(inputPath);
        var cropX = sourceImage.Width / 2;
        var cropY = 0;
        var cropWidth = sourceImage.Width - cropX;
        var cropHeight = (int)(sourceImage.Height * 0.22);

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"preprocessed-header-{Guid.NewGuid()}.png");

        using var processedBitmap = ProcessImage(sourceImage, cropWidth, cropHeight, cropX, cropY);
        processedBitmap.Save(outputPath, ImageFormat.Png);
        return Task.FromResult(outputPath);
    }

    private static Bitmap ProcessImage(Image sourceImage, int sourceWidth, int sourceHeight, int cropX, int cropY)
    {
        var scale = 2;
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;

        using var resizedBitmap = new Bitmap(width, height);
        resizedBitmap.SetResolution(300, 300);

        using (var graphics = Graphics.FromImage(resizedBitmap))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.Clear(Color.White);
            graphics.DrawImage(
                sourceImage,
                new Rectangle(0, 0, width, height),
                new Rectangle(cropX, cropY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);
        }

        var processedBitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = resizedBitmap.GetPixel(x, y);

                // Weighted grayscale followed by a hard threshold improves OCR on invoices.
                var gray = (int)((pixel.R * 0.299) + (pixel.G * 0.587) + (pixel.B * 0.114));
                var normalized = Math.Clamp((gray - 128) * 2 + 128, 0, 255);
                var binary = normalized > 170 ? 255 : 0;

                processedBitmap.SetPixel(x, y, Color.FromArgb(binary, binary, binary));
            }
        }

        return processedBitmap;
    }
}
