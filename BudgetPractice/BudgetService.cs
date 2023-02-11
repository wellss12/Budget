using System;
using System.Collections.Generic;
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
        if (budgets is null || !budgets.Any())
        {
            return 0;
        }

        var startBudget = GetBudget(startTime, budgets);
        var endBudget = GetBudget(endTime, budgets);

        if (IsSameMonth(startTime, endTime))
        {
            if (IsFullMonth(startTime, endTime))
            {
                return startBudget.Amount;
            }
            else
            {
                var days = (endTime - startTime).Days;
                days++;

                var _ = DateTime.TryParse(startBudget.YearMonth, out var dateTime);
                var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

                return startBudget.Amount / daysInMonth * days;
            }
        }
        else
        {
            var startTimeDaysInMonth = GetDaysInMonth(startTime);
            var endTimeDaysInMonth = GetDaysInMonth(endTime);
            
            var amountForOneDayInFirstMonth = GetAmountForOneDay(startBudget.Amount, startTimeDaysInMonth);
            var amountForOneDayInLastMonth = GetAmountForOneDay(endBudget.Amount, endTimeDaysInMonth);

            var firstMonthTotalDays = startTimeDaysInMonth - startTime.Day + 1;
            var firstMonthAmount = amountForOneDayInFirstMonth * firstMonthTotalDays;
            var lastMonthAmount = amountForOneDayInLastMonth * endTime.Day;
            
            return firstMonthAmount + lastMonthAmount;
        }
    }

    private static int GetAmountForOneDay(int amount, int daysInMonth)
    {
        return amount / daysInMonth;
    }

    private static int GetDaysInMonth(DateTime time)
    {
        return DateTime.DaysInMonth(time.Year, time.Month);
    }

    private static bool IsSameMonth(DateTime startTime, DateTime endTime)
    {
        return startTime.ToString("yyyyMM") == endTime.ToString("yyyyMM");
    }

    private static Budget? GetBudget(DateTime time, IEnumerable<Budget> budgets)
    {
        return budgets.FirstOrDefault(t => t.YearMonth == time.ToString("yyyyMM"));
    }

    private decimal GetBudgetInMonth(DateTime startTime1, DateTime endTime1, Budget budget)
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

    private static bool IsFullMonth(DateTime startTime, DateTime endTime)
    {
        return startTime == new DateTime(startTime.Year, startTime.Month, 1)
               && endTime == new DateTime(endTime.Year, endTime.Month, 1).AddMonths(1).AddDays(-1);
    }
}