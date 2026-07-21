using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketing.Query.Domain.Adresses;

//ComplexType: Indica que la clase es un tipo complejo y
//no se mapeará a una tabla separada en la base de datos.
//En su lugar, sus propiedades se mapearán como columnas
//en la tabla de la entidad que lo contiene.
[ComplexType]
public class Address
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }    
}
