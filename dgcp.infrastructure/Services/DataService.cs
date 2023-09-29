using dgcp.domain.Abstractions;
using dgcp.domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using dgcp.api;
using HtmlAgilityPack;
using Microsoft.IdentityModel.Tokens;

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

        public async Task<Paged<TenderFinal>> GetTenderPagedAsync(int? page = 1, int? limit = default, DateTime? startDate = default, DateTime? endDate = default, string? empresa = default, string rubros = null)
        {
            if (limit >= 500)
            {
                limit = 500;
            }
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

            // Si se proporciona un valor para 'empresa', filtra los tenders por ese valor
            if (!string.IsNullOrEmpty(empresa))
            {
                query = query.Where(t => t.EmpresaIds.Contains(empresa));
            }

            // Si se proporcionan valores para 'rubros', filtra los tenders por esos valores
            if (!string.IsNullOrEmpty(rubros))
            {
                try
                {
                    // Convertir la cadena de rubros separados por comas en una lista de enteros
                    var rubroIds = rubros.Split(',')
                                          .Select(int.Parse)
                                          .ToList();

                    // Obtener los TenderReleaseOcid que coinciden con los rubros dados
                    var matchingOcids = this._ctx.TenderItem
                        .Where(ti => rubroIds.Contains(ti.Classification))  // Asumiendo que 'Classification' es el campo que contiene el rubro en forma de entero
                        .Select(ti => ti.Tender.ReleaseOcid)
                        .Distinct();

                    // Filtrar los TenderFinal que tienen un TenderReleaseOcid en la lista de matchingOcids
                    query = query.Where(tf => matchingOcids.Contains(tf.ReleaseOcid));
                }
                catch (Exception ex)
                {
                    // Aquí puedes manejar la excepción, por ejemplo, registrando el error
                    Console.WriteLine($"Ocurrió un error al filtrar por rubros: {ex.Message}");
                }
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

                    Description = tender.Description,

                    Date = tender.Date,

                    Status = tender.Status,
                    Amount = tender.Amount,
                    Currency = tender.Currency,

                    ProcuringEntity = tender.ProcuringEntity,

                    StartDate = tender.StartDate,
                    EndDate = tender.EndDate,

                    DocumentUrl = tender.DocumentUrl,
                    EmpresaIds = tender.EmpresaIds
                };

                // Realiza el web scraping para obtener la fase y el estado
                var webData = await GetWebData(tender.DocumentUrl);
                tenderFinal.Fase = webData[0];
                tenderFinal.estado = webData[1];
                tenderFinal.ProcedureType = webData[2];
                tenderFinal.ContractType = webData[3];
                _ctx.TendersFinal.Add(tenderFinal);
                await _ctx.SaveChangesAsync();
            }
            else
            {
                var webData = await GetWebData(tender.DocumentUrl);
                existingTender.Fase = webData[0];
                existingTender.estado = webData[1];
                existingTender.ProcedureType = webData[2];
                existingTender.ContractType = webData[3];
                if(tender.EmpresaIds != null)
                existingTender.EmpresaIds = tender.EmpresaIds;
                await UpdateTenderFinalAsync(existingTender);
            }
        }
        static async Task<string[]> GetWebData(string Documenturl)
        {
            int retryCount = 0;
            const int maxRetries = 2;
            while (retryCount <= maxRetries)
            {
                try
                {
                    var url = Documenturl;
                    var httpClient = new HttpClient();
                    var html = await httpClient.GetStringAsync(url);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    // Aquí es donde seleccionas el nodo específico que quieres extraer.
                    var currentPhaseNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='fdsRequestSummaryInfo_tblDetail_trRowPhase_tdCell2_spnPhase']");
                    var statusNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='fdsRequestSummaryInfo_tblDetail_trRowState_tdCell2_spnState']");
                    var procedureTypeNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='fdsRequestSummaryInfo_tblDetail_trRowProcedureType_tdCell2_spnProcedureType']");
                    var contractTypeNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='fdsObjectOfTheContract_tblDetail_trRowTypeOfContract_tdCell2_spnTypeOfContract']");

                    //Se retorna los resultados de la busqueda
                    // Traducción de los valores
                    string currentPhase = currentPhaseNode?.InnerText ?? "Desconocido1";
                    string status = statusNode?.InnerText ?? "Desconocido1";
                    string procedureType = procedureTypeNode?.InnerText ?? "Desconocido1";
                    string contractType = contractTypeNode?.InnerText ?? "Desconocido1";

                    switch (status)
                    {
                        case "Published":
                            status = "Proceso publicado";
                            break;
                        case "ClosedForReplies":
                            status = "Proceso con etapa cerrada";
                            break;
                        case "Awarded":
                            status = "Proceso adjudicado y celebrado";
                            break;
                        case "Canceled":
                            status = "Proceso cancelado";
                            break;
                        case "NonAwarded":
                            status = "Proceso desierto";
                            break;
                        case "Suspended":
                            status = "Suspendido";
                            break;
                        // Agrega más casos según sea necesario
                        default:
                            break;
                    }
                    switch (contractType)
                    {
                        case "Goods":
                            contractType = "Bienes";
                            break;
                        case "Services":
                            contractType = "Servicios";
                            break;
                        // Agrega más casos según sea necesario
                        default:
                            break;
                    }


                    //Se retorna los resultados de la busqueda
                    return new[] { currentPhase, status, procedureType, contractType };
                }
                catch (Exception ex)
                {
                    // Incrementa el contador de intentos fallidos
                    retryCount++;

                    // Si se alcanza el número máximo de intentos, registra el error y devuelve "Desconocido"
                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine($"Ocurrió un error al obtener los datos de la web: {ex.Message}");
                        return new[] { "Desconocido2", "Desconocido2", "Desconocido2", "Desconocido2" };
                    }
                }
            }
            return new[] { "Desconocido3", "Desconocido3", "Desconocido3", "Desconocido3" };
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
                List<string> empresaIds = new List<string>();

                // Determine which company the tender belongs to
                foreach (var empresa in empresaSettings)
                {
                    if (tender.Items.Any(i => empresa.Categories.Contains(i.Classification))
                        || empresa.Keywords.Any(kw => tender.Description.Contains(kw))){

                        // Add the company id to the list
                        empresaIds.Add(empresa.Id);
                    }
                }

                // Assign the company ids to the tender
                tender.EmpresaIds = string.Join(",", empresaIds);

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
                    Id = empresaConfig["Id"],
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
            List<Tender> filteredTenders = new List<Tender>();

            foreach (var tender in tenders)
            {
                if (tender.Items != null)
                {
                    foreach (var item in tender.Items)
                    {
                        if (allCategories.Contains(item.Classification))
                        {
                            filteredTenders.Add(tender);
                            break;
                        }
                    }
                }

                if (!filteredTenders.Contains(tender) && tender.Description != null)
                {
                    foreach (var keyword in allKeywords)
                    {
                        if (tender.Description.Contains(keyword))
                        {
                            filteredTenders.Add(tender);
                            break;
                        }
                    }
                }
            }
            return filteredTenders;
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

        public async Task EvaluateTenderFinalWithUnknownOrNullProperties()
        {
            // Obtener todos los registros donde ProcuringEntity es 'Desconocido'
            var unknownTenders = await _ctx.TendersFinal
            .Where(t =>
                t.ReleaseId == "Desconocido" || t.ReleaseId == null ||
                t.ReleaseOcid == "Desconocido" || t.ReleaseOcid == null ||
                t.TenderId == "Desconocido" || t.TenderId == null ||
                t.Publisher == "Desconocido" || t.Publisher == null ||
                t.Description == "Desconocido" || t.Description == null ||
                t.Date == null ||
                t.Status == "Desconocido" || t.Status == null ||
                t.Currency == "Desconocido" || t.Currency == null ||
                t.ProcuringEntity == "Desconocido" || t.ProcuringEntity == null ||
                t.DocumentUrl == "Desconocido" || t.DocumentUrl == null ||
                t.EmpresaIds == "Desconocido" || t.EmpresaIds == null ||
                t.Fase == "Desconocido" || t.Fase == null ||
                t.estado == "Desconocido" || t.estado == null ||
                t.ProcedureType == "Desconocido" || t.ProcedureType == null ||
                t.ContractType == "Desconocido" || t.ContractType == null
            )
            .ToListAsync();


            // Realizar alguna evaluación o actualización en los registros
            foreach (var tender in unknownTenders)
            {
                Tender newTender = new Tender
                {
                    ReleaseId = tender.ReleaseId,
                    ReleaseOcid = tender.ReleaseOcid,
                    TenderId = tender.TenderId,
                    Publisher = tender.Publisher,
                    Description = tender.Description,
                    Date = tender.Date,
                    Status = tender.Status,
                    Amount = tender.Amount,
                    Currency = tender.Currency,
                    ProcuringEntity = tender.ProcuringEntity,
                    StartDate = tender.StartDate,
                    EndDate = tender.EndDate,
                    DocumentUrl = tender.DocumentUrl,
                    EmpresaIds = tender.EmpresaIds
                };

                await AddTenderToFinalAsync(newTender);

            }

            // Guardar los cambios en la base de datos
            await _ctx.SaveChangesAsync();
        }

        public async Task SaveCurrentIndexAsync(int index)
        {
            // Buscar el registro con Id = 1
            var currentUrl = await _ctx.CurrentUrls.FindAsync(1);

            // Si el registro no existe, créalo
            if (currentUrl == null)
            {
                currentUrl = new CurrentUrl { Id = 1, CurrentIndex = index };
                _ctx.CurrentUrls.Add(currentUrl);
            }
            else
            {
                // Actualizar el CurrentIndex del registro existente
                currentUrl.CurrentIndex = index;
                _ctx.CurrentUrls.Update(currentUrl);
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task<int> GetCurrentIndexAsync()
        {
            var currentUrl = await _ctx.CurrentUrls.FirstOrDefaultAsync(); // Obtener el primer registro
            return currentUrl?.CurrentIndex ?? 0; // Devolver el índice o 0 si no hay registros
        }

        public async Task ResetCurrentIndexAsync()
        {
            var currentUrl = await _ctx.CurrentUrls.FirstOrDefaultAsync(); // Obtener el primer registro
            if (currentUrl != null)
            {
                currentUrl.CurrentIndex = 0;
                _ctx.CurrentUrls.Update(currentUrl);
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<bool> AddFailedUrlAsync(FailedUrl failedUrl)
        {
            try
            {
                _ctx.FailedUrls.Add(failedUrl);
                await _ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<List<FailedUrl>> GetRetryableFailedUrlsAsync()
        {
            return await _ctx.FailedUrls
                .Where(f => f.RetryCount < 5 && !f.IsPermanentlyFailed)
                .ToListAsync();
        }

        public async Task<bool> DeleteFailedUrlAsync(int id)
        {
            var failedUrl = await _ctx.FailedUrls.FindAsync(id);
            if (failedUrl == null)
            {
                return false;
            }

            _ctx.FailedUrls.Remove(failedUrl);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFailedUrlAsync(FailedUrl failedUrl)
        {
            _ctx.Entry(failedUrl).State = EntityState.Modified;
            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FailedUrlExists(failedUrl.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        private bool FailedUrlExists(int id)
        {
            return _ctx.FailedUrls.Any(e => e.Id == id);
        }

        public async Task UpdateNullDescriptionsInTenders()
        {
            // Encuentra todos los registros en la tabla Tenders donde Description es NULL
            var tendersWithNullDescription = await _ctx.Tenders.Where(t => t.Description == null).ToListAsync();

            // Actualiza la columna Description a 'N/A' para esos registros
            foreach (var tender in tendersWithNullDescription)
            {
                tender.Description = "N/A";
            }

            // Guarda los cambios en la base de datos
            await _ctx.SaveChangesAsync();
        }
    }

}