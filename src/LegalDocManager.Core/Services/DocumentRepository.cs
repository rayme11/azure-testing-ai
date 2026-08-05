using LegalDocManager.Core.Entities;
using LegalDocManager.Core.Interfaces;

namespace LegalDocManager.Core.Services;

/// <summary>
/// In-memory repository for development/testing.
/// In production, this would be SQL Server or CosmosDB.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly Dictionary<Guid, LegalDocument> _documents = new();

    public Task<LegalDocument?> GetByIdAsync(Guid id)
    {
        _documents.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IEnumerable<LegalDocument>> GetAllAsync()
    {
        var docs = _documents.Values.Where(d => !d.IsDeleted).ToList();
        return Task.FromResult<IEnumerable<LegalDocument>>(docs);
    }

    public Task<IEnumerable<LegalDocument>> GetFilteredAsync(DocumentFilter filter)
    {
        var query = _documents.Values.Where(d => !d.IsDeleted);

        if (!string.IsNullOrEmpty(filter.CaseNumber))
            query = query.Where(d => d.CaseNumber.Contains(filter.CaseNumber));
        
        if (!string.IsNullOrEmpty(filter.DocumentType))
            query = query.Where(d => d.DocumentType == filter.DocumentType);
        
        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(d => d.Status == filter.Status);

        // Pagination
        var total = query.Count();
        var paged = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult<IEnumerable<LegalDocument>>(paged);
    }

    public Task<LegalDocument> CreateAsync(LegalDocument document)
    {
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;
        _documents[document.Id] = document;
        return Task.FromResult(document);
    }

    public Task<LegalDocument> UpdateAsync(LegalDocument document)
    {
        document.UpdatedAt = DateTime.UtcNow;
        _documents[document.Id] = document;
        return Task.FromResult(document);
    }

    public Task<bool> SoftDeleteAsync(Guid id)
    {
        if (_documents.TryGetValue(id, out var doc))
        {
            doc.IsDeleted = true;
            doc.DeletedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> CanDeleteAsync(Guid id)
    {
        // Check retention policy - simplified
        if (_documents.TryGetValue(id, out var doc))
        {
            var retentionEnd = doc.CreatedAt.AddYears(doc.RetentionYears);
            var canDelete = DateTime.UtcNow >= retentionEnd;
            return Task.FromResult(canDelete);
        }
        return Task.FromResult(false);
    }
}
