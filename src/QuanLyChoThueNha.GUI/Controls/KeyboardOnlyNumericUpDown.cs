using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyChoThueNha.GUI.Controls
{
    public class KeyboardOnlyNumericUpDown : NumericUpDown
    {
        private const int WmMouseWheel = 0x020A;
        private bool _pendingBoundsFix;

        public KeyboardOnlyNumericUpDown()
        {
            ThousandsSeparator = true;
            TextAlign = HorizontalAlignment.Left;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            HideSpinnerButtons();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HideSpinnerButtons();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            HideSpinnerButtons();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            HideSpinnerButtons();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handled)
                handled.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            base.OnKeyDown(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmMouseWheel)
                return;

            base.WndProc(ref m);
        }

        private void HideSpinnerButtons()
        {
            if (!IsHandleCreated)
                return;

            ForceEditorBounds();
            if (_pendingBoundsFix)
                return;

            _pendingBoundsFix = true;
            BeginInvoke(new MethodInvoker(delegate
            {
                ForceEditorBounds();
                _pendingBoundsFix = false;
            }));
        }

        private void ForceEditorBounds()
        {
            Control editor = null;
            foreach (Control child in Controls)
            {
                if (child.GetType().Name.IndexOf("UpDownButtons", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    child.Visible = false;
                    child.Enabled = false;
                    child.Width = 0;
                    continue;
                }

                editor = child;
            }

            if (editor != null)
            {
                editor.Location = new Point(0, 0);
                editor.Width = ClientSize.Width;
                editor.Height = ClientSize.Height;
            }
        }
    }
}
