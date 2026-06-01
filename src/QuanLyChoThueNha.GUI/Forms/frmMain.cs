using System;
using System.Drawing;
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
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.GUI.Forms.TaiSan;
using QuanLyChoThueNha.GUI.Forms.TraNha;

namespace QuanLyChoThueNha.GUI.Forms
{
    public partial class frmMain : MaterialForm
    {
        private FlowLayoutPanel _menuGroups;
        private Panel _drawerPanel;
        private FlowLayoutPanel _drawerItems;
        private Label _drawerTitle;
        private string _activeMenuKey;

        private class MenuItemInfo
        {
            public MenuItemInfo(string text, EventHandler handler)
            {
                Text = text;
                Handler = handler;
            }

            public string Text { get; private set; }
            public EventHandler Handler { get; private set; }
        }

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
            Resize += delegate { CapNhatKichThuocMenu(); };

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
            bool qDashboard = CoQuyenNhanVien(NhanVienPhanQuyenService.Dashboard);
            bool qTaiSan = CoQuyenNhanVien(NhanVienPhanQuyenService.TaiSan);
            bool qHopDong = CoQuyenNhanVien(NhanVienPhanQuyenService.HopDong);
            bool qThanhToan = CoQuyenNhanVien(NhanVienPhanQuyenService.ThanhToan);
            bool qBaoCao = CoQuyenNhanVien(NhanVienPhanQuyenService.BaoCao);

            panelSidebar.Controls.Clear();
            panelSidebar.Height = 50;
            panelSidebar.Padding = new Padding(12, 6, 12, 6);
            panelSidebar.FlowDirection = FlowDirection.TopDown;
            panelSidebar.WrapContents = false;
            EnsureDrawer();

            _menuGroups = new FlowLayoutPanel
            {
                Width = Math.Max(900, ClientSize.Width - 24),
                Height = 38,
                WrapContents = false,
                BackColor = Color.FromArgb(25, 118, 210),
                Margin = new Padding(0)
            };

            panelSidebar.Controls.Add(_menuGroups);

            AddGuideButton();

            if (!laKhach && (laAdmin || qDashboard))
                AddMenuGroup("dashboard", "Tong quan", new[] { new MenuItemInfo("Dashboard", btnDashboard_Click) });
            if (laKhach)
                AddMenuGroup("khach", "Khach hang", new[] { new MenuItemInfo("Trang khach hang", btnKhachHangHome_Click) });
            if (laAdmin || (laNhanVien && qTaiSan))
                AddMenuGroup("taisan", "Tai san", new[]
                {
                    new MenuItemInfo("Khu vuc", btnKhuVuc_Click),
                    new MenuItemInfo("Toa nha", btnToa_Click),
                    new MenuItemInfo("Loai can ho", btnLoaiCanHo_Click),
                    new MenuItemInfo("Can ho", btnCanHo_Click),
                    new MenuItemInfo("Tien nghi", btnTienNghi_Click),
                    new MenuItemInfo("Gia dich vu", btnGiaDichVu_Click)
                });
            if (laAdmin || (laNhanVien && qHopDong))
                AddMenuGroup("hopdong", "Hop dong", new[]
                {
                    new MenuItemInfo("Khach thue", btnKhachThue_Click),
                    new MenuItemInfo("Dat truoc", btnPhieuDatTruoc_Click),
                    new MenuItemInfo("Hop dong", btnHopDong_Click),
                    new MenuItemInfo("Gia han", btnGiaHan_Click)
                });
            if (laAdmin || (laNhanVien && qThanhToan))
                AddMenuGroup("thanhtoan", "Thanh toan", new[]
                {
                    new MenuItemInfo("Hoa don", btnHoaDon_Click),
                    new MenuItemInfo("Phieu tra nha", btnPhieuTraNha_Click),
                    new MenuItemInfo("Xu ly vi pham", btnViPham_Click)
                });
            if (laAdmin)
                AddMenuGroup("quantri", "Quan tri", new[]
                {
                    new MenuItemInfo("Tai khoan", btnQuanLyTaiKhoan_Click),
                    new MenuItemInfo("Tai khoan khach", btnQuanLyTaiKhoanKhach_Click),
                    new MenuItemInfo("Phan quyen NV", btnPhanQuyenNhanVien_Click)
                });
            if (laAdmin || (laNhanVien && qBaoCao))
                AddMenuGroup("baocao", "Bao cao", new[] { new MenuItemInfo("Bao cao & thong ke", btnBaoCao_Click) });

            AddLogoutButton();
            CapNhatKichThuocMenu();
        }

        private void CapNhatKichThuocMenu()
        {
            if (_menuGroups == null) return;
            var width = Math.Max(720, ClientSize.Width - 24);
            _menuGroups.Width = width;
            if (_drawerPanel != null) _drawerPanel.Height = panelNoidung.ClientSize.Height;
        }

        private bool CoQuyenNhanVien(string maChucNang)
        {
            if (!SessionContext.LaNhanVien) return false;
            return new NhanVienPhanQuyenService().CoQuyen(SessionContext.MaNguoiDung, maChucNang);
        }

        private void AddMenuGroup(string key, string text, MenuItemInfo[] children)
        {
            var button = CreateMenuButton(text, true);
            button.Tag = children;
            button.Click += delegate
            {
                _activeMenuKey = key;
                ShowMenuItems(children);
                HighlightMenuGroup(button);
            };
            _menuGroups.Controls.Add(button);

            if (string.IsNullOrEmpty(_activeMenuKey))
            {
                _activeMenuKey = key;
                HighlightMenuGroup(button);
            }
        }

