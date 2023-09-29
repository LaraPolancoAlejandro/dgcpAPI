using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using dgcp.domain.Abstractions;
using dgcp.domain.Models;


namespace dgcp.infrastructure.Services
{
    public class UsersService : IUsersService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IDataService _service;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration; // Inyectar configuración
        private readonly IServiceScopeFactory _scope;

        public UsersService(IDataService service, IHttpClientFactory clientFactory, IServiceScopeFactory scope, IConfiguration configuration)
        {
            this._service = service;
            this._client = clientFactory.CreateClient(); // Usar IHttpClientFactory
            this._scope = scope;
            this._configuration = configuration; // Configuración inyectada
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserNameOrEmail), // Asegúrate de que esta propiedad exista
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Añade más claims según sea necesario
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
