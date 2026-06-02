using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuanLyChoThueNha.GUI.Controls
{
    public class RoundedButton : Button
    {
        public int Radius { get; set; } = 10;
        private Color _fillColor = Color.FromArgb(37, 99, 235);
        private Color _borderColor = Color.FromArgb(37, 99, 235);
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                SyncFlatAppearance();
                Invalidate();
            }
        }

        private bool _isHovered = false;
        private bool _isPressed = false;

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            BackColor = Color.FromArgb(37, 99, 235);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TextAlign = ContentAlignment.MiddleCenter;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SyncFlatAppearance();

            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { _isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { _isPressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Erase corners with parent's backcolor
            Color parentColor = Parent != null ? Parent.BackColor : Color.White;
            if (parentColor == Color.Transparent && Parent != null && Parent.Parent != null)
            {
                parentColor = Parent.Parent.BackColor;
            }
            using (var parentBrush = new SolidBrush(parentColor))
            {
                pevent.Graphics.FillRectangle(parentBrush, ClientRectangle);
            }

            // 2. Select fill color based on state
            Color fillColor = _fillColor.A == 0 ? BackColor : _fillColor;
            if (!Enabled)
            {
                fillColor = Color.FromArgb(203, 213, 225); // Disabled grey
            }
            else if (_isPressed)
            {
                fillColor = GetPressedColor(fillColor);
            }
            else if (_isHovered)
            {
                fillColor = GetHoverColor(fillColor);
            }

            // 3. Draw rounded shape and border
            using (var path = CreatePath(ClientRectangle, Radius))
            using (var brush = new SolidBrush(fillColor))
            using (var pen = new Pen(Enabled ? BorderColor : Color.FromArgb(203, 213, 225), 1))
            {
                pevent.Graphics.FillPath(brush, path);
                pevent.Graphics.DrawPath(pen, path);
            }

            // 4. Draw text
            var textRect = ClientRectangle;
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            if (TextAlign == ContentAlignment.MiddleLeft)
            {
                flags |= TextFormatFlags.Left;
                textRect = new Rectangle(ClientRectangle.X + 16, ClientRectangle.Y, ClientRectangle.Width - 20, ClientRectangle.Height);
            }
            else if (TextAlign == ContentAlignment.MiddleRight)
            {
                flags |= TextFormatFlags.Right;
                textRect = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 16, ClientRectangle.Height);
            }
            else
            {
                flags |= TextFormatFlags.HorizontalCenter;
            }

            TextRenderer.DrawText(pevent.Graphics, Text, Font, textRect,
                Enabled ? ForeColor : Color.FromArgb(100, 116, 139),
                flags);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            _fillColor = BackColor;
            UseVisualStyleBackColor = false;
            SyncFlatAppearance();
            Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            SyncFlatAppearance();
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UseVisualStyleBackColor = false;
            SyncFlatAppearance();
            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UseVisualStyleBackColor = false;
            SyncFlatAppearance();
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            UseVisualStyleBackColor = false;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UseVisualStyleBackColor = false;
            SyncFlatAppearance();
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // The control is fully custom painted in OnPaint.
        }

        private void SyncFlatAppearance()
        {
            var baseColor = _fillColor.A == 0 ? BackColor : _fillColor;
            FlatAppearance.MouseOverBackColor = ToFlatAppearanceColor(GetHoverColor(baseColor));
            FlatAppearance.MouseDownBackColor = ToFlatAppearanceColor(GetPressedColor(baseColor));
            FlatAppearance.BorderColor = ToFlatAppearanceColor(BorderColor);
        }

        private Color ToFlatAppearanceColor(Color color)
        {
            if (color.A != 0) return color;
            if (BackColor.A != 0) return BackColor;
            if (Parent != null && Parent.BackColor.A != 0) return Parent.BackColor;
            return Color.White;
        }

        private static Color GetHoverColor(Color color)
        {
            if (color.A == 0) return color;
            int r = Math.Min(255, color.R + 25);
            int g = Math.Min(255, color.G + 25);
            int b = Math.Min(255, color.B + 25);
            return Color.FromArgb(color.A, r, g, b);
        }

        private static Color GetPressedColor(Color color)
        {
            if (color.A == 0) return color;
            int r = Math.Max(0, color.R - 25);
            int g = Math.Max(0, color.G - 25);
            int b = Math.Max(0, color.B - 25);
            return Color.FromArgb(color.A, r, g, b);
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
