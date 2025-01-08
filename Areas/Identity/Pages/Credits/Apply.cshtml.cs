using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Models;
using System.Collections.Generic;
using System.Linq;
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Pages.Credits
{
    public class ApplyModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ApplyModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreditApplication CreditApplication { get; set; }

        public Credit SelectedCredit { get; set; }
        public List<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();

        public async Task OnGetAsync(int? creditId)
        {
            if (creditId.HasValue)
            {
                SelectedCredit = await _context.Credits
                    .FirstOrDefaultAsync(c => c.CreditId == creditId);

                if (SelectedCredit != null)
                {
                    CreditApplication = new CreditApplication
                    {
                        LoanAmount = SelectedCredit.MaxAmount,
                        DurationMonths = SelectedCredit.MaxDurationMonths,
                        Purpose = SelectedCredit.Name
                    };

                    PaymentSchedules = CalculatePaymentSchedule(SelectedCredit.MaxAmount, SelectedCredit.MaxDurationMonths, SelectedCredit.InterestRate);
                }
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("/Credits/Index");
        }

        private List<PaymentSchedule> CalculatePaymentSchedule(decimal loanAmount, int durationMonths, decimal annualInterestRate)
        {
            var paymentSchedules = new List<PaymentSchedule>();
            decimal monthlyInterestRate = annualInterestRate / 100 / 12;
            decimal monthlyPayment = CalculateMonthlyPayment(loanAmount, durationMonths, monthlyInterestRate);
            decimal remainingBalance = loanAmount;

            for (int i = 1; i <= durationMonths; i++)
            {
                decimal interestPayment = remainingBalance * monthlyInterestRate;
                decimal principalPayment = monthlyPayment - interestPayment;
                remainingBalance -= principalPayment;

                paymentSchedules.Add(new PaymentSchedule
                {
                    Month = i,
                    PaymentAmount = monthlyPayment,
                    Principal = principalPayment,
                    Interest = interestPayment,
                    RemainingBalance = remainingBalance
                });
            }

            return paymentSchedules;
        }

        private decimal CalculateMonthlyPayment(decimal loanAmount, int durationMonths, decimal monthlyInterestRate)
        {
            decimal factor = (decimal)Math.Pow((double)(1 + monthlyInterestRate), durationMonths);
            return loanAmount * (monthlyInterestRate * factor) / (factor - 1);
        }
    }

    public class CreditApplication
    {
        public decimal LoanAmount { get; set; }
        public int DurationMonths { get; set; }
        public string Purpose { get; set; }
    }

    public class PaymentSchedule
    {
        public int Month { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}