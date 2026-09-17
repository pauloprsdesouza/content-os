namespace ContentOS.Application.Commerce.Ports;

public sealed class CommerceProviderNotConfiguredException : Exception
{
    public CommerceProviderNotConfiguredException(string message)
        : base(message)
    {
    }
}
