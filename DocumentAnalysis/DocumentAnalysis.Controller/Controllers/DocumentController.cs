using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAnalysis.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly DocumentAnalysisService _documentAnalysisService;

    public DocumentController(DocumentAnalysisService documentAnalysisService)
    {
        _documentAnalysisService = documentAnalysisService ?? throw new ArgumentNullException(nameof(documentAnalysisService));
    }

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AnalyzedDocumentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnalyzedDocumentResponseDto>> Analyze([FromForm] AnalyzeDocumentRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _documentAnalysisService.AnalyzeAsync(request.File, request.UserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnalyzedDocumentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyzedDocumentResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _documentAnalysisService.GetByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { error = $"Document with ID {id} not found." });

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AnalyzedDocumentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnalyzedDocumentResponseDto>>> GetAll([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var results = await _documentAnalysisService.GetAllAsync(userId, cancellationToken);
        return Ok(results);
    }

    [HttpPost("{id}/warranty-draft")]
    [ProducesResponseType(typeof(WarrantyDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarrantyDraftDto>> CreateWarrantyDraft(Guid id, [FromBody] CreateWarrantyDraftRequestDto request, CancellationToken cancellationToken)
    {
        var draft = await _documentAnalysisService.CreateWarrantyDraftAsync(id, request, cancellationToken);
        return Ok(draft);
    }
}

public class AnalyzeDocumentRequestDto
{
    public IFormFile File { get; set; } = default!;
    public Guid? UserId { get; set; }
}
