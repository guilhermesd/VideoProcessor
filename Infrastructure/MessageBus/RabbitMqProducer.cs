using Domain.Interfaces.ServicosExternos;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace Infrastructure.MessageBus
{
    public class RabbitMqProducer : IRabbitMqProducer
    {
        private readonly IModel _channel;

        public RabbitMqProducer(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMq:Host"] ?? "localhost",
                Port = 5672,
                UserName = config["RabbitMq:UserName"] ?? "guest",
                Password = config["RabbitMq:Password"] ?? "guest"
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.QueueDeclare("video-processing-queue", true, false, false, null);
        }

        public void Publish(string queue, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish("", queue, null, body);
        }
    }

}
