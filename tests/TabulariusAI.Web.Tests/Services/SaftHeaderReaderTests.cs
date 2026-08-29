using System.Text;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

/// <summary>
/// Verifies secure parsing and validation behavior for SAF-T (PT) header and structural data.
/// </summary>
public sealed class SaftHeaderReaderTests
{
    /// <summary>Verifies representative SAF-T (PT) master data and accounting transaction parsing.</summary>
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
              <GeneralLedgerEntries><Journal><JournalID>VEN</JournalID><Description>Vendas</Description><Transaction><TransactionID>T1</TransactionID><Period>3</Period><TransactionDate>2026-03-15</TransactionDate><SourceID>TEST</SourceID><Description>Venda teste</Description><DocArchivalNumber>1</DocArchivalNumber><TransactionType>N</TransactionType><GLPostingDate>2026-03-15</GLPostingDate><CustomerID>C1</CustomerID><Lines><DebitLine><RecordID>1</RecordID><AccountID>2111</AccountID><SourceDocumentID>FT 1/1</SourceDocumentID><SystemEntryDate>2026-03-15T10:00:00</SystemEntryDate><Description>Cliente</Description><DebitAmount>123.00</DebitAmount></DebitLine><CreditLine><RecordID>2</RecordID><AccountID>71</AccountID><SourceDocumentID>FT 1/1</SourceDocumentID><SystemEntryDate>2026-03-15T10:00:00</SystemEntryDate><Description>Venda</Description><CreditAmount>123.00</CreditAmount></CreditLine></Lines></Transaction></Journal></GeneralLedgerEntries>
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
        Assert.Single(result.Suppliers);
        Assert.Equal("S1", result.Suppliers[0].PartyId);
        Assert.Single(result.Products);
        Assert.Equal("P1", result.Products[0].ProductCode);
        Assert.Single(result.Transactions);
        Assert.Equal("VEN", result.Transactions[0].JournalId);
        Assert.Equal("T1", result.Transactions[0].TransactionId);
        Assert.Equal(new DateOnly(2026, 3, 15), result.Transactions[0].TransactionDate);
        Assert.Equal("C1", result.Transactions[0].CustomerId);
        Assert.Equal(2, result.Transactions[0].Lines.Count);
        Assert.Equal(123.00m, result.Transactions[0].TotalDebit);
        Assert.Equal(123.00m, result.Transactions[0].TotalCredit);
        Assert.Equal("D", result.Transactions[0].Lines[0].Side);
        Assert.Equal("2111", result.Transactions[0].Lines[0].AccountId);
        Assert.Equal("C", result.Transactions[0].Lines[1].Side);
        Assert.Equal("71", result.Transactions[0].Lines[1].AccountId);
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
