using System.Diagnostics;
using System.Text;
using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Domain.Exceptions;
using DocumentAnalysis.Infrastructure.Configuration;
using DocumentAnalysis.Infrastructure.Parsing;
using Microsoft.Extensions.Options;

namespace DocumentAnalysis.Infrastructure.Tasks;

public class TesseractImageTextExtractor : IDocumentTextExtractor
{
    private readonly TesseractOptions _options;
    private readonly IImagePreprocessor _imagePreprocessor;

    public TesseractImageTextExtractor(IOptions<TesseractOptions> options, IImagePreprocessor imagePreprocessor)
    {
        _options = options.Value;
        _imagePreprocessor = imagePreprocessor ?? throw new ArgumentNullException(nameof(imagePreprocessor));
    }

    public bool CanHandle(string contentType, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ExtractedDocumentData> ExtractAsync(string filePath, string contentType, string fileName, CancellationToken cancellationToken = default)
    {
        var preprocessedPath = await _imagePreprocessor.PreprocessAsync(filePath, cancellationToken);
        var headerPath = await _imagePreprocessor.CropHeaderAsync(filePath, cancellationToken);

        try
        {
            var text = await RunTesseractAsync(preprocessedPath, cancellationToken);
            if (!string.Equals(headerPath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                var headerText = await RunTesseractAsync(headerPath, cancellationToken, pageSegmentationMode: 6);
                if (!string.IsNullOrWhiteSpace(headerText))
                    text = $"{headerText}{Environment.NewLine}{text}";
            }

            if (string.IsNullOrWhiteSpace(text))
                throw new DomainException("No text could be extracted from the image.");

            return DocumentHeuristicsParser.Parse(text, usedOcr: true);
        }
        finally
        {
            if (!string.Equals(preprocessedPath, filePath, StringComparison.OrdinalIgnoreCase) && File.Exists(preprocessedPath))
                File.Delete(preprocessedPath);
            if (!string.Equals(headerPath, filePath, StringComparison.OrdinalIgnoreCase) && File.Exists(headerPath))
                File.Delete(headerPath);
        }
    }

    private async Task<string> RunTesseractAsync(string inputPath, CancellationToken cancellationToken, int? pageSegmentationMode = null)
    {
        var outputBasePath = Path.Combine(Path.GetTempPath(), $"tesseract-{Guid.NewGuid()}");
        var arguments = BuildArguments(inputPath, outputBasePath, pageSegmentationMode);

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new DomainException($"Could not start Tesseract. Verify '{_options.ExecutablePath}' is installed and available.", ex);
        }

        var standardError = new StringBuilder();
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null)
                standardError.AppendLine(args.Data);
        };

        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);

        var txtOutputPath = $"{outputBasePath}.txt";

        try
        {
            if (process.ExitCode != 0)
                throw new DomainException($"Tesseract OCR failed: {standardError.ToString().Trim()}");
            if (!File.Exists(txtOutputPath))
                throw new DomainException("Tesseract OCR did not produce an output file.");

            return await File.ReadAllTextAsync(txtOutputPath, cancellationToken);
        }
        finally
        {
            if (File.Exists(txtOutputPath))
                File.Delete(txtOutputPath);
        }
    }

    private string BuildArguments(string inputPath, string outputBasePath, int? pageSegmentationMode)
    {
        var arguments = new StringBuilder();
        arguments.Append('"').Append(inputPath).Append("\" ");
        arguments.Append('"').Append(outputBasePath).Append('"');
        arguments.Append(" -l ").Append(_options.Language);
        if (pageSegmentationMode.HasValue)
            arguments.Append(" --psm ").Append(pageSegmentationMode.Value);

        if (!string.IsNullOrWhiteSpace(_options.TessDataPath))
        {
            arguments.Append(" --tessdata-dir ");
            arguments.Append('"').Append(_options.TessDataPath).Append('"');
        }

        return arguments.ToString();
    }
}
