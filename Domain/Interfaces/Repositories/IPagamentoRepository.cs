using Crosscutting.DTOs;
using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IPagamentoRepository
    {
        Task AddAsync(Pagamento pagamento);
        Task<List<Pagamento>> ObterPagamentosAsync(ObterPagamentosDTO dto);
    }
}
