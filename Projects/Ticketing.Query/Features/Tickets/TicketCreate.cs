using MediatR;
using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Domain.Employees;
using Ticketing.Query.Domain.Tickets;
using Ticketing.Query.Domain.TicketTypes;

namespace Ticketing.Query.Features.Tickets;

public sealed class TicketCreate
{
    public record TicketCreateCommand(
            string ID, 
            string Username, 
            int? TicketType, 
            string DetailError) : IRequest<string>;

    public class TicketCreateCommandhandler : IRequestHandler<TicketCreateCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TicketCreateCommandhandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(TicketCreateCommand request, CancellationToken cancellationToken)
        {
            //1 insertar data de employee
            var employee = await _unitOfWork.EmployeeRepository.GetByUserNameAsync(request.Username);
            if (employee is null) 
            {
                employee = Employee.Create(string.Empty, string.Empty, null!, request.Username);
                _unitOfWork.EmployeeRepository.AddEntity(employee);
            }
            //2 Luego insertar data del ticket
            var ticket = Ticket.Create(
                new Guid(request.ID),
                TicketType.Create(request.TicketType),
                request.DetailError
            );
            _unitOfWork.RepositoryGeneric<Ticket>().AddEntity(ticket);
            //3 Insertar data del ticketEmployee
            var ticketEmployee = TicketEmployee.Create(ticket, employee);
            _unitOfWork.RepositoryGeneric<TicketEmployee>().AddEntity(ticketEmployee);
            await _unitOfWork.Complete();

            return Convert.ToString(ticket.Id)!;
        }
    }
}
