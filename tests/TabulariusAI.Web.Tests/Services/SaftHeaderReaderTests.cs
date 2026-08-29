using System.Text;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class SaftHeaderReaderTests
{
    [Fact]
    public async Task ReadAsync_ValidSaft_ReturnsMasterDataDocumentsAndStructuralCounts()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
              <Header><AuditFileVersion>1.04_01</AuditFileVersion><TaxRegistrationNumber>999999990</TaxRegistrationNumber><CompanyName>Empresa Teste, Lda.</CompanyName><FiscalYear>2026</FiscalYear><StartDate>2026-01-01</StartDate><EndDate>2026-12-31</EndDate><ProductID>Test</ProductID><ProductVersion>1.0</ProductVersion></Header>
              <MasterFiles><GeneralLedgerAccounts><Account><AccountID>11</AccountID><AccountDescription>Caixa</AccountDescription><OpeningDebitBalance>10.00</OpeningDebitBalance><OpeningCreditBalance>0</OpeningCreditBalance><ClosingDebitBalance>20.00</ClosingDebitBalance><ClosingCreditBalance>0</ClosingCreditBalance></Account></GeneralLedgerAccounts><Customer><CustomerID>C1</CustomerID><AccountID>2111</AccountID><CustomerTaxID>500000001</CustomerTaxID><CompanyName>Cliente Teste</CompanyName></Customer><Supplier><SupplierID>S1</SupplierID><AccountID>2211</AccountID><SupplierTaxID>500000002</SupplierTaxID><CompanyName>Fornecedor Teste</CompanyName></Supplier><Product><ProductType>P</ProductType><ProductCode>P1</ProductCode><ProductGroup>Mercadorias</ProductGroup><ProductDescription>Produto Teste</ProductDescription><ProductNumberCode>5600000000011</ProductNumberCode></Product></MasterFiles>
              <GeneralLedgerEntries><Journal><JournalID>VEN</JournalID><Description>Vendas</Description><Transaction><TransactionID>T1</TransactionID><Period>3</Period><TransactionDate>2026-03-15</TransactionDate><SourceID>TEST</SourceID><Description>Venda teste</Description><DocArchivalNumber>1</DocArchivalNumber><TransactionType>N</TransactionType><GLPostingDate>2026-03-15</GLPostingDate><CustomerID>C1</CustomerID><Lines><DebitLine><RecordID>1</RecordID><AccountID>2111</AccountID><SourceDocumentID>FT 1/1</SourceDocumentID><SystemEntryDate>2026-03-15T10:00:00</SystemEntryDate><Description>Cliente</Description><DebitAmount>123.00</DebitAmount></DebitLine><CreditLine><RecordID>2</RecordID><AccountID>71</AccountID><SourceDocumentID>FT 1/1</SourceDocumentID><SystemEntryDate>2026-03-15T10:00:00</SystemEntryDate><Description>Venda</Description><CreditAmount>123.00</CreditAmount></CreditLine></Lines></Transaction></Journal></GeneralLedgerEntries>
              <SourceDocuments><SalesInvoices><Invoice><InvoiceNo>FT 1/1</InvoiceNo><DocumentStatus><InvoiceStatus>N</InvoiceStatus></DocumentStatus><InvoiceDate>2026-03-15</InvoiceDate><InvoiceType>FT</InvoiceType><SourceID>TEST</SourceID><SystemEntryDate>2026-03-15T10:00:00</SystemEntryDate><CustomerID>C1</CustomerID><Line><LineNumber>1</LineNumber><ProductCode>P1</ProductCode><ProductDescription>Produto Teste</ProductDescription><Quantity>1</Quantity><UnitOfMeasure>UN</UnitOfMeasure><UnitPrice>100.00</UnitPrice><TaxPointDate>2026-03-15</TaxPointDate><CreditAmount>100.00</CreditAmount><Tax><TaxType>IVA</TaxType><TaxCode>NOR</TaxCode><TaxPercentage>23</TaxPercentage></Tax></Line><DocumentTotals><TaxPayable>23.00</TaxPayable><NetTotal>100.00</NetTotal><GrossTotal>123.00</GrossTotal></DocumentTotals></Invoice></SalesInvoices><MovementOfGoods><StockMovement><DocumentNumber>GT 1/1</DocumentNumber></StockMovement></MovementOfGoods><WorkingDocuments><WorkDocument><DocumentNumber>OR 1/1</DocumentNumber></WorkDocument></WorkingDocuments><Payments><Payment><PaymentRefNo>RC 1/1</PaymentRefNo></Payment></Payments></SourceDocuments>
            </AuditFile>
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)); var result = await new SaftHeaderReader().ReadAsync(stream);
        Assert.Equal("1.04_01", result.SaftVersion); Assert.Single(result.Accounts); Assert.Single(result.Customers); Assert.Single(result.Suppliers); Assert.Single(result.Products); Assert.Single(result.Transactions); Assert.Equal(2, result.Transactions[0].Lines.Count); Assert.Equal(123m, result.Transactions[0].TotalDebit); Assert.Equal(123m, result.Transactions[0].TotalCredit);
        var invoice = Assert.Single(result.SalesInvoices); Assert.Equal("FT 1/1", invoice.InvoiceNo); Assert.Equal("FT", invoice.InvoiceType); Assert.Equal("C1", invoice.CustomerId); Assert.Equal(100m, invoice.NetTotal); Assert.Equal(23m, invoice.TaxPayable); Assert.Equal(123m, invoice.GrossTotal); var line = Assert.Single(invoice.Lines); Assert.Equal("P1", line.ProductCode); Assert.Equal(23m, line.TaxPercentage); Assert.Equal(100m, line.LineAmount);
        Assert.Equal(1, result.TransactionCount); Assert.Equal(1, result.SalesInvoiceCount); Assert.Equal(1, result.MovementOfGoodsCount); Assert.Equal(1, result.WorkingDocumentCount); Assert.Equal(1, result.PaymentCount);
    }

    [Fact] public async Task ReadAsync_NonSaftXml_ThrowsInvalidDataException() { await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<Document><Value>test</Value></Document>")); var exception = await Assert.ThrowsAsync<InvalidDataException>(() => new SaftHeaderReader().ReadAsync(stream)); Assert.Contains("SAF-T (PT)", exception.Message, StringComparison.Ordinal); }
    [Fact] public async Task ReadAsync_DocumentTypeDeclaration_ThrowsInvalidDataException() { const string xml = "<!DOCTYPE AuditFile [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><AuditFile>&xxe;</AuditFile>"; await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)); await Assert.ThrowsAsync<InvalidDataException>(() => new SaftHeaderReader().ReadAsync(stream)); }
}
