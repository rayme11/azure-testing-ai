using AgenticTesting.Engine.Skills;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace LegalDocManager.Api.Controllers;

/// <summary>
/// API Controller for AI-powered test generation and validation.
/// Exposes the agentic testing pipeline via REST endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestGenerationController : ControllerBase
{
    private readonly SkillOrchestrator _orchestrator;
    private readonly ILogger<TestGenerationController> _logger;

    public TestGenerationController(
        SkillOrchestrator orchestrator,
        ILogger<TestGenerationController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Execute the complete agentic pipeline: Generate → Validate → Store
    /// </summary>
    /// <param name="request">Test generation request with requirements</param>
    /// <returns>Pipeline execution result with generated tests or rejection reason</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(TestGenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TestGenerationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateTests([FromBody] TestGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting test generation pipeline for session {SessionId}", 
                request.SessionId ?? Guid.NewGuid().ToString());

            var context = new SkillContext
            {
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                UserId = request.UserId ?? "anonymous",
                Parameters = request.Parameters ?? new Dictionary<string, object>()
            };

            var result = await _orchestrator.GenerateValidatedTestsAsync(
                request.Requirements, 
                context);

            var response = new TestGenerationResponse
            {
                Success = result.Success,
                SessionId = result.SessionId,
                GeneratedTests = result.Success ? result.GeneratedTests : null,
                Error = result.Error,
                RejectionReason = result.RejectionReason,
                Stages = result.Stages.Select(s => new PipelineStageDto
                {
                    Name = s.Name,
                    Success = s.Success,
                    DurationMs = (long)s.Duration.TotalMilliseconds
                }).ToList(),
                Metadata = result.Metadata.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value?.ToString() ?? string.Empty),
                TotalDurationMs = (long)result.TotalDuration.TotalMilliseconds
            };

            if (result.Success)
            {
                _logger.LogInformation("Test generation completed successfully. Quality score: {Score}",
                    result.Metadata.GetValueOrDefault("qualityScore", "N/A"));
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Test generation rejected: {Reason}", result.RejectionReason);
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test generation pipeline");
            return StatusCode(500, new TestGenerationResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Quick validation endpoint - check test quality without full pipeline
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateTests([FromBody] ValidationRequest request)
    {
        try
        {
            var result = await _orchestrator.QuickValidateAsync(request.TestContent);

            return Ok(new ValidationResponse
            {
                IsValid = result.IsValid,
                QualityScore = result.Score,
                Feedback = result.Feedback,
                Metadata = result.Metadata.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? string.Empty)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in validation");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get metrics on the 40% reduction effectiveness
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(MetricsResponse), StatusCodes.Status200OK)]
    public IActionResult GetMetrics()
    {
        // In production, this would query Azure Monitor or Cosmos DB
        // For demo purposes, returning sample metrics
        return Ok(new MetricsResponse
        {
            TotalRequests = 150,
            SuccessfulGenerations = 90,
            RejectedTests = 60,
            RejectionRate = 0.40,
            AverageQualityScore = 0.82,
            StageMetrics = new Dictionary<string, StageMetrics>
            {
                ["Syntax Validation"] = new StageMetrics
                {
                    TestsProcessed = 150,
                    TestsRejected = 30,
                    RejectionRate = 0.20
                },
                ["Semantic Quality"] = new StageMetrics
                {
                    TestsProcessed = 120,
                    TestsRejected = 18,
                    RejectionRate = 0.15
                },
                ["Flaky Detection"] = new StageMetrics
                {
                    TestsProcessed = 102,
                    TestsRejected = 6,
                    RejectionRate = 0.06
                }
            },
            AverageResponseTimeMs = 3500
        });
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}

// Request/Response DTOs
public class TestGenerationRequest
{
    [Description("Test requirements or user story description")]
    public string Requirements { get; set; } = string.Empty;
    
    [Description("Optional session ID for tracking")]
    public string? SessionId { get; set; }
    
    [Description("User ID for attribution")]
    public string? UserId { get; set; }
    
    [Description("Additional parameters for the generation")]
    public Dictionary<string, object>? Parameters { get; set; }
}

public class TestGenerationResponse
{
    public bool Success { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? GeneratedTests { get; set; }
    public string? Error { get; set; }
    public string? RejectionReason { get; set; }
    public List<PipelineStageDto> Stages { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public long TotalDurationMs { get; set; }
}

public class PipelineStageDto
{
    public string Name { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long DurationMs { get; set; }
}

public class ValidationRequest
{
    public string TestContent { get; set; } = string.Empty;
}

public class ValidationResponse
{
    public bool IsValid { get; set; }
    public double QualityScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class MetricsResponse
{
    public int TotalRequests { get; set; }
    public int SuccessfulGenerations { get; set; }
    public int RejectedTests { get; set; }
    public double RejectionRate { get; set; }
    public double AverageQualityScore { get; set; }
    public Dictionary<string, StageMetrics> StageMetrics { get; set; } = new();
    public double AverageResponseTimeMs { get; set; }
}

public class StageMetrics
{
    public int TestsProcessed { get; set; }
    public int TestsRejected { get; set; }
    public double RejectionRate { get; set; }
}
