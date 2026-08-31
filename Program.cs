using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TripleClickHold;

internal static class Program
{
    private const string MutexName = "Local\\TripleClickHold-1A3E8D0C";

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(a => string.Equals(a, "--self-check", StringComparison.OrdinalIgnoreCase)))
            return SelfCheck.Run();

        var renderIndex = Array.FindIndex(args, a => string.Equals(a, "--render-settings", StringComparison.OrdinalIgnoreCase));
        if (renderIndex >= 0 && renderIndex + 1 < args.Length)
        {
            ApplicationConfiguration.Initialize();
            using var settings = new SettingsForm(TripleSettings.Default, _ => null);
            settings.StartPosition = FormStartPosition.Manual;
            settings.Location = new Point(-32000, -32000);
            settings.ShowInTaskbar = false;
            CreateControls(settings);
            NativeMethods.ShowWindow(settings.Handle, 4);
            Application.DoEvents();
            settings.PerformLayout();
            using var image = new Bitmap(settings.Width, settings.Height);
            settings.DrawToBitmap(image, new Rectangle(Point.Empty, settings.Size));
            image.Save(args[renderIndex + 1], ImageFormat.Png);
            NativeMethods.ShowWindow(settings.Handle, 0);
            return 0;
        }

        using var mutex = new Mutex(true, MutexName, out var created);
        if (!created)
            return 0;

        ApplicationConfiguration.Initialize();
        using var form = new MainForm();
        Application.Run(form);
        return 0;
    }

    private static void CreateControls(Control parent)
    {
        parent.CreateControl();
        foreach (Control child in parent.Controls)
            CreateControls(child);
    }
}
