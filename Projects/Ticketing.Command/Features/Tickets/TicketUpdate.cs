using FluentValidation;
using MediatR;
using Ticketing.Command.Features.Apis;

namespace Ticketing.Command.Features.Tickets;

public class TicketUpdate : IMinimalApi
{
    public void AddEnpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        throw new NotImplementedException();
    }

    public sealed class TicketUpdateRequest(
            int ticketType, 
            string description,
            string username
        )
    {
        public int TicketType { get; } = ticketType;
        public string Description { get; } = description;
        public string UserName { get; } = username;
    }

    public record TicketUpdateCommand(
            string Id,
            TicketUpdateRequest TicketUpdateRequest
        ):IRequest<bool>;

    public class TicketUpdateCommandValidator : AbstractValidator<TicketUpdateCommand>
    {
        public TicketUpdateCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.TicketUpdateRequest)
                .SetValidator(new TicketUpdateRequestValidator());

        }
    }

    public class TicketUpdateRequestValidator:AbstractValidator<TicketUpdateRequest>
    {
        public TicketUpdateRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage("Ingrese una descripción");
            RuleFor(x => x.UserName)
                .NotEmpty()
                    .WithMessage("Ingrese un username");
            RuleFor(x => x.TicketType)
                .NotEmpty()
                    .WithMessage("Ingrese el tipo de ticket")
                .InclusiveBetween(1, 5)
                    .WithMessage("El tipo ticket puede estar en el rango de 1 a 5");
        }
    }
}
