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
    [Fact]
    public async Task UploadSaftAsync_MissingFile_ReturnsImportWithValidationError()
    {
        await using var database = new TestDatabase();
        var controller = CreateController(database, new StubSaftHeaderReader(CreateAnalysis()));

        var result = await controller.UploadSaftAsync(null, CancellationToken.None);

        Assert.Equal("Import", Assert.IsType<ViewResult>(result).ViewName);
        Assert.False(controller.ModelState.IsValid);
    }

    /// <summary>Verifies that a non-XML extension is rejected before the SAF-T parser is called.</summary>
    [Fact]
    public async Task UploadSaftAsync_NonXmlFile_ReturnsImportWithValidationError()
    {
        await using var database = new TestDatabase();
        var reader = new StubSaftHeaderReader(CreateAnalysis());
        var controller = CreateController(database, reader);

        var result = await controller.UploadSaftAsync(CreateFile("source.txt", "not xml"), CancellationToken.None);

        Assert.Equal("Import", Assert.IsType<ViewResult>(result).ViewName);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(0, reader.CallCount);
    }

    /// <summary>Verifies that a valid SAF-T (PT) analysis creates the entity, dossier and import once.</summary>
    [Fact]
    public async Task UploadSaftAsync_ValidFile_PersistsEntityDossierAndImport()
    {
        await using var database = new TestDatabase();
        var analysis = CreateAnalysis();
        var controller = CreateController(database, new StubSaftHeaderReader(analysis));

        var result = await controller.UploadSaftAsync(CreateFile("company.xml", "<AuditFile />"), CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Import", view.ViewName);
        Assert.Same(analysis, view.Model);
        Assert.Single(await database.Context.AccountingEntities.AsNoTracking().ToListAsync());
        Assert.Single(await database.Context.AnalysisDossiers.AsNoTracking().ToListAsync());
        var import = Assert.Single(await database.Context.SaftImports.AsNoTracking().ToListAsync());
        Assert.Equal("company.xml", import.OriginalFileName);
        Assert.Equal(new DateOnly(2026, 1, 1), import.StartDate);
        Assert.Equal(new DateOnly(2026, 12, 31), import.EndDate);
        Assert.NotNull(import.ContentHash);
        Assert.Equal(64, import.ContentHash!.Length);
    }

    /// <summary>Verifies that exact duplicate file content is rejected without invoking the parser a second time.</summary>
    [Fact]
    public async Task UploadSaftAsync_DuplicateContent_IsRejectedBeforeSecondParse()
    {
        await using var database = new TestDatabase();
        var reader = new StubSaftHeaderReader(CreateAnalysis());
        var firstController = CreateController(database, reader);
        await firstController.UploadSaftAsync(CreateFile("first.xml", "same content"), CancellationToken.None);
        var secondController = CreateController(database, reader);

        var result = await secondController.UploadSaftAsync(CreateFile("renamed.xml", "same content"), CancellationToken.None);

        Assert.Equal("Import", Assert.IsType<ViewResult>(result).ViewName);
        Assert.False(secondController.ModelState.IsValid);
        Assert.Equal(1, reader.CallCount);
        Assert.Single(await database.Context.SaftImports.AsNoTracking().ToListAsync());
    }

    /// <summary>Verifies that an invalid fiscal year returned by parsing is rejected and not persisted as an import.</summary>
    [Fact]
    public async Task UploadSaftAsync_InvalidFiscalYear_ReturnsValidationErrorWithoutImport()
    {
        await using var database = new TestDatabase();
        var analysis = CreateAnalysis();
        analysis.FiscalYear = "invalid";
        var controller = CreateController(database, new StubSaftHeaderReader(analysis));

        var result = await controller.UploadSaftAsync(CreateFile("company.xml", "content"), CancellationToken.None);

        Assert.Equal("Import", Assert.IsType<ViewResult>(result).ViewName);
        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(await database.Context.SaftImports.AsNoTracking().ToListAsync());
    }

    /// <summary>Creates a controller with deterministic TempData support for isolated action tests.</summary>
    private static HomeController CreateController(TestDatabase database, ISaftHeaderReader reader)
    {
        var controller = new HomeController(reader, database.Context)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.TempData = new TempDataDictionary(controller.HttpContext, new TestTempDataProvider());
        return controller;
    }

    /// <summary>Creates an in-memory form file with a stable file name and content.</summary>
    private static IFormFile CreateFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "saftFile", fileName);
    }

    /// <summary>Creates a minimal valid parsed SAF-T (PT) result for persistence tests.</summary>
    private static SaftHeaderViewModel CreateAnalysis() => new()
    {
        SaftVersion = "1.04_01",
        TaxRegistrationNumber = "500000200",
        CompanyName = "Test Company",
        FiscalYear = "2026",
        StartDate = "2026-01-01",
        EndDate = "2026-12-31",
        ProductId = "Tests",
        ProductVersion = "1.0"
    };

    /// <summary>Provides a deterministic SAF-T reader result and tracks parser invocations.</summary>
    private sealed class StubSaftHeaderReader(SaftHeaderViewModel result) : ISaftHeaderReader
    {
        /// <summary>Gets the number of parser invocations made during the test.</summary>
        public int CallCount { get; private set; }

        /// <summary>Returns the configured SAF-T result and increments the invocation counter.</summary>
        public Task<SaftHeaderViewModel> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    /// <summary>Provides an in-memory TempData implementation for controller tests.</summary>
    private sealed class TestTempDataProvider : ITempDataProvider
    {
        /// <summary>Loads an empty TempData dictionary.</summary>
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        /// <summary>Accepts TempData persistence without external storage.</summary>
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
