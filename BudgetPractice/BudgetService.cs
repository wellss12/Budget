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

        if (IsSameMonth(startTime, endTime))
        {
            if (IsFullMonth(startTime, endTime))
            {
                return startBudget.Amount;
            }
            else
            {
                return GetAmountForSameMonthRange(startTime, endTime, startBudget);
            }
        }
        else
        {
            return GetAmountForDifferentMonthRange(startTime, endTime, budgets);
        }
    }

    private static decimal GetAmountForDifferentMonthRange(DateTime startTime, DateTime endTime, List<Budget> budgets)
    {
        var firstMonthTotalAmount = GetBudget(startTime, budgets)?.Amount ?? 0;
        var secondMonthTotalAmount = GetBudget(endTime, budgets)?.Amount ?? 0;
        
        var firstMonthAmount = GetFirstMonthAmount(startTime, firstMonthTotalAmount);
        var lastMonthAmount = GetLastMonthAmount(endTime, secondMonthTotalAmount);
        var middleMonthsAmount = GetMiddleMonthsAmount(startTime, endTime, budgets);

        return firstMonthAmount + middleMonthsAmount + lastMonthAmount;
    }

    private static decimal GetAmountForSameMonthRange(DateTime startTime, DateTime endTime, Budget? startBudget)
    {
        var days = (endTime - startTime).Days;
        days++;

        var _ = DateTime.TryParse(startBudget.YearMonth, out var dateTime);
        var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

        return startBudget.Amount / daysInMonth * days;
    }

    private static int GetMiddleMonthsAmount(DateTime startTime, DateTime endTime, List<Budget> budgets)
    {
        var nextMonth = new DateTime(startTime.Year, startTime.AddMonths(1).Month, 1);
        var middleAmount = 0;
        while (nextMonth < new DateTime(endTime.Year, endTime.Month, 1))
        {
            var currentBudget = budgets.First(budget => budget.YearMonth == nextMonth.ToString("yyyyMM"));
            middleAmount += currentBudget.Amount;
            nextMonth = nextMonth.AddMonths(1);
        }

        return middleAmount;
    }

    private static int GetLastMonthAmount(DateTime endTime, int totalAmount)
    {
        var endTimeDaysInMonth = GetDaysInMonth(endTime);
        var amountForOneDayInLastMonth = GetAmountForOneDay(totalAmount, endTimeDaysInMonth);
        return amountForOneDayInLastMonth * endTime.Day;
    }

    private static int GetFirstMonthAmount(DateTime startTime, int totalAmount)
    {
        var startTimeDaysInMonth = GetDaysInMonth(startTime);
        var amountForOneDayInFirstMonth = GetAmountForOneDay(totalAmount, startTimeDaysInMonth);
        return amountForOneDayInFirstMonth * (startTimeDaysInMonth - startTime.Day + 1);
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