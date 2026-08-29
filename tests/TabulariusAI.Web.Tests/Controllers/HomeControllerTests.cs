using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Controllers;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Controllers;

/// <summary>Verifies SAF-T (PT) upload validation, duplicate protection and persistence behavior.</summary>
public sealed class HomeControllerTests
{
    /// <summary>Verifies that a missing upload is rejected before parsing.</summary>
    [Fact] public async Task UploadSaftAsync_MissingFile_ReturnsImportWithValidationError() { await using var database=new TestDatabase(); var controller=CreateController(database,new StubSaftHeaderReader(CreateAnalysis())); var result=await controller.UploadSaftAsync(null,CancellationToken.None); Assert.Equal("Import",Assert.IsType<ViewResult>(result).ViewName); Assert.False(controller.ModelState.IsValid); }
    /// <summary>Verifies that a non-XML extension is rejected before parsing.</summary>
    [Fact] public async Task UploadSaftAsync_NonXmlFile_ReturnsImportWithValidationError() { await using var database=new TestDatabase(); var reader=new StubSaftHeaderReader(CreateAnalysis()); var controller=CreateController(database,reader); var result=await controller.UploadSaftAsync(CreateFile("source.txt","not xml"),CancellationToken.None); Assert.Equal("Import",Assert.IsType<ViewResult>(result).ViewName); Assert.False(controller.ModelState.IsValid); Assert.Equal(0,reader.CallCount); }
    /// <summary>Verifies successful minimal import persistence.</summary>
    [Fact] public async Task UploadSaftAsync_ValidFile_PersistsEntityDossierAndImport() { await using var database=new TestDatabase(); var analysis=CreateAnalysis(); var controller=CreateController(database,new StubSaftHeaderReader(analysis)); var result=await controller.UploadSaftAsync(CreateFile("company.xml","<AuditFile />"),CancellationToken.None); Assert.Same(analysis,Assert.IsType<ViewResult>(result).Model); Assert.Single(await database.Context.AccountingEntities.AsNoTracking().ToListAsync()); Assert.Single(await database.Context.AnalysisDossiers.AsNoTracking().ToListAsync()); var import=Assert.Single(await database.Context.SaftImports.AsNoTracking().ToListAsync()); Assert.Equal("company.xml",import.OriginalFileName); Assert.Equal(64,import.ContentHash!.Length); }
    /// <summary>Verifies exact duplicate content is rejected before a second parse.</summary>
    [Fact] public async Task UploadSaftAsync_DuplicateContent_IsRejectedBeforeSecondParse() { await using var database=new TestDatabase(); var reader=new StubSaftHeaderReader(CreateAnalysis()); await CreateController(database,reader).UploadSaftAsync(CreateFile("first.xml","same content"),CancellationToken.None); var second=CreateController(database,reader); var result=await second.UploadSaftAsync(CreateFile("renamed.xml","same content"),CancellationToken.None); Assert.Equal("Import",Assert.IsType<ViewResult>(result).ViewName); Assert.False(second.ModelState.IsValid); Assert.Equal(1,reader.CallCount); }
    /// <summary>Verifies invalid fiscal years are rejected.</summary>
    [Fact] public async Task UploadSaftAsync_InvalidFiscalYear_ReturnsValidationErrorWithoutImport() { await using var database=new TestDatabase(); var analysis=CreateAnalysis(); analysis.FiscalYear="invalid"; var controller=CreateController(database,new StubSaftHeaderReader(analysis)); Assert.Equal("Import",Assert.IsType<ViewResult>(await controller.UploadSaftAsync(CreateFile("company.xml","content"),CancellationToken.None)).ViewName); Assert.False(controller.ModelState.IsValid); Assert.Empty(await database.Context.SaftImports.AsNoTracking().ToListAsync()); }
    /// <summary>Verifies parser validation failures are surfaced without persistence.</summary>
    [Fact] public async Task UploadSaftAsync_ReaderRejectsFile_ReturnsValidationError() { await using var database=new TestDatabase(); var controller=CreateController(database,new ThrowingSaftHeaderReader()); var result=await controller.UploadSaftAsync(CreateFile("invalid.xml","invalid"),CancellationToken.None); Assert.Equal("Import",Assert.IsType<ViewResult>(result).ViewName); Assert.False(controller.ModelState.IsValid); Assert.Empty(database.Context.SaftImports); }
    /// <summary>Verifies a complete parsed result persists all currently supported master-data and ledger collections.</summary>
    [Fact] public async Task UploadSaftAsync_CompleteAnalysis_PersistsMasterDataAndLedger() { await using var database=new TestDatabase(); var analysis=CreateAnalysis(); analysis.Accounts.Add(new(){AccountId="2111",Description="Clientes",OpeningDebitBalance=10m,ClosingDebitBalance=125m,TaxonomyReference="AR"}); analysis.Customers.Add(new(){PartyId="C001",AccountId="2111",TaxId="500000301",CompanyName="Customer One"}); analysis.Suppliers.Add(new(){PartyId="S001",AccountId="2211",TaxId="500000302",CompanyName="Supplier One"}); analysis.Products.Add(new(){ProductType="S",ProductCode="SERV1",ProductGroup="CONS",ProductDescription="Consulting",ProductNumberCode="560000000001"}); var transaction=new SaftTransactionViewModel{JournalId="GJ",JournalDescription="General",TransactionId="TX-1",Period=1,TransactionDate=new(2026,1,10),SourceId="user",Description="Sale",DocArchivalNumber="DOC1",TransactionType="N",GlPostingDate=new(2026,1,10),CustomerId="C001"}; transaction.Lines.Add(new(){RecordId="1",AccountId="2111",SourceDocumentId="FT 1/1",SystemEntryDate=new DateTime(2026,1,10,12,0,0,DateTimeKind.Utc),Description="Debit",Side="D",Amount=123m}); transaction.Lines.Add(new(){RecordId="2",AccountId="7211",Description="Credit",Side="C",Amount=123m}); analysis.Transactions.Add(transaction); var result=await CreateController(database,new StubSaftHeaderReader(analysis)).UploadSaftAsync(CreateFile("complete.xml","complete content"),CancellationToken.None); Assert.IsType<ViewResult>(result); Assert.Single(await database.Context.SaftAccounts.ToListAsync()); Assert.Single(await database.Context.SaftCustomers.ToListAsync()); Assert.Single(await database.Context.SaftSuppliers.ToListAsync()); Assert.Single(await database.Context.SaftProducts.ToListAsync()); var persisted=Assert.Single(await database.Context.SaftTransactions.Include(x=>x.Lines).ToListAsync()); Assert.Equal(2,persisted.Lines.Count); Assert.Equal(123m,persisted.Lines.Where(x=>x.Side=="D").Sum(x=>x.Amount)); }
    /// <summary>Verifies later imports reuse the same entity and fiscal-year dossier while refreshing the entity name.</summary>
    [Fact] public async Task UploadSaftAsync_SameTaxIdAndYear_ReusesEntityAndDossier() { await using var database=new TestDatabase(); var first=CreateAnalysis(); await CreateController(database,new StubSaftHeaderReader(first)).UploadSaftAsync(CreateFile("q1.xml","q1"),CancellationToken.None); var second=CreateAnalysis(); second.CompanyName="Renamed Company"; second.StartDate="2026-04-01"; second.EndDate="2026-06-30"; await CreateController(database,new StubSaftHeaderReader(second)).UploadSaftAsync(CreateFile("q2.xml","q2"),CancellationToken.None); Assert.Single(await database.Context.AccountingEntities.ToListAsync()); Assert.Single(await database.Context.AnalysisDossiers.ToListAsync()); Assert.Equal(2,await database.Context.SaftImports.CountAsync()); Assert.Equal("Renamed Company",(await database.Context.AccountingEntities.SingleAsync()).Name); }
    /// <summary>Verifies malformed optional period dates are persisted as null rather than inventing dates.</summary>
    [Fact] public async Task UploadSaftAsync_InvalidPeriodDates_PersistsNullDates() { await using var database=new TestDatabase(); var analysis=CreateAnalysis(); analysis.StartDate="bad"; analysis.EndDate="2026/12/31"; await CreateController(database,new StubSaftHeaderReader(analysis)).UploadSaftAsync(CreateFile("dates.xml","dates"),CancellationToken.None); var import=await database.Context.SaftImports.SingleAsync(); Assert.Null(import.StartDate); Assert.Null(import.EndDate); }
    /// <summary>Verifies the error page exposes the request trace identifier when no activity exists.</summary>
    [Fact] public async Task Error_WithoutActivity_UsesHttpTraceIdentifier() { await using var database=new TestDatabase(); var controller=CreateController(database,new StubSaftHeaderReader(CreateAnalysis())); controller.HttpContext.TraceIdentifier="trace-test"; var model=Assert.IsType<ErrorViewModel>(Assert.IsType<ViewResult>(controller.Error()).Model); Assert.Equal("trace-test",model.RequestId); }
    /// <summary>Verifies the presentation and import GET actions return views.</summary>
    [Fact] public async Task NavigationActions_ReturnViews() { await using var database=new TestDatabase(); var controller=CreateController(database,new StubSaftHeaderReader(CreateAnalysis())); Assert.IsType<ViewResult>(controller.Index()); Assert.IsType<ViewResult>(controller.Import()); }

    /// <summary>Creates a controller with deterministic TempData support.</summary>
    private static HomeController CreateController(TestDatabase database,ISaftHeaderReader reader) { var controller=new HomeController(reader,database.Context){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext()}}; controller.TempData=new TempDataDictionary(controller.HttpContext,new TestTempDataProvider()); return controller; }
    /// <summary>Creates an in-memory form file.</summary>
    private static IFormFile CreateFile(string fileName,string content) { var bytes=Encoding.UTF8.GetBytes(content); return new FormFile(new MemoryStream(bytes),0,bytes.Length,"saftFile",fileName); }
    /// <summary>Creates a minimal valid parsed SAF-T result.</summary>
    private static SaftHeaderViewModel CreateAnalysis()=>new(){SaftVersion="1.04_01",TaxRegistrationNumber="500000200",CompanyName="Test Company",FiscalYear="2026",StartDate="2026-01-01",EndDate="2026-12-31",ProductId="Tests",ProductVersion="1.0"};
    /// <summary>Provides a deterministic SAF-T reader result.</summary>
    private sealed class StubSaftHeaderReader(SaftHeaderViewModel result):ISaftHeaderReader { /// <summary>Gets parser invocation count.</summary>
        public int CallCount{get;private set;} /// <summary>Returns the configured result.</summary>
        public Task<SaftHeaderViewModel> ReadAsync(Stream stream,CancellationToken cancellationToken=default){CallCount++;return Task.FromResult(result);} }
    /// <summary>Provides a SAF-T reader that reports invalid input.</summary>
    private sealed class ThrowingSaftHeaderReader:ISaftHeaderReader { /// <summary>Throws the same validation exception expected from the parser boundary.</summary>
        public Task<SaftHeaderViewModel> ReadAsync(Stream stream,CancellationToken cancellationToken=default)=>throw new InvalidDataException("Invalid SAF-T test input."); }
    /// <summary>Provides in-memory TempData.</summary>
    private sealed class TestTempDataProvider:ITempDataProvider { /// <summary>Loads empty TempData.</summary>
        public IDictionary<string,object> LoadTempData(HttpContext context)=>new Dictionary<string,object>(); /// <summary>Accepts TempData writes.</summary>
        public void SaveTempData(HttpContext context,IDictionary<string,object> values){} }
}
