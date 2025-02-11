using System;
using System.Linq;
using System.Threading.Tasks;
using Inzynierka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inzynierka.Services
{
    public class RecurringTransactionJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecurringTransactionJob> _logger;

        public RecurringTransactionJob(ApplicationDbContext context, ILogger<RecurringTransactionJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessRecurringTransactions()
        {
            _logger.LogInformation("Starting recurring transaction job.");

            var today = DateTime.UtcNow.Date;
            var recurringTransactions = await _context.Transactions
                .Where(t => t.IsRecurring && t.RecurrenceEndDate >= today)
                .ToListAsync();

            _logger.LogInformation($"Found {recurringTransactions.Count} recurring transactions to process.");

            foreach (var transaction in recurringTransactions)
            {
                _logger.LogInformation($"Processing transaction {transaction.TransactionId} with start date {transaction.Date}.");

                var nextDate = transaction.Date;

                if (nextDate < today)
                {
                    _logger.LogInformation($"Transaction {transaction.TransactionId} has a start date in the past. Backfilling missed transactions.");

                    while (nextDate < today && nextDate <= transaction.RecurrenceEndDate)
                    {
                        _logger.LogInformation($"Checking for transaction on {nextDate}.");

                        var existingTransaction = await _context.Transactions
                            .FirstOrDefaultAsync(t => t.UserId == transaction.UserId &&
                                                      t.CategoryId == transaction.CategoryId &&
                                                      t.Amount == transaction.Amount &&
                                                      t.Date == nextDate);

                        if (existingTransaction == null)
                        {
                            _logger.LogInformation($"Creating new transaction for {nextDate}.");

                            var newTransaction = new Transaction
                            {
                                CategoryId = transaction.CategoryId,
                                Amount = transaction.Amount,
                                Note = transaction.Note,
                                Date = nextDate,
                                UserId = transaction.UserId,
                                IsRecurring = transaction.IsRecurring,
                                RecurrenceFrequency = transaction.RecurrenceFrequency,
                                RecurrenceEndDate = transaction.RecurrenceEndDate
                            };

                            _context.Transactions.Add(newTransaction);
                        }
                        else
                        {
                            _logger.LogInformation($"Transaction already exists for {nextDate}. Skipping.");
                        }

                        nextDate = GetNextRecurrenceDate(nextDate, transaction.RecurrenceFrequency);
                    }

                    transaction.Date = nextDate;
                    _logger.LogInformation($"Updated transaction {transaction.TransactionId} to next date {nextDate}.");
                }
                else
                {
                    if (nextDate <= today && nextDate <= transaction.RecurrenceEndDate)
                    {
                        _logger.LogInformation($"Creating new transaction for {nextDate}.");

                        var newTransaction = new Transaction
                        {
                            CategoryId = transaction.CategoryId,
                            Amount = transaction.Amount,
                            Note = transaction.Note,
                            Date = nextDate,
                            UserId = transaction.UserId,
                            IsRecurring = transaction.IsRecurring,
                            RecurrenceFrequency = transaction.RecurrenceFrequency,
                            RecurrenceEndDate = transaction.RecurrenceEndDate
                        };

                        _context.Transactions.Add(newTransaction);
                        transaction.Date = GetNextRecurrenceDate(nextDate, transaction.RecurrenceFrequency);
                        _logger.LogInformation($"Updated transaction {transaction.TransactionId} to next date {transaction.Date}.");
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Recurring transaction job completed successfully.");
        }

        private DateTime GetNextRecurrenceDate(DateTime lastDate, string frequency)
        {
            return frequency switch
            {
                "Weekly" => lastDate.AddDays(7),
                "Monthly" => lastDate.AddMonths(1),
                "Yearly" => lastDate.AddYears(1),
                _ => lastDate
            };
        }
    }
}