
using Common.Core.Events;
using Confluent.Kafka;
using System.Text.Json;
using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Infrastructure.Converters;

namespace Ticketing.Query.Infrastructure.Consumers;

//Esta clase va a trabajar en background
//y va a estar escuchando los eventos que se
//publiquen en el bus de eventos, para luego enviarlos al EventHandler
public class ConsumerHostedService : IHostedService
{
    private readonly ILogger<ConsumerHostedService> _logger;
    private readonly ConsumerConfig _config;
    private readonly IServiceProvider _serviceProvider;

    public ConsumerHostedService(ILogger<ConsumerHostedService> logger,
                                 ConsumerConfig config,
                                 IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("El event consumer esta trabajando");
        // nombre del topic a suscribirse (debe coincidir con el topic donde se publican eventos)
        var topic = "KAFKA_TOPIC";

        // -------------------------
        // 1) Construcción del consumer
        // -------------------------
        // Se crea un Consumer de Confluent.Kafka con la configuración inyectada.
        // Se especifican los deserializadores para clave y valor (en este caso UTF8 -> string).
        using var consumer = new ConsumerBuilder<string, string>(_config)
                                .SetKeyDeserializer(Deserializers.Utf8)
                                .SetValueDeserializer(Deserializers.Utf8)
                                .Build();

        // -------------------------
        // 2) Suscripción al topic
        // -------------------------
        // El consumidor se subscribe al topic indicado. A partir de aquí recibirá mensajes publicados.
        consumer.Subscribe(topic);

        // -------------------------
        // 3) Bucle de consumo
        // -------------------------
        // Loop continuo que consume mensajes. Se utiliza el CancellationToken para poder detenerlo.
        while (true)
        {
            // Consume un mensaje (bloqueante hasta que llega uno o se cancela).
            var consumeResult = consumer.Consume(cancellationToken);
            if (consumeResult is null) continue; // no hay resultado, seguir esperando
            if (consumeResult.Message is null) continue; // mensaje nulo, ignorar

            // -------------------------
            // 4) Deserialización del evento
            // -------------------------
            // Configuramos JsonSerializer para soportar la conversión personalizada de eventos
            var options = new JsonSerializerOptions
            {
                Converters = { new EventJsonConverter() }
            };
            // Deserializamos el payload (string JSON) a la clase base BaseEvent
            var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);

            if (@event is null) throw new ArgumentNullException($"El {nameof(@event)} no se pudo procesr el json parser");

            // -------------------------
            // 5) Resolución del handler mediante DI
            // -------------------------
            // Creamos un scope para resolver dependencias con lifetime Scoped/Transient
            using IServiceScope scope = _serviceProvider.CreateScope();
            var _eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler>();

            // -------------------------
            // 6) Invocación del handler correspondiente
            // -------------------------
            // Buscamos mediante reflexión el método On que acepte el tipo concreto de evento
            var handlerMethod =  _eventHandler.GetType().GetMethod("On", new Type[] {@event.GetType()});
            if (handlerMethod is null)
            {
                // Si no hay método, se considera error de configuración/implementación
                throw new ArgumentNullException(nameof(handlerMethod), "No se encontro el método handler correspondiente");
            }

            // Invocamos el handler pasando la instancia del evento
            handlerMethod.Invoke(_eventHandler, new object[] { @event });

            // -------------------------
            // 7) Commit del offset
            // -------------------------
            // Confirmamos a Kafka que hemos procesado el mensaje correctamente (avanzar offset)
            consumer.Commit(consumeResult);
        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Aquí se debería implementar la lógica para detener el servicio de forma ordenada.
        // Por ejemplo: señalizar al bucle de consumo que finalice y cerrar/Dispose del consumer.
        // Actualmente no implementado intencionalmente.
        return Task.CompletedTask;
    }
}
