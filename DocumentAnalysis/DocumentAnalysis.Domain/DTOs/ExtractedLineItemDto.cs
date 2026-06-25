namespace DocumentAnalysis.Domain.DTOs;

public class ExtractedLineItemDto
{
    public string Description { get; set; } = string.Empty;

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? Amount { get; set; }
}
