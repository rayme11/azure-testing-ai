using LegalDocManager.Core.Entities;

namespace LegalDocManager.Core.Interfaces;

/// <summary>
/// Repository pattern for document storage.
/// Abstracts data access - can be SQL, CosmosDB, or in-memory for testing.
/// </summary>
public interface IDocumentRepository
{
    // Read operations
    Task<LegalDocument?> GetByIdAsync(Guid id);
    Task<IEnumerable<LegalDocument>> GetAllAsync();
    Task<IEnumerable<LegalDocument>> GetFilteredAsync(DocumentFilter filter);
    
    // Write operations
    Task<LegalDocument> CreateAsync(LegalDocument document);
    Task<LegalDocument> UpdateAsync(LegalDocument document);
    Task<bool> SoftDeleteAsync(Guid id);
    
    // Compliance
    Task<bool> CanDeleteAsync(Guid id); // Check retention policy
}

/// <summary>
/// Filter criteria for document queries
/// </summary>
public class DocumentFilter
{
    public string? CaseNumber { get; set; }
    public string? DocumentType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
