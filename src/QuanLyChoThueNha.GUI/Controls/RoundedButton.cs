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
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            SyncFlatAppearance();

            MouseEnter += (s, e) => { if (Enabled) { _isHovered = true;  Invalidate(); } };
            MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; Invalidate(); };
            MouseDown  += (s, e) => { if (Enabled) { _isPressed = true;  Invalidate(); } };
            MouseUp    += (s, e) => { _isPressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Xóa góc bo tròn bằng màu nền cha
            Color parentBg = Parent != null ? Parent.BackColor : Color.White;
            if (parentBg == Color.Transparent && Parent?.Parent != null)
                parentBg = Parent.Parent.BackColor;
            if (parentBg == Color.Transparent) parentBg = Color.White;
            using (var bgBrush = new SolidBrush(parentBg))
                g.FillRectangle(bgBrush, ClientRectangle);

            // 2. Chọn màu nền, viền và chữ theo trạng thái
            Color baseColor   = _fillColor.A == 0 ? BackColor : _fillColor;
            Color baseBorder  = BorderColor;
            Color baseFore    = ForeColor;

            Color fillColor, borderColor, textColor;

            if (!Enabled)
            {
                // Disabled: làm nhạt màu gốc (pha 60% trắng + 40% màu gốc)
                // Giữ nhận dạng màu của nút nhưng rõ ràng là không thao tác được
                fillColor   = BlendWithWhite(baseColor, 0.60f);
                borderColor = BlendWithWhite(baseBorder, 0.60f);
                textColor   = BlendWithWhite(baseFore, 0.55f);
            }
            else if (_isPressed)
            {
                // Pressed: tối hơn màu gốc
                fillColor   = Darken(baseColor, 30);
                borderColor = Darken(baseBorder, 30);
                textColor   = baseFore;
            }
            else if (_isHovered)
            {
                // Hover: sáng hơn màu gốc
                fillColor   = Lighten(baseColor, 20);
                borderColor = Lighten(baseBorder, 20);
                textColor   = baseFore;
            }
            else
            {
                // Enabled bình thường: màu đầy đủ, rõ nét
                fillColor   = baseColor;
                borderColor = baseBorder;
                textColor   = baseFore;
            }

            // 3. Vẽ hình bo tròn + viền
            using (var path = CreateRoundedPath(ClientRectangle, Radius))
            {
                using (var brush = new SolidBrush(fillColor))
                    g.FillPath(brush, path);
                using (var pen = new Pen(borderColor, 1.5f))
                    g.DrawPath(pen, path);
            }

            // 4. Vẽ chữ căn giữa
            var textRect = ClientRectangle;
            var flags    = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            if (TextAlign == ContentAlignment.MiddleLeft)
            {
                flags |= TextFormatFlags.Left;
                textRect = new Rectangle(textRect.X + 14, textRect.Y, textRect.Width - 18, textRect.Height);
            }
            else if (TextAlign == ContentAlignment.MiddleRight)
            {
                flags |= TextFormatFlags.Right;
                textRect = new Rectangle(textRect.X, textRect.Y, textRect.Width - 14, textRect.Height);
            }
            else
            {
                flags |= TextFormatFlags.HorizontalCenter;
            }

            TextRenderer.DrawText(g, Text, Font, textRect, textColor, flags);
        }

        // ── State events ─────────────────────────────────────────────────────────

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            // Đổi cursor: disabled thì dùng con trỏ mặc định
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            SyncFlatAppearance();
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
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
            // Toàn bộ được vẽ trong OnPaint — không vẽ nền mặc định.
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SyncFlatAppearance()
        {
            var base2 = _fillColor.A == 0 ? BackColor : _fillColor;
            FlatAppearance.MouseOverBackColor = ToOpaque(Lighten(base2, 20));
            FlatAppearance.MouseDownBackColor = ToOpaque(Darken(base2, 30));
            FlatAppearance.BorderColor        = ToOpaque(BorderColor);
        }

        private Color ToOpaque(Color c)
        {
            if (c.A != 0) return c;
            if (BackColor.A != 0) return BackColor;
            if (Parent != null && Parent.BackColor.A != 0) return Parent.BackColor;
            return Color.White;
        }

        /// <summary>Pha màu với trắng: ratio=0 → màu gốc, ratio=1 → trắng.</summary>
        private static Color BlendWithWhite(Color c, float ratio)
        {
            ratio = Math.Max(0f, Math.Min(1f, ratio));
            int r = (int)(c.R + (255 - c.R) * ratio);
            int g = (int)(c.G + (255 - c.G) * ratio);
            int b = (int)(c.B + (255 - c.B) * ratio);
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b)));
        }

        private static Color Lighten(Color c, int amount)
        {
            return Color.FromArgb(
                Math.Min(255, c.R + amount),
                Math.Min(255, c.G + amount),
                Math.Min(255, c.B + amount));
        }

        private static Color Darken(Color c, int amount)
        {
            return Color.FromArgb(
                Math.Max(0, c.R - amount),
                Math.Max(0, c.G - amount),
                Math.Max(0, c.B - amount));
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 1 || rect.Height <= 1)
            {
                path.AddRectangle(new Rectangle(0, 0, 1, 1));
                return path;
            }
            int d = radius * 2;
            var r = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            path.AddArc(r.Left,      r.Top,      d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top,      d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.Left,      r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
