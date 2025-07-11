using Application.Middleares;
using Application.UseCases;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using Infrastructure.MessageBus;
using Infrastructure.Repositories;
using Infrastructure.ServicosExternos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🔐 Swagger + Autenticação
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Processador de Vídeos", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// 🔐 Configuração do JWT
var key = Encoding.UTF8.GetBytes("sua_chave_secreta_super_segura_aqui"); // troque por uma segura vinda do appsettings ou secret
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // true em produção
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Host"]; // ex: redis:6379
});

// Repositórios
builder.Services.AddScoped<IPagamentoContext, PagamentoContext>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();

// Serviços externos
builder.Services.AddScoped<IProducaoService, ProducaoService>();
builder.Services.AddHttpClient<IProducaoService, ProducaoService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoServiceFake>();

// Use cases
builder.Services.AddScoped<IGerarPagamentoUseCase, GerarPagamentoUseCase>();
builder.Services.AddScoped<IAtualizarPagamentoUseCase, AtualizarPagamentoUseCase>();
builder.Services.AddScoped<IAuthUseCase, AuthUseCase>();

// Mensageria
builder.Services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseMiddleware<ExceptionsMiddleware>();

// 🔐 Middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }
