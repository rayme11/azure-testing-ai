using LegalDocManager.Core.Entities;

namespace LegalDocManager.Core.Interfaces;

/// <summary>
/// Handles legal compliance requirements:
/// - Document classification
/// - Retention policies
/// - Audit logging
/// - GDPR/legal holds
/// </summary>
public interface IComplianceService
{
    // Document classification using AI
    Task<DocumentClassification> ClassifyDocumentAsync(LegalDocument document);
    
    // Validation before operations
    Task<ComplianceCheckResult> ValidateDocumentAsync(LegalDocument document);
    Task<bool> CanDeleteDocumentAsync(Guid documentId);
    
    // Retention management
    Task<int> CalculateRetentionYearsAsync(string documentType, string sensitivityLevel);
}

public class DocumentClassification
{
    public string DocumentType { get; set; } = string.Empty;
    public string SensitivityLevel { get; set; } = string.Empty;
    public int RetentionYears { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> KeyEntities { get; set; } = new();
}

public class ComplianceCheckResult
{
    public bool IsCompliant { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
