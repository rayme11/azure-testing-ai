using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AgenticTesting.Engine.Skills;

/// <summary>
/// AI Skill for generating test cases from requirements and API specs.
/// </summary>
public class TestGenerationSkill : ISkill
{
    private readonly Kernel _kernel;

    public string Name => "TestGeneration";
    public string Description => "Generates comprehensive test cases from requirements";

    public TestGenerationSkill(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<ValidationResult> ValidateAsync(string input, SkillContext context)
    {
        var canExecute = !string.IsNullOrWhiteSpace(input) && input.Length > 10;
        return new ValidationResult
        {
            CanExecute = canExecute,
            Confidence = canExecute ? 0.9 : 0.0,
            Reason = canExecute ? null : "Input too short or empty"
        };
    }

    public async Task<SkillResult> ExecuteAsync(string input, SkillContext context)
    {
        try
        {
            var prompt = $@"You are an expert QA engineer specializing in legal document management systems.

TASK: Generate comprehensive test cases based on the following input.

INPUT:
{input}

REQUIREMENTS:
1. Cover positive scenarios (happy paths)
2. Cover negative scenarios (error handling)
3. Include edge cases and boundary values
4. Add security test cases (authorization, input validation)
5. Include data governance compliance tests (GDPR, retention)

CRITICAL INSTRUCTIONS:
1. Return ONLY valid JSON - no markdown, no explanations, no code blocks
2. The response must start with '[' and end with ']'
3. Ensure all JSON is properly formatted with double quotes

OUTPUT FORMAT - JSON ARRAY:
[
  {{
    ""testId"": ""TC001"",
    ""testName"": ""Descriptive test name"",
    ""testType"": ""Unit|Integration|E2E|API|Security|Compliance"",
    ""description"": ""What this test validates"",
    ""preconditions"": ""Setup required"",
    ""steps"": [""Step 1"", ""Step 2""],
    ""expectedResults"": ""Expected outcome"",
    ""priority"": ""Critical|High|Medium|Low"",
    ""tags"": [""tag1"", ""tag2""]
  }}
]

Generate at least 5 test cases. Return ONLY the JSON array, nothing else.";

            var result = await _kernel.InvokePromptAsync(prompt);
            var output = result.ToString();
            
            // Clean up common LLM formatting issues
            output = output.Trim();
            if (output.StartsWith("```json"))
                output = output.Substring(7);
            if (output.StartsWith("```"))
                output = output.Substring(3);
            if (output.EndsWith("```"))
                output = output.Substring(0, output.Length - 3);
            output = output.Trim();

            // Parse confidence from the response
            var confidence = CalculateConfidence(output);

            return new SkillResult
            {
                Success = true,
                Output = output,
                ConfidenceScore = confidence,
                Metadata = new Dictionary<string, object>
                {
                    ["skillName"] = Name,
                    ["inputLength"] = input.Length,
                    ["outputLength"] = output.Length
                }
            };
        }
        catch (Exception ex)
        {
            return new SkillResult
            {
                Success = false,
                Error = ex.Message,
                ConfidenceScore = 0
            };
        }
    }

    [KernelFunction, Description("Generate unit tests for a specific code component")]
    public async Task<string> GenerateUnitTestsAsync(
        [Description("The code or component description to test")] string code,
        [Description("Programming language")] string language = "C#")
    {
        var prompt = $"""
Generate comprehensive unit tests for the following {language} code:

```
{code}
```

Include:
1. Positive test cases
2. Edge cases (null, empty, boundary values)
3. Exception handling tests
4. Mock setup where appropriate

Return the test code in {language}.
""";

        var result = await _kernel.InvokePromptAsync(prompt);
        return result.ToString();
    }

    [KernelFunction, Description("Generate API test cases from OpenAPI spec")]
    public async Task<string> GenerateApiTestsAsync(
        [Description("OpenAPI specification or endpoint description")] string apiSpec,
        [Description("Test framework to use")] string framework = "xUnit")
    {
        var prompt = $"""
Generate API integration tests using {framework} for:

{apiSpec}

Generate tests for:
1. Happy path scenarios
2. Validation errors (400)
3. Authentication errors (401/403)
4. Not found errors (404)
5. Server errors (500)

Include proper assertions and test data.
""";

        var result = await _kernel.InvokePromptAsync(prompt);
        return result.ToString();
    }

    private double CalculateConfidence(string output)
    {
        // Simple heuristic: more test cases = higher confidence
        var testCount = output.Split("testId").Length - 1;
        var hasEdgeCases = output.Contains("edge", StringComparison.OrdinalIgnoreCase);
        var hasNegative = output.Contains("negative", StringComparison.OrdinalIgnoreCase) || 
                          output.Contains("error", StringComparison.OrdinalIgnoreCase);

        double score = Math.Min(testCount * 0.1, 0.5);
        if (hasEdgeCases) score += 0.25;
        if (hasNegative) score += 0.25;

        return Math.Min(score, 1.0);
    }
}
