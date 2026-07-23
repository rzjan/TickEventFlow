
using Common.Core.Events;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
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
    private CancellationTokenSource? _cts;
    private Task? _executingTask;
    private const int MaxHandlerRetries = 3;
    private readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(1);

    public ConsumerHostedService(ILogger<ConsumerHostedService> logger,
                                 IOptions<ConsumerConfig> config,
                                 IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config.Value;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("El event consumer esta trabajando");

        // Crear un CTS ligado al token del host para poder cancelar internamente
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        // Iniciar la tarea de ejecución en background
        _executingTask = Task.Run(() => ExecuteAsync(_cts.Token));

        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // nombre del topic a suscribirse (debe coincidir con el topic donde se publican eventos)
        var topic = "KAFKA_TOPIC";

        using var consumer = new ConsumerBuilder<string, string>(_config)
                                .SetKeyDeserializer(Deserializers.Utf8)
                                .SetValueDeserializer(Deserializers.Utf8)
                                .Build();

        consumer.Subscribe(topic);

        var options = new JsonSerializerOptions
        {
            Converters = { new EventJsonConverter() }
        };

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult = null;
                try
                {
                    consumeResult = consumer.Consume(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Se solicitó cancelación, salir del loop
                    break;
                }

                if (consumeResult is null) continue;
                if (consumeResult.Message is null) continue;

                var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);
                if (@event is null)
                {
                    _logger.LogWarning("Mensaje recibido no pudo deserializarse a BaseEvent. Offset: {Offset}", consumeResult.Offset);
                    // No commiteamos para permitir inspección/manual handling
                    continue;
                }

                // Intentos de manejo con backoff simple. Crear un scope por intento para evitar que
                // el mismo DbContext (y sus entidades fallidas) queden en estado rastreado entre reintentos.
                var handled = false;
                for (int attempt = 1; attempt <= MaxHandlerRetries && !handled; attempt++)
                {
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    var _eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler>();

                    var handlerMethod = _eventHandler.GetType().GetMethod("On", new Type[] { @event.GetType() });
                    if (handlerMethod is null)
                    {
                        _logger.LogError("No se encontro el método handler correspondiente para {EventType}", @event.GetType().Name);
                        break;
                    }

                    try
                    {
                        var invokeResult = handlerMethod.Invoke(_eventHandler, new object[] { @event });
                        if (invokeResult is System.Threading.Tasks.Task task)
                        {
                            await task.ConfigureAwait(false);
                        }

                        // Si llegamos acá, fue procesado correctamente
                        consumer.Commit(consumeResult);
                        handled = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando evento {EventType} en intento {Attempt}", @event.GetType().Name, attempt);

                        if (attempt < MaxHandlerRetries)
                        {
                            var delay = TimeSpan.FromMilliseconds(BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogWarning("Evento {EventType} falló tras {Attempts} intentos. Offset: {Offset}", @event.GetType().Name, MaxHandlerRetries, consumeResult.Offset);
                        }
                    }
                }
            }
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cerrando consumer");
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Señalizar cancelación a la tarea de consumo
        if (_cts is null)
            return;

        try
        {
            _cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Ignorar
        }

        if (_executingTask is not null)
        {
            // Esperar hasta que termine o hasta que el host fuerce el cierre
            await Task.WhenAny(_executingTask, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)).ConfigureAwait(false);
        }
    }
}
