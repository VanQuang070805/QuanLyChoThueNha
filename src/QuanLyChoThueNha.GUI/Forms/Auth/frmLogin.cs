using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;

namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    /// <summary>
    /// Form Đăng nhập — TV1 phụ trách.
    /// Sử dụng MaterialSkin cho giao diện hiện đại.
    /// BCrypt xác thực mật khẩu ở tầng BLL (AuthService).
    /// </summary>
    public partial class frmLogin : MaterialForm
    {
        private readonly AuthService _authService;

        public frmLogin()
        {
            InitializeComponent();

            // ── Cấu hình MaterialSkin ────────────────────────────────────
            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);

            _authService = new AuthService();
        }

        // ── Xử lý đăng nhập ─────────────────────────────────────────────
        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            // Bước 1: Kiểm tra đầu vào rỗng
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text) ||
                string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Bước 2 & 3: AuthService xác thực (hash BCrypt) → SessionContext
            string loi;
            bool ketQua = _authService.DangNhap(
                txtTenDangNhap.Text.Trim(),
                txtMatKhau.Text,
                out loi);

            if (!ketQua)
            {
                // Bước 2 — Dữ liệu SAI: cảnh báo lỗi
                MessageBox.Show(loi, "Đăng nhập thất bại",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMatKhau.Clear();
                txtMatKhau.Focus();
                return;
            }

            // Bước 3 — Dữ liệu ĐÚNG: mở form chính
            var frmMain = new frmMain();
            frmMain.Show();
            this.Hide();
            frmMain.FormClosed += (s, args) => this.Close();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Cho phép nhấn Enter để đăng nhập
        private void txtMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnDangNhap_Click(sender, e);
        }
    }
}
