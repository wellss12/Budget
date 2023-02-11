using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BudgetPractice;

public class BudgetTests
{
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

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 1));

        BudgetShouldBe(budget, 1000);
    }

    [Test]
    public void query_two_day_in_single_month_with_budget_in_month_start()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_day_in_single_month_in_month_end()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 30), new DateTime(2023, 3, 31));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_day_in_single_month_with_zero_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 0)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 0);
    }

    [Test]
    public void query_with_not_exist_budget()
    {
        _budgetRepo.GetAll().Returns(new List<Budget>());

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 0);
    }


    [Test]
    public void query_full_month()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 31));

        BudgetShouldBe(budget, 31000);
    }

    [Test]
    public void query_two_full_month()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 3000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 4, 30));

        BudgetShouldBe(budget, 34000);
    }


    // [Test]
    // public void query_two_day_in_cross_month_with_budget()
    // {
    //     GivenGetAllReturn(new List<Budget>
    //     {
    //         CreateBudget("202303", 31000),
    //         CreateBudget("202304", 15000)
    //     });
    //
    //     var budget = QueryBudget(new DateTime(2023, 3, 31), new DateTime(2023, 4, 1));
    //
    //     BudgetShouldBe(budget, 1500);
    // }

    private static void BudgetShouldBe(decimal budget, int expected)
    {
        budget.Should().Be(expected);
    }

    private decimal QueryBudget(DateTime startTime, DateTime endTime)
    {
        return _budgetService.Query(startTime, endTime);
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