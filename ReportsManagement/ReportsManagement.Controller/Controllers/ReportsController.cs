using Microsoft.AspNetCore.Mvc;
using ReportsManagement.Domain.DTOs;
using ReportsManagement.Service.Services;

namespace ReportsManagement.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    [HttpGet("portfolio")]
    [ProducesResponseType(typeof(ReportPreviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportPreviewDto>> GetPortfolioPreview([FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GetPortfolioPreviewAsync(userId, cancellationToken);
        return Ok(report);
    }

    [HttpGet("portfolio/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportPortfolio([FromQuery] string format = "pdf", [FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var file = await _reportService.ExportPortfolioAsync(format, userId, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("expiring-warranties")]
    [ProducesResponseType(typeof(ReportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportPreviewDto>> GetExpiringWarrantiesPreview([FromQuery] int daysAhead = 30, [FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GetExpiringWarrantiesPreviewAsync(daysAhead, userId, cancellationToken);
        return Ok(report);
    }

    [HttpGet("expiring-warranties/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportExpiringWarranties([FromQuery] string format = "xlsx", [FromQuery] int daysAhead = 30, [FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var file = await _reportService.ExportExpiringWarrantiesAsync(format, daysAhead, userId, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
