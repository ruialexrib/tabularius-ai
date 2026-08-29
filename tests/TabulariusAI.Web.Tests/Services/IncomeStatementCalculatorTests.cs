using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class IncomeStatementCalculatorTests
{
    [Fact]
    public void Calculate_UsesClassSixAndSevenMovementAccounts()
    {
        var accounts = new[]
        {
            new SaftAccount { AccountId = "62", Description = "Fornecimentos" },
            new SaftAccount { AccountId = "6221", Description = "Eletricidade" },
            new SaftAccount { AccountId = "71", Description = "Vendas" },
            new SaftAccount { AccountId = "7111", Description = "Vendas nacionais" },
            new SaftAccount { AccountId = "12", Description = "Banco" }
        };
        var lines = new[]
        {
            new SaftTransactionLine { AccountId = "6221", Side = "D", Amount = 100m },
            new SaftTransactionLine { AccountId = "6221", Side = "C", Amount = 10m },
            new SaftTransactionLine { AccountId = "7111", Side = "C", Amount = 300m },
            new SaftTransactionLine { AccountId = "7111", Side = "D", Amount = 20m },
            new SaftTransactionLine { AccountId = "12", Side = "D", Amount = 280m }
        };
        var model = IncomeStatementCalculator.Calculate(accounts, lines);
        Assert.Single(model.Expenses); Assert.Single(model.Income);
        Assert.Equal(90m, model.TotalExpenses); Assert.Equal(280m, model.TotalIncome); Assert.Equal(190m, model.NetResult);
        Assert.Equal("6221", model.Expenses[0].AccountId); Assert.Equal("7111", model.Income[0].AccountId);
    }

    [Fact]
    public void Calculate_IgnoresAggregateAccountsEvenWhenTheyHaveValues()
    {
        var accounts = new[] { new SaftAccount { AccountId = "71", Description = "Vendas" }, new SaftAccount { AccountId = "711", Description = "Vendas mercadorias" } };
        var lines = new[] { new SaftTransactionLine { AccountId = "71", Side = "C", Amount = 1000m }, new SaftTransactionLine { AccountId = "711", Side = "C", Amount = 100m } };
        var model = IncomeStatementCalculator.Calculate(accounts, lines);
        var row = Assert.Single(model.Income);
        Assert.Equal("711", row.AccountId); Assert.Equal(100m, model.TotalIncome);
    }

    [Fact]
    public void Calculate_HandlesNegativeNetResult()
    {
        var accounts = new[] { new SaftAccount { AccountId = "621", Description = "Subcontratos" }, new SaftAccount { AccountId = "721", Description = "Serviços" } };
        var lines = new[] { new SaftTransactionLine { AccountId = "621", Side = "D", Amount = 250m }, new SaftTransactionLine { AccountId = "721", Side = "C", Amount = 100m } };
        var model = IncomeStatementCalculator.Calculate(accounts, lines);
        Assert.Equal(-150m, model.NetResult);
    }
}
