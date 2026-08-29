using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class TrialBalanceCalculatorTests
{
    [Fact]
    public void Calculate_WithMovements_CalculatesClosingBalance()
    {
        var accounts = new[] { new SaftAccount { AccountId = "12", Description = "Depósitos à ordem", OpeningDebitBalance = 100m, ClosingDebitBalance = 130m } };
        var lines = new[] { new SaftTransactionLine { AccountId = "12", Side = "D", Amount = 50m }, new SaftTransactionLine { AccountId = "12", Side = "C", Amount = 20m } };
        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, lines));
        Assert.Equal(50m, row.DebitMovements); Assert.Equal(20m, row.CreditMovements); Assert.Equal(130m, row.ClosingDebit); Assert.False(row.HasClosingDifference); Assert.False(row.IsAggregateAccount);
    }

    [Fact]
    public void Calculate_WithNetCredit_UsesClosingCredit()
    {
        var accounts = new[] { new SaftAccount { AccountId = "22", Description = "Fornecedores", OpeningCreditBalance = 40m, ClosingCreditBalance = 65m } };
        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, new[] { new SaftTransactionLine { AccountId = "22", Side = "C", Amount = 25m } }));
        Assert.Equal(0m, row.ClosingDebit); Assert.Equal(65m, row.ClosingCredit); Assert.False(row.HasClosingDifference);
    }

    [Fact]
    public void Calculate_WithReportedMismatch_FlagsDifferenceForMovementAccount()
    {
        var accounts = new[] { new SaftAccount { AccountId = "1111", Description = "Caixa A", OpeningDebitBalance = 10m, ClosingDebitBalance = 99m } };
        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, Array.Empty<SaftTransactionLine>()));
        Assert.True(row.HasClosingDifference); Assert.False(row.IsAggregateAccount);
    }

    [Fact]
    public void Calculate_WithChildAccount_MarksParentAsAggregateAndSuppressesDifference()
    {
        var accounts = new[]
        {
            new SaftAccount { AccountId = "11", Description = "Caixa", OpeningDebitBalance = 100m, ClosingDebitBalance = 150m },
            new SaftAccount { AccountId = "1111", Description = "Caixa A", OpeningDebitBalance = 100m, ClosingDebitBalance = 150m }
        };
        var lines = new[] { new SaftTransactionLine { AccountId = "1111", Side = "D", Amount = 50m } };
        var rows = TrialBalanceCalculator.Calculate(accounts, lines);
        var parent = Assert.Single(rows, item => item.AccountId == "11");
        var child = Assert.Single(rows, item => item.AccountId == "1111");
        Assert.True(parent.IsAggregateAccount); Assert.False(parent.HasClosingDifference);
        Assert.False(child.IsAggregateAccount); Assert.False(child.HasClosingDifference);
    }

    [Fact]
    public void Calculate_WithLowercaseSides_AggregatesMovements()
    {
        var accounts = new[] { new SaftAccount { AccountId = "21", Description = "Clientes" } };
        var lines = new[] { new SaftTransactionLine { AccountId = "21", Side = "d", Amount = 7m }, new SaftTransactionLine { AccountId = "21", Side = "c", Amount = 2m } };
        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, lines));
        Assert.Equal(7m, row.DebitMovements); Assert.Equal(2m, row.CreditMovements); Assert.Equal(5m, row.ClosingDebit);
    }
}
