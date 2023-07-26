using dgcp.domain.Abstractions;
using dgcp.domain.Models;
using dgcp.service.Dtos;
using System.Text.Json;

namespace dgcp.infrastructure.Services;

internal class ClientService : IClientService
{
    private readonly IDataService _service;
    private readonly HttpClient _client;

    public ClientService(IDataService service)
    {
        this._service = service;
        this._client = new HttpClient();
    }

    public async Task<List<PageDto>> GetReleaseDataAsync(string host = default, int limit = 500, int year = 2023)
    {
        int page = 1;
        var pageDtos = new List<PageDto>();
        var url = $"{host}/year/{year}/{page}?limit={limit}";

        // Load all visited URLs into memory
        var visitedUrls = await this._service.GetAllVisitedUrlsAsync();

        while (!string.IsNullOrEmpty(url))
        {
            // Verifica si la URL fue visitada
            if (!visitedUrls.Any(v => v.Url == url))
            {
                var response = await this._client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var streamData = await response.Content.ReadAsStreamAsync();
                var pageDto = await JsonSerializer.DeserializeAsync<PageDto>(streamData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                pageDtos.Add(pageDto);

                // Reasignar url
                if (pageDto.Pagination.Next != null)
                {
                    // Añadir URL a DataBase y memoria
                    visitedUrls.Add(new VisitedUrl { Url = url, VisitDate = DateTime.UtcNow });
                    await this._service.AddVisitedUrlAsync(new VisitedUrl { Url = url, VisitDate = DateTime.UtcNow });
                    url = pageDto.Pagination?.Next;
                }
                else
                {
                    if (year < Convert.ToInt32(DateTime.Now.Year))
                    {
                        year++;
                        url = $"{host}/{year}/1?limit={limit}";
                    }
                    else return pageDtos;
                }
            }
            else
            {
                page++;
                url = $"{host}/year/{year}/{page}?limit={limit}";
                continue;
            }
        }
        return pageDtos;
    }

    public async Task<ReleaseRootDto> GetReleaseRootAsync(string url)
    {
        var response = await this._client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var streamData = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<ReleaseRootDto>(streamData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}