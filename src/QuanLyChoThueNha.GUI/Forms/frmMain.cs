using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Auth;
using QuanLyChoThueNha.GUI.Forms.BaoCao;
using QuanLyChoThueNha.GUI.Forms.HopDong;
using QuanLyChoThueNha.GUI.Forms.KhachHang;
using QuanLyChoThueNha.GUI.Forms.NhanVien;
using QuanLyChoThueNha.GUI.Forms.TaiSan;
using QuanLyChoThueNha.GUI.Forms.TraNha;

namespace QuanLyChoThueNha.GUI.Forms
{
    public partial class frmMain : MaterialForm
    {
        public frmMain()
        {
            InitializeComponent();

            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);

            CauHinhMenu();
            HienThiThongTinNguoiDung();

            if (SessionContext.LaAdmin)
                MoForm(new frmDashboard());
            else if (SessionContext.LaNhanVien)
                MoForm(new frmNhanVienHome());
            else if (SessionContext.LaKhachThue)
                MoForm(new frmKhachHangHome());
        }

        private void CauHinhMenu()
        {
            bool laAdmin = SessionContext.LaAdmin;
            bool laNhanVien = SessionContext.LaNhanVien;
            bool laKhach = SessionContext.LaKhachThue;

            SetVisible(tabDashboard, !laKhach, btnDashboard);
            SetVisible(tabKhachHang, laKhach, btnKhachHangHome);

            SetVisible(tabTaiSan, laAdmin || laNhanVien,
                btnKhuVuc, btnToa, btnLoaiCanHo, btnCanHo, btnTienNghi, btnGiaDichVu);

            SetVisible(tabHopDong, laAdmin || laNhanVien,
                btnKhachThue, btnPhieuDatTruoc, btnHopDong, btnGiaHan);

            SetVisible(tabTraNha, laAdmin || laNhanVien,
                btnLoaiHoaDon, btnHoaDon, btnPhieuTraNha, btnViPham);

            SetVisible(tabTaiKhoan, laAdmin, btnQuanLyTaiKhoan, btnQuanLyTaiKhoanKhach);
            SetVisible(tabBaoCao, laAdmin, btnBaoCao);
        }

        private void SetVisible(Control header, bool visible, params Control[] controls)
        {
            header.Visible = visible;
            foreach (var control in controls)
                control.Visible = visible;
        }

        private void HienThiThongTinNguoiDung()
        {
            Text = string.Format("Quan ly Cho thue Nha - {0} [{1}]", SessionContext.HoTen, SessionContext.VaiTro);
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Ban co chac muon dang xuat?", "Xac nhan",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            new AuthService().DangXuat();
            new frmLogin().Show();
            Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MoForm(new frmDashboard());
        }

        private void btnKhachHangHome_Click(object sender, EventArgs e)
        {
            MoForm(new frmKhachHangHome());
        }

        private void btnKhuVuc_Click(object sender, EventArgs e) { MoForm(new frmKhuVuc()); }
        private void btnToa_Click(object sender, EventArgs e) { MoForm(new frmToa()); }
        private void btnLoaiCanHo_Click(object sender, EventArgs e) { MoForm(new frmLoaiCanHo()); }
        private void btnCanHo_Click(object sender, EventArgs e) { MoForm(new frmCanHo()); }
        private void btnTienNghi_Click(object sender, EventArgs e) { MoForm(new frmTienNghi()); }
        private void btnGiaDichVu_Click(object sender, EventArgs e) { MoForm(new frmGiaDichVu()); }

        private void btnKhachThue_Click(object sender, EventArgs e) { MoForm(new frmKhachThue()); }
        private void btnPhieuDatTruoc_Click(object sender, EventArgs e) { MoForm(new frmPhieuDatTruoc()); }
        private void btnHopDong_Click(object sender, EventArgs e) { MoForm(new frmHopDong()); }
        private void btnGiaHan_Click(object sender, EventArgs e) { MoForm(new frmGiaHanHopDong()); }

        private void btnHoaDon_Click(object sender, EventArgs e) { MoForm(new frmHoaDonThanhToan()); }
        private void btnLoaiHoaDon_Click(object sender, EventArgs e) { MoForm(new frmLoaiHoaDon()); }
        private void btnPhieuTraNha_Click(object sender, EventArgs e) { MoForm(new frmPhieuTraNha()); }
        private void btnViPham_Click(object sender, EventArgs e) { MoForm(new frmPhieuXuLyViPham()); }

        private void btnQuanLyTaiKhoan_Click(object sender, EventArgs e) { MoForm(new frmQuanLyTaiKhoan()); }
        private void btnQuanLyTaiKhoanKhach_Click(object sender, EventArgs e) { MoForm(new frmQuanLyTaiKhoanKhach()); }
        private void btnBaoCao_Click(object sender, EventArgs e) { MoForm(new frmBaoCao()); }

        private void MoForm(Form form)
        {
            panelNoidung.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panelNoidung.Controls.Add(form);
            panelNoidung.Tag = form;
            form.Show();
        }
    }
}
