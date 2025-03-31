using Inzynierka.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Inzynierka.Models;

namespace Inzynierka.Areas.Identity.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public decimal TotalIncome { get; set; }
        public decimal PeriodIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal PeriodExpense { get; set; }
        public decimal Balance { get; set; }
        public List<object> DoughnutChartData { get; set; }
        public List<object> splineChartData { get; set; }
        public List<object> LastTransactions { get; set; }

        public async Task OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TotalIncome = 0;
                PeriodIncome = 0;
                TotalExpense = 0;
                PeriodExpense = 0;
                Balance = 0;
                DoughnutChartData = new List<object>();
                splineChartData = new List<object>();
                LastTransactions = new List<object>();
                return;
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            List<Transaction> AllTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            TotalIncome = AllTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            TotalExpense = AllTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);

            Balance = TotalIncome - TotalExpense;

            DateTime StartDate = DateTime.Today.AddDays(-29);
            DateTime EndDate = DateTime.Today;
            List<Transaction> SelectedTransactions = AllTransactions
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToList();
            PeriodIncome = SelectedTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            PeriodExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);

            DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.TitleWithIcon,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US")),
                }).OrderByDescending(l => l.amount).ToList<object>();

            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd.MM"),
                    Income = k.Sum(l => l.Amount)
                }).ToList();

            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd.MM"),
                    Expense = k.Sum(l => l.Amount)
                }).ToList();

            string[] last30Days = Enumerable.Range(0, 30)
                .Select(i => StartDate.AddDays(i).ToString("dd.MM"))
                .ToArray();

            splineChartData = last30Days.Select(day => new
            {
                day = day,
                income = IncomeSummary.FirstOrDefault(x => x.Day == day)?.Income ?? 0,
                expense = ExpenseSummary.FirstOrDefault(x => x.Day == day)?.Expense ?? 0
            }).ToList<object>();

            LastTransactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .Select(t => new
                {
                    Date = t.Date.ToString("yyyy-MM-dd"),
                    Category = t.Category.TitleWithIcon,
                    Amount = t.Amount,
                    Type = t.Category.Type
                }).ToListAsync<object>();

            LastTransactions ??= new List<object>();
        }

        public class SplineChartData
        {
            public string Day { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
        }

        public class LastTransaction
        {
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public decimal Amount { get; set; }
            public string Type { get; set; }
        }
    }
}