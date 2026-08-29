using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class BalanceSheetCalculatorTests
{
    [Fact]
    public void Calculate_ClassifiesCommonMovementAccounts()
    {
        var accounts = new[]
        {
            new SaftAccount { AccountId="12",Description="Banco",ClosingDebitBalance=1000m },
            new SaftAccount { AccountId="2111",Description="Clientes",ClosingDebitBalance=500m },
            new SaftAccount { AccountId="2211",Description="Fornecedores",ClosingCreditBalance=400m },
            new SaftAccount { AccountId="51",Description="Capital",ClosingCreditBalance=1100m }
        };
        var model=BalanceSheetCalculator.Calculate(accounts);
        Assert.Equal(1500m,model.TotalAssets);Assert.Equal(400m,model.TotalLiabilities);Assert.Equal(1100m,model.TotalEquity);Assert.True(model.IsBalanced);
    }

    [Fact]
    public void Calculate_ExcludesAggregateAccounts()
    {
        var accounts=new[]{new SaftAccount{AccountId="21",Description="Clientes",ClosingDebitBalance=1000m},new SaftAccount{AccountId="2111",Description="Cliente A",ClosingDebitBalance=100m}};
        var model=BalanceSheetCalculator.Calculate(accounts);
        var row=Assert.Single(model.Assets);Assert.Equal("2111",row.AccountId);Assert.Equal(100m,model.TotalAssets);
    }

    [Fact]
    public void Calculate_KeepsUnknownAccountsVisibleForReview()
    {
        var model=BalanceSheetCalculator.Calculate(new[]{new SaftAccount{AccountId="91",Description="Conta interna",ClosingDebitBalance=50m}});
        Assert.Single(model.Unclassified);Assert.False(model.IsBalanced);
    }
}
