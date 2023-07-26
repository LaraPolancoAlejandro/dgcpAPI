using dgcp.domain.Abstractions;
using dgcp.domain.Models;
using Microsoft.EntityFrameworkCore;

namespace dgcp.infrastructure.Services
{
    internal class DataService : IDataService
    {
        private readonly AppDbContext _ctx;

        public DataService(AppDbContext ctx)
        {
            this._ctx = ctx;
        }

        public async Task<Paged<Tender>> GetTenderPagedAsync(int? page = default, int? limit = default)
        {
            var paged = new Paged<Tender>(page, limit);
            paged.Items = await this._ctx.Tenders.OrderByDescending(x => x.StartDate)
                .Skip(paged.Skip).Take(paged.Limit).ToListAsync();
            return paged;
        }

        public Task<List<string>> GetAllFinalOcidsAsync() => this._ctx.TendersFinal.Select(t => t.ReleaseOcid).ToListAsync();

        public Task<List<VisitedUrl>> GetAllVisitedUrlsAsync() => this._ctx.VisitedUrls.ToListAsync();
        public Task<Tender> GetTenderByIdAsync(string releaseOcid) => this._ctx.Tenders.FirstOrDefaultAsync(t => t.ReleaseOcid == releaseOcid);
        public Task<TenderFinal> GetTenderFinalByIdAsync(string tenderId) => this._ctx.TendersFinal.FirstOrDefaultAsync(t => t.TenderId == tenderId);
        public async Task<List<Tender>> GetFilteredTendersAsync(string[] keywords, int[] categories)
        {
            // Primero, obtenemos todos los Tenders y sus Items de la base de datos
            var tenders = await this._ctx.Tenders.Include(t => t.Items).ToListAsync();

            // Luego, filtramos los Tenders en memoria
            return tenders.Where(t => t.Status == "active"
            && ((t.Description != null && keywords.Any(kw => t.Description.Contains(kw)))
            || (t.Items != null && t.Items.Any(i => categories.Contains(i.Classification))))).ToList();
        }

        public async Task AddTenderAsync(Tender tender)
        {
            await this._ctx.Tenders.AddAsync(tender);
        }
        public async Task AddTenderToFinalAsync(Tender tender)
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
            this._ctx.TendersFinal.Add(tenderFinal);
            await this._ctx.SaveChangesAsync();
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