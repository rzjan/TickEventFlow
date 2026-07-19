using Common.Core.Events;
using FluentValidation;
using MediatR;
using Ticketing.Command.Application.Aggregates;
using Ticketing.Command.Domain.Abstract;
using Ticketing.Command.Domain.EventModels;
using Ticketing.Command.Features.Apis;

namespace Ticketing.Command.Features.Tickets;

public sealed class TicketCreate : IMinimalApi
{
    public void AddEnpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {



        endpointRouteBuilder.MapPost("/api/tickets", async (
            TicketCreateRequest ticketCreateRequest,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            try
            {
                var id = Guid.CreateVersion7(DateTimeOffset.UtcNow).ToString();
                var command = new TicketCreateCommand(id, ticketCreateRequest);
                var result = await mediator.Send(command);
                if (result)
                {
                    return Results.Ok(new { message = "Ticket creado exitosamente" });
                }

                return Results.Problem("Error al crear el ticket");
            }
            catch (Exception)
            {
                // No exponer detalles de la excepción al cliente; registrar internamente en el handler
                return Results.Problem("Error al crear el ticket");
            }
        });
    }

    /// Representa una solicitud para crear un nuevo ticket de error, incluyendo información sobre el usuario, el tipo
    /// de error y los detalles asociados.    
    public sealed class TicketCreateRequest(
            string username,
            int? typeError,
            string detailError
    )
    {
        public string Username { get; set; } = username;
        public int? TypeError { get; set; } = typeError;
        public string DetailError { get; set; } = detailError;
    }

    /// Comando que encapsula la solicitud de creación de un ticket, implementando la interfaz IRequest de MediatR
    public record TicketCreateCommand(string Id, TicketCreateRequest ticketCreateRequest) : IRequest<bool>;

    /// Valida la solicitud de creación de un ticket, asegurando que los campos requeridos estén presentes
    public class TicketCreateCommandValidator : AbstractValidator<TicketCreateCommand>
    {
        public TicketCreateCommandValidator()
        {
            RuleFor(x => x.ticketCreateRequest)
            .SetValidator(new TicketCreateValidator());

            RuleFor(x => x.Id).NotEmpty().WithMessage("Ingrese el Id del eveento");
        }
    }

    /// Valida los campos específicos de la solicitud de creación de un ticket, 
    /// como el nombre de usuario y los detalles del error
    public class TicketCreateValidator : AbstractValidator<TicketCreateRequest>
    {
        public TicketCreateValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Ingrese un username")
                .EmailAddress()
                    .WithMessage("Debe ser un email válido");

            RuleFor(x => x.TypeError)
                 .NotEmpty()
                    .WithMessage("Debe existir el tipo de error")
                 .InclusiveBetween(1, 5)
                    .WithMessage("El tipo de error debe estar entre 1 y 5");



            RuleFor(x => x.DetailError).NotEmpty()
            .WithMessage("Ingrese el detalle del error");
        }
    }

    /// Manejador del comando de creación de un ticket, 
    /// responsable de procesar la solicitud y persistir el evento en la base de datos
    public sealed class TicketCreateCommandHandler(
        IEventSourcingHandler<TicketAggregate> eventSourcingHandler,
        ILogger<TicketCreateCommandHandler> logger
    ) : IRequestHandler<TicketCreateCommand, bool>
    {
        private readonly ILogger<TicketCreateCommandHandler> _logger = logger;
        private readonly IEventSourcingHandler<TicketAggregate> 
            _eventSourceingHandler = eventSourcingHandler;

        public async Task<bool> Handle(
            TicketCreateCommand request,
            CancellationToken cancellationToken
        )
        {
           var aggregate = new TicketAggregate(request);
           await _eventSourceingHandler.SaveChanges(aggregate, cancellationToken);
           return true;
        }
    }
}
