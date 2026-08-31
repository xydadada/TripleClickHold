using System.Runtime.InteropServices;

namespace TripleClickHold;

internal static class NativeMethods
{
    internal const int WhMouseLl = 14;
    internal const int WmQuit = 0x0012;
    internal const int WmHotkey = 0x0312;
    internal const int WmApp = 0x8000;
    internal const int WmAppHookReady = WmApp + 1;
    internal const int WmAppHookStopped = WmApp + 2;
    internal const int WmLButtonDown = 0x0201;
    internal const int WmLButtonUp = 0x0202;
    internal const int WmRButtonDown = 0x0204;
    internal const int WmRButtonUp = 0x0205;
    internal const int WmXButtonDown = 0x020B;
    internal const int WmXButtonUp = 0x020C;
    internal const uint XButton1 = 0x0001;
    internal const uint XButton2 = 0x0002;
    internal const uint LlMhfInjected = 0x00000001;
    internal const uint MouseEventfLeftDown = 0x0002;
    internal const uint MouseEventfLeftUp = 0x0004;
    internal const uint MouseEventfRightDown = 0x0008;
    internal const uint MouseEventfRightUp = 0x0010;
    internal const int HotkeyIdToggle = 1;
    internal const int HotkeyIdExit = 2;
    internal const uint ModControl = 0x0002;
    internal const uint ModAlt = 0x0001;
    internal const uint ModShift = 0x0004;
    internal const uint ModWin = 0x0008;
    internal const uint VkF8 = 0x77;
    internal const uint VkF11 = 0x7A;
    internal static readonly nuint InjectionMarker = unchecked((nuint)0x5452434C484F4C44UL);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point { internal int X; internal int Y; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MsllHookStruct
    {
        internal Point Point;
        internal uint MouseData;
        internal uint Flags;
        internal uint Time;
        internal nuint DwExtraInfo;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate nint LowLevelMouseProc(int code, nuint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Msg
    {
        internal nint HWnd;
        internal uint Message;
        internal nuint WParam;
        internal nint LParam;
        internal uint Time;
        internal Point Point;
        internal uint Private;
    }

    [DllImport("user32.dll", SetLastError = true)] internal static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc proc, nint module, uint threadId);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")] internal static extern nint CallNextHookEx(nint hook, int code, nuint wParam, nint lParam);
    [DllImport("kernel32.dll")] internal static extern nint GetModuleHandle(string? name);
    [DllImport("user32.dll")] internal static extern int GetMessage(out Msg msg, nint hWnd, uint min, uint max);
    [DllImport("user32.dll")] internal static extern bool TranslateMessage(ref Msg msg);
    [DllImport("user32.dll")] internal static extern nint DispatchMessage(ref Msg msg);
    [DllImport("user32.dll")] internal static extern void PostQuitMessage(int code);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool PostThreadMessage(uint threadId, uint message, nuint wParam, nint lParam);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool RegisterHotKey(nint hWnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool UnregisterHotKey(nint hWnd, int id);
    [DllImport("user32.dll")] internal static extern bool ShowWindow(nint hWnd, int command);
    [DllImport("user32.dll")] internal static extern void mouse_event(uint flags, uint dx, uint dy, uint data, nuint extraInfo);
}
