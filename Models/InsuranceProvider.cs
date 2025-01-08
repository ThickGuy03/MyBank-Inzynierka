using System.ComponentModel.DataAnnotations;

namespace Inzynierka.Models
{
    public class InsuranceProvider
    {
        [Key]
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string InsuranceType { get; set; }
        public string CoverageType { get; set; }
        public decimal Premium { get; set; }
        public decimal Deductible { get; set; }
        public double Rating { get; set; }
    }
}
