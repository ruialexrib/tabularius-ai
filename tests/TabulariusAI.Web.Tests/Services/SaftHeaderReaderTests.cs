using System.Text;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

/// <summary>
/// Verifies secure parsing and validation behavior for SAF-T (PT) header and structural data.
/// </summary>
public sealed class SaftHeaderReaderTests
{
    /// <summary>Verifies representative SAF-T (PT) header, account, customer, supplier and product parsing.</summary>
    [Fact]
    public async Task ReadAsync_ValidSaft_ReturnsMasterDataAndStructuralCounts()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
              <Header><AuditFileVersion>1.04_01</AuditFileVersion><TaxRegistrationNumber>999999990</TaxRegistrationNumber><CompanyName>Empresa Teste, Lda.</CompanyName><FiscalYear>2026</FiscalYear><StartDate>2026-01-01</StartDate><EndDate>2026-12-31</EndDate><ProductID>Test</ProductID><ProductVersion>1.0</ProductVersion></Header>
              <MasterFiles>
                <GeneralLedgerAccounts><Account><AccountID>11</AccountID><AccountDescription>Caixa</AccountDescription><OpeningDebitBalance>10.00</OpeningDebitBalance><OpeningCreditBalance>0</OpeningCreditBalance><ClosingDebitBalance>20.00</ClosingDebitBalance><ClosingCreditBalance>0</ClosingCreditBalance></Account></GeneralLedgerAccounts>
                <Customer><CustomerID>C1</CustomerID><AccountID>2111</AccountID><CustomerTaxID>500000001</CustomerTaxID><CompanyName>Cliente Teste</CompanyName></Customer>
                <Supplier><SupplierID>S1</SupplierID><AccountID>2211</AccountID><SupplierTaxID>500000002</SupplierTaxID><CompanyName>Fornecedor Teste</CompanyName></Supplier>
                <Product><ProductType>P</ProductType><ProductCode>P1</ProductCode><ProductGroup>Mercadorias</ProductGroup><ProductDescription>Produto Teste</ProductDescription><ProductNumberCode>5600000000011</ProductNumberCode></Product>
              </MasterFiles>
              <GeneralLedgerEntries><Journal><Transaction><TransactionID>T1</TransactionID></Transaction></Journal></GeneralLedgerEntries>
              <SourceDocuments><SalesInvoices><Invoice><InvoiceNo>FT 1/1</InvoiceNo></Invoice></SalesInvoices><MovementOfGoods><StockMovement><DocumentNumber>GT 1/1</DocumentNumber></StockMovement></MovementOfGoods><WorkingDocuments><WorkDocument><DocumentNumber>OR 1/1</DocumentNumber></WorkDocument></WorkingDocuments><Payments><Payment><PaymentRefNo>RC 1/1</PaymentRefNo></Payment></Payments></SourceDocuments>
            </AuditFile>
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = await new SaftHeaderReader().ReadAsync(stream);
        Assert.Equal("1.04_01", result.SaftVersion);
        Assert.Single(result.Accounts);
        Assert.Equal("11", result.Accounts[0].AccountId);
        Assert.Single(result.Customers);
        Assert.Equal("C1", result.Customers[0].PartyId);
        Assert.Equal("Cliente Teste", result.Customers[0].CompanyName);
        Assert.Single(result.Suppliers);
        Assert.Equal("S1", result.Suppliers[0].PartyId);
        Assert.Equal("Fornecedor Teste", result.Suppliers[0].CompanyName);
        Assert.Single(result.Products);
        Assert.Equal("P", result.Products[0].ProductType);
        Assert.Equal("P1", result.Products[0].ProductCode);
        Assert.Equal("Mercadorias", result.Products[0].ProductGroup);
        Assert.Equal("Produto Teste", result.Products[0].ProductDescription);
        Assert.Equal("5600000000011", result.Products[0].ProductNumberCode);
        Assert.Equal(1, result.ProductCount);
        Assert.Equal(1, result.TransactionCount);
        Assert.Equal(1, result.SalesInvoiceCount);
        Assert.Equal(1, result.MovementOfGoodsCount);
        Assert.Equal(1, result.WorkingDocumentCount);
        Assert.Equal(1, result.PaymentCount);
    }

    /// <summary>Verifies that arbitrary XML is rejected at the SAF-T (PT) import boundary.</summary>
    [Fact]
    public async Task ReadAsync_NonSaftXml_ThrowsInvalidDataException()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<Document><Value>test</Value></Document>"));
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => new SaftHeaderReader().ReadAsync(stream));
        Assert.Contains("SAF-T (PT)", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Verifies that external entity declarations are rejected by secure XML settings.</summary>
    [Fact]
    public async Task ReadAsync_DocumentTypeDeclaration_ThrowsInvalidDataException()
    {
        const string xml = "<!DOCTYPE AuditFile [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><AuditFile>&xxe;</AuditFile>";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        await Assert.ThrowsAsync<InvalidDataException>(() => new SaftHeaderReader().ReadAsync(stream));
    }
}
