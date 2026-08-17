using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Tests.Shared;

public class MappingUtilitiesTests
{
    [Fact]
    public void AmountToCents_converts_dollars_to_cents()
    {
        Assert.Equal(1000000L, MappingUtilities.AmountToCents(10000.00m));
    }

    [Fact]
    public void AmountToCents_rounds_midpoint_away_from_zero()
    {
        Assert.Equal(1001L, MappingUtilities.AmountToCents(10.005m));
    }

    [Fact]
    public void FixedWidth_truncates_a_value_longer_than_the_width()
    {
        Assert.Equal("HelloWor", MappingUtilities.FixedWidth("HelloWorld", 8));
    }

    [Fact]
    public void FixedWidth_pads_a_value_shorter_than_the_width()
    {
        Assert.Equal("Hi      ", MappingUtilities.FixedWidth("Hi", 8));
    }
}
