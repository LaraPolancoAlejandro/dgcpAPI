using dgcp.domain.Models;
using dgcp.service.Dtos;

namespace dgcp.domain.Abstractions;

/// <summary>
/// This is the simplest implementation of service. :)
/// </summary>
public interface IDataService
{
    Task SaveChangesAsync();
    //Task<Paged<Tender>> GetTenderPagedAsync(int? page = default, int? limit = default, DateTime? startDate = default);
    Task<Paged<TenderFinal>> GetTenderPagedAsync(int? page = default, int? limit = default, DateTime? startDate = default, DateTime? endDate = default, string? empresa = default, string rubros = null);
    Task AddTenderAsync(Tender tender);
    Task<List<string>> GetAllFinalOcidsAsync();
    Task<List<VisitedUrl>> GetAllVisitedUrlsAsync();
    Task<Tender> GetTenderByIdAsync(string releaseOcid);
    Task<List<Tender>> GetFilteredTendersAsync();
    Task<TenderFinal> GetTenderFinalByIdAsync(string tenderId);
    Task<bool> AddFailedUrlAsync(FailedUrl failedUrl);
    Task AddVisitedUrlAsync(VisitedUrl visitedUrl);
    Task AddTenderToFinalAsync(Tender tender);
    Task FilterAndInsertTendersAsync();
    Task UpdateTenderFinalAsync(TenderFinal tenderFinal);
    Task RemoveTenderFinal(TenderFinal tenderFinal);
    Task EvaluateTenderFinalWithUnknownOrNullProperties();
    Task<int> GetCurrentIndexAsync();
    Task SaveCurrentIndexAsync(int currentIndex);
    Task ResetCurrentIndexAsync();
    Task<List<FailedUrl>> GetRetryableFailedUrlsAsync();
    Task<bool> DeleteFailedUrlAsync(int id);
    Task<bool> UpdateFailedUrlAsync(FailedUrl failedUrl);
    Task UpdateNullDescriptionsInTenders();
}