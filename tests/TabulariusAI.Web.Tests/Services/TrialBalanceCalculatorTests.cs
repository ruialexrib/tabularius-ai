using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

/// <summary>Verifies deterministic trial-balance calculations.</summary>
public sealed class TrialBalanceCalculatorTests
{
    /// <summary>Verifies that opening balances and debit and credit movements produce the expected closing balance.</summary>
    [Fact]
    public void Calculate_WithMovements_CalculatesClosingBalance()
    {
        var accounts = new[] { new SaftAccount { AccountId = "12", Description = "Depósitos à ordem", OpeningDebitBalance = 100m, ClosingDebitBalance = 130m } };
        var lines = new[]
        {
            new SaftTransactionLine { AccountId = "12", Side = "D", Amount = 50m },
            new SaftTransactionLine { AccountId = "12", Side = "C", Amount = 20m }
        };

        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, lines));

        Assert.Equal(50m, row.DebitMovements);
        Assert.Equal(20m, row.CreditMovements);
        Assert.Equal(130m, row.ClosingDebit);
        Assert.Equal(0m, row.ClosingCredit);
        Assert.False(row.HasClosingDifference);
    }

    /// <summary>Verifies that a net credit position is represented in the closing credit column.</summary>
    [Fact]
    public void Calculate_WithNetCredit_UsesClosingCredit()
    {
        var accounts = new[] { new SaftAccount { AccountId = "22", Description = "Fornecedores", OpeningCreditBalance = 40m, ClosingCreditBalance = 65m } };
        var lines = new[] { new SaftTransactionLine { AccountId = "22", Side = "C", Amount = 25m } };

        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, lines));

        Assert.Equal(0m, row.ClosingDebit);
        Assert.Equal(65m, row.ClosingCredit);
        Assert.False(row.HasClosingDifference);
    }

    /// <summary>Verifies that account comparison identifies a difference from the closing balance reported in SAF-T.</summary>
    [Fact]
    public void Calculate_WithReportedMismatch_FlagsDifference()
    {
        var accounts = new[] { new SaftAccount { AccountId = "11", Description = "Caixa", OpeningDebitBalance = 10m, ClosingDebitBalance = 99m } };

        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, Array.Empty<SaftTransactionLine>()));

        Assert.Equal(10m, row.ClosingDebit);
        Assert.True(row.HasClosingDifference);
    }

    /// <summary>Verifies that debit and credit side codes are interpreted without case sensitivity.</summary>
    [Fact]
    public void Calculate_WithLowercaseSides_AggregatesMovements()
    {
        var accounts = new[] { new SaftAccount { AccountId = "21", Description = "Clientes" } };
        var lines = new[]
        {
            new SaftTransactionLine { AccountId = "21", Side = "d", Amount = 7m },
            new SaftTransactionLine { AccountId = "21", Side = "c", Amount = 2m }
        };

        var row = Assert.Single(TrialBalanceCalculator.Calculate(accounts, lines));

        Assert.Equal(7m, row.DebitMovements);
        Assert.Equal(2m, row.CreditMovements);
        Assert.Equal(5m, row.ClosingDebit);
    }
}
