namespace Crosscutting.DTOs
{
    public class PagamentoResponse
    {
        public Guid IdPagamento { get; set; }

        public string? QrCode { get; set; }
    }
}
