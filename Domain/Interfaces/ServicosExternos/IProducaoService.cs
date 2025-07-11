namespace Domain.Interfaces.ServicosExternos
{
    public enum StatusPedido
    {
        EmPreparacao = 1,
        Pronto = 2
    }

    public class RegistraStatusPedidoDTO
    {
        public Guid idPedido { get; set; }
        public StatusPedido StatusPedido { get; set; }
    }

    public interface IProducaoService
    {
        public Task<bool> RegistrarStatusProducaoPedido(RegistraStatusPedidoDTO registraStatusPedidoDTO);
    }
}