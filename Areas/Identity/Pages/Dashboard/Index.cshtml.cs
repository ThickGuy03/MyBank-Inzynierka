using Inzynierka.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inzynierka.Areas.Identity.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal TotalIncome { get; set; }
        public decimal PeriodIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal PeriodExpense { get; set; }
        public decimal Balance { get; set; }
        public List<object> DoughnutChartData { get; set; }
        public List<object> splineChartData { get; set; }

        public async Task OnGetAsync()
        {
            List<Inzynierka.Models.Transaction> AllTransactions = await _context.Transactions.Include(x => x.Category).ToListAsync();

            TotalIncome = AllTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            TotalExpense = AllTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);

            Balance = TotalIncome - TotalExpense;

            DateTime StartDate = DateTime.Today.AddDays(-29);
            DateTime EndDate = DateTime.Today;
            List<Inzynierka.Models.Transaction> SelectedTransactions = AllTransactions
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToList();
            PeriodIncome=SelectedTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            PeriodExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);

            DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.TitleWithIcon,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C2"),
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

        }
        public class SplineChartData
        {
            public string Day { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
        }
    }
}
