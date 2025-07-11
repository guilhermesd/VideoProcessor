namespace Crosscutting.DTOs
{
    public class ObterPagamentosDTO
    {
        public Guid? IdPedido { get; set; }

        public string? StatusPagamento {  get; set; }
    }
}