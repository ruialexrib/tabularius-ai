using System.Text;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class SaftTaxTableReaderTests
{
    [Fact]
    public async Task ReadAsync_TaxTable_MapsEntries()
    {
        const string xml="""
<AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01"><Header><AuditFileVersion>1.04_01</AuditFileVersion><TaxRegistrationNumber>999999990</TaxRegistrationNumber><CompanyName>Test Company</CompanyName><FiscalYear>2026</FiscalYear></Header><MasterFiles><TaxTable><TaxTableEntry><TaxType>IVA</TaxType><TaxCountryRegion>PT</TaxCountryRegion><TaxCode>NOR</TaxCode><Description>Taxa normal</Description><TaxExpirationDate>2026-12-31</TaxExpirationDate><TaxPercentage>23</TaxPercentage></TaxTableEntry><TaxTableEntry><TaxType>IS</TaxType><TaxCountryRegion>PT</TaxCountryRegion><TaxCode>SEL</TaxCode><Description>Imposto do selo</Description><TaxAmount>1.5</TaxAmount></TaxTableEntry></TaxTable></MasterFiles></AuditFile>
""";
        await using var stream=new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result=await new SaftHeaderReader().ReadAsync(stream);
        Assert.Equal(2,result.TaxEntryCount);
        var vat=Assert.Single(result.TaxEntries,item=>item.TaxType=="IVA");
        Assert.Equal("NOR",vat.TaxCode); Assert.Equal(23m,vat.TaxPercentage); Assert.Equal(new DateOnly(2026,12,31),vat.TaxExpirationDate);
        var stamp=Assert.Single(result.TaxEntries,item=>item.TaxType=="IS"); Assert.Equal(1.5m,stamp.TaxAmount); Assert.Null(stamp.TaxPercentage);
    }
}
