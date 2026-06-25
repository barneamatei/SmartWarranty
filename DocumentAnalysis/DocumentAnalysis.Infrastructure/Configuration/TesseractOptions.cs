namespace DocumentAnalysis.Infrastructure.Configuration;

public class TesseractOptions
{
    public string ExecutablePath { get; set; } = "tesseract";

    public string? TessDataPath { get; set; }

    public string Language { get; set; } = "eng";
}
