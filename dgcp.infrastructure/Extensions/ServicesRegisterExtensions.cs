using dgcp.domain.Abstractions;
using dgcp.infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dgcp.infrastructure.Extensions
{
    public static class ServicesRegisterExtensions
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IDataService, DataService>();
        }
    }
}