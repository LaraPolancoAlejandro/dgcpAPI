using dgcp.domain.Abstractions;
using dgcp.domain.Models;
using dgcp.service.Dtos;
using Microsoft.Extensions.Options;
using System.Threading;

namespace dgcp.api;

public class BackgroundWorker : BackgroundService
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 10);
    private IClientService _api;
    private IDataService _db;
    private readonly IOptions<ApiSettings> _settings;
    private readonly IServiceScopeFactory _scope;
    private readonly ILogger<BackgroundWorker> _logger;
    public BackgroundWorker(IServiceScopeFactory scope,
        IOptions<ApiSettings> settings, ILogger<BackgroundWorker> logger)
    {
        this._scope = scope;
        this._settings = settings;
        this._logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Updating...");

            using var scope = this._scope.CreateScope();
            this._api = scope.ServiceProvider.GetRequiredService<IClientService>();
            this._db = scope.ServiceProvider.GetRequiredService<IDataService>();

            var pages = await _api.GetReleaseDataAsync(this._settings.Value.Host);
            var tasks = new List<Task>();

            foreach (var pageDto in pages)
            {
                foreach (var pageItem in pageDto.Data)
                {
                    tasks.Add(ProcessPageItemAsync(pageItem));
                }
            }

            await Task.WhenAll(tasks);
            await this._db.FilterAndInsertTendersAsync(_settings.Value.Keywords, _settings.Value.Categories);

            var ocids = await this._db.GetAllFinalOcidsAsync();
            tasks = new List<Task>();

            foreach (var ocid in ocids)
            {
                tasks.Add(UpdateTenderStatusAsync(ocid));
            }

            await Task.WhenAll(tasks);
            await Task.Delay(TimeSpan.FromMinutes(this._settings.Value.UpdateTimeout), stoppingToken);
        }
    }

    private async Task ProcessPageItemAsync(PageItemDto pageItem)
    {
        await _semaphore.WaitAsync();
        try
        {
            using var scope = this._scope.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<IClientService>();
            var db = scope.ServiceProvider.GetRequiredService<IDataService>();
            var root = await _api.GetReleaseRootAsync(pageItem.Url);
            if (root?.Releases != null)
            {
                foreach (var release in root.Releases)
                {
                    if (release != null)
                    {
                        if (await _db.GetTenderByIdAsync(release.Ocid) != null)
                        {
                            continue;
                        }

                        var tender = new Tender
                        {
                            ReleaseId = release.Id,
                            ReleaseOcid = release.Ocid,
                            TenderId = release.Tender.Id,

                            Publisher = root.Publisher.Name,
                            PublishedDate = root.PublishedDate,
                            PublicationPolicy = root.PublicationPolicy,

                            Description = release.Tender.Description,

                            Date = release.Date,

                            Status = release.Tender.Status,
                            Amount = release.Tender.Value.Amount,
                            Currency = release.Tender.Value.Currency,

                            ProcuringEntity = release.Tender.ProcuringEntity.Name,

                            StartDate = release.Tender.TenderPeriod.StartDate,
                            EndDate = release.Tender.TenderPeriod.EndDate,

                            DocumentUrl = release.Tender.Documents.First().url
                        };

                        if (release.Tender.Items != null)
                        {
                            tender.Items = release.Tender.Items.Select(e =>
                                new TenderItem { Classification = int.Parse(e.Classification.Id.ToString()) }).ToList();
                        }
                        await _db.AddTenderAsync(tender);
                    }
                    else _logger.LogInformation("Se encontró un release null en la lista de Releases.");
                }
                await _db.SaveChangesAsync();
            }
        }
        finally
        {
            _semaphore.Release();
        }
        
    }
    private async Task UpdateTenderStatusAsync(string ocid)
    {
        await _semaphore.WaitAsync();
        try
        {
            using var scope = this._scope.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<IClientService>();
            var db = scope.ServiceProvider.GetRequiredService<IDataService>();
            var root = await _api.GetReleaseRootAsync($"{this._settings.Value.Host}/release/{ocid}");
            if (root?.Releases != null)
            {
                foreach (var release in root.Releases)
                {
                    var existingTenderFinal = await _db.GetTenderFinalByIdAsync(release.Tender.Id);
                    if (existingTenderFinal != null && existingTenderFinal.Status != release.Tender.Status)
                    {
                        existingTenderFinal.Status = release.Tender.Status;
                        await _db.UpdateTenderFinalAsync(existingTenderFinal);
                    }
                }
                await _db.SaveChangesAsync();
            }
        }
        finally
        {
            _semaphore.Release();
        }
        
    }
}