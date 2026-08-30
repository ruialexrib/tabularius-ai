using Microsoft.AspNetCore.Mvc;
using TabulariusAI.Web.Controllers;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Controllers;

public sealed class DossierPaymentsControllerTests
{
    [Fact]
    public async Task Payments_Search_ReturnsOnlySelectedImportMatches()
    {
        await using var database=new TestDatabase();
        var entity=new AccountingEntity{Name="Entity",TaxRegistrationNumber="500000001"};database.Context.AccountingEntities.Add(entity);await database.Context.SaveChangesAsync();
        var dossier=new AnalysisDossier{AccountingEntityId=entity.Id,Name="Exercício 2026",FiscalYear=2026};database.Context.AnalysisDossiers.Add(dossier);await database.Context.SaveChangesAsync();
        var first=Import(dossier.Id,"first.xml");var second=Import(dossier.Id,"second.xml");database.Context.SaftImports.AddRange(first,second);await database.Context.SaveChangesAsync();
        database.Context.SaftPayments.AddRange(Payment(first.Id,"RC 1/1","C1",123m),Payment(first.Id,"RC 1/2","C2",50m),Payment(second.Id,"RC 2/1","C1",999m));await database.Context.SaveChangesAsync();
        var result=await new DossierController(database.Context).Payments(dossier.Id,first.Id,"C1",ct:CancellationToken.None);
        var model=Assert.IsType<SaftListViewModel<SaftPayment>>(Assert.IsType<ViewResult>(result).Model);
        var payment=Assert.Single(model.List.Items);Assert.Equal("RC 1/1",payment.PaymentRefNo);
    }

    [Fact]
    public async Task Payment_FromDifferentSelectedImport_ReturnsNotFound()
    {
        await using var database=new TestDatabase();
        var entity=new AccountingEntity{Name="Entity",TaxRegistrationNumber="500000001"};database.Context.AccountingEntities.Add(entity);await database.Context.SaveChangesAsync();
        var dossier=new AnalysisDossier{AccountingEntityId=entity.Id,Name="Exercício 2026",FiscalYear=2026};database.Context.AnalysisDossiers.Add(dossier);await database.Context.SaveChangesAsync();
        var first=Import(dossier.Id,"first.xml");var second=Import(dossier.Id,"second.xml");database.Context.SaftImports.AddRange(first,second);await database.Context.SaveChangesAsync();
        var payment=Payment(second.Id,"RC 2/1","C1",100m);database.Context.SaftPayments.Add(payment);await database.Context.SaveChangesAsync();
        Assert.IsType<NotFoundResult>(await new DossierController(database.Context).Payment(dossier.Id,first.Id,payment.Id,CancellationToken.None));
    }

    private static SaftImport Import(int dossierId,string file)=>new(){DossierId=dossierId,OriginalFileName=file,SaftVersion="1.04_01",StartDate=new(2026,1,1),EndDate=new(2026,12,31),ImportedAtUtc=DateTime.UtcNow};
    private static SaftPayment Payment(int importId,string reference,string customer,decimal total)=>new(){SaftImportId=importId,PaymentRefNo=reference,TransactionDate=new(2026,6,30),PaymentType="RC",SourceId="TEST",CustomerId=customer,PaymentStatus="N",NetTotal=total,TaxPayable=0,GrossTotal=total};
}
