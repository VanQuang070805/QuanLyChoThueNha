using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;

namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    /// <summary>
    /// Form ÄÄƒng nháº­p â€” TV1 phá»¥ trÃ¡ch.
    /// Sá»­ dá»¥ng MaterialSkin cho giao diá»‡n hiá»‡n Ä‘áº¡i.
    /// BCrypt xÃ¡c thá»±c máº­t kháº©u á»Ÿ táº§ng BLL (AuthService).
    /// </summary>
    public partial class frmLogin : MaterialForm
    {
        private readonly AuthService _authService;
        private readonly string[] _allowedRoles;
        private readonly string _portalName;

        public frmLogin() : this(null, "Dang nhap he thong")
        {
        }

        public frmLogin(string[] allowedRoles, string portalName)
        {
            InitializeComponent();
            _allowedRoles = allowedRoles;
            _portalName = portalName;
            Text = portalName;
            lblTieuDe.Text = portalName;
            if (_allowedRoles != null)
            {
                if (_allowedRoles.Length == 1 && _allowedRoles[0] == "KhachThue")
                    lblHuongDan.Text = "Dành cho khách thuê. Đăng nhập để xem phiếu đặt trước, hợp đồng và hóa đơn của bạn.";
                else
                    lblHuongDan.Text = "Dành cho Admin/Nhân viên. Đăng nhập để xử lý phiếu đặt trước, hợp đồng, hóa đơn, email và tài khoản khách.";
                btnThoat.Text = "ĐÓNG CỔNG";
            }
            else
            {
                lblHuongDan.Text = "Nhập tài khoản được cấp để truy cập đúng chức năng theo vai trò.";
            }

            // â”€â”€ Cáº¥u hÃ¬nh MaterialSkin â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);

            _authService = new AuthService();
            TryApplyLogo();
        }

        private void TryApplyLogo()
        {
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (!File.Exists(logoPath)) return;

            var logo = new PictureBox
            {
                Image = Image.FromFile(logoPath),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(70, 82),
                Size = new Size(48, 48),
                BackColor = Color.Transparent
            };
            lblTieuDe.Location = new Point(132, lblTieuDe.Location.Y);
            Controls.Add(logo);
            logo.BringToFront();
        }

        // â”€â”€ Xá»­ lÃ½ Ä‘Äƒng nháº­p â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            // BÆ°á»›c 1: Kiá»ƒm tra Ä‘áº§u vÃ o rá»—ng
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text) ||
                string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                MessageBox.Show("Vui lÃ²ng nháº­p Ä‘áº§y Ä‘á»§ tÃªn Ä‘Äƒng nháº­p vÃ  máº­t kháº©u.",
                    "ThÃ´ng bÃ¡o", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // BÆ°á»›c 2 & 3: AuthService xÃ¡c thá»±c (hash BCrypt) â†’ SessionContext
            string loi;
            bool ketQua = _authService.DangNhap(
                txtTenDangNhap.Text.Trim(),
                txtMatKhau.Text,
                out loi);

            if (!ketQua)
            {
                // BÆ°á»›c 2 â€” Dá»¯ liá»‡u SAI: cáº£nh bÃ¡o lá»—i
                MessageBox.Show(loi, "ÄÄƒng nháº­p tháº¥t báº¡i",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMatKhau.Clear();
                txtMatKhau.Focus();
                return;
            }

            // BÆ°á»›c 3 â€” Dá»¯ liá»‡u ÄÃšNG: má»Ÿ form chÃ­nh
            if (_allowedRoles != null && Array.IndexOf(_allowedRoles, SessionContext.VaiTro) < 0)
            {
                _authService.DangXuat();
                MessageBox.Show("Tai khoan nay khong thuoc cong " + _portalName + ".",
                    "Sai cong dang nhap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var frmMain = new frmMain();
            frmMain.Show();
            this.Hide();
            frmMain.FormClosed += (s, args) => this.Close();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (_allowedRoles != null)
                Close();
            else
                Application.Exit();
        }

        // Cho phÃ©p nháº¥n Enter Ä‘á»ƒ Ä‘Äƒng nháº­p
        private void txtMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnDangNhap_Click(sender, e);
        }
    }
}

