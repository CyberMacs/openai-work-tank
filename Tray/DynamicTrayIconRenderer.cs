using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace OpenAIWorkTank.Tray;

public static class DynamicTrayIconRenderer
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool DestroyIcon(IntPtr handle);
    public static Icon Create(int? remaining, string state, bool stale)
    {
        using var bitmap = new Bitmap(64, 64); using var g = Graphics.FromImage(bitmap); g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.Transparent);
        var accent = remaining switch { null => Color.FromArgb(116, 192, 255), >= 80 => Color.FromArgb(44, 210, 147), >= 40 => Color.FromArgb(255, 195, 62), >= 15 => Color.FromArgb(255, 139, 54), _ => Color.FromArgb(244, 86, 93) };
        using var background = new SolidBrush(Color.FromArgb(stale ? 150 : 245, 12, 28, 54)); g.FillEllipse(background, 1, 1, 62, 62);
        var text = state == "missing" ? "X" : state == "auth" ? "!" : remaining?.ToString() ?? "?";
        using var font = new Font("Segoe UI", text.Length > 1 ? 38 : 44, FontStyle.Bold, GraphicsUnit.Pixel);
        using var numberBrush = new SolidBrush(Color.White); g.DrawString(text, font, numberBrush, new RectangleF(2, 0, 60, 46), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        // A small robot badge remains visible without compromising the percentage digits.
        using var robot = new SolidBrush(Color.FromArgb(235, 240, 248)); g.FillRoundedRectangle(robot, new Rectangle(23, 48, 18, 11), 4);
        using var face = new SolidBrush(Color.FromArgb(15, 35, 70)); g.FillRoundedRectangle(face, new Rectangle(26, 51, 12, 5), 2);
        using var eye = new SolidBrush(accent); g.FillEllipse(eye, 28, 52, 2, 2); g.FillEllipse(eye, 35, 52, 2, 2);
        using var antenna = new Pen(accent, 2); g.DrawLine(antenna, 32, 48, 32, 44); g.FillEllipse(eye, 30, 41, 4, 4);
        var handle = bitmap.GetHicon(); try { using var icon = Icon.FromHandle(handle); return (Icon)icon.Clone(); } finally { DestroyIcon(handle); }
    }
    private static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = new GraphicsPath(); path.AddArc(rect.Left, rect.Top, radius, radius, 180, 90); path.AddArc(rect.Right - radius, rect.Top, radius, radius, 270, 90); path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); path.AddArc(rect.Left, rect.Bottom - radius, radius, radius, 90, 90); path.CloseFigure(); g.FillPath(brush, path);
    }
}
