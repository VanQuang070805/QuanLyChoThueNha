using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
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
        private FlowLayoutPanel _menuItems;
        private string _activeMenuKey;
        private bool _dangDangXuat;

        private Panel _panelHeader;
        private Button _btnHamburger;
        private Label _lblAppTitle;
        private FlowLayoutPanel _menuFlow;
        private System.Collections.Generic.Dictionary<string, Button> _parentButtons = new System.Collections.Generic.Dictionary<string, Button>();
        private System.Collections.Generic.Dictionary<string, FlowLayoutPanel> _subPanels = new System.Collections.Generic.Dictionary<string, FlowLayoutPanel>();
        private bool _isSidebarCollapsed;

        public Form FormDangNhapNguon { get; set; }
        public bool DangDangXuat { get { return _dangDangXuat; } }

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
            _parentButtons.Clear();
            _subPanels.Clear();

            // Set up sidebar properties
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 250;
            panelSidebar.BackColor = Color.FromArgb(248, 250, 252); // slate-50

            // Create panelHeader
            _panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(226, 232, 240), // slate-200
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            _btnHamburger = new Button
            {
                Dock = DockStyle.Left,
                Width = 50,
                FlatStyle = FlatStyle.Flat,
                Text = "≡",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105), // slate-600
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            _btnHamburger.FlatAppearance.BorderSize = 0;
            _btnHamburger.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240); // slate-200
            _btnHamburger.Click += delegate { ToggleSidebar(); };

            _lblAppTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "QUẢN LÝ THUÊ NHÀ",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59), // slate-800
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            _panelHeader.Controls.Add(_lblAppTitle);
            _panelHeader.Controls.Add(_btnHamburger);

            // Create menuFlow panel
            _menuFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 250, 252), // slate-50
                Margin = new Padding(0),
                Padding = new Padding(0, 10, 0, 10)
            };
            _menuFlow.HorizontalScroll.Maximum = 0;
            _menuFlow.HorizontalScroll.Visible = false;

            panelSidebar.Controls.Add(_menuFlow);
            panelSidebar.Controls.Add(_panelHeader);

            // Add Menu Groups
            if (!laKhach && (laAdmin || qDashboard))
            {
                AddVerticalMenuSingleItem("dashboard", btnDashboard_Click);
            }
            if (laKhach)
            {
                AddVerticalMenuSingleItem("khach", btnKhachHangHome_Click);
            }
            if (laAdmin || (laNhanVien && qTaiSan))
            {
                AddVerticalMenuGroup("taisan", new[]
                {
                    new MenuItemInfo("Khu vực", btnKhuVuc_Click),
                    new MenuItemInfo("Tòa nhà", btnToa_Click),
                    new MenuItemInfo("Loại căn hộ", btnLoaiCanHo_Click),
                    new MenuItemInfo("Căn hộ", btnCanHo_Click),
                    new MenuItemInfo("Tiện nghi", btnTienNghi_Click),
                    new MenuItemInfo("Giá dịch vụ", btnGiaDichVu_Click)
                });
            }
            if (laAdmin || (laNhanVien && qHopDong))
            {
                AddVerticalMenuGroup("hopdong", new[]
                {
                    new MenuItemInfo("Khách thuê", btnKhachThue_Click),
                    new MenuItemInfo("Đặt trước", btnPhieuDatTruoc_Click),
                    new MenuItemInfo("Hợp đồng", btnHopDong_Click),
                    new MenuItemInfo("Gia hạn", btnGiaHan_Click)
                });
            }
            if (laAdmin || (laNhanVien && qThanhToan))
            {
                AddVerticalMenuGroup("thanhtoan", new[]
                {
                    new MenuItemInfo("Hóa đơn", btnHoaDon_Click),
                    new MenuItemInfo("Phiếu trả nhà", btnPhieuTraNha_Click),
                    new MenuItemInfo("Xử lý vi phạm", btnViPham_Click)
                });
            }
            if (laAdmin)
            {
                AddVerticalMenuGroup("quantri", new[]
                {
                    new MenuItemInfo("Tài khoản", btnQuanLyTaiKhoan_Click),
                    new MenuItemInfo("Tài khoản khách", btnQuanLyTaiKhoanKhach_Click),
                    new MenuItemInfo("Phân quyền NV", btnPhanQuyenNhanVien_Click),
                    new MenuItemInfo("Lịch sử Email", btnEmailLog_Click)
                });
            }
            if (laAdmin || (laNhanVien && qBaoCao))
            {
                AddVerticalMenuSingleItem("baocao", btnBaoCao_Click);
            }

            // Separator spacer
            var spacer = new Panel { Width = 250, Height = 20, BackColor = Color.Transparent, Margin = new Padding(0) };
            _menuFlow.Controls.Add(spacer);

            // Add Guide and Logout buttons
            AddVerticalMenuSingleItem("huongdan", delegate { new frmHuongDanSuDung().ShowDialog(this); });
            AddVerticalMenuSingleItem("dangxuat", btnDangXuat_Click);
        }

        private void SetActiveMenu(string activeKey)
        {
            _activeMenuKey = activeKey;
            foreach (var kvp in _parentButtons)
            {
                var key = kvp.Key;
                var btn = kvp.Value as RoundedButton;
                if (btn == null) continue;

                if (key == activeKey)
                {
                    btn.BackColor = Color.FromArgb(219, 234, 254); // blue-100
                    btn.ForeColor = Color.FromArgb(29, 78, 216);   // blue-700
                    btn.BorderColor = Color.FromArgb(147, 197, 253); // blue-300
                }
                else
                {
                    btn.BackColor = Color.Transparent;
                    btn.BorderColor = Color.Transparent;
                    if (key == "dangxuat")
                    {
                        btn.ForeColor = Color.FromArgb(239, 68, 68); // red-500
                    }
                    else if (key == "huongdan")
                    {
                        btn.ForeColor = Color.FromArgb(14, 165, 233); // sky-500
                    }
                    else
                    {
                        btn.ForeColor = Color.FromArgb(71, 85, 105); // slate-600
                    }
                }
            }
        }

        private void AddVerticalMenuSingleItem(string key, EventHandler handler)
        {
            var btn = new RoundedButton
            {
                Width = 226,
                Height = 40,
                Radius = 8,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105), // slate-600
                BackColor = Color.Transparent,
                BorderColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(12, 4, 12, 4)
            };

            if (key == "dangxuat")
            {
                btn.ForeColor = Color.FromArgb(239, 68, 68); // red-500
            }
            else if (key == "huongdan")
            {
                btn.ForeColor = Color.FromArgb(14, 165, 233); // sky-500
            }

            btn.Click += handler;
            btn.Click += delegate { SetActiveMenu(key); };
            _parentButtons[key] = btn;
            _menuFlow.Controls.Add(btn);

            btn.Text = GetTextForGroup(key);
        }

        private void AddVerticalMenuGroup(string key, MenuItemInfo[] children)
        {
            var btnGroup = new RoundedButton
            {
                Width = 226,
                Height = 40,
                Radius = 8,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105), // slate-600
                BackColor = Color.Transparent,
                BorderColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(12, 4, 12, 4)
            };

            _parentButtons[key] = btnGroup;
            _menuFlow.Controls.Add(btnGroup);

            var subPanel = new FlowLayoutPanel
            {
                Width = 226,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(12, 0, 12, 0),
                Padding = new Padding(16, 4, 8, 4), // Indent child items
                BackColor = Color.FromArgb(239, 246, 255), // blue-50 tinted
                Visible = false
            };
            _subPanels[key] = subPanel;

            foreach (var child in children)
            {
                var btnChild = new RoundedButton
                {
                    Width = 194,
                    Height = 32,
                    Radius = 6,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(37, 99, 235), // blue-600
                    BackColor = Color.Transparent,
                    BorderColor = Color.FromArgb(147, 197, 253), // blue-300 border
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 1, 0, 1),
                    Text = "▸  " + child.Text
                };
                // Add hover and click effect
                var localBtn = btnChild;
                localBtn.MouseEnter += delegate
                {
                    localBtn.BackColor = Color.FromArgb(219, 234, 254); // blue-100
                    localBtn.BorderColor = Color.FromArgb(59, 130, 246); // blue-500
                    localBtn.Invalidate();
                };
                localBtn.MouseLeave += delegate
                {
                    localBtn.BackColor = Color.Transparent;
                    localBtn.BorderColor = Color.FromArgb(147, 197, 253); // blue-300
                    localBtn.Invalidate();
                };
                localBtn.MouseDown += delegate
                {
                    localBtn.BackColor = Color.FromArgb(191, 219, 254); // blue-200 pressed
                    localBtn.Invalidate();
                };
                localBtn.MouseUp += delegate
                {
                    localBtn.BackColor = Color.FromArgb(219, 234, 254); // back to hover
                    localBtn.Invalidate();
                };
                btnChild.Click += child.Handler;
                btnChild.Click += delegate { SetActiveMenu(key); };
                subPanel.Controls.Add(btnChild);
            }

            _menuFlow.Controls.Add(subPanel);

            btnGroup.Click += delegate
            {
                if (_isSidebarCollapsed)
                {
                    ToggleSidebar();
                }
                subPanel.Visible = !subPanel.Visible;
                btnGroup.Text = GetTextForGroup(key);
                SetActiveMenu(key);
            };

            btnGroup.Text = GetTextForGroup(key);
        }

        private void ToggleSidebar()
        {
            _isSidebarCollapsed = !_isSidebarCollapsed;
            if (_isSidebarCollapsed)
            {
                panelSidebar.Width = 60;
                _lblAppTitle.Visible = false;
                _panelHeader.Width = 60;
                _menuFlow.Width = 60;

                foreach (var panel in _subPanels.Values)
                {
                    panel.Visible = false;
                }

                foreach (var kvp in _parentButtons)
                {
                    var btn = kvp.Value as RoundedButton;
                    if (btn == null) continue;
                    btn.Width = 44;
                    btn.Margin = new Padding(8, 4, 8, 4);
                    btn.Text = GetIconForGroup(kvp.Key);
                    btn.TextAlign = ContentAlignment.MiddleCenter;
                }
            }
            else
            {
                panelSidebar.Width = 250;
                _lblAppTitle.Visible = true;
                _panelHeader.Width = 250;
                _menuFlow.Width = 250;

                foreach (var kvp in _parentButtons)
                {
                    var btn = kvp.Value as RoundedButton;
                    if (btn == null) continue;
                    btn.Width = 226;
                    btn.Margin = new Padding(12, 4, 12, 4);
                    btn.Text = GetTextForGroup(kvp.Key);
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                }
            }
        }

        private string GetIconForGroup(string key)
        {
            switch (key)
            {
                case "dashboard": return "📊";
                case "khach":     return "👤";
                case "taisan":    return "🏢";
                case "hopdong":   return "📝";
                case "thanhtoan": return "💳";
                case "quantri":   return "⚙️";
                case "baocao":    return "📈";
                case "huongdan":  return "❓";
                case "dangxuat":  return "🚪";
                default:          return "🔹";
            }
        }

        private string GetTextForGroup(string key)
        {
            var isExpanded = !_subPanels.ContainsKey(key) || _subPanels[key].Visible;
            var arrow = isExpanded ? " ▾" : " ▸";
            switch (key)
            {
                case "dashboard": return "📊  Tổng quan";
                case "khach":     return "👤  Khách hàng";
                case "taisan":    return "🏢  Quản lý tài sản" + arrow;
                case "hopdong":   return "📝  Quản lý hợp đồng" + arrow;
                case "thanhtoan": return "💳  Thanh toán & Trả" + arrow;
                case "quantri":   return "⚙️  Quản trị hệ thống" + arrow;
                case "baocao":    return "📈  Báo cáo & Thống kê";
                case "huongdan":  return "❓  Hướng dẫn sử dụng";
                case "dangxuat":  return "🚪  Đăng xuất";
                default:          return "🔹  Menu";
            }
        }

        private void CapNhatKichThuocMenu()
        {
        }

        private bool CoQuyenNhanVien(string maChucNang)
        {
            if (!SessionContext.LaNhanVien) return false;
            return new NhanVienPhanQuyenService().CoQuyen(SessionContext.MaNguoiDung, maChucNang);
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
            Text = string.Format("Quản lý Cho thuê Nhà - {0} [{1}]", SessionContext.HoTen, SessionContext.VaiTro);
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            new AuthService().DangXuat();
            _dangDangXuat = true;
            if (FormDangNhapNguon != null && !FormDangNhapNguon.IsDisposed)
            {
                FormDangNhapNguon.Show();
                FormDangNhapNguon.Activate();
            }
            else
            {
                new frmLogin().Show();
            }
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
        private void btnEmailLog_Click(object sender, EventArgs e) { MoForm(new frmEmailLog()); }
        private void btnBaoCao_Click(object sender, EventArgs e) { MoForm(new frmBaoCao()); }

        private void MoForm(Form form)
        {
            panelNoidung.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ControlBox = false; // Hide nested min/max/close control box buttons!
            form.Dock = DockStyle.Fill;
            panelNoidung.Controls.Add(form);
            panelNoidung.Tag = form;
            form.Show();
        }
    }
}
