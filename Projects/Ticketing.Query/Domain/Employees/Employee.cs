using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Domain.Adresses;

namespace Ticketing.Query.Domain.Employees;

public class Employee:Entity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required Address Address { get; set; } //Se vambia por objet value luego
    public required string Email { get; set; }

}
