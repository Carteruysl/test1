using NSubstitute;

namespace BudgetTDD;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Invalid_date_range()
    {
        var budgetRepo = Substitute.For<IBudgetRepo>();

        budgetRepo.GetAll().Returns(new List<Budget>
        {
            new Budget
            {
                YearMonth = "202501",
                Amount = 310
            },
        });

        var budgetService = new BudgetService(budgetRepo);
        var start = new DateTime(2025, 1, 3);
        var end = new DateTime(2025, 1, 1);
        var budget = budgetService.Query(start, end);

        Assert.AreEqual(budget, 0);
    }

    [Test]
    public void Get_one_day()
    {
        var budgetRepo = Substitute.For<IBudgetRepo>();

        budgetRepo.GetAll().Returns(new List<Budget>
        {
            new Budget
            {
                YearMonth = "202501",
                Amount = 310
            },
        });

        var budgetService = new BudgetService(budgetRepo);
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 1, 1);
        var budget = budgetService.Query(start, end);

        Assert.AreEqual(budget, 10);
    }

    [Test]
    public void Get_Budget_0()
    {
        var budgetRepo = Substitute.For<IBudgetRepo>();

        budgetRepo.GetAll().Returns(new List<Budget>
        {
            new Budget
            {
                YearMonth = "202503",
                Amount = 0
            },
            new Budget
            {
                YearMonth = "202504",
                Amount = 3000
            }
        });

        var budgetService = new BudgetService(budgetRepo);
        var start = new DateTime(2025, 3, 29);
        var end = new DateTime(2025, 4, 2);
        var budget = budgetService.Query(start, end);

        Assert.AreEqual(budget, 200);
    }

    [Test]
    public void Get_cross_month()
    {
        var budgetRepo = Substitute.For<IBudgetRepo>();

        budgetRepo.GetAll().Returns(new List<Budget>
        {
            new Budget
            {
                YearMonth = "202503",
                Amount = 310
            },
            new Budget
            {
                YearMonth = "202504",
                Amount = 3000
            },
            new Budget
            {
                YearMonth = "202505",
                Amount = 620
            }
        });

        var budgetService = new BudgetService(budgetRepo);
        var start = new DateTime(2025, 3, 29);
        var end = new DateTime(2025, 5, 2);
        var budget = budgetService.Query(start, end);

        Assert.AreEqual(budget, 3070);
    }
    
    [Test]
    public void Get_leap_month_Budget()
    {
        var budgetRepo = Substitute.For<IBudgetRepo>();

        budgetRepo.GetAll().Returns(new List<Budget>
        {
            new Budget
            {
                YearMonth = "202402",
                Amount = 290
            },
            new Budget
            {
                YearMonth = "202403",
                Amount = 3100
            }
        });

        var budgetService = new BudgetService(budgetRepo);
        var start = new DateTime(2024, 2, 27);
        var end = new DateTime(2024, 3, 02);
        var budget = budgetService.Query(start, end);

        Assert.AreEqual(budget, 230);
    }
}
public interface IBudgetRepo
{
    List<Budget> GetAll();
}
public class Budget
{
    public string YearMonth { get; set; }

    public decimal Amount { get; set; }
}
public class BudgetService(IBudgetRepo budgetRepo)
{
    private readonly IBudgetRepo _budgetRepo = budgetRepo;

    public decimal Query(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return 0;
        }

        var budgets = _budgetRepo.GetAll();
        decimal totalBudget = 0;

        var currentDate = new DateTime(start.Year, start.Month, 1);
        var endDate = new DateTime(end.Year, end.Month, 1);

        while (currentDate <= endDate)
        {
            var budget = budgets.FirstOrDefault(b => b.YearMonth == currentDate.ToString("yyyyMM"));

            if (budget != null)
            {
                var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                var periodStart = currentDate < start ? start : currentDate;
                var periodEnd = currentDate.AddMonths(1).AddDays(-1) > end ? end : currentDate.AddMonths(1).AddDays(-1);
                var daysInPeriod = (periodEnd - periodStart).Days + 1;

                totalBudget += (budget.Amount / daysInMonth) * daysInPeriod;
            }

            currentDate = currentDate.AddMonths(1);
        }

        return totalBudget;
    }
}