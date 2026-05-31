using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuanLyChoThueNha.GUI.Controls
{
    public class RoundedButton : Button
    {
        public int Radius { get; set; } = 10;
        public Color BorderColor { get; set; } = Color.FromArgb(37, 99, 235);

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(37, 99, 235);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TextAlign = ContentAlignment.MiddleCenter;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreatePath(ClientRectangle, Radius))
            using (var brush = new SolidBrush(Enabled ? BackColor : Color.FromArgb(203, 213, 225)))
            using (var pen = new Pen(Enabled ? BorderColor : Color.FromArgb(203, 213, 225), 1))
            {
                pevent.Graphics.FillPath(brush, path);
                pevent.Graphics.DrawPath(pen, path);
            }

            TextRenderer.DrawText(pevent.Graphics, Text, Font, ClientRectangle,
                Enabled ? ForeColor : Color.FromArgb(100, 116, 139),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
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
