using Inzynierka.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inzynierka.Areas.Identity.Pages.Insurance
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InsurancePlan> InsurancePlans { get; set; }
        public List<string> InsuranceTypes { get; set; } // Strongly-typed list of insurance types

        public async Task OnGetAsync()
        {
            // Fetch insurance plans from the database
            InsurancePlans = await _context.InsuranceProviders
                .Select(p => new InsurancePlan
                {
                    Provider = p.ProviderName,
                    InsuranceType = p.InsuranceType,
                    Coverage = p.CoverageType,
                    Premium = p.Premium,
                    Deductible = p.Deductible,
                    Rating = p.Rating
                })
                .ToListAsync();

            // Populate InsuranceTypes with unique insurance types
            InsuranceTypes = InsurancePlans
                .Select(p => p.InsuranceType)
                .Distinct()
                .ToList();
        }
        public class InsurancePlan
        {
            public string Provider { get; set; }
            public string InsuranceType { get; set; }
            public string Coverage { get; set; }
            public decimal Premium { get; set; }
            public decimal Deductible { get; set; }
            public double Rating { get; set; }
        }
    }
}