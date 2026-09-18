using System.Net;
using ContentOS.Infrastructure.Knowledge;
using Shouldly;

namespace ContentOS.Application.UnitTests;

public sealed class SsrfAddressPolicyTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.1.2.3")]
    [InlineData("192.168.0.8")]
    [InlineData("172.16.0.1")]
    [InlineData("169.254.169.254")]
    [InlineData("0.0.0.0")]
    public void Private_and_link_local_addresses_are_blocked(string value)
    {
        SsrfAddressPolicy.IsBlocked(IPAddress.Parse(value)).ShouldBeTrue();
    }

    [Fact]
    public void Public_address_is_allowed()
    {
        SsrfAddressPolicy.IsBlocked(IPAddress.Parse("1.1.1.1")).ShouldBeFalse();
    }
}
