using Crosscutting.DTOs;
using Crosscutting.DTOs.MercadoPago;
using Crosscutting.Exceptions;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;

namespace Application.UseCases
{
    public interface IGerarPagamentoUseCase
    {
        Task<PagamentoResponse> ExecutarAsync(PagamentoRequest pagamentoRequest);
    }

    public class GerarPagamentoUseCase : IGerarPagamentoUseCase
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IPagamentoRepository _pagamentoRepository;

        public GerarPagamentoUseCase(IMercadoPagoService mercadoPagoService, IPagamentoRepository pagamentoRepository)
        {
            _mercadoPagoService = mercadoPagoService;
            _pagamentoRepository = pagamentoRepository;
        }

        public async Task<PagamentoResponse> ExecutarAsync(PagamentoRequest pagamentoRequest)
        {
            var pagamento = new Pagamento(pagamentoRequest.IdPedido, Pagamento.STATUS_AGUARDANDO, DateTime.Now);

            var responseMercadoPago = await _mercadoPagoService.GerarQRCodeAsync(pagamento.IdPedido, pagamentoRequest.Valor);

            await _pagamentoRepository.AddAsync(pagamento);

            return new PagamentoResponse
            {
                IdPagamento = pagamento.IdPagamento,
                QrCode = responseMercadoPago.qr_data
            };
        }
    }

    public interface IAtualizarPagamentoUseCase
    {
        Task<bool> ExecutarAsync(MercadoPagoWebhookModel pagamentoRequest);
    }

    public class AtualizarPagamentoUseCase : IAtualizarPagamentoUseCase
    {
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly IProducaoService _producaoService;

        public AtualizarPagamentoUseCase(IPagamentoRepository pagamentoRepository, IProducaoService producaoService)
        {
            _pagamentoRepository = pagamentoRepository;
            _producaoService = producaoService;
        }

        public async Task<bool> ExecutarAsync(MercadoPagoWebhookModel pagamentoRequest)
        {
            var pagamentos = await _pagamentoRepository.ObterPagamentosAsync(new ObterPagamentosDTO
            {
                IdPedido = Guid.Parse(pagamentoRequest.Data.external_reference)
            });

            if (pagamentos.Count() == 0)
                throw new NotFoundException("Pedido não encontrado");

            var pagamento = new Pagamento(Guid.Parse(pagamentoRequest.Data.external_reference),
                pagamentoRequest.Data.Status == "approved" ? Pagamento.STATUS_PAGO : Pagamento.STATUS_CANCELADO,
                DateTime.Now);

            await _pagamentoRepository.AddAsync(pagamento);

            await _producaoService.RegistrarStatusProducaoPedido(new RegistraStatusPedidoDTO
            {
                idPedido = pagamento.IdPedido,
                StatusPedido = StatusPedido.EmPreparacao
            });

            return true;
        }
    }
}
