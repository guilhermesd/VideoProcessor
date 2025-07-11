namespace Crosscutting.DTOs
{
    public class PagamentoRequest
    {
        public Guid IdPedido { get; set; }

        public decimal Valor { get; set; }
    }
}
