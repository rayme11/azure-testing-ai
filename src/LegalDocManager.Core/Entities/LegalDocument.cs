namespace LegalDocManager.Core.Entities;

/// <summary>
/// Represents a legal document in the system.
/// This is the core entity for document management.
/// </summary>
public class LegalDocument
{
    public Guid Id { get; set; }
    
    // Case identification
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    
    // Document classification
    public string DocumentType { get; set; } = string.Empty; // Contract, Brief, Motion, etc.
    public string Content { get; set; } = string.Empty;
    
    // Status tracking
    public string Status { get; set; } = "Draft"; // Draft, Review, Approved, Archived
    
    // Compliance & governance
    public string SensitivityLevel { get; set; } = "Internal"; // Public, Internal, Confidential, Restricted
    public int RetentionYears { get; set; } = 7; // Legal retention requirement
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    
    // Soft delete for compliance
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
