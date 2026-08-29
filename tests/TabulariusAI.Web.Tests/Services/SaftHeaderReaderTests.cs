using System.Text;
using TabulariusAI.Web.Services;

namespace TabulariusAI.Web.Tests.Services;

/// <summary>
/// Verifies secure parsing and validation behavior for SAF-T (PT) header and structural data.
/// </summary>
public sealed class SaftHeaderReaderTests
{
    /// <summary>
    /// Verifies that a representative synthetic SAF-T (PT) document is parsed without losing adjacent header fields.
    /// </summary>
    [Fact]
    public async Task ReadAsync_ValidSaft_ReturnsHeaderAndStructuralCounts()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
              <Header>
                <AuditFileVersion>1.04_01</AuditFileVersion>
                <CompanyID>TEST</CompanyID>
                <TaxRegistrationNumber>999999990</TaxRegistrationNumber>
                <CompanyName>Empresa Teste, Lda.</CompanyName>
                <FiscalYear>2026</FiscalYear>
                <StartDate>2026-01-01</StartDate>
                <EndDate>2026-12-31</EndDate>
                <ProductID>Tabularius Test Fixture</ProductID>
                <ProductVersion>1.0</ProductVersion>
              </Header>
              <MasterFiles>
                <GeneralLedgerAccounts>
                  <Account><AccountID>11</AccountID></Account>
                  <Account><AccountID>12</AccountID></Account>
                </GeneralLedgerAccounts>
                <Customer><CustomerID>C1</CustomerID></Customer>
                <Supplier><SupplierID>S1</SupplierID></Supplier>
                <Product><ProductCode>P1</ProductCode></Product>
              </MasterFiles>
              <GeneralLedgerEntries>
                <Journal><Transaction><TransactionID>T1</TransactionID></Transaction></Journal>
              </GeneralLedgerEntries>
              <SourceDocuments>
                <SalesInvoices><Invoice><InvoiceNo>FT 1/1</InvoiceNo></Invoice></SalesInvoices>
                <MovementOfGoods><StockMovement><DocumentNumber>GT 1/1</DocumentNumber></StockMovement></MovementOfGoods>
                <WorkingDocuments><WorkDocument><DocumentNumber>OR 1/1</DocumentNumber></WorkDocument></WorkingDocuments>
                <Payments><Payment><PaymentRefNo>RC 1/1</PaymentRefNo></Payment></Payments>
              </SourceDocuments>
            </AuditFile>
            """;

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var reader = new SaftHeaderReader();

        var result = await reader.ReadAsync(stream);

        Assert.Equal("1.04_01", result.SaftVersion);
        Assert.Equal("999999990", result.TaxRegistrationNumber);
        Assert.Equal("Empresa Teste, Lda.", result.CompanyName);
        Assert.Equal("2026", result.FiscalYear);
        Assert.Equal("2026-01-01", result.StartDate);
        Assert.Equal("2026-12-31", result.EndDate);
        Assert.Equal(2, result.AccountCount);
        Assert.Equal(1, result.CustomerCount);
        Assert.Equal(1, result.SupplierCount);
        Assert.Equal(1, result.ProductCount);
        Assert.Equal(1, result.TransactionCount);
        Assert.Equal(1, result.SalesInvoiceCount);
        Assert.Equal(1, result.MovementOfGoodsCount);
        Assert.Equal(1, result.WorkingDocumentCount);
        Assert.Equal(1, result.PaymentCount);
    }

    /// <summary>
    /// Verifies that arbitrary XML is rejected at the SAF-T (PT) import boundary.
    /// </summary>
    [Fact]
    public async Task ReadAsync_NonSaftXml_ThrowsInvalidDataException()
    {
        const string xml = "<Document><Value>test</Value></Document>";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var reader = new SaftHeaderReader();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(stream));

        Assert.Contains("SAF-T (PT)", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that external entity declarations are rejected by the secure XML reader configuration.
    /// </summary>
    [Fact]
    public async Task ReadAsync_DocumentTypeDeclaration_ThrowsInvalidDataException()
    {
        const string xml = "<!DOCTYPE AuditFile [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><AuditFile>&xxe;</AuditFile>";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var reader = new SaftHeaderReader();

        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(stream));
    }
}
