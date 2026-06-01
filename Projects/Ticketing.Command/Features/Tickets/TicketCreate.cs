using AutoMapper;
using Common.Core.Events;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using Ticketing.Command.Domain.EventModels;

namespace Ticketing.Command.Features.Tickets;

public class TicketCreate
{    
    /// Representa una solicitud para crear un nuevo ticket de error, incluyendo información sobre el usuario, el tipo
    /// de error y los detalles asociados.    
    public sealed class TicketCreateRequest(
            string username,
            string typeError,
            string detailError
    )
    {
        public string Username { get; set; } = username;
        public string TypeError { get; set; } = typeError;
        public string DetailError { get; set; } = detailError;
    }

    /// Comando que encapsula la solicitud de creación de un ticket, implementando la interfaz IRequest de MediatR
    public record TicketCreateCommand(TicketCreateRequest ticketCreateRequest) : IRequest<bool>;

    /// Valida la solicitud de creación de un ticket, asegurando que los campos requeridos estén presentes
    public class TicketCreateCommandValidator : AbstractValidator<TicketCreateCommand>
    {
        public TicketCreateCommandValidator()
        {
            RuleFor(x => x.ticketCreateRequest)
            .SetValidator(new TicketCreateValidator());
        }
    }

    /// Valida los campos específicos de la solicitud de creación de un ticket, 
    /// como el nombre de usuario y los detalles del error
    public class TicketCreateValidator : AbstractValidator<TicketCreateRequest>
    {
        public TicketCreateValidator()
        {
            RuleFor(x => x.Username).NotEmpty()
            .WithMessage("Ingrese un username");

            RuleFor(x => x.DetailError).NotEmpty()
            .WithMessage("Ingrese el detalle del error");
        }
    }

    /// Manejador del comando de creación de un ticket, 
    /// responsable de procesar la solicitud y persistir el evento en la base de datos
    public sealed class TicketCreateCommandHandler(
        IEventModelRepository eventModelRepository,
        IMapper mapper
    ): IRequestHandler<TicketCreateCommand, bool>
    {
        private readonly IEventModelRepository _eventModelRepository = eventModelRepository;
        private readonly IMapper _mapper = mapper;


        public async Task<bool> Handle(
            TicketCreateCommand request,
            CancellationToken cancellationToken
        )
        {

            var ticketEventData = _mapper
            .Map<TicketCreatedEvent>(request.ticketCreateRequest);

            // Crear un nuevo modelo de evento con los datos del ticket creado,
            // incluyendo información como el timestamp, el identificador del agregado,
            // el tipo de evento y los datos del evento
            var eventModel = new EventModel
            {
                Timestamp = DateTime.UtcNow,
                AggregateIdentifier = Guid
                            .CreateVersion7(DateTimeOffset.UtcNow).ToString(),
                AggregateType = "TicketAggregate",
                Version = 1,
                EventType = "TicketCreatedEvent",
                EventData = ticketEventData
            };

            IClientSessionHandle session = await _eventModelRepository
            .BeginSessionAsync(cancellationToken);

            try
            {
                _eventModelRepository.BeginTransaction(session);
                await _eventModelRepository
                .InsertOneAsync(eventModel, session, cancellationToken);

                await _eventModelRepository
                .CommitTransactionAsync(session, cancellationToken);

                _eventModelRepository.DisposeSession(session);

                return true;

            }
            catch (Exception)
            {
                await _eventModelRepository
                .RollbackTransactionAsync(session, cancellationToken);

                _eventModelRepository.DisposeSession(session);
                return false;
            }
        }
    }
}
