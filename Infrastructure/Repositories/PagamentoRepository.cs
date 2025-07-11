using Crosscutting.DTOs;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Infrastructure.Repositories
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly IPagamentoContext _context;

        public PagamentoRepository(IPagamentoContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Pagamento pagamento)
        {
            await _context.Pagamentos.InsertOneAsync(pagamento);
        }

        public async Task<List<Pagamento>> ObterPagamentosAsync(ObterPagamentosDTO dto)
        {
            var pagamentoQuery = _context.Pagamentos.AsQueryable();

            if (dto.IdPedido.HasValue)
            {
                pagamentoQuery.Where(c => c.IdPedido == dto.IdPedido);
            }

            // Filtrar por status específico (opcional)
            if (!string.IsNullOrEmpty(dto.StatusPagamento))
            {
                pagamentoQuery.Where(c => c.StatusPagamento == dto.StatusPagamento);
            }

            // Obtem os pagamentos filtrados
            return await pagamentoQuery.OrderByDescending(c => c.DataStatusPagamento)
                .ToListAsync();
        }

    }
}