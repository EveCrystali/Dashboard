using Dashboard.Core.Abstractions;

namespace Dashboard.Core.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
