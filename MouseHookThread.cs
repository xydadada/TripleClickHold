using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace TripleClickHold;

internal sealed class MouseHookThread : IDisposable
{
    private readonly ClickWorker _worker;
    private readonly SettingsState _settings;
    private readonly Action _toggleRequested;
    private readonly ConcurrentQueue<WorkerCommand> _commands = new();
    private readonly AutoResetEvent _wake = new(false);
    private readonly Thread _thread;
    private readonly NativeMethods.LowLevelMouseProc _callback;
    private volatile bool _enabled;
    private volatile bool _stopping;
    private nint _hook;
    private uint _threadId;
    private int _leftDown;
    private int _rightDown;
    private int _sideDown;

    internal MouseHookThread(ClickWorker worker, SettingsState settings, Action toggleRequested)
    {
        _worker = worker;
        _settings = settings;
        _toggleRequested = toggleRequested;
        _callback = HookCallback;
        _thread = new Thread(Run) { IsBackground = true, Name = "TripleClickHold.MouseHook" };
        _thread.Start();
    }

    internal bool IsEnabled => _enabled;
    internal void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
        {
            Interlocked.Exchange(ref _leftDown, 0);
            Interlocked.Exchange(ref _rightDown, 0);
            _worker.Enqueue(new WorkerCommand(WorkerCommandKind.Disable, MouseButton.Left));
        }
    }

    private void Run()
    {
        _threadId = NativeThreadId();
        _hook = NativeMethods.SetWindowsHookEx(NativeMethods.WhMouseLl, _callback, NativeMethods.GetModuleHandle(null), 0);
        if (_hook == 0) return;
        while (!_stopping && NativeMethods.GetMessage(out var msg, 0, 0, 0) > 0)
        {
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
        }
        NativeMethods.UnhookWindowsHookEx(_hook);
        _hook = 0;
        _worker.Enqueue(new WorkerCommand(WorkerCommandKind.Disable, MouseButton.Left));
    }

    private static uint NativeThreadId() => GetCurrentThreadId();

    private nint HookCallback(int code, nuint wParam, nint lParam)
    {
        if (code < 0)
            return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
        var data = Marshal.PtrToStructure<NativeMethods.MsllHookStruct>(lParam);
        if ((data.Flags & NativeMethods.LlMhfInjected) != 0 || data.DwExtraInfo == NativeMethods.InjectionMarker)
            return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
        if (wParam == NativeMethods.WmXButtonDown || wParam == NativeMethods.WmXButtonUp)
        {
            var isDown = wParam == NativeMethods.WmXButtonDown;
            var sideChanged = isDown ? Interlocked.Exchange(ref _sideDown, 1) == 0 : Interlocked.Exchange(ref _sideDown, 0) != 0;
            if (isDown && sideChanged) _toggleRequested();
            return 1;
        }
        if (!_enabled)
            return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
        var button = wParam switch
        {
            (nuint)NativeMethods.WmLButtonDown or (nuint)NativeMethods.WmLButtonUp => MouseButton.Left,
            (nuint)NativeMethods.WmRButtonDown or (nuint)NativeMethods.WmRButtonUp => MouseButton.Right,
            _ => (MouseButton)(-1)
        };
        if ((int)button < 0)
            return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
        var enabledForButton = button == MouseButton.Left ? _settings.Current.LeftEnabled : _settings.Current.RightEnabled;
        if (!enabledForButton)
        {
            ref var disabledState = ref (button == MouseButton.Left ? ref _leftDown : ref _rightDown);
            Interlocked.Exchange(ref disabledState, 0);
            return NativeMethods.CallNextHookEx(_hook, code, wParam, lParam);
        }
        var down = wParam == (nuint)(button == MouseButton.Left ? NativeMethods.WmLButtonDown : NativeMethods.WmRButtonDown);
        ref var state = ref (button == MouseButton.Left ? ref _leftDown : ref _rightDown);
        var changed = down ? Interlocked.Exchange(ref state, 1) == 0 : Interlocked.Exchange(ref state, 0) != 0;
        if (changed)
            _worker.Enqueue(new WorkerCommand(down ? WorkerCommandKind.PhysicalDown : WorkerCommandKind.PhysicalUp, button));
        return 1;
    }

    public void Dispose()
    {
        if (_stopping) return;
        _stopping = true;
        _enabled = false;
        Interlocked.Exchange(ref _sideDown, 0);
        if (_threadId != 0) NativeMethods.PostThreadMessage(_threadId, NativeMethods.WmQuit, 0, 0);
        if (Thread.CurrentThread != _thread) _thread.Join(TimeSpan.FromSeconds(2));
        _wake.Dispose();
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
}
