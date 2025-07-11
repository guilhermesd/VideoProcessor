using Domain.Entities;
using FluentAssertions;

namespace Tests.Unidade
{
    public class PagamentoTests
    {
        [Fact]
        public void Construtor_Deve_Criar_Pagamento_Valido()
        {
            // Arrange
            var idPedido = Guid.NewGuid();
            var status = "Pago";
            var dataStatus = DateTime.UtcNow;

            // Act
            var pagamento = new Pagamento(idPedido, status, dataStatus);

            // Assert
            pagamento.IdPedido.Should().Be(idPedido);
            pagamento.StatusPagamento.Should().Be(status);
            pagamento.DataStatusPagamento.Should().BeCloseTo(dataStatus, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData("Invalido")]
        [InlineData("")]
        [InlineData(null)]
        public void Construtor_Deve_Lancar_Excecao_Para_Status_Invalido(string statusInvalido)
        {
            // Arrange
            var idPedido = Guid.NewGuid();
            var dataStatus = DateTime.UtcNow;

            // Act
            Action acao = () => new Pagamento(idPedido, statusInvalido, dataStatus);

            // Assert
            acao.Should().Throw<ArgumentException>()
                .WithMessage("O status está incorreto");
        }
    }
}
