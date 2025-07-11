Feature: Gerar pagamento

  Scenario: Deve gerar um pagamento com sucesso
    Given que o sistema está conectado a um banco MongoDB em memória
    And que tenho um pedido com valor de 100 reais
    When eu envio a requisição para gerar o pagamento
    Then o pagamento deve ser criado com sucesso