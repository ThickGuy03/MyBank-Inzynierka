using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inzynierka.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        //foreign key
        public int CategoryId {  get; set; }
        public Category Category { get; set; }  

        public int Amount { get; set; }

        [Column(TypeName = "navchar(100)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow; 
    }
}
