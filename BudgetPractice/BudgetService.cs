using System;
using System.Linq;

namespace BudgetPractice;

public class BudgetService
{
    private readonly IBudgetRepo _budgetRepo;

    public BudgetService(IBudgetRepo budgetRepo)
    {
        _budgetRepo = budgetRepo;
    }

    public decimal Query(DateTime startTime, DateTime endTime)
    {
        var budgets = _budgetRepo.GetAll();
        var startBudget = budgets.First(t => t.YearMonth == startTime.ToString("yyyyMM"));
        var endBudget = budgets.First(t => t.YearMonth == endTime.ToString("yyyyMM"));

        if (startTime.ToString("yyyyMM") == endTime.ToString("yyyyMM"))
        {
            return GetBudgetInMonth(startTime, endTime, startBudget);
        }

        return startBudget.Amount + endBudget.Amount;

        decimal GetBudgetInMonth(DateTime startTime1, DateTime endTime1, Budget budget)
        {
            if (IsFullMonth(startTime1, endTime1))
            {
                return budget.Amount;
            }

            var days = (endTime1 - startTime1).Days;
            days++;

            var _ = DateTime.TryParse(budget.YearMonth, out var dateTime);
            var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

            return budget.Amount / daysInMonth * days;
        }
    }

    private static bool IsFullMonth(DateTime startTime, DateTime endTime)
    {
        return startTime == new DateTime(startTime.Year, startTime.Month, 1)
               && endTime == new DateTime(endTime.Year, endTime.Month, 1).AddMonths(1).AddDays(-1);
    }
}