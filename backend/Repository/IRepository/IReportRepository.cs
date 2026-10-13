// Interfaces/IReportRepository.cs
using System;
using backend.Models;
using backend.Models.Dtos;
using backend.Models.Dtos.BookQueries;

namespace backend.Repository.IRepository;

public interface IReportRepository
{
    Task<Report> CreateAsync(Report report);
    Task<Report?> GetByIdAsync(int id);
    Task<PagedResponse<Report>> GetAllAsync(ReportQueryParameters parameters);
    Task<List<Report>> GetByStatusAsync(ReportStatus status);
    Task<List<Report>> GetByBookIdAsync(int bookId);
    Task<List<Report>> GetByReporterIdAsync(string reporterId);
    Task<Report> UpdateAsync(Report report);
    Task<ReportSummaryDto> GetSummaryAsync();
    Task<bool> ExistsAsync(int id);
}


// Task<bool> DeleteAsync(int id);