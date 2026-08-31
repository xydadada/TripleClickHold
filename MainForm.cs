using System.Drawing;
using System.Windows.Forms;

namespace TripleClickHold;

internal sealed class MainForm : Form
{
    private readonly SettingsState _settings;
    private readonly ClickWorker _worker;
    private readonly MouseHookThread _hook;
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly StatusOverlayForm _statusOverlay;
    private SettingsForm? _settingsForm;
    private bool _exiting;

    internal MainForm()
    {
        _settings = new SettingsState(SettingsStore.Load());
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        Opacity = 0;
        Size = new Size(1, 1);
        StartPosition = FormStartPosition.Manual;
        Location = new Point(-32000, -32000);
        _worker = new ClickWorker(_settings);
        _hook = new MouseHookThread(_worker, _settings, RequestToggleFromSideButton);
        _statusOverlay = new StatusOverlayForm();
        _toggleItem = new ToolStripMenuItem { CheckOnClick = true };
        _toggleItem.Click += (_, _) => SetEnabled(_toggleItem.Checked);
        var settingsItem = new ToolStripMenuItem("设置…");
        settingsItem.Click += (_, _) => ShowSettings();
        var exitItem = new ToolStripMenuItem("退出");
        exitItem.Click += (_, _) => ExitApplication();
        var menu = new ContextMenuStrip();
        menu.Items.Add(_toggleItem); menu.Items.Add(settingsItem); menu.Items.Add(new ToolStripSeparator()); menu.Items.Add(exitItem);
        _tray = new NotifyIcon { Icon = SystemIcons.Application, Visible = true, ContextMenuStrip = menu };
        _tray.DoubleClick += (_, _) => ShowSettings();
        Shown += (_, _) => BeginInvoke(ShowSettings);
        SetEnabled(false, false);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        TryRegisterHotkeys(_settings.Current, out _);
        if (_settings.Current.StartEnabled) SetEnabled(true);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WmHotkey)
        {
            if (m.WParam.ToInt32() == NativeMethods.HotkeyIdToggle) SetEnabled(!_hook.IsEnabled);
            else if (m.WParam.ToInt32() == NativeMethods.HotkeyIdExit) ExitApplication();
        }
        base.WndProc(ref m);
    }

    private void SetEnabled(bool enabled, bool notify = true)
    {
        _hook.SetEnabled(enabled);
        _toggleItem.Checked = enabled;
        _toggleItem.Text = enabled ? "关闭三倍点击" : "启用三倍点击";
        _tray.Text = _settings.Current.ShowTrayStatus ? $"三倍点击保持器（{(enabled ? "已启用" : "已关闭")}）" : "三倍点击保持器";
        if (notify && IsHandleCreated) _statusOverlay.ShowStatus(enabled);
    }

    private void RequestToggleFromSideButton()
    {
        if (_exiting || IsDisposed) return;
        try
        {
            BeginInvoke(() =>
            {
                if (!_exiting && !IsDisposed) SetEnabled(!_hook.IsEnabled);
            });
        }
        catch (InvalidOperationException) { }
    }

    private void ShowSettings()
    {
        if (_exiting) return;
        if (_settingsForm is { IsDisposed: false } existing)
        {
            if (existing.WindowState == FormWindowState.Minimized) existing.WindowState = FormWindowState.Normal;
            if (!existing.Visible) existing.Show();
            existing.BringToFront();
            existing.Activate();
            return;
        }
        var form = new SettingsForm(_settings.Current, ApplySettings);
        _settingsForm = form;
        form.FormClosed += (_, _) =>
        {
            if (ReferenceEquals(_settingsForm, form)) _settingsForm = null;
            form.Dispose();
        };
        form.Show();
    }

    private string? ApplySettings(TripleSettings value)
    {
        var previous = _settings.Current;
        var wasEnabled = _hook.IsEnabled;
        if (!SettingsStore.Save(value, out var saveError)) return "保存失败：" + saveError;
        if (wasEnabled) _hook.SetEnabled(false);
        _settings.Set(value);
        if (!TryRegisterHotkeys(value, out var hotkeyError))
        {
            _settings.Set(previous); SettingsStore.Save(previous, out _); TryRegisterHotkeys(previous, out _);
            if (wasEnabled) _hook.SetEnabled(true);
            return hotkeyError;
        }
        if (wasEnabled) _hook.SetEnabled(true);
        SetEnabled(_hook.IsEnabled, false);
        return null;
    }

    private bool TryRegisterHotkeys(TripleSettings settings, out string error)
    {
        error = string.Empty;
        if (!IsHandleCreated) return true;
        NativeMethods.UnregisterHotKey(Handle, NativeMethods.HotkeyIdToggle);
        NativeMethods.UnregisterHotKey(Handle, NativeMethods.HotkeyIdExit);
        if (!NativeMethods.RegisterHotKey(Handle, NativeMethods.HotkeyIdToggle, settings.ToggleModifiers, settings.ToggleKey))
        { error = "切换热键注册失败，可能与其他程序冲突。"; return false; }
        if (!NativeMethods.RegisterHotKey(Handle, NativeMethods.HotkeyIdExit, settings.ExitModifiers, settings.ExitKey))
        { NativeMethods.UnregisterHotKey(Handle, NativeMethods.HotkeyIdToggle); error = "退出热键注册失败，可能与其他程序冲突。"; return false; }
        return true;
    }

    private void ExitApplication()
    {
        if (_exiting) return;
        _exiting = true;
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (IsHandleCreated)
            { NativeMethods.UnregisterHotKey(Handle, NativeMethods.HotkeyIdToggle); NativeMethods.UnregisterHotKey(Handle, NativeMethods.HotkeyIdExit); }
            _settingsForm?.Close();
            _statusOverlay.Dispose();
            _tray.Visible = false; _tray.Dispose(); _hook.Dispose(); _worker.Dispose();
        }
        base.Dispose(disposing);
    }
}
