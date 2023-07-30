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

        public DataService(AppDbContext ctx, IOptions<ApiSettings> apiSettings)
        {
            _ctx = ctx;
            _apiSettings = apiSettings.Value;
        }

        //public async Task<Paged<Tender>> GetTenderPagedAsync(int? page = default, int? limit = default, DateTime? startDate = default)
        //{
        //    var paged = new Paged<Tender>(page, limit);

        //    IQueryable<Tender> query = this._ctx.Tenders.Include(t => t.Items);

        //    var keywords = _apiSettings.Keywords;
        //    var categories = _apiSettings.Categories;

        //    if (startDate.HasValue)
        //    {
        //        query = query.Where(t => t.StartDate >= startDate.Value);
        //    }

        //    var tenders = await query.ToListAsync();

        //    if ((keywords != null && keywords.Length > 0) || (categories != null && categories.Length > 0))
        //    {
        //        tenders = tenders.Where(t => t.Status == "active" &&
        //            ((t.Description != null && keywords != null && keywords.Any(kw => t.Description.Contains(kw))) ||
        //            (t.Items != null && categories != null && t.Items.Any(i => categories.Contains(i.Classification))))).ToList();
        //    }

        //    paged.Items = tenders.OrderByDescending(x => x.StartDate)
        //        .Skip(paged.Skip)
        //        .Take(paged.Limit)
        //        .ToList();

        //    return paged;
        //}

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

                    DocumentUrl = tender.DocumentUrl
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
                await UpdateTenderFinalAsync(existingTender);
            }
        }
        public async Task AddVisitedUrlAsync(VisitedUrl visitedUrl)
        {
            await this._ctx.VisitedUrls.AddAsync(visitedUrl);
            await this._ctx.SaveChangesAsync();
        }

        public async Task FilterAndInsertTendersAsync(string[] keywords, int[] categories)
        {
            var filteredTenders = await GetFilteredTendersAsync(keywords, categories);
            foreach (var tender in filteredTenders)
            {
                await AddTenderToFinalAsync(tender);
            }
        }
        public async Task<List<Tender>> GetFilteredTendersAsync(string[] keywords, int[] categories)
        {
            // First, we get all the Tenders and their Items from the database
            var tenders = await this._ctx.Tenders.Include(t => t.Items).ToListAsync();

            // Then, we filter the Tenders in memory
            //return tenders.Where(t => t.Status == "active"
            //&& ((t.Description != null && keywords.Any(kw => t.Description.Contains(kw)))
            //|| (t.Items != null && t.Items.Any(i => categories.Contains(i.Classification))))).ToList();

            // Then, we filter the Tenders in memory
            return tenders.Where(t => ((t.Description != null && keywords.Any(kw => t.Description.Contains(kw)))
            || (t.Items != null && t.Items.Any(i => categories.Contains(i.Classification))))).ToList();
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