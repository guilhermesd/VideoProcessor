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
using TechTalk.SpecFlow;
using Testcontainers.MongoDb;

namespace Tests.BDD
{
    [Binding]
    public class GerarPagamentoSteps : IAsyncLifetime
    {
        private readonly ScenarioContext _context;
        private readonly MongoDbContainer _mongoContainer;
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private PagamentoRequest _request = null!;
        private HttpResponseMessage _response = null!;
        private string _responseContent = null!;

        public GerarPagamentoSteps(ScenarioContext context)
        {
            _context = context;
            _mongoContainer = new MongoDbBuilder()
                .WithImage("mongo:7.0")
                .WithCleanUp(true)
                .WithName($"mongo-specflow-{Guid.NewGuid()}")
                .WithPortBinding(27017, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();

            var connString = _mongoContainer.GetConnectionString();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var settings = new Dictionary<string, string>
                        {
                            ["MongoDB:ConnectionString"] = connString,
                            ["MongoDB:DatabaseName"] = "SpecflowTestDb"
                        };
                        configBuilder.AddInMemoryCollection(settings);
                    });

                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<IProducaoService>();
                        services.AddScoped<IProducaoService, ProducaoServiceMock>();
                    });
                });

            _client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            await _mongoContainer.DisposeAsync();
        }

        [Given("que o sistema está conectado a um banco MongoDB em memória")]
        public void GivenQueOSistemaEstaConectadoAoMongo()
        {
            // Já está conectado via `InitializeAsync`
        }

        [Given("que tenho um pedido com valor de 100 reais")]
        public void GivenQueTenhoUmPedidoComValor()
        {
            _request = new PagamentoRequest
            {
                IdPedido = Guid.NewGuid(),
                Valor = 100.00m
            };
        }

        [When("eu envio a requisição para gerar o pagamento")]
        public async Task WhenEnvioARequisicaoParaGerarPagamento()
        {
            _response = await _client.PostAsJsonAsync("/api/pagamentos", _request);
            _responseContent = await _response.Content.ReadAsStringAsync();
        }

        [Then("o pagamento deve ser criado com sucesso")]
        public void ThenPagamentoCriadoComSucesso()
        {
            _response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            await InitializeAsync();
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            await DisposeAsync();
        }
    }
}
