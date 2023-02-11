using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace BudgetPractice;

public class BudgetTests
{
    private decimal _budgetAmount;
    private IBudgetRepo _budgetRepo;
    private BudgetService _budgetService;

    [SetUp]
    public void SetUp()
    {
        _budgetRepo = Substitute.For<IBudgetRepo>();
        _budgetService = new BudgetService(_budgetRepo);
    }


    [Test]
    public void query_one_day_in_single_month_with_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 1));

        ThenBudgetAmountShouldBe(1000);
    }

    [Test]
    public void query_multi_day_in_single_month_with_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 5));

        ThenBudgetAmountShouldBe(5000);
    }

    [Test]
    public void date_time_try_parse_has_bug()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202304", 30000)
        });

        WhenQueryBudget(new DateTime(2023, 4, 1), new DateTime(2023, 4, 5));

        ThenBudgetAmountShouldBe(5000);
    }

    [Test]
    public void query_multi_day_in_single_month_with_zero_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 0)
        });

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 5));

        ThenBudgetAmountShouldBe(0);
    }

    [Test]
    public void query_with_empty_budget()
    {
        GivenGetAllReturnEmpty();

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        ThenBudgetAmountShouldBe(0);
    }

    [Test]
    public void query_with_null_budget()
    {
        GivenGetAllReturnNull();

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        ThenBudgetAmountShouldBe(0);
    }

    [Test]
    public void query_full_month()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 31));

        ThenBudgetAmountShouldBe(31000);
    }

    [Test]
    public void query_two_full_month()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 3000)
        });

        WhenQueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 4, 30));

        ThenBudgetAmountShouldBe(34000);
    }


    [Test]
    public void query_cross_two_month_with_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 15000)
        });

        WhenQueryBudget(new DateTime(2023, 3, 31), new DateTime(2023, 4, 2));

        ThenBudgetAmountShouldBe(2000);
    }

    [Test]
    public void query_cross_three_month_with_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 30000),
            CreateBudget("202305", 31000)
        });

        WhenQueryBudget(new DateTime(2023, 3, 31), new DateTime(2023, 5, 5));

        ThenBudgetAmountShouldBe(1000 + 30000 + 5000);
    }

    private void GivenGetAllReturnNull()
    {
        _budgetRepo.GetAll().ReturnsNull();
    }

    private void GivenGetAllReturnEmpty()
    {
        _budgetRepo.GetAll().Returns(new List<Budget>());
    }

    private void ThenBudgetAmountShouldBe(int expected)
    {
        _budgetAmount.Should().Be(expected);
    }

    private void WhenQueryBudget(DateTime startTime, DateTime endTime)
    {
        _budgetAmount = _budgetService.Query(startTime, endTime);
    }


    private static Budget CreateBudget(string yearMonth, int amount)
    {
        return new Budget
        {
            YearMonth = yearMonth,
            Amount = amount
        };
    }

    private void GivenGetAllReturn(List<Budget> budgets)
    {
        _budgetRepo.GetAll().Returns(t => budgets);
    }
}