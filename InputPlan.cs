namespace TripleClickHold;

internal enum MouseButton
{
    Left,
    Right
}

internal enum OutputAction
{
    Down,
    Up
}

internal readonly record struct PlannedAction(MouseButton Button, OutputAction Action);

internal static class InputPlan
{
    internal static PlannedAction[] Begin(MouseButton button, TripleSettings settings)
    {
        var count = Math.Clamp(settings.ClickCount, 1, 20);
        var actions = new List<PlannedAction>(count * 2);
        for (var i = 0; i < count; i++)
        {
            actions.Add(new PlannedAction(button, OutputAction.Down));
            var isLast = i == count - 1;
            if (!isLast || !settings.HoldLastDown)
                actions.Add(new PlannedAction(button, OutputAction.Up));
        }
        return actions.ToArray();
    }

    internal static PlannedAction End(MouseButton button) => new(button, OutputAction.Up);
}
