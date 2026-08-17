using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Tests.Shared;

public class FieldMappingTests
{
    [Fact]
    public void FieldMapping_exposes_request_and_cba_response_sides_of_the_mapping()
    {
        var mapping = new FieldMapping("SourceBankBsb", "015141", "BSB Number", "015-141");

        Assert.Equal("SourceBankBsb", mapping.RequestField);
        Assert.Equal("015141", mapping.RequestValue);
        Assert.Equal("BSB Number", mapping.CbaResponseField);
        Assert.Equal("015-141", mapping.CbaResponseValue);
    }

    [Fact]
    public void FieldMapping_allows_null_request_and_response_values()
    {
        var mapping = new FieldMapping("IntermediaryBankSwiftCode", null, "Intermediary Bank - Bank Code", null);

        Assert.Null(mapping.RequestValue);
        Assert.Null(mapping.CbaResponseValue);
    }

    [Fact]
    public void FieldMapping_equality_is_by_value_since_it_is_a_record()
    {
        var first = new FieldMapping("RecordType", "1", "Record Type", "1");
        var second = new FieldMapping("RecordType", "1", "Record Type", "1");

        Assert.Equal(first, second);
    }

    [Fact]
    public void LineMapping_exposes_the_line_key_and_its_ordered_fields()
    {
        var fields = new List<FieldMapping> { new("RecordType", "1", "Record Type", "1") };

        var line = new LineMapping("detail1", fields);

        Assert.Equal("detail1", line.Line);
        Assert.Same(fields, line.Fields);
        Assert.Single(line.Fields);
    }
}
