using Crosscutting.DTOs.MercadoPago;

namespace Domain.Interfaces.ServicosExternos
{
    public interface IMercadoPagoService
    {
        Task<MercadoPagaoQrCodeResponse> GerarQRCodeAsync(Guid idPedido, decimal valor);
    }
}