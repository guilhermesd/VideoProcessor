namespace Crosscutting.DTOs.MercadoPago
{
    public class MercadoPagaoQrCodeResponse
    {
        /// <summary>
        /// Identificador da transação no mercado livre
        /// </summary>
        public string external_reference { get; set; }

        /// <summary>
        /// QrCode Gerado
        /// </summary>
        public string qr_data { get; set; }

    }
}
