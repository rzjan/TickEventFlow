using Common.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Query.Domain.TicketTypes;

namespace Ticketing.Query.Infrastructure.Configuration;

public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketTypes");
        builder.HasKey(t => t.Id);
        
        IEnumerable<TicketType> ticketTypes = Enum
                                                .GetValues<TicketTypeEnum>()
                                                //.Cast<TicketTypeEnum>()
                                                .Select(e => TicketType.Create((int)e));

        builder.HasData(ticketTypes);
    }
}