        private void ShowMenuItems(MenuItemInfo[] items)
        {
            EnsureDrawer();
            _drawerItems.Controls.Clear();
            foreach (var item in items)
            {
                var button = CreateDrawerButton(item.Text);
                button.Click += delegate(object sender, EventArgs e)
                {
                    HideDrawer();
                    item.Handler(sender, e);
                };
                _drawerItems.Controls.Add(button);
            }
            _drawerPanel.Visible = true;
            _drawerPanel.BringToFront();
        }

        private void EnsureDrawer()
        {
            if (_drawerPanel != null && !_drawerPanel.IsDisposed) return;

            _drawerPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                Visible = false,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 0, 10)
            };

            var drawerRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                BackColor = Color.White
            };
            drawerRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            drawerRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _drawerTitle = new Label
            {
                Text = "Danh mục",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 118, 210),
                Padding = new Padding(18, 0, 12, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            drawerRoot.Controls.Add(_drawerTitle, 0, 0);

            _drawerItems = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 0, 10, 0),
                BackColor = Color.White
            };
            drawerRoot.Controls.Add(_drawerItems, 0, 1);
            _drawerPanel.Controls.Add(drawerRoot);
            panelNoidung.Controls.Add(_drawerPanel);
            _drawerPanel.BringToFront();
        }

        private void HideDrawer()
        {
            if (_drawerPanel != null) _drawerPanel.Visible = false;
        }

        private Button CreateDrawerButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Width = 246,
                Height = 42,
                Margin = new Padding(4, 4, 4, 4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(219, 234, 254);
            return button;
        }

        private void HighlightMenuGroup(Button activeButton)
        {
            foreach (Control control in _menuGroups.Controls)
            {
                var button = control as Button;
                if (button == null) continue;
                button.BackColor = button == activeButton ? Color.White : Color.FromArgb(25, 118, 210);
                button.ForeColor = button == activeButton ? Color.FromArgb(25, 118, 210) : Color.White;
            }
        }

        private Button CreateMenuButton(string text, bool group)
        {
            var button = new Button
            {
                Text = group ? text + "  v" : text,
                Width = group ? 128 : 142,
                Height = 30,
                Margin = new Padding(3, 2, 3, 2),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = group ? Color.FromArgb(25, 118, 210) : Color.FromArgb(21, 101, 192),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            button.FlatAppearance.BorderSize = group ? 1 : 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(187, 222, 251);
            button.FlatAppearance.MouseOverBackColor = group ? Color.FromArgb(227, 242, 253) : Color.FromArgb(30, 136, 229);
            return button;
        }

        private void AddLogoutButton()
        {
            var button = CreateMenuButton("Dang xuat", true);
            button.Text = "Đăng xuất";
            button.Width = 112;
            button.BackColor = Color.FromArgb(198, 40, 40);
            button.Click += btnDangXuat_Click;
            _menuGroups.Controls.Add(button);
        }

        private void AddGuideButton()
        {
            var button = CreateMenuButton("Hướng dẫn tôi", true);
            button.Text = "Hướng dẫn tôi";
            button.Width = 132;
            button.BackColor = Color.FromArgb(14, 165, 233);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(186, 230, 253);
            button.Click += delegate { new frmHuongDanSuDung().ShowDialog(this); };
            _menuGroups.Controls.Add(button);
        }

        private void SetVisible(Control header, bool visible, params Control[] controls)
        {
            header.Visible = visible;
            foreach (var control in controls)
                control.Visible = visible;
        }

        private void AddGroup(MaterialLabel label, string text)
        {
            label.AutoSize = false;
            label.Depth = 0;
            label.Font = new System.Drawing.Font("Roboto", 9F, System.Drawing.FontStyle.Bold);
            label.ForeColor = System.Drawing.Color.FromArgb(230, 255, 255, 255);
            label.Size = new System.Drawing.Size(110, 34);
            label.Margin = new Padding(10, 4, 4, 0);
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label.Text = text;
            panelSidebar.Controls.Add(label);
        }

        private void AddButton(MaterialButton button, string text, EventHandler handler)
        {
            button.Depth = 0;
            button.Size = new System.Drawing.Size(126, 34);
            button.Margin = new Padding(4, 4, 4, 0);
            button.Text = text;
            button.Type = MaterialButton.MaterialButtonType.Text;
            button.HighEmphasis = false;
            button.UseAccentColor = false;
            button.Click += handler;
            panelSidebar.Controls.Add(button);
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
        private void btnPhanQuyenNhanVien_Click(object sender, EventArgs e) { MoForm(new frmPhanQuyenNhanVien()); }
        private void btnBaoCao_Click(object sender, EventArgs e) { MoForm(new frmBaoCao()); }

        private void MoForm(Form form)
        {
            panelNoidung.Controls.Clear();
            _drawerPanel = null;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            var materialForm = form as MaterialForm;
            if (materialForm != null)
            {
                materialForm.FormStyle = MaterialForm.FormStyles.StatusAndActionBar_None;
                materialForm.Padding = new Padding(0);
            }
            form.Dock = DockStyle.Fill;
            panelNoidung.Controls.Add(form);
            panelNoidung.Tag = form;
            form.Show();
            EnsureDrawer();
            HideDrawer();
        }
    }
}
