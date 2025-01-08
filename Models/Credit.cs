using System.ComponentModel.DataAnnotations;

namespace Inzynierka.Models
{
    public class Credit
    {
        [Key]
        public int CreditId { get; set; }
        public string Name { get; set; } 
        public decimal InterestRate { get; set; } 
        public int MaxDurationMonths { get; set; } 
        public decimal MaxAmount { get; set; } 
        public string Description { get; set; }
    }
}