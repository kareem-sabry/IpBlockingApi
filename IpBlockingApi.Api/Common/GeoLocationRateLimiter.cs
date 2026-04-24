namespace IpBlockingApi.Common;

/// <summary>
/// Singleton sliding-window rate limiter for the geolocation HTTP client.
/// Max 45 calls per 60-second window — protects the free ipapi.co tier.
/// </summary>
public sealed class GeoLocationRateLimiter
{
    private const int MaxCallsPerWindow = 45;
    private int _callsInWindow = 0;
    private DateTime _windowStart = DateTime.UtcNow;
    private readonly object _lock = new();

    public bool TryConsume()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            if ((now - _windowStart).TotalSeconds >= 60)
            {
                _windowStart = now;
                _callsInWindow = 0;
            }

            if (_callsInWindow >= MaxCallsPerWindow) return false;
            _callsInWindow++;
            return true;
        }
    }
}