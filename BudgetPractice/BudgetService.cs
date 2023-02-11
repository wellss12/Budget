using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetPractice;

public class DateTimeRange
{
    private readonly DateTime _endTime;
    private readonly DateTime _startTime;

    public DateTimeRange(DateTime startTime, DateTime endTime)
    {
        _startTime = startTime;
        _endTime = endTime;
    }

    public bool IsSameMonth()
    {
        return _startTime.ToString("yyyyMM") == _endTime.ToString("yyyyMM");
    }

    public bool IsFullMonth()
    {
        return _startTime == new DateTime(_startTime.Year, _startTime.Month, 1)
               && _endTime == new DateTime(_endTime.Year, _endTime.Month, 1).AddMonths(1).AddDays(-1);
    }

    public int GetTotalDays()
    {
        var days = (_endTime - _startTime).Days;
        days++;
        return days;
    }
}

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

        var startBudget = GetBudget(startTime, budgets);
        _dateTimeRange = new DateTimeRange(startTime, endTime);
        if (_dateTimeRange.IsSameMonth())
        {
            if (_dateTimeRange.IsFullMonth())
            {
                return startBudget.Amount;
            }

            return GetAmountForSameMonth(startBudget);
        }

        return GetAmountForDifferentMonth(startTime, endTime, budgets);
    }

    private static decimal GetAmountForDifferentMonth(DateTime startTime, DateTime endTime, List<Budget> budgets)
    {
        var firstMonthTotalAmount = GetBudget(startTime, budgets)?.Amount ?? 0;
        var secondMonthTotalAmount = GetBudget(endTime, budgets)?.Amount ?? 0;

        var firstMonthAmount = GetFirstMonthAmount(startTime, firstMonthTotalAmount);
        var lastMonthAmount = GetLastMonthAmount(endTime, secondMonthTotalAmount);
        var middleMonthsAmount = GetMiddleMonthsAmount(startTime, endTime, budgets);

        return firstMonthAmount + middleMonthsAmount + lastMonthAmount;
    }

    private decimal GetAmountForSameMonth(Budget? startBudget)
    {
        var days = _dateTimeRange.GetTotalDays();
        var _ = DateOnly.TryParseExact(startBudget.YearMonth, "yyyyMM", out var dateTime);
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

    private static Budget? GetBudget(DateTime time, IEnumerable<Budget> budgets)
    {
        return budgets.FirstOrDefault(t => t.YearMonth == time.ToString("yyyyMM"));
    }
}