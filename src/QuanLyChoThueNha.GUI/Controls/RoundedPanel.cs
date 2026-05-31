using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuanLyChoThueNha.GUI.Controls
{
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 12;
        public Color BorderColor { get; set; } = Color.FromArgb(226, 232, 240);
        public int BorderThickness { get; set; } = 1;

        public RoundedPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Padding = new Padding(12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreatePath(ClientRectangle, Radius))
            using (var brush = new SolidBrush(BackColor))
            using (var pen = new Pen(BorderColor, BorderThickness))
            {
                e.Graphics.FillPath(brush, path);
                if (BorderThickness > 0)
                    e.Graphics.DrawPath(pen, path);
            }
        }

        protected override void OnResize(System.EventArgs eventargs)
        {
            base.OnResize(eventargs);
            using (var path = CreatePath(ClientRectangle, Radius))
                Region = new Region(path);
            Invalidate();
        }

        private static GraphicsPath CreatePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 1 || rect.Height <= 1)
            {
                path.AddRectangle(new Rectangle(0, 0, 1, 1));
                return path;
            }
            var d = radius * 2;
            var r = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
