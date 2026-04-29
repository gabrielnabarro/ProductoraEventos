using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AUDIT_LOG.AddAsync(auditLog, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<AuditLog> AuditLogs, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.AUDIT_LOG
            .AsNoTracking()
            .OrderByDescending(auditLog => auditLog.CreatedAt)
            .ThenByDescending(auditLog => auditLog.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var auditLogs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (auditLogs, totalCount);
    }
}
