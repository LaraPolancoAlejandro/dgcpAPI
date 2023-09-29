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

            //await _api.GetReleaseDataAsync(this._settings.Value.Host);

            //var tasks = new List<Task>();
            //var retryFailedUrlsTask = _api.RetryFailedUrlsAsync();
            //tasks.Add(retryFailedUrlsTask);

            //await Task.WhenAll(tasks);

            //await this._db.UpdateNullDescriptionsInTenders();

            //await this._db.FilterAndInsertTendersAsync();

            await this._db.EvaluateTenderFinalWithUnknownOrNullProperties();

            await Task.Delay(TimeSpan.FromMinutes(this._settings.Value.UpdateTimeout), stoppingToken);
        }
    }

    
}