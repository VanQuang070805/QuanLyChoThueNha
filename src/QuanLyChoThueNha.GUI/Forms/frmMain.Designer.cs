namespace QuanLyChoThueNha.GUI.Forms
{
    partial class frmMain
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Panel panelNoidung;

        private MaterialSkin.Controls.MaterialLabel tabDashboard;
        private MaterialSkin.Controls.MaterialLabel tabKhachHang;
        private MaterialSkin.Controls.MaterialLabel tabTaiSan;
        private MaterialSkin.Controls.MaterialLabel tabHopDong;
        private MaterialSkin.Controls.MaterialLabel tabTraNha;
        private MaterialSkin.Controls.MaterialLabel tabTaiKhoan;
        private MaterialSkin.Controls.MaterialLabel tabBaoCao;

        private MaterialSkin.Controls.MaterialButton btnDashboard;
        private MaterialSkin.Controls.MaterialButton btnKhachHangHome;
        private MaterialSkin.Controls.MaterialButton btnKhuVuc;
        private MaterialSkin.Controls.MaterialButton btnToa;
        private MaterialSkin.Controls.MaterialButton btnLoaiCanHo;
        private MaterialSkin.Controls.MaterialButton btnCanHo;
        private MaterialSkin.Controls.MaterialButton btnTienNghi;
        private MaterialSkin.Controls.MaterialButton btnGiaDichVu;
        private MaterialSkin.Controls.MaterialButton btnKhachThue;
        private MaterialSkin.Controls.MaterialButton btnPhieuDatTruoc;
        private MaterialSkin.Controls.MaterialButton btnHopDong;
        private MaterialSkin.Controls.MaterialButton btnGiaHan;
        private MaterialSkin.Controls.MaterialButton btnLoaiHoaDon;
        private MaterialSkin.Controls.MaterialButton btnHoaDon;
        private MaterialSkin.Controls.MaterialButton btnPhieuTraNha;
        private MaterialSkin.Controls.MaterialButton btnViPham;
        private MaterialSkin.Controls.MaterialButton btnQuanLyTaiKhoan;
        private MaterialSkin.Controls.MaterialButton btnQuanLyTaiKhoanKhach;
        private MaterialSkin.Controls.MaterialButton btnBaoCao;
        private MaterialSkin.Controls.MaterialButton btnDangXuat;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.panelNoidung = new System.Windows.Forms.Panel();

            this.tabDashboard = new MaterialSkin.Controls.MaterialLabel();
            this.tabKhachHang = new MaterialSkin.Controls.MaterialLabel();
            this.tabTaiSan = new MaterialSkin.Controls.MaterialLabel();
            this.tabHopDong = new MaterialSkin.Controls.MaterialLabel();
            this.tabTraNha = new MaterialSkin.Controls.MaterialLabel();
            this.tabTaiKhoan = new MaterialSkin.Controls.MaterialLabel();
            this.tabBaoCao = new MaterialSkin.Controls.MaterialLabel();

            this.btnDashboard = new MaterialSkin.Controls.MaterialButton();
            this.btnKhachHangHome = new MaterialSkin.Controls.MaterialButton();
            this.btnKhuVuc = new MaterialSkin.Controls.MaterialButton();
            this.btnToa = new MaterialSkin.Controls.MaterialButton();
            this.btnLoaiCanHo = new MaterialSkin.Controls.MaterialButton();
            this.btnCanHo = new MaterialSkin.Controls.MaterialButton();
            this.btnTienNghi = new MaterialSkin.Controls.MaterialButton();
            this.btnGiaDichVu = new MaterialSkin.Controls.MaterialButton();
            this.btnKhachThue = new MaterialSkin.Controls.MaterialButton();
            this.btnPhieuDatTruoc = new MaterialSkin.Controls.MaterialButton();
            this.btnHopDong = new MaterialSkin.Controls.MaterialButton();
            this.btnGiaHan = new MaterialSkin.Controls.MaterialButton();
            this.btnLoaiHoaDon = new MaterialSkin.Controls.MaterialButton();
            this.btnHoaDon = new MaterialSkin.Controls.MaterialButton();
            this.btnPhieuTraNha = new MaterialSkin.Controls.MaterialButton();
            this.btnViPham = new MaterialSkin.Controls.MaterialButton();
            this.btnQuanLyTaiKhoan = new MaterialSkin.Controls.MaterialButton();
            this.btnQuanLyTaiKhoanKhach = new MaterialSkin.Controls.MaterialButton();
            this.btnBaoCao = new MaterialSkin.Controls.MaterialButton();
            this.btnDangXuat = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();

            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Width = 250;

            this.panelNoidung.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNoidung.BackColor = System.Drawing.SystemColors.Control;

            this.AddGroup(this.tabDashboard, "TONG QUAN");
            this.AddButton(this.btnDashboard, "Dashboard", new System.EventHandler(this.btnDashboard_Click));

            this.AddGroup(this.tabKhachHang, "KHACH HANG");
            this.AddButton(this.btnKhachHangHome, "Trang khach hang", new System.EventHandler(this.btnKhachHangHome_Click));

            this.AddGroup(this.tabTaiSan, "TAI SAN & DANH MUC");
            this.AddButton(this.btnKhuVuc, "Khu vuc", new System.EventHandler(this.btnKhuVuc_Click));
            this.AddButton(this.btnToa, "Toa nha", new System.EventHandler(this.btnToa_Click));
            this.AddButton(this.btnLoaiCanHo, "Loai can ho", new System.EventHandler(this.btnLoaiCanHo_Click));
            this.AddButton(this.btnCanHo, "Can ho", new System.EventHandler(this.btnCanHo_Click));
            this.AddButton(this.btnTienNghi, "Tien nghi", new System.EventHandler(this.btnTienNghi_Click));
            this.AddButton(this.btnGiaDichVu, "Gia dich vu", new System.EventHandler(this.btnGiaDichVu_Click));

            this.AddGroup(this.tabHopDong, "HOP DONG");
            this.AddButton(this.btnKhachThue, "Khach thue", new System.EventHandler(this.btnKhachThue_Click));
            this.AddButton(this.btnPhieuDatTruoc, "Dat truoc", new System.EventHandler(this.btnPhieuDatTruoc_Click));
            this.AddButton(this.btnHopDong, "Hop dong", new System.EventHandler(this.btnHopDong_Click));
            this.AddButton(this.btnGiaHan, "Gia han", new System.EventHandler(this.btnGiaHan_Click));

            this.AddGroup(this.tabTraNha, "THANH TOAN & TRA NHA");
            this.AddButton(this.btnLoaiHoaDon, "Loai hoa don", new System.EventHandler(this.btnLoaiHoaDon_Click));
            this.AddButton(this.btnHoaDon, "Hoa don", new System.EventHandler(this.btnHoaDon_Click));
            this.AddButton(this.btnPhieuTraNha, "Phieu tra nha", new System.EventHandler(this.btnPhieuTraNha_Click));
            this.AddButton(this.btnViPham, "Xu ly vi pham", new System.EventHandler(this.btnViPham_Click));

            this.AddGroup(this.tabTaiKhoan, "QUAN TRI");
            this.AddButton(this.btnQuanLyTaiKhoan, "Tai khoan", new System.EventHandler(this.btnQuanLyTaiKhoan_Click));
            this.AddButton(this.btnQuanLyTaiKhoanKhach, "Tai khoan khach", new System.EventHandler(this.btnQuanLyTaiKhoanKhach_Click));

            this.AddGroup(this.tabBaoCao, "BAO CAO");
            this.AddButton(this.btnBaoCao, "Bao cao & thong ke", new System.EventHandler(this.btnBaoCao_Click));

            this.btnDangXuat.Depth = 0;
            this.btnDangXuat.Size = new System.Drawing.Size(112, 34);
            this.btnDangXuat.Margin = new System.Windows.Forms.Padding(8, 4, 0, 0);
            this.btnDangXuat.Text = "DANG XUAT";
            this.btnDangXuat.HighEmphasis = true;
            this.btnDangXuat.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnDangXuat.UseAccentColor = true;
            this.btnDangXuat.Click += new System.EventHandler(this.btnDangXuat_Click);
            this.panelSidebar.Controls.Add(this.btnDangXuat);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 900);
            this.Controls.Add(this.panelNoidung);
            this.Controls.Add(this.panelSidebar);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quan ly Cho thue Nha";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
        }

    }
}
