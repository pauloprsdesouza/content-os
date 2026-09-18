using ContentOS.SharedKernel;

namespace ContentOS.Application.UnitTests;

public sealed class SequentialIdGenerator : IIdGenerator
{
    private int _sequence;

    public Guid NewId()
    {
        _sequence++;
        var bytes = new byte[16];
        bytes[15] = (byte)_sequence;
        return new Guid(bytes);
    }
}
