namespace TripleClickHold;

internal static class SelfCheck
{
    internal static int Run()
    {
        var failures = new List<string>();
        var defaults = TripleSettings.Default;
        Check(InputPlan.Begin(MouseButton.Left, defaults).Length == 5, "left begin has five events", failures);
        Check(InputPlan.Begin(MouseButton.Right, defaults).Length == 5, "right begin has five events", failures);
        var left = InputPlan.Begin(MouseButton.Left, defaults);
        Check(left[0] == new PlannedAction(MouseButton.Left, OutputAction.Down), "left starts down", failures);
        Check(left[1] == new PlannedAction(MouseButton.Left, OutputAction.Up), "left first up", failures);
        Check(left[2] == new PlannedAction(MouseButton.Left, OutputAction.Down), "left second down", failures);
        Check(left[3] == new PlannedAction(MouseButton.Left, OutputAction.Up), "left second up", failures);
        Check(left[4] == new PlannedAction(MouseButton.Left, OutputAction.Down), "left third down held", failures);
        Check(InputPlan.End(MouseButton.Left) == new PlannedAction(MouseButton.Left, OutputAction.Up), "left release", failures);
        Check(InputPlan.End(MouseButton.Right) == new PlannedAction(MouseButton.Right, OutputAction.Up), "right release", failures);
        Check(InputPlan.Begin(MouseButton.Left, defaults with { ClickCount = 1 }).Length == 1, "single click holds down", failures);
        Check(InputPlan.Begin(MouseButton.Right, defaults with { ClickCount = 20 }).Length == 39, "twenty clicks are bounded", failures);
        Check(InputPlan.Begin(MouseButton.Left, defaults with { HoldLastDown = false }).Length == 6, "no-hold emits full clicks", failures);
        Check((defaults with { ClickCount = 99, MinDelayMs = -5, MaxDelayMs = 2 }).Normalized() is { ClickCount: 20, MinDelayMs: 0, MaxDelayMs: 2 }, "settings normalization", failures);
        Check((defaults with { MinDelayMs = 20, MaxDelayMs = 5 }).Normalized() is { MinDelayMs: 20, MaxDelayMs: 20 }, "delay range normalization", failures);
        Check(DelayChooser.Next(defaults with { MinDelayMs = 7, MaxDelayMs = 30 }) == 7, "fixed delay", failures);
        var seeded = new Random(12345);
        var randomDelays = Enumerable.Range(0, 100).Select(_ => DelayChooser.Next(defaults with { MinDelayMs = 7, MaxDelayMs = 30, RandomDelay = true }, seeded)).ToArray();
        Check(randomDelays.All(value => value is >= 7 and <= 30), "random delay bounds", failures);
        Check(randomDelays.Distinct().Count() > 1, "random delay varies", failures);
        Check(NativeMethods.InjectionMarker != 0, "injection marker", failures);
        if (failures.Count != 0)
        {
            Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
            return 1;
        }
        Console.WriteLine("PASS: triple-click-hold offline checks");
        return 0;
    }

    private static void Check(bool value, string name, ICollection<string> failures)
    {
        if (!value) failures.Add("FAIL: " + name);
    }
}
