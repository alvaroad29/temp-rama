// Repositories/ReportRepository.cs
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;
using backend.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Report> CreateAsync(Report report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<Report?> GetByIdAsync(int id)
    {
        return await _context.Reports
            .Include(r => r.Book)
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedByAdmin)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PagedResponse<Report>> GetAllAsync(ReportQueryParameters parameters)
    {
        var query = _context.Reports
            .Include(r => r.Book)
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedByAdmin)
            .AsQueryable();

        // Aplicar filtros
        if (parameters.Status.HasValue)
        {
            query = query.Where(r => r.Status == parameters.Status.Value);
        }

        if (parameters.ReportType.HasValue)
        {
            query = query.Where(r => r.ReportType == parameters.ReportType.Value);
        }

        if (parameters.BookId.HasValue)
        {
            query = query.Where(r => r.BookId == parameters.BookId.Value);
        }

        if (!string.IsNullOrEmpty(parameters.ReporterId))
        {
            query = query.Where(r => r.ReporterId == parameters.ReporterId);
        }

        // Aplicar ordenamiento
        query = parameters.SortBy?.ToLower() switch
        {
            "title" => parameters.SortDescending
                ? query.OrderByDescending(r => r.Title)
                : query.OrderBy(r => r.Title),
            "status" => parameters.SortDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "type" => parameters.SortDescending
                ? query.OrderByDescending(r => r.ReportType)
                : query.OrderBy(r => r.ReportType),
            _ => parameters.SortDescending
                ? query.OrderByDescending(r => r.CreationDate)
                : query.OrderBy(r => r.CreationDate)
        };

        // Obtener total de registros
        var totalRecords = await query.CountAsync();

        // Aplicar paginación
        var reports = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResponse<Report>(
            reports,
            parameters.PageNumber,
            parameters.PageSize,
            totalRecords
        );
    }

    public async Task<List<Report>> GetByStatusAsync(ReportStatus status)
    {
        return await _context.Reports
            .Include(r => r.Book)
            .Include(r => r.Reporter)
            .Include(r => r.ReviewedByAdmin)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync();
    }

    public async Task<List<Report>> GetByBookIdAsync(int bookId)
    {
        return await _context.Reports
            .Include(r => r.Book)
            .Include(r => r.Reporter)
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync();
    }

    public async Task<List<Report>> GetByReporterIdAsync(string reporterId)
    {
        return await _context.Reports
            .Include(r => r.Book)
            .Include(r => r.Reporter)
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync();
    }

    public async Task<Report> UpdateAsync(Report report)
    {
        report.ReviewedAt = DateTime.Now;
        _context.Reports.Update(report);
        await _context.SaveChangesAsync();
        return report;
    }



    public async Task<ReportSummaryDto> GetSummaryAsync()
    {
        var reports = await _context.Reports.ToListAsync();

        return new ReportSummaryDto
        {
            TotalReports = reports.Count,
            PendingReports = reports.Count(r => r.Status == ReportStatus.Pendiente),
            InReviewReports = reports.Count(r => r.Status == ReportStatus.EnRevision),
            ResolvedReports = reports.Count(r => r.Status == ReportStatus.Resuelto)
        };
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Reports.AnyAsync(r => r.Id == id);
    }

}


       // public async Task<bool> DeleteAsync(int id)
    // {
    //     var report = await _context.Reports.FindAsync(id);
    //     if (report == null)
    //         return false;

    //     _context.Reports.Remove(report);
    //     await _context.SaveChangesAsync();
    //     return true;
    // }