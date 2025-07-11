using Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IPagamentoContext
    {
        IMongoCollection<Pagamento> Pagamentos { get; }
        IMongoCollection<VideoJob> VideoJobs { get; }
        IMongoCollection<User> Users { get; }
    }

    public class PagamentoContext : IPagamentoContext
    {
        private readonly IMongoDatabase _database;

        public PagamentoContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }

        public IMongoCollection<Pagamento> Pagamentos => _database.GetCollection<Pagamento>("Pagamentos");
        public IMongoCollection<VideoJob> VideoJobs => _database.GetCollection<VideoJob>("VideoJobs");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}