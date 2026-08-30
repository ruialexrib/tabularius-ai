using System.Text;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class SaftPaymentsReaderTests
{
    [Fact]
    public async Task ReadAsync_Payment_MapsHeaderTotalsAndSettledDocument()
    {
        const string xml="""
<AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01"><Header><AuditFileVersion>1.04_01</AuditFileVersion><TaxRegistrationNumber>999999990</TaxRegistrationNumber><CompanyName>Empresa Teste</CompanyName><FiscalYear>2026</FiscalYear></Header><SourceDocuments><Payments><Payment><PaymentRefNo>RC 1/1</PaymentRefNo><TransactionDate>2026-06-30</TransactionDate><PaymentType>RC</PaymentType><Description>Recebimento</Description><SourceID>TEST</SourceID><CustomerID>C1</CustomerID><SystemEntryDate>2026-06-30T12:00:00</SystemEntryDate><DocumentStatus><PaymentStatus>N</PaymentStatus></DocumentStatus><Line><LineNumber>1</LineNumber><SourceDocumentID><OriginatingON>FT 1/10</OriginatingON><InvoiceDate>2026-06-15</InvoiceDate></SourceDocumentID><Description>Liquidação FT 1/10</Description><CreditAmount>123.45</CreditAmount></Line><DocumentTotals><TaxPayable>23.45</TaxPayable><NetTotal>100.00</NetTotal><GrossTotal>123.45</GrossTotal></DocumentTotals></Payment></Payments></SourceDocuments></AuditFile>
""";
        await using var stream=new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result=await new SaftHeaderReader().ReadAsync(stream);
        var payment=Assert.Single(result.Payments);
        Assert.Equal("RC 1/1",payment.PaymentRefNo);
        Assert.Equal("C1",payment.CustomerId);
        Assert.Equal(123.45m,payment.GrossTotal);
        var line=Assert.Single(payment.Lines);
        Assert.Equal("FT 1/10",line.OriginatingOn);
        Assert.Equal(new DateOnly(2026,6,15),line.InvoiceDate);
        Assert.Equal(123.45m,line.CreditAmount);
    }

    [Fact]
    public async Task ReadAsync_PaymentWithoutDocumentTotals_Throws()
    {
        const string xml="""
<AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01"><Header><AuditFileVersion>1.04_01</AuditFileVersion><TaxRegistrationNumber>999999990</TaxRegistrationNumber><CompanyName>Empresa Teste</CompanyName><FiscalYear>2026</FiscalYear></Header><SourceDocuments><Payments><Payment><PaymentRefNo>RC 1/1</PaymentRefNo><TransactionDate>2026-06-30</TransactionDate><PaymentType>RC</PaymentType><SourceID>TEST</SourceID></Payment></Payments></SourceDocuments></AuditFile>
""";
        await using var stream=new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var exception=await Assert.ThrowsAsync<InvalidDataException>(()=>new SaftHeaderReader().ReadAsync(stream));
        Assert.Contains("DocumentTotals",exception.Message);
    }
}
