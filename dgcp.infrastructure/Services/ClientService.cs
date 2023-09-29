using dgcp.domain.Abstractions;
using dgcp.domain.Models;
using dgcp.service.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace dgcp.infrastructure.Services;

internal class ClientService : IClientService
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 10);
    private readonly IDataService _service;
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scope;
    private IClientService _api;
    public ClientService(IDataService service, IServiceScopeFactory scope)
    {
        this._service = service;
        this._client = new HttpClient();
        this._scope = scope;
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
                HttpClientHandler clientHandler = new HttpClientHandler();
                
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                HttpClient client = new HttpClient(clientHandler);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(120));  // Cancel after 30 seconds

                var response = await client.GetAsync(url, cts.Token);
                response.EnsureSuccessStatusCode();
                var streamData = await response.Content.ReadAsStreamAsync();
                var pageDto = await JsonSerializer.DeserializeAsync<PageDto>(streamData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                pageDtos.Add(pageDto);

                List<Task<bool>> taskList = new List<Task<bool>>();

                // Agregar tareas a la lista de tareas, comenzando desde currentIndex
                int currentIndex = await this._service.GetCurrentIndexAsync();
                for (int i = currentIndex ; i < pageDto.Data.Count; i++)
                {
                    taskList.Add(ProcessPageItemAsync(pageDto.Data[i]));
                }

                // Ejecutar todas las tareas en paralelo y esperar a que todas se completen
                bool[] results = await Task.WhenAll(taskList);

                // Contar los elementos procesados con éxito
                int processedItemCount = results.Count(r => r);
                currentIndex = await this._service.GetCurrentIndexAsync();

                // Comprueba si todos los elementos se han procesado con éxito
                if ((processedItemCount+currentIndex) >= pageDto.Data.Count && pageDto.Pagination.Next != null)
                {
                    // Guarda la URL en la base de datos
                    await this._service.ResetCurrentIndexAsync();  // Restablecer currentIndex a 0
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


    private async Task<bool> ProcessPageItemAsync(PageItemDto pageItem)
    {
        await _semaphore.WaitAsync();
        try
        {
            using var scope = this._scope.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<IClientService>();
            var db = scope.ServiceProvider.GetRequiredService<IDataService>();
            var root = await GetReleaseRootAsync(pageItem.Url);
            int currentIndex = await _service.GetCurrentIndexAsync();
            if (root?.Releases != null)
            {
                foreach (var release in root.Releases)
                {
                    if (release != null && release.Tender != null)
                    {
                        if (await _service.GetTenderByIdAsync(release.Ocid) != null)
                        {
                            currentIndex++;
                            await _service.SaveCurrentIndexAsync(currentIndex);
                            return true;
                        }
                        var tender = new Tender
                        {
                            ReleaseId = release.Id,
                            ReleaseOcid = release.Ocid,
                            TenderId = release.Tender.Id,
                            Publisher = root.Publisher?.Name,
                            Description = release.Tender.Description,
                            Date = release.Date,
                            Status = release.Tender.Status,
                            Amount = release.Tender.Value.Amount,
                            Currency = release.Tender.Value.Currency,
                            ProcuringEntity = release.Tender.ProcuringEntity.Name,
                            StartDate = release.Tender.TenderPeriod.StartDate,
                            EndDate = release.Tender.TenderPeriod.EndDate,
                            DocumentUrl = release.Tender.Documents.FirstOrDefault().url
                        };

                        if (release.Tender.Items != null)
                        {
                            tender.Items = release.Tender.Items.Select(e =>
                                new TenderItem { Classification = int.Parse(e.Classification.Id.ToString()) }).ToList();
                        }
                        await _service.AddTenderAsync(tender);
                    }
                    else
                    {
                        return false;
                    }
                }
                currentIndex++;
                await _service.SaveCurrentIndexAsync(currentIndex);
                await _service.SaveChangesAsync();
                return true;
            }
            else
            {
                // Intentar guardar la URL fallida
                var failedUrl = new FailedUrl
                {
                    Url = pageItem.Url,
                    VisitDate = DateTime.UtcNow,
                    RetryCount = 0,
                    IsPermanentlyFailed = false
                };
                if (await _service.AddFailedUrlAsync(failedUrl))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task RetryFailedUrlsAsync()
    {
        // 1. Cargar todas las URLs fallidas con RetryCount < 5 y IsPermanentlyFailed = false
        var failedUrls = await _service.GetRetryableFailedUrlsAsync();

        foreach (var failedUrl in failedUrls)
        {
            // 2. Intentar procesar la URL fallida
            bool success = await ProcessFailedUrlAsync(failedUrl.Url);

            // 3. Actualizar la entrada en la base de datos según el resultado
            if (success)
            {
                //Eliminar la URL de la tabla de URLs fallidas
                await _service.DeleteFailedUrlAsync(failedUrl.Id);
            }
            else
            {
                // 4. Incrementar el contador de reintentos y actualizar la entrada
                failedUrl.RetryCount++;
                if (failedUrl.RetryCount >= 5)
                {
                    failedUrl.IsPermanentlyFailed = true;
                }
                await _service.UpdateFailedUrlAsync(failedUrl);
            }
        }
    }

    public async Task<bool> ProcessFailedUrlAsync(string url)
    {
        await _semaphore.WaitAsync();
        try
        {
            using var scope = this._scope.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<IClientService>();
            var db = scope.ServiceProvider.GetRequiredService<IDataService>();
            var root = await GetReleaseRootAsync(url);
            if (root?.Releases != null)
            {
                foreach (var release in root.Releases)
                {
                    if (release != null && release.Tender != null)
                    {
                        if (await _service.GetTenderByIdAsync(release.Ocid) != null)
                        {
                            return true;
                        }
                        var tender = new Tender
                        {
                            ReleaseId = release.Id,
                            ReleaseOcid = release.Ocid,
                            TenderId = release.Tender.Id,
                            Publisher = root.Publisher?.Name,
                            Description = release.Tender.Description,
                            Date = release.Date,
                            Status = release.Tender.Status,
                            Amount = release.Tender.Value.Amount,
                            Currency = release.Tender.Value.Currency,
                            ProcuringEntity = release.Tender.ProcuringEntity.Name,
                            StartDate = release.Tender.TenderPeriod.StartDate,
                            EndDate = release.Tender.TenderPeriod.EndDate,
                            DocumentUrl = release.Tender.Documents.FirstOrDefault().url
                        };

                        if (release.Tender.Items != null)
                        {
                            tender.Items = release.Tender.Items.Select(e =>
                                new TenderItem { Classification = int.Parse(e.Classification.Id.ToString()) }).ToList();
                        }
                        await _service.AddTenderAsync(tender);
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task<ReleaseRootDto> GetReleaseRootAsync(string url)
    {
        string encodedUrl = Uri.EscapeUriString(url);
        try
        {
            //Console.WriteLine($"About to send request to {encodedUrl}");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(120));  // Cancel after 30 seconds

            var response = await client.GetAsync(encodedUrl, cts.Token);

            //Console.WriteLine("Received response");

            response.EnsureSuccessStatusCode();

            var streamData = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<ReleaseRootDto>(streamData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"HttpRequestException: {e.Message}");
            return null;
        }
        catch (TaskCanceledException e)
        {
            Console.WriteLine($"Task was canceled: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
            return null;
        }
    }


}