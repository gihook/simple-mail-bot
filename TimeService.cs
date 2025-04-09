public class TimeService
{
    private readonly DateTime _startTimestamp;

    public TimeService()
    {
        _startTimestamp = DateTime.UtcNow;
    }

    public DateTime GetStartTimestamp() => _startTimestamp;
}
