using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Inzynierka.Services;
using Microsoft.Extensions.Logging;

namespace Inzynierka.Areas.Identity.Pages.Credits
{
    public class ApplyModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApplyModel> _logger;


        public ApplyModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ApplyModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public CreditApplication CreditApplication { get; set; }

        public Credit SelectedCredit { get; set; }
        public List<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
        public string EligibilityMessage { get; set; }
        public bool IsEligible { get; set; }

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

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync method started.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User is null. Redirecting to login page.");
                return RedirectToPage("/Account/Login");
            }

            _logger.LogInformation("User: {UserId}", user.Id);

            // Debug: Check if SelectedCredit is null
            if (SelectedCredit == null)
            {
                _logger.LogWarning("SelectedCredit is null.");
                ModelState.AddModelError(string.Empty, "Selected credit is null. Please select a valid credit.");
                return Page();
            }

            _logger.LogInformation("SelectedCredit.CreditId: {CreditId}", SelectedCredit.CreditId);

            // Retrieve the SelectedCredit from the database using the CreditId from the form
            SelectedCredit = await _context.Credits
                .FirstOrDefaultAsync(c => c.CreditId == SelectedCredit.CreditId);

            if (SelectedCredit == null)
            {
                _logger.LogWarning("SelectedCredit not found in the database.");
                ModelState.AddModelError(string.Empty, "Selected credit not found in the database.");
                return Page();
            }

            _logger.LogInformation("SelectedCredit retrieved: {CreditName}", SelectedCredit.Name);

            // Calculate the payment schedule
            PaymentSchedules = CalculatePaymentSchedule(CreditApplication.LoanAmount, CreditApplication.DurationMonths, SelectedCredit.InterestRate);

            // Debug: Check if PaymentSchedules is null or empty
            if (PaymentSchedules == null || !PaymentSchedules.Any())
            {
                _logger.LogWarning("PaymentSchedules is null or empty.");
                ModelState.AddModelError(string.Empty, "Failed to calculate the payment schedule.");
                return Page();
            }

            _logger.LogInformation("Payment schedule calculated successfully.");

            // Get the first payment amount
            decimal firstPaymentAmount = PaymentSchedules.FirstOrDefault()?.PaymentAmount ?? 0;

            // Get the user's current balance
            decimal currentBalance = await GetUserCurrentBalanceAsync(user.Id);

            _logger.LogInformation("User's current balance: {CurrentBalance}", currentBalance);

            // Check if the user can afford the first payment
            if (currentBalance >= firstPaymentAmount)
            {
                IsEligible = true;
                EligibilityMessage = "You are eligible for this credit. You can afford the first payment.";
                _logger.LogInformation("User is eligible for the credit.");
            }
            else
            {
                IsEligible = false;
                EligibilityMessage = $"You are not eligible for this credit. You need an additional {firstPaymentAmount - currentBalance:C2} to afford the first payment.";
                _logger.LogInformation("User is not eligible for the credit.");
            }

            if (IsEligible)
            {
                // Debug: Check if the category for loan payments exists
                var loanPaymentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Title == "Loan Payment");

                if (loanPaymentCategory == null)
                {
                    _logger.LogWarning("Loan payment category not found.");
                    ModelState.AddModelError(string.Empty, "Loan payment category not found. Please add a category for loan payments.");
                    return Page();
                }

                _logger.LogInformation("Loan payment category found: {CategoryId}", loanPaymentCategory.CategoryId);

                // Add predicted payments as transactions
                foreach (var payment in PaymentSchedules)
                {
                    var transaction = new Transaction
                    {
                        UserId = user.Id,
                        CategoryId = loanPaymentCategory.CategoryId,
                        Amount = payment.PaymentAmount,
                        Note = $"Loan payment for {SelectedCredit?.Name} - Month {payment.Month}",
                        Date = DateTime.UtcNow.AddMonths(payment.Month),
                        IsRecurring = false
                    };
                    _context.Transactions.Add(transaction);
                    _logger.LogInformation("Added transaction for month {Month}: {Amount}", payment.Month, payment.PaymentAmount);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Transactions saved to the database.");
            }

            return Page();
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

        private async Task<decimal> GetUserCurrentBalanceAsync(string userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .SumAsync(t => t.Amount);
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