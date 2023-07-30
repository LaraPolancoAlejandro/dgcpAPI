using dgcp.domain.Abstractions;
using dgcp.domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using dgcp.api;


namespace dgcp.infrastructure.Services
{
    internal class DataService : IDataService
    {
        private readonly AppDbContext _ctx;
        private readonly ApiSettings _apiSettings;
        private readonly IConfiguration _configuration;
        public DataService(AppDbContext ctx, IOptions<ApiSettings> apiSettings, IConfiguration configuration)
        {
            _ctx = ctx;
            _apiSettings = apiSettings.Value;
            _configuration = configuration;
        }

        public async Task<Paged<TenderFinal>> GetTenderPagedAsync(int? page = default, int? limit = default, DateTime? startDate = default, DateTime? endDate = default)
        {
            var paged = new Paged<TenderFinal>(page, limit);
            IQueryable<TenderFinal> query = this._ctx.TendersFinal;

            if (startDate.HasValue)
            {
                query = query.Where(t => t.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.StartDate <= endDate.Value);
            }

            paged.Items = await query.OrderByDescending(x => x.StartDate)
                .Skip(paged.Skip)
                .Take(paged.Limit)
                .ToListAsync();

            return paged;
        }

        public Task<List<string>> GetAllFinalOcidsAsync() => this._ctx.TendersFinal.Select(t => t.ReleaseOcid).ToListAsync();

        public Task<List<VisitedUrl>> GetAllVisitedUrlsAsync() => this._ctx.VisitedUrls.ToListAsync();
        public Task<Tender> GetTenderByIdAsync(string releaseOcid) => this._ctx.Tenders.FirstOrDefaultAsync(t => t.ReleaseOcid == releaseOcid);
        public Task<TenderFinal> GetTenderFinalByIdAsync(string tenderId) => this._ctx.TendersFinal.FirstOrDefaultAsync(t => t.TenderId == tenderId);


        public async Task AddTenderAsync(Tender tender)
        {
            await this._ctx.Tenders.AddAsync(tender);
        }
        public async Task AddTenderToFinalAsync(Tender tender)
        {
            var existingTender = await _ctx.TendersFinal
            .FirstOrDefaultAsync(t => t.ReleaseOcid == tender.ReleaseOcid);

            if (existingTender == null)
            {
                var tenderFinal = new TenderFinal
                {
                    ReleaseId = tender.ReleaseId,
                    ReleaseOcid = tender.ReleaseOcid,
                    TenderId = tender.TenderId,

                    Publisher = tender.Publisher,
                    PublishedDate = tender.PublishedDate,
                    PublicationPolicy = tender.PublicationPolicy,

                    Description = tender.Description,

                    Date = tender.Date,

                    Status = tender.Status,
                    Amount = tender.Amount,
                    Currency = tender.Currency,

                    ProcuringEntity = tender.ProcuringEntity,

                    StartDate = tender.StartDate,
                    EndDate = tender.EndDate,

                    DocumentUrl = tender.DocumentUrl,
                    Empresa = tender.Empresa
                };

                _ctx.TendersFinal.Add(tenderFinal);
                await _ctx.SaveChangesAsync();
            }
            else
            {
                // El registro ya existe, puedes decidir qué hacer aquí.
                //if (tender.Status != "active")
                //{
                //    // Si el estado ya no es 'active', elimina el registro
                //    await RemoveTenderFinal(existingTender);
                //}

                // Si el estado sigue siendo 'active', actualiza el registro
                // Copia las propiedades de 'tender' a 'existingTender' aquí
                existingTender.Empresa = tender.Empresa;
                await UpdateTenderFinalAsync(existingTender);
            }
        }
        public async Task AddVisitedUrlAsync(VisitedUrl visitedUrl)
        {
            await this._ctx.VisitedUrls.AddAsync(visitedUrl);
            await this._ctx.SaveChangesAsync();
        }

        public async Task FilterAndInsertTendersAsync()
        {
            var filteredTenders = await GetFilteredTendersAsync();
            var empresaSettings = GetEmpresaSettings(); // This method would return the settings for each company

            foreach (var tender in filteredTenders)
            {
                // Create a list to store the names of the companies
                List<string> empresas = new List<string>();

                // Determine which company the tender belongs to
                foreach (var empresa in empresaSettings)
                {
                    if (tender.Items.Any(i => empresa.Categories.Contains(i.Classification))
                        || empresa.Keywords.Any(kw => tender.Description.Contains(kw)))
                    {
                        // Add the company name to the list
                        empresas.Add(empresa.Name);
                    }
                }

                // Assign the company names to the tender
                tender.Empresa = string.Join(", ", empresas);

                await AddTenderToFinalAsync(tender);
            }
        }

        public List<EmpresaSettings> GetEmpresaSettings()
        {
            // Get the configuration from appsettings.json
            var configuration = _configuration;

            // Get the settings for each company
            var empresaSettings = new List<EmpresaSettings>();
            var empresasConfig = configuration.GetSection("Empresas").GetChildren();

            foreach (var empresaConfig in empresasConfig)
            {
                var empresaSetting = new EmpresaSettings
                {
                    Name = empresaConfig.Key,
                    Categories = empresaConfig.GetSection("Categories").GetChildren().Select(c => int.Parse(c.Value)).ToArray(),
                    Keywords = empresaConfig.GetSection("Keywords").GetChildren().Select(c => c.Value).ToArray()
                };

                // If the company doesn't have any keywords, assign an empty array
                if (empresaSetting.Keywords == null || empresaSetting.Keywords.Length == 0)
                {
                    empresaSetting.Keywords = new string[0];
                }

                empresaSettings.Add(empresaSetting);
            }

            return empresaSettings;
        }

        public async Task<List<Tender>> GetFilteredTendersAsync()
        {
            // Get all keywords and categories from all companies
            var allKeywords = this._apiSettings.Empresas.SelectMany(e => e.Value.Keywords).ToArray();
            var allCategories = this._apiSettings.Empresas.SelectMany(e => e.Value.Categories).ToArray();

            // First, we get all the Tenders and their Items from the database
            var tenders = await this._ctx.Tenders.Include(t => t.Items).ToListAsync();

            // Then, we filter the Tenders in memory
            return tenders.Where(t => ((t.Description != null && allKeywords.Any(kw => t.Description.Contains(kw)))
            || (t.Items != null && t.Items.Any(i => allCategories.Contains(i.Classification))))).ToList();
        }

        public Task UpdateTenderFinalAsync(TenderFinal tenderFinal)
        {
            this._ctx.TendersFinal.Attach(tenderFinal);
            this._ctx.Entry(tenderFinal).State = EntityState.Modified;
            return this._ctx.SaveChangesAsync();
        }
        public Task RemoveTenderFinal(TenderFinal tenderFinal)
        {
            this._ctx.TendersFinal.Remove(tenderFinal);
            return this._ctx.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => this._ctx.SaveChangesAsync();
    }
}