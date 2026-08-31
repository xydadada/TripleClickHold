namespace TripleClickHold;

internal static class DelayChooser
{
    internal static int Next(TripleSettings settings, Random? random = null)
    {
        settings = settings.Normalized();
        if (!settings.RandomDelay || settings.MinDelayMs == settings.MaxDelayMs)
            return settings.MinDelayMs;
        return (random ?? Random.Shared).Next(settings.MinDelayMs, settings.MaxDelayMs + 1);
    }
}
