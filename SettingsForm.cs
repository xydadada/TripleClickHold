using System.Drawing;
using System.Windows.Forms;

namespace TripleClickHold;

internal sealed class SettingsForm : Form
{
    private readonly Func<TripleSettings, string?> _apply;
    private readonly NumericUpDown _clickCount;
    private readonly NumericUpDown _minDelay;
    private readonly NumericUpDown _maxDelay;
    private readonly CheckBox _randomDelay;
    private readonly CheckBox _holdLast;
    private readonly CheckBox _left;
    private readonly CheckBox _right;
    private readonly CheckBox _startEnabled;
    private readonly CheckBox _showTray;
    private readonly ComboBox _toggleKey;
    private readonly ComboBox _exitKey;
    private readonly CheckBox _toggleCtrl;
    private readonly CheckBox _toggleAlt;
    private readonly CheckBox _toggleShift;
    private readonly CheckBox _exitCtrl;
    private readonly CheckBox _exitAlt;
    private readonly CheckBox _exitShift;
    private readonly Label _preview;
    private readonly Label _status;

    internal SettingsForm(TripleSettings initial, Func<TripleSettings, string?> apply)
    {
        _apply = apply;
        Text = "三倍点击保持器 - 设置";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(620, 650);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 2, RowCount = 9 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        for (var i = 0; i < 9; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var title = new Label { Text = "三倍点击保持器设置", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 12) };
        root.Controls.Add(title, 0, 0); root.SetColumnSpan(title, 2);

        _clickCount = Numeric(1, 20, initial.ClickCount);
        AddRow(root, 1, "每次按下输出点击次数", _clickCount);
        var delayPanel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        _minDelay = Numeric(0, 100, initial.MinDelayMs);
        _maxDelay = Numeric(0, 100, initial.MaxDelayMs);
        _randomDelay = new CheckBox { Text = "在最小–最大范围内随机", AutoSize = true, Checked = initial.RandomDelay };
        delayPanel.Controls.Add(new Label { Text = "最小", AutoSize = true, Margin = new Padding(0, 7, 3, 0) });
        delayPanel.Controls.Add(_minDelay);
        delayPanel.Controls.Add(new Label { Text = "最大", AutoSize = true, Margin = new Padding(8, 7, 3, 0) });
        delayPanel.Controls.Add(_maxDelay);
        delayPanel.Controls.Add(_randomDelay);
        AddRow(root, 2, "模拟事件间隔（毫秒）", delayPanel);

        var behavior = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        _holdLast = new CheckBox { Text = "长按时保持最后一次按下", AutoSize = true, Checked = initial.HoldLastDown };
        behavior.Controls.Add(_holdLast);
        AddRow(root, 3, "长按行为", behavior);

        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        _left = new CheckBox { Text = "左键", AutoSize = true, Checked = initial.LeftEnabled };
        _right = new CheckBox { Text = "右键", AutoSize = true, Checked = initial.RightEnabled };
        buttons.Controls.Add(_left); buttons.Controls.Add(_right);
        AddRow(root, 4, "启用的鼠标键", buttons);

        var toggle = KeyPanel(initial.ToggleModifiers, initial.ToggleKey, out _toggleKey, out _toggleCtrl, out _toggleAlt, out _toggleShift);
        AddRow(root, 5, "切换热键", toggle);
        var exit = KeyPanel(initial.ExitModifiers, initial.ExitKey, out _exitKey, out _exitCtrl, out _exitAlt, out _exitShift);
        AddRow(root, 6, "退出热键", exit);

        var startup = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        _startEnabled = new CheckBox { Text = "程序启动时立即启用（默认关闭）", AutoSize = true, Checked = initial.StartEnabled };
        _showTray = new CheckBox { Text = "在托盘提示启用状态", AutoSize = true, Checked = initial.ShowTrayStatus };
        startup.Controls.Add(_startEnabled); startup.Controls.Add(_showTray);
        AddRow(root, 7, "启动与提示", startup);

        _preview = new Label { AutoSize = true, ForeColor = Color.DarkSlateGray, Margin = new Padding(0, 10, 0, 10) };
        root.Controls.Add(new Label { Text = "当前方案", AutoSize = true, Margin = new Padding(0, 10, 0, 10) }, 0, 8);
        root.Controls.Add(_preview, 1, 8);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
        var cancel = new Button { Text = "取消", AutoSize = true };
        var defaults = new Button { Text = "恢复默认", AutoSize = true };
        var save = new Button { Text = "保存并应用", AutoSize = true, DialogResult = DialogResult.None };
        bottom.Controls.Add(cancel); bottom.Controls.Add(save); bottom.Controls.Add(defaults);
        Controls.Add(bottom);
        AcceptButton = save; CancelButton = cancel;
        cancel.Click += (_, _) => Close();

        _clickCount.ValueChanged += (_, _) => UpdatePreview();
        _minDelay.ValueChanged += (_, _) => { if (_maxDelay.Value < _minDelay.Value) _maxDelay.Value = _minDelay.Value; UpdatePreview(); };
        _maxDelay.ValueChanged += (_, _) => { if (_minDelay.Value > _maxDelay.Value) _minDelay.Value = _maxDelay.Value; UpdatePreview(); };
        _randomDelay.CheckedChanged += (_, _) => UpdatePreview();
        _holdLast.CheckedChanged += (_, _) => UpdatePreview();
        defaults.Click += (_, _) => LoadValues(TripleSettings.Default);
        save.Click += (_, _) => SaveAndClose();
        _status = new Label { AutoSize = true, ForeColor = Color.Firebrick, Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft };
        Controls.Add(_status);
        UpdatePreview();
    }

