using System.Drawing;
using System.Windows.Forms;

namespace TripleClickHold;

internal sealed class StatusOverlayForm : Form
{
    private readonly Label _label;
    private readonly System.Windows.Forms.Timer _timer;

    internal StatusOverlayForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        ShowIcon = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.FromArgb(32, 32, 32);
        ForeColor = Color.White;
        Opacity = 0.9;
        Padding = new Padding(10, 5, 10, 5);
        _label = new Label { AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent, Font = new Font("Segoe UI", 10f, FontStyle.Regular) };
        Controls.Add(_label);
        _timer = new System.Windows.Forms.Timer { Interval = 1100 };
        _timer.Tick += (_, _) => { _timer.Stop(); Hide(); };
    }

    internal void ShowStatus(bool enabled)
    {
        _label.Text = enabled ? "连点：已开启" : "连点：已关闭";
        var point = Cursor.Position;
        var working = Screen.FromPoint(point).WorkingArea;
        var size = GetPreferredSize(Size.Empty);
        var x = Math.Clamp(point.X + 14, working.Left, working.Right - size.Width);
        var y = Math.Clamp(point.Y + 18, working.Top, working.Bottom - size.Height);
        Location = new Point(x, y);
        Size = size;
        if (!Visible) Show();
        else { BringToFront(); }
        _timer.Stop();
        _timer.Start();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x00000080 | 0x08000000; // WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0084) { m.Result = new nint(-1); return; } // HTTRANSPARENT: never intercept clicks
        base.WndProc(ref m);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
