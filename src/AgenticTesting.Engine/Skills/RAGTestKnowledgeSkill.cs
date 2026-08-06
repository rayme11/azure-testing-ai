using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace AgenticTesting.Engine.Skills;

/// <summary>
/// RAG (Retrieval-Augmented Generation) skill for retrieving relevant test knowledge.
/// </summary>
public class RAGTestKnowledgeSkill : ISkill
{
    private readonly Kernel _kernel;
    private readonly ISemanticTextMemory _memory;

    public string Name => "RAGTestKnowledge";
    public string Description => "Retrieves relevant test patterns and API context from knowledge base";

    public RAGTestKnowledgeSkill(Kernel kernel, ISemanticTextMemory memory)
    {
        _kernel = kernel;
        _memory = memory;
    }

    public async Task<ValidationResult> ValidateAsync(string input, SkillContext context)
    {
        return new ValidationResult 
        { 
            CanExecute = true, 
            Confidence = 0.95 
        };
    }

    public async Task<SkillResult> ExecuteAsync(string input, SkillContext context)
    {
        try
        {
            // Search for relevant API documentation
            var apiDocs = new List<MemoryQueryResult>();
            await foreach (var doc in _memory.SearchAsync(
                collection: "api-documentation",
                query: input,
                limit: 5,
                minRelevanceScore: 0.7))
            {
                apiDocs.Add(doc);
            }

            // Search for existing test patterns
            var testPatterns = new List<MemoryQueryResult>();
            await foreach (var pattern in _memory.SearchAsync(
                collection: "test-patterns",
                query: input,
                limit: 3,
                minRelevanceScore: 0.6))
            {
                testPatterns.Add(pattern);
            }

            var contextBuilder = new System.Text.StringBuilder();
            
            contextBuilder.AppendLine("## Relevant API Documentation:");
            foreach (var doc in apiDocs)
            {
                contextBuilder.AppendLine($"- {doc.Metadata.Text} (relevance: {doc.Relevance:F2})");
            }

            contextBuilder.AppendLine("\n## Similar Test Patterns:");
            foreach (var pattern in testPatterns)
            {
                contextBuilder.AppendLine($"- {pattern.Metadata.Text} (relevance: {pattern.Relevance:F2})");
            }

            return new SkillResult
            {
                Success = true,
                Output = contextBuilder.ToString(),
                Metadata = new Dictionary<string, object>
                {
                    ["apiDocsFound"] = apiDocs.Count,
                    ["patternsFound"] = testPatterns.Count
                }
            };
        }
        catch (Exception ex)
        {
            return new SkillResult
            {
                Success = false,
                Error = $"RAG retrieval failed: {ex.Message}",
                Output = "Continuing without additional context..."
            };
        }
    }

    /// <summary>
    /// Store validated test knowledge for future retrieval
    /// </summary>
    public async Task StoreTestKnowledgeAsync(string testContent, string category)
    {
        try
        {
            var testId = Guid.NewGuid().ToString();
            await _memory.SaveInformationAsync(
                collection: "test-patterns",
                id: testId,
                text: testContent,
                description: $"Validated test pattern - {category}"
            );
        }
        catch
        {
            // Silently fail - storage is optional
        }
    }
}
