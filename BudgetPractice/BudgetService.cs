using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetPractice;

public class BudgetService
{
    private readonly IBudgetRepo _budgetRepo;
    private DateTimeRange _dateTimeRange;

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

        _dateTimeRange = new DateTimeRange(startTime, endTime);
        var startBudget = GetBudget(_dateTimeRange.StartTime, budgets);
        if (_dateTimeRange.IsSameMonth())
        {
            if (_dateTimeRange.IsFullMonth())
            {
                return startBudget.Amount;
            }

            return GetAmountForSingleMonth(startBudget);
        }

        return GetAmountForMultiMonth(budgets);
    }

    private decimal GetAmountForMultiMonth(List<Budget> budgets)
    {
        var firstMonthTotalAmount = GetBudget(_dateTimeRange.StartTime, budgets)?.Amount ?? 0;
        var secondMonthTotalAmount = GetBudget(_dateTimeRange.EndTime, budgets)?.Amount ?? 0;

        var firstMonthAmount = GetFirstMonthAmount(firstMonthTotalAmount);
        var lastMonthAmount = GetLastMonthAmount(secondMonthTotalAmount);
        var middleMonthsAmount = GetMiddleMonthsAmount(budgets);

        return firstMonthAmount + middleMonthsAmount + lastMonthAmount;
    }

    private decimal GetAmountForSingleMonth(Budget? budget)
    {
        var days = _dateTimeRange.GetTotalDays();
        var _ = DateOnly.TryParseExact(budget.YearMonth, "yyyyMM", out var dateTime);
        var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

        return budget.Amount / daysInMonth * days;
    }

    private int GetMiddleMonthsAmount(List<Budget> budgets)
    {
        var nextMonth = new DateTime(_dateTimeRange.StartTime.Year, _dateTimeRange.StartTime.AddMonths(1).Month, 1);
        var middleAmount = 0;
        while (nextMonth < new DateTime(_dateTimeRange.EndTime.Year, _dateTimeRange.EndTime.Month, 1))
        {
            var currentBudget = budgets.First(budget => budget.YearMonth == nextMonth.ToString("yyyyMM"));
            middleAmount += currentBudget.Amount;
            nextMonth = nextMonth.AddMonths(1);
        }

        return middleAmount;
    }

    private int GetLastMonthAmount(int totalAmount)
    {
        var endTimeDaysInMonth = GetDaysInMonth(_dateTimeRange.EndTime);
        var amountForOneDayInLastMonth = GetAmountForOneDay(totalAmount, endTimeDaysInMonth);
        return amountForOneDayInLastMonth * _dateTimeRange.EndTime.Day;
    }

    private int GetFirstMonthAmount(int totalAmount)
    {
        var startTimeDaysInMonth = GetDaysInMonth(_dateTimeRange.StartTime);
        var amountForOneDayInFirstMonth = GetAmountForOneDay(totalAmount, startTimeDaysInMonth);
        return amountForOneDayInFirstMonth * (startTimeDaysInMonth - _dateTimeRange.StartTime.Day + 1);
    }

    private static int GetAmountForOneDay(int amount, int daysInMonth)
    {
        return amount / daysInMonth;
    }

    private static int GetDaysInMonth(DateTime time)
    {
        return DateTime.DaysInMonth(time.Year, time.Month);
    }

    private static Budget? GetBudget(DateTime time, IEnumerable<Budget> budgets)
    {
        return budgets.FirstOrDefault(t => t.YearMonth == time.ToString("yyyyMM"));
    }
}