namespace TripleClickHold;

internal static class MouseOutput
{
    internal static void Emit(PlannedAction action)
    {
        var flags = (action.Button, action.Action) switch
        {
            (MouseButton.Left, OutputAction.Down) => NativeMethods.MouseEventfLeftDown,
            (MouseButton.Left, OutputAction.Up) => NativeMethods.MouseEventfLeftUp,
            (MouseButton.Right, OutputAction.Down) => NativeMethods.MouseEventfRightDown,
            (MouseButton.Right, OutputAction.Up) => NativeMethods.MouseEventfRightUp,
            _ => 0u
        };
        if (flags != 0)
            NativeMethods.mouse_event(flags, 0, 0, 0, NativeMethods.InjectionMarker);
    }
}
