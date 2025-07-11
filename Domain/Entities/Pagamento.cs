using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class Pagamento
    {
        public const string STATUS_AGUARDANDO = "Aguardando";
        public const string STATUS_PAGO = "Pago";
        public const string STATUS_CANCELADO = "Cancelado";

        public static readonly string[] STATUS = new string[] { STATUS_AGUARDANDO, STATUS_PAGO, STATUS_CANCELADO };

        public Pagamento(Guid idPedido, string statusPagamento, DateTime dataStatusPagamento)
        {
            if (!STATUS.Contains(statusPagamento))
                throw new ArgumentException("O status está incorreto");

            IdPagamento = Guid.NewGuid();
            IdPedido = idPedido;
            AtualizarStatusPagamento(statusPagamento, dataStatusPagamento);
        }

        public void AtualizarStatusPagamento(string statusPagamento, DateTime dataStatusPagamento)
        {
            StatusPagamento = statusPagamento;
            DataStatusPagamento = dataStatusPagamento;
        }

        // Construtor sem parâmetros necessário para deserialização
        private Pagamento() { }


        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid IdPagamento { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public Guid IdPedido { get; private set; }

        public string StatusPagamento { get; private set; }

        public DateTime DataStatusPagamento { get; private set; }
    }
}