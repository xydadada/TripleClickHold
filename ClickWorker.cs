using System.Collections.Concurrent;

namespace TripleClickHold;

internal enum WorkerCommandKind { PhysicalDown, PhysicalUp, Disable, Stop }
internal readonly record struct WorkerCommand(WorkerCommandKind Kind, MouseButton Button);

internal sealed class ClickWorker : IDisposable
{
    private readonly SettingsState _settings;
    private readonly ConcurrentQueue<WorkerCommand> _queue = new();
    private readonly AutoResetEvent _wake = new(false);
    private readonly Thread _thread;
    private readonly bool[] _held = new bool[2];
    private long _beginGeneration;
    private int _disposed;

    internal ClickWorker(SettingsState settings)
    {
        _settings = settings;
        _thread = new Thread(Run) { IsBackground = true, Name = "TripleClickHold.Output" };
        _thread.Start();
    }

    internal void Enqueue(WorkerCommand command)
    {
        if (Volatile.Read(ref _disposed) == 0)
        {
            if (command.Kind == WorkerCommandKind.PhysicalDown)
                Interlocked.Increment(ref _beginGeneration);
            _queue.Enqueue(command);
            _wake.Set();
        }
    }

    private void Run()
    {
        while (true)
        {
            while (_queue.TryDequeue(out var command))
            {
                if (command.Kind == WorkerCommandKind.Stop)
                {
                    ReleaseAll();
                    return;
                }

                switch (command.Kind)
                {
                    case WorkerCommandKind.PhysicalDown:
                        Begin(command.Button);
                        break;
                    case WorkerCommandKind.PhysicalUp:
                        End(command.Button);
                        break;
                    case WorkerCommandKind.Disable:
                        ReleaseAll();
                        break;
                }
            }
            _wake.WaitOne();
        }
    }

    private void Begin(MouseButton button)
    {
        var index = (int)button;
        if (_held[index]) return;
        var settings = _settings.Current;
        if (button == MouseButton.Left && !settings.LeftEnabled) return;
        if (button == MouseButton.Right && !settings.RightEnabled) return;
        var plan = InputPlan.Begin(button, settings);
        var generation = Volatile.Read(ref _beginGeneration);
        var syntheticDown = false;
        for (var i = 0; i < plan.Length; i++)
        {
            if (generation != Volatile.Read(ref _beginGeneration))
            {
                if (syntheticDown) MouseOutput.Emit(InputPlan.End(button));
                _held[index] = false;
                return;
            }
            var action = plan[i];
            MouseOutput.Emit(action);
            syntheticDown = action.Action == OutputAction.Down;
            if (i + 1 < plan.Length)
            {
                var delay = DelayChooser.Next(settings);
                if (delay > 0) Thread.Sleep(delay);
            }
        }
        if (generation != Volatile.Read(ref _beginGeneration))
        {
            if (syntheticDown) MouseOutput.Emit(InputPlan.End(button));
            _held[index] = false;
            return;
        }
        _held[index] = settings.HoldLastDown;
    }

    private void End(MouseButton button)
    {
        var index = (int)button;
        if (!_held[index]) return;
        MouseOutput.Emit(InputPlan.End(button));
        _held[index] = false;
    }

    private void ReleaseAll()
    {
        for (var i = 0; i < _held.Length; i++)
        {
            if (!_held[i]) continue;
            MouseOutput.Emit(InputPlan.End((MouseButton)i));
            _held[i] = false;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _queue.Enqueue(new WorkerCommand(WorkerCommandKind.Stop, MouseButton.Left));
        _wake.Set();
        if (Thread.CurrentThread != _thread) _thread.Join(TimeSpan.FromSeconds(2));
        _wake.Dispose();
    }
}
