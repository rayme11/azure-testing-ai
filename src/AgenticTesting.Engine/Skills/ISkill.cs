using Microsoft.SemanticKernel;

namespace AgenticTesting.Engine.Skills;

/// <summary>
/// Base interface for all AI skills in the testing engine.
/// </summary>
public interface ISkill
{
    string Name { get; }
    string Description { get; }
    
    /// <summary>
    /// Validates if this skill can handle the given input.
    /// </summary>
    Task<ValidationResult> ValidateAsync(string input, SkillContext context);
    
    /// <summary>
    /// Executes the skill with the given input.
    /// </summary>
    Task<SkillResult> ExecuteAsync(string input, SkillContext context);
}

/// <summary>
/// Context passed to skills during execution.
/// </summary>
public class SkillContext
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "anonymous";
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public CancellationToken CancellationToken { get; set; }
}

/// <summary>
/// Result of skill validation.
/// </summary>
public class ValidationResult
{
    public bool CanExecute { get; set; }
    public string? Reason { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Result of skill execution.
/// </summary>
public class SkillResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string? Error { get; set; }
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Logs { get; set; } = new();
}
