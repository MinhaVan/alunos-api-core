using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using Aluno.Core.Domain.Interfaces.Repositories;

namespace Aluno.Core.Data.Implementations;

public class RabbitMqRepository : IRabbitMqRepository
{
    private readonly ConnectionFactory _factory;

    public RabbitMqRepository(ConnectionFactory factory)
    {
        _factory = factory;
    }

    public Task PublishAsync<T>(string queue, T data)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var message = System.Text.Json.JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            exchange: "",
            routingKey: queue,
            basicProperties: null,
            body: body
        );

        return Task.CompletedTask;
    }
}