using Domain.Interfaces.ServicosExternos;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.ServicosExternos
{
    public class ProducaoService : IProducaoService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public ProducaoService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }
        public async Task<bool> RegistrarStatusProducaoPedido(RegistraStatusPedidoDTO registraStatusPedido)
        {
            var response = await _http.PostAsJsonAsync($"{_configuration["UrlProducao"]}/api/producao/registra-status-pedido", registraStatusPedido);
            response.EnsureSuccessStatusCode();
            return true;
        }
    }

    public class ProducaoServiceMock : IProducaoService
    {
        public async Task<bool> RegistrarStatusProducaoPedido(RegistraStatusPedidoDTO registraStatusPedido)
        {
            return true;
        }
    }
}