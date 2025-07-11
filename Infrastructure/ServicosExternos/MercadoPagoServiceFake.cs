using Crosscutting.DTOs.MercadoPago;
using Domain.Interfaces.ServicosExternos;

namespace Infrastructure.ServicosExternos
{
    public class MercadoPagoServiceFake : IMercadoPagoService
    {
        public async Task<MercadoPagaoQrCodeResponse> GerarQRCodeAsync(Guid idPedido, decimal valor)
        {
            return new MercadoPagaoQrCodeResponse
            {
                external_reference = idPedido.ToString(),
                qr_data = Guid.NewGuid().ToString()
            };
        }
    }
}