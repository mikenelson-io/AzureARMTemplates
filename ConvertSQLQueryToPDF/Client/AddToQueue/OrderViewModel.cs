using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace AddToQueue.Model
{
    [Table(Name = "orders")]
    public class OrderViewModel
    {
        [Column(IsPrimaryKey = true)] public int OrderId { get; set; }
        [Column] public System.DateTime OrderDate 	{ get; set; }
        [Column] public string  		FirstName  	{ get; set; }
        [Column] public string  		LastName   	{ get; set; }
        [Column] public string  		Address    	{ get; set; }
        [Column] public string  		City       	{ get; set; }
        [Column] public string  		State      	{ get; set; }
        [Column] public string  		PostalCode 	{ get; set; }
        [Column] public string  		Country    	{ get; set; }
        [Column] public string  		Phone      	{ get; set; }
        [Column] public string  		Email      	{ get; set; }
        [Column] public decimal 		Total      	{ get; set; }
    }
}