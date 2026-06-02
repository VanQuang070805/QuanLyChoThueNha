using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuanLyChoThueNha.GUI.Controls
{
    public class PlaceholderTextBox : TextBox
    {
        private const int EmSetcuebanner = 0x1501;
        private string _placeholder = string.Empty;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value ?? string.Empty;
                if (IsHandleCreated) ApplyPlaceholder();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyPlaceholder();
        }

        private void ApplyPlaceholder()
        {
            SendMessage(Handle, EmSetcuebanner, (IntPtr)1, _placeholder);
        }
    }
}
