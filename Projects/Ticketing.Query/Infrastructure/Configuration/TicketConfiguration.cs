using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Query.Domain.Tickets;
using Ticketing.Query.Domain.TicketTypes;

namespace Ticketing.Query.Infrastructure.Configuration;

public class TicketEmployeeConfiguration : IEntityTypeConfiguration<TicketEmployee>
{
    public void Configure(EntityTypeBuilder<TicketEmployee> builder)
    {
        builder.ToTable("TicketEmployees");
        builder.HasKey(te => new { te.TicketId, te.EmploteeId });
    }
}
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t=>t.TicketType)
            .HasConversion(
                v => v!.Id,
                v => TicketType.Create(v)
            );

        //relacion muchos a muchos entre Ticket y Employee
        //a traves de la tabla intermedia TicketEmployee
        builder
            .HasMany(e => e.Employees)
            .WithMany(t => t.Tickets)
            //Se crea la tabla intermedia TicketEmployee
            .UsingEntity<TicketEmployee>(
                //Se configura la relación entre TicketEmployee y Employee
                j => j
                .HasOne(p => p.Employee)
                .WithMany(p => p.TicketEmployees)
                .HasForeignKey(p => p.EmploteeId),
                //Se configura la relación entre TicketEmployee y Ticket
                j => j
                .HasOne(p => p.Ticket)
                .WithMany(p => p.TicketEmployees)
                .HasForeignKey(p => p.TicketId),
                //Clave primaria compuesta
                j => { 
                  j.HasKey(t => new { t.TicketId, t.EmploteeId });
                }
            );
    }
}
