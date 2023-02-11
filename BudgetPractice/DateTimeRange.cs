using System;

namespace BudgetPractice;

public class DateTimeRange
{
    public DateTime EndTime { get; }
    public DateTime StartTime { get; }

    public DateTimeRange(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }


    public bool IsSameMonth()
    {
        return StartTime.ToString("yyyyMM") == EndTime.ToString("yyyyMM");
    }

    public bool IsFullMonth()
    {
        return StartTime == new DateTime(StartTime.Year, StartTime.Month, 1)
               && EndTime == new DateTime(EndTime.Year, EndTime.Month, 1).AddMonths(1).AddDays(-1);
    }

    public int GetTotalDays()
    {
        var days = (EndTime - StartTime).Days;
        days++;
        return days;
    }
}