using Domain.Interfaces.ServicosExternos;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infrastructure.MessageBus
{
    public class RabbitMqConsumer : IRabbitMqConsumer
    {
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqConsumer(IConfiguration config)
        {
            _config = config;
            ConnectWithRetry().GetAwaiter().GetResult();
        }

        private async Task ConnectWithRetry(int maxAttempts = 5, int delaySeconds = 5)
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMq:Host"] ?? "localhost",
                Port = 5672,
                UserName = _config["RabbitMq:Username"] ?? "guest",
                Password = _config["RabbitMq:Password"] ?? "guest",
                DispatchConsumersAsync = true
            };

            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxAttempts)
            {
                try
                {
                    Console.WriteLine($"[RabbitMQ] Tentando conectar (tentativa {attempt + 1}) em: {factory.HostName}:{factory.Port}");

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare("video-processing-queue", true, false, false, null);

                    Console.WriteLine("[RabbitMQ] Conectado com sucesso!");
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine($"[RabbitMQ] Falha na conexão: {ex.Message}");
                    attempt++;
                    await Task.Delay(delaySeconds * 1000);
                }
            }

            throw new Exception("[RabbitMQ] Não foi possível conectar após várias tentativas", lastException);
        }

        public void Consume(string queue, Func<string, Task> handleMessage)
        {
            if (_channel == null)
                throw new InvalidOperationException("Canal RabbitMQ não está inicializado");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await handleMessage(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQ] Erro ao processar mensagem: {ex.Message}");
                    // Se quiser reencaminhar ou rejeitar:
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
        }
    }
}