    private static NumericUpDown Numeric(int min, int max, int value) => new() { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Width = 90 };

    private static void AddRow(TableLayoutPanel root, int row, string label, Control control)
    {
        root.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 7, 8, 7) }, 0, row);
        control.Margin = new Padding(0, 4, 0, 4);
        root.Controls.Add(control, 1, row);
    }

    private static FlowLayoutPanel KeyPanel(uint modifiers, uint key, out ComboBox combo, out CheckBox ctrl, out CheckBox alt, out CheckBox shift)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        ctrl = new CheckBox { Text = "Ctrl", AutoSize = true, Checked = (modifiers & NativeMethods.ModControl) != 0 };
        alt = new CheckBox { Text = "Alt", AutoSize = true, Checked = (modifiers & NativeMethods.ModAlt) != 0 };
        shift = new CheckBox { Text = "Shift", AutoSize = true, Checked = (modifiers & NativeMethods.ModShift) != 0 };
        combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
        combo.Items.AddRange(KeyList.Items.Select(x => x.Name).Cast<object>().ToArray());
        combo.SelectedIndex = KeyList.IndexOf(key);
        panel.Controls.Add(ctrl); panel.Controls.Add(alt); panel.Controls.Add(shift); panel.Controls.Add(combo);
        return panel;
    }

    private void LoadValues(TripleSettings value)
    {
        value = value.Normalized();
        _clickCount.Value = value.ClickCount; _minDelay.Value = value.MinDelayMs; _maxDelay.Value = value.MaxDelayMs; _randomDelay.Checked = value.RandomDelay; _holdLast.Checked = value.HoldLastDown;
        _left.Checked = value.LeftEnabled; _right.Checked = value.RightEnabled; _startEnabled.Checked = value.StartEnabled; _showTray.Checked = value.ShowTrayStatus;
        SetKey(_toggleKey, _toggleCtrl, _toggleAlt, _toggleShift, value.ToggleModifiers, value.ToggleKey);
        SetKey(_exitKey, _exitCtrl, _exitAlt, _exitShift, value.ExitModifiers, value.ExitKey);
        _status.Text = string.Empty; UpdatePreview();
    }

    private static void SetKey(ComboBox combo, CheckBox ctrl, CheckBox alt, CheckBox shift, uint modifiers, uint key)
    {
        combo.SelectedIndex = KeyList.IndexOf(key); ctrl.Checked = (modifiers & NativeMethods.ModControl) != 0; alt.Checked = (modifiers & NativeMethods.ModAlt) != 0; shift.Checked = (modifiers & NativeMethods.ModShift) != 0;
    }

    private uint Modifiers(CheckBox ctrl, CheckBox alt, CheckBox shift) => (ctrl.Checked ? NativeMethods.ModControl : 0) | (alt.Checked ? NativeMethods.ModAlt : 0) | (shift.Checked ? NativeMethods.ModShift : 0);

    private TripleSettings ReadValues() => new TripleSettings(
        (int)_clickCount.Value, (int)_minDelay.Value, (int)_maxDelay.Value, _randomDelay.Checked, _holdLast.Checked, _left.Checked, _right.Checked,
        Modifiers(_toggleCtrl, _toggleAlt, _toggleShift), KeyList.Items[_toggleKey.SelectedIndex].Value,
        Modifiers(_exitCtrl, _exitAlt, _exitShift), KeyList.Items[_exitKey.SelectedIndex].Value,
        _startEnabled.Checked, _showTray.Checked).Normalized();

    private void SaveAndClose()
    {
        var value = ReadValues();
        if (value.ToggleKey == value.ExitKey && value.ToggleModifiers == value.ExitModifiers)
        {
            _status.Text = "切换热键和退出热键不能完全相同。";
            return;
        }
        var error = _apply(value);
        if (error is not null) { _status.Text = error; return; }
        Close();
    }

    private void UpdatePreview()
    {
        var count = (int)_clickCount.Value;
        var pieces = string.Join(" → ", Enumerable.Repeat("按下-松开", Math.Max(0, count - 1)));
        var sequence = _holdLast.Checked ? (pieces.Length == 0 ? "按下（保持）" : pieces + " → 按下（保持）") : string.Join(" → ", Enumerable.Repeat("按下-松开", count));
        var interval = _randomDelay.Checked ? $"{_minDelay.Value}–{_maxDelay.Value} ms，每次随机" : $"{_minDelay.Value} ms，固定";
        _preview.Text = sequence + Environment.NewLine + "间隔：" + interval;
    }
}
