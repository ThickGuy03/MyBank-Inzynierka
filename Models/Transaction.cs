using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inzynierka.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

       
        [Required] 
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))] 
        public Category Category { get; set; }

        [Required] 
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(100)")] 
        [MaxLength(100, ErrorMessage = "Note cannot exceed 100 characters.")]
        public string? Note { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow; 

        [Required]
        public string UserId { get; set; }
        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category==null? "": Category.Icon+" "+Category.Title;
            }
        }
        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category==null || Category.Type=="Expense")? "-":"+") + Amount.ToString("C2");
            }
        }
    }
}
