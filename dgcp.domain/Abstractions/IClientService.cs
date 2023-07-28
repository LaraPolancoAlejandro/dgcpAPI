using dgcp.service.Dtos;

namespace dgcp.domain.Abstractions;

/// <summary>
/// This is the simplest implementation of service. :)
/// </summary>
public interface IClientService
{
    Task<List<PageDto>> GetReleaseDataAsync(string host = default, int limit = 500, int year = 2023);
    Task<ReleaseRootDto> GetReleaseRootAsync(string url);
}