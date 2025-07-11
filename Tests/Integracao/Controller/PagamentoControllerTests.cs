using Crosscutting.DTOs;
using Crosscutting.DTOs.MercadoPago;
using Domain.Interfaces.ServicosExternos;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Infrastructure.ServicosExternos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Testcontainers.MongoDb;

namespace Tests.Integracao.Controller
{
    public class PagamentoControllerTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoContainer;
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        public PagamentoControllerTests()
        {
            _mongoContainer = new MongoDbBuilder()
                .WithImage("mongo:7.0")
                .WithCleanUp(true)
                .WithName($"mongo-pedidos-test{Guid.NewGuid()}")
                .WithPortBinding(27017, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();

            var connectionString = _mongoContainer.GetConnectionString(); // exemplo: "mongodb://localhost:32776"

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var inMemorySettings = new Dictionary<string, string>
                        {
                            ["MongoDB:ConnectionString"] = connectionString,
                            ["MongoDB:DatabaseName"] = "TestDb"
                        };

                        configBuilder.AddInMemoryCollection(inMemorySettings);
                    });

                    builder.ConfigureServices(services =>
                    {
                        // Se necessário, substitua repositórios ou services
                        services.RemoveAll<IProducaoService>(); // Garante que o real não está registrado
                        services.AddScoped<IProducaoService, ProducaoServiceMock>();

                    });
                });

            _client = _factory.CreateClient();
        }

        private PagamentoRequest request = new() 
        {
            IdPedido = Guid.NewGuid(),
            Valor = 100.00m
        };

        public async Task DisposeAsync()
        {
            await _mongoContainer.DisposeAsync();
        }

        private async Task<string> GerarPagamento()
        {
            var response = await _client.PostAsJsonAsync("/api/pagamentos", request);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            return await response.Content.ReadAsStringAsync();
        }

        [Fact(DisplayName = "POST /api/pagamentos - Deve gerar pagamento com sucesso")]
        public async Task Post_GerarPagamento_DeveCriarPagamento()
        {
            var content = await GerarPagamento();
            content.Should().Contain("qrCode").And.Contain("idPagamento");
        }

        [Fact(DisplayName = "POST /api/pagamentos/webhook/notificacoes - Deve processar notificação")]
        public async Task Post_WebhookNotificacoes_DeveAtualizarPagamento()
        {
            var pagamentoResponse = JsonConvert.DeserializeObject<PagamentoResponse>(await GerarPagamento());

            var webhook = new MercadoPagoWebhookModel
            {
                Id = 12345,
                LiveMode = true,
                Type = "payment",
                DateCreated = DateTime.UtcNow,
                UserId = 123,
                ApiVersion = "v1",
                Action = "payment.created",
                Data = new Data
                {
                    Id = "999999999",
                    Status = "approved",
                    external_reference = Guid.NewGuid().ToString()
                }
            };

            var response = await _client.PostAsJsonAsync("/api/pagamentos/webhook/notificacoes", webhook);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
