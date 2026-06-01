using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.GUI.Forms.HopDong;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.Auth
{
    /// <summary>
    /// Giao dien quan ly tai khoan KhachThue — danh rieng cho Admin.
    /// Hien thi thong tin khach + tai khoan; ho tro xem, sua thong tin khach, khoa/mo khoa.
    /// </summary>
    public class frmQuanLyTaiKhoanKhach : MaterialForm
    {
        // ViewModel noi bo gop thong tin KhachThue + TaiKhoan
        private class KhachTaiKhoanVM
        {
            public string MaKhach      { get; set; }
            public string MaTaiKhoan   { get; set; }
            public string TenDangNhap  { get; set; }
            public string MatKhau      { get; set; }
            public string HoTen        { get; set; }
            public string SoCMND       { get; set; }
            public string DiaChi       { get; set; }
            public DateTime? NgaySinh  { get; set; }
            public string Email        { get; set; }
            public string SoDienThoai  { get; set; }
            public bool   TrangThai    { get; set; }
        }

        private enum FormMode { View, Editing }

        private readonly KhachThueService _khachSvc  = new KhachThueService();
        private readonly TaiKhoanService  _tkSvc     = new TaiKhoanService();
        private FormMode _mode = FormMode.View;

        private DataGridView grid;
        private PlaceholderTextBox txtSearch;
        private Label lblStatus;

        private MaterialTextBox txtMaKhach;
        private MaterialTextBox txtMaTaiKhoan;
        private MaterialTextBox txtTenDangNhap;
        private MaterialTextBox txtHoTen;
        private MaterialTextBox txtCmnd;
        private MaterialTextBox txtDiaChi;
        private DateTimePicker  dtpNgaySinh;
        private MaterialTextBox txtEmail;
        private MaterialTextBox txtSdt;
        private CheckBox        chkTrangThai;

        private MaterialButton btnSua;
        private MaterialButton btnThemMoi;
        private MaterialButton btnLamMoi;
        private MaterialButton btnKhoa;
        private MaterialButton btnMoKhoa;
        private Label _lblMsg;

        private List<KhachTaiKhoanVM> _allData = new List<KhachTaiKhoanVM>();

        public frmQuanLyTaiKhoanKhach()
        {
            Text = "Quản lý tài khoản khách thuê";
            Size = new Size(1180, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BuildLayout();
            ReloadData();
        }

        // ── Build layout ─────────────────────────────────────────────────────────

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2,
                Padding = new Padding(12, 18, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            // Left: search + grid + status
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

            var searchPanel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                BackColor = Color.White,
                BorderColor = Color.FromArgb(226, 232, 240),
                Padding = new Padding(14, 8, 14, 8)
            };
            var searchLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            searchLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            searchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            searchLayout.Controls.Add(new Label
            {
                Text = "Tìm kiếm",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99)
            }, 0, 0);
            txtSearch = new PlaceholderTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                Placeholder = "Tìm theo họ tên, CMND, tên đăng nhập"
            };
            txtSearch.TextChanged += delegate { FilterGrid(); };
            searchLayout.Controls.Add(txtSearch, 0, 1);
            searchPanel.Controls.Add(searchLayout);
            left.Controls.Add(searchPanel, 0, 0);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, MultiSelect = false, RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            grid.SelectionChanged += Grid_SelectionChanged;
            left.Controls.Add(grid, 0, 1);

            lblStatus = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            left.Controls.Add(lblStatus, 0, 2);

            // Right: detail panel
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1,
                Padding = new Padding(12, 0, 0, 0)
            };

            txtMaKhach     = Txt("(tự sinh)", true);
            txtMaTaiKhoan  = Txt("(tự sinh)", true);
            txtTenDangNhap = Txt("Tên đăng nhập", true);  // khong cho doi TDN o day
            txtHoTen       = Txt("Họ tên", false);
            txtCmnd        = Txt("CMND/CCCD", false);
            txtDiaChi      = Txt("Địa chỉ", false);
            dtpNgaySinh    = new DateTimePicker
            {
                Dock = DockStyle.Top, Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy", MaxDate = DateTime.Today,
                Value = DateTime.Today.AddYears(-18), Enabled = false
            };
            txtEmail = Txt("Email", false);
            txtSdt   = Txt("Số điện thoại", false);
            chkTrangThai = new CheckBox
            {
                Dock = DockStyle.Top, Text = "Hoạt động", Height = 28, Enabled = false
            };

            // Bat dau o View mode - tat tat ca truong sua
            SetEditorsEnabled(false);

            AddField(right, "Mã khách",      txtMaKhach);
            AddField(right, "Mã tài khoản",  txtMaTaiKhoan);
            AddField(right, "Tên đăng nhập", txtTenDangNhap);
            AddField(right, "Họ tên *",      txtHoTen);
            AddField(right, "CMND/CCCD *",   txtCmnd);
            AddField(right, "Địa chỉ",       txtDiaChi);
            AddField(right, "Ngày sinh",     dtpNgaySinh);
            AddField(right, "Email",         txtEmail);
            AddField(right, "Số điện thoại", txtSdt);
            AddField(right, "Trạng thái",    chkTrangThai);

            var commands = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            btnSua    = Btn("Sửa thông tin", BtnSua_Click);
            btnThemMoi = Btn("Thêm mới", BtnThemMoi_Click);
            btnLamMoi = Btn("Làm mới",       BtnLamMoi_Click);
            btnKhoa   = Btn("Khóa",          BtnKhoa_Click);
            btnMoKhoa = Btn("Mở khóa",       BtnMoKhoa_Click);
            commands.Controls.Add(btnSua);
            commands.Controls.Add(btnThemMoi);
            commands.Controls.Add(btnLamMoi);
            commands.Controls.Add(btnKhoa);
            commands.Controls.Add(btnMoKhoa);
            right.Controls.Add(commands);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);

            _lblMsg = new Label
            {
                Dock = DockStyle.Bottom, Height = 34, AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Font = new Font("Segoe UI", 9f), Visible = false
            };
            Controls.Add(root);
            Controls.Add(_lblMsg);
        }

        private void ShowMsg(string msg, bool isError = true)
        {
            _lblMsg.Text = msg;
            _lblMsg.BackColor = isError ? Color.MistyRose : Color.FromArgb(220, 240, 255);
            _lblMsg.ForeColor = isError ? Color.DarkRed : Color.Navy;
            _lblMsg.Visible = true;
        }

        private void HideMsg() { _lblMsg.Visible = false; }

        // ── Mode management ──────────────────────────────────────────────────────

        private void EnterViewMode()
        {
            _mode = FormMode.View;
            HideMsg(); SetEditorsEnabled(false);
            btnSua.Text    = "Sửa thông tin";
            btnLamMoi.Text = "Làm mới";
            UpdateActionButtons();
        }

        private void EnterEditMode()
        {
            _mode = FormMode.Editing;
            HideMsg();
            // Chi cho sua thong tin khach, khong cho sua tai khoan
            txtHoTen.ReadOnly    = false;
            txtCmnd.ReadOnly     = false;
            txtDiaChi.ReadOnly   = false;
            txtEmail.ReadOnly    = false;
            txtSdt.ReadOnly      = false;
            dtpNgaySinh.Enabled  = true;
            btnSua.Text    = "Lưu";
            btnLamMoi.Text = "Hủy";
            btnKhoa.Enabled   = false;
            btnMoKhoa.Enabled = false;
        }

        private void SetEditorsEnabled(bool enabled)
        {
            txtHoTen.ReadOnly   = !enabled;
            txtCmnd.ReadOnly    = !enabled;
            txtDiaChi.ReadOnly  = !enabled;
            txtEmail.ReadOnly   = !enabled;
            txtSdt.ReadOnly     = !enabled;
            dtpNgaySinh.Enabled = enabled;
        }

        private void UpdateActionButtons()
        {
            var item = CurrentVM();
            bool hasSelection = item != null;
            btnSua.Enabled    = hasSelection;
            btnKhoa.Enabled   = hasSelection && item.TrangThai;
            btnMoKhoa.Enabled = hasSelection && !item.TrangThai;
        }

        // ── Button handlers ──────────────────────────────────────────────────────

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View)
            {
                if (CurrentVM() == null) { ShowMsg("Chọn khách cần sửa."); return; }
                EnterEditMode();
                return;
            }
            // Edit mode -> luu
            var vm = CurrentVM();
            if (vm == null) { ShowMsg("Mất lựa chọn. Bấm 'Hủy' và chọn lại."); return; }
            var khach = _khachSvc.LayTheoMa(vm.MaKhach);
            if (khach == null) return;
            if (string.IsNullOrWhiteSpace(txtHoTen.Text)) { ShowMsg("Họ tên không được để trống."); return; }
            khach.HoTen  = txtHoTen.Text.Trim();
            khach.SoCMND = txtCmnd.Text.Trim();
            khach.DiaChi = txtDiaChi.Text.Trim();
            khach.NgaySinh = dtpNgaySinh.Value.Date;
            try
            {
                _khachSvc.Sua(khach);

                // Cap nhat email/sdt vao TaiKhoan
                var tk = _tkSvc.LayTheoMa(vm.MaTaiKhoan);
                if (tk != null)
                {
                    tk.Email        = txtEmail.Text.Trim();
                    tk.SoDienThoai  = txtSdt.Text.Trim();
                    _tkSvc.Sua(tk);
                }
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                ShowMsg("Lỗi lưu dữ liệu: " + inner.Message);
                return;
            }

            ReloadData();
            EnterViewMode();
            RefreshDetailEditors();
        }

        private void BtnThemMoi_Click(object sender, EventArgs e)
        {
            using (var form = new frmKhachThue())
            {
                form.ShowDialog(this);
            }
            ReloadData();
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.Editing)
            {
                // Huy - khoi phuc tu grid
                var vm = CurrentVM();
                if (vm != null) { BindToForm(vm); EnterViewMode(); return; }
            }
            ReloadData();
            grid.ClearSelection();
            ClearForm();
        }

        private void BtnKhoa_Click(object sender, EventArgs e)
        {
            ChangeStatus(false);
        }

        private void BtnMoKhoa_Click(object sender, EventArgs e)
        {
            ChangeStatus(true);
        }

        private void ChangeStatus(bool status)
        {
            var vm = CurrentVM();
            if (vm == null) { ShowMsg("Chọn tài khoản."); return; }
            string loi;
            try
            {
                if (!_tkSvc.DoiTrangThai(vm.MaTaiKhoan, status, SessionContext.VaiTro, out loi))
                {
                    ShowMsg(loi);
                    return;
                }
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                ShowMsg("Loi luu du lieu: " + inner.Message);
                return;
            }
            ReloadData();
        }

        // ── Data ─────────────────────────────────────────────────────────────────

        private void ReloadData()
        {
            // Join KhachThue + TaiKhoan
            var khachs = _khachSvc.LayTatCa().ToDictionary(k => k.MaTaiKhoan, k => k);
            var taiKhoans = _tkSvc.LayTheoVaiTro("KhachThue").ToDictionary(t => t.MaTaiKhoan, t => t);

            _allData = khachs.Values.Select(k =>
            {
                TaiKhoan tk;
                taiKhoans.TryGetValue(k.MaTaiKhoan, out tk);
                return new KhachTaiKhoanVM
                {
                    MaKhach     = k.MaKhach,
                    MaTaiKhoan  = k.MaTaiKhoan,
                    TenDangNhap = tk?.TenDangNhap ?? string.Empty,
                    MatKhau     = tk?.MatKhauHash ?? string.Empty,
                    HoTen       = k.HoTen,
                    SoCMND      = k.SoCMND,
                    DiaChi      = k.DiaChi,
                    NgaySinh    = k.NgaySinh,
                    Email       = tk?.Email ?? string.Empty,
                    SoDienThoai = tk?.SoDienThoai ?? string.Empty,
                    TrangThai   = tk?.TrangThai ?? true
                };
            }).OrderBy(v => v.HoTen).ToList();

            FilterGrid();
        }

        private void FilterGrid()
        {
            var kw = TextFormatHelper.NormalizeSearch(txtSearch.Text);
            var data = string.IsNullOrEmpty(kw)
                ? _allData
                : _allData.Where(v =>
                    TextFormatHelper.ContainsNormalized(v.HoTen, kw) ||
                    TextFormatHelper.ContainsNormalized(v.SoCMND, kw) ||
                    TextFormatHelper.ContainsNormalized(v.TenDangNhap, kw) ||
                    TextFormatHelper.ContainsNormalized(v.Email, kw) ||
                    TextFormatHelper.ContainsNormalized(v.SoDienThoai, kw)).ToList();

            grid.DataSource = new BindingList<KhachTaiKhoanVM>(data);
            RenameColumns();
            lblStatus.Text = string.Format("Số dòng: {0}", data.Count);

            // To mau dong bi khoa
            foreach (DataGridViewRow row in grid.Rows)
            {
                var vm = row.DataBoundItem as KhachTaiKhoanVM;
                if (vm != null && !vm.TrangThai)
                    row.DefaultCellStyle.BackColor = Color.LightGray;
            }

            grid.ClearSelection();
            ClearForm();
        }

        private void RenameColumns()
        {
            Rename("MaKhach", "Mã khách");
            Rename("MaTaiKhoan", "Mã tài khoản");
            Rename("TenDangNhap", "Tên đăng nhập");
            Rename("MatKhau", "Mật khẩu");
            Rename("HoTen", "Họ tên");
            Rename("SoCMND", "CMND/CCCD");
            Rename("DiaChi", "Địa chỉ");
            Rename("NgaySinh", "Ngày sinh");
            Rename("SoDienThoai", "Số điện thoại");
            Rename("TrangThai", "Trạng thái");
        }

        private void Rename(string columnName, string header)
        {
            if (grid.Columns.Contains(columnName)) grid.Columns[columnName].HeaderText = header;
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            var vm = grid.CurrentRow.DataBoundItem as KhachTaiKhoanVM;
            if (vm == null) return;
            BindToForm(vm);
            EnterViewMode();
        }

        private void BindToForm(KhachTaiKhoanVM vm)
        {
            txtMaKhach.Text     = vm.MaKhach;
            txtMaTaiKhoan.Text  = vm.MaTaiKhoan;
            txtTenDangNhap.Text = vm.TenDangNhap;
            txtHoTen.Text       = vm.HoTen;
            txtCmnd.Text        = vm.SoCMND;
            txtDiaChi.Text      = vm.DiaChi;
            if (vm.NgaySinh.HasValue) dtpNgaySinh.Value = vm.NgaySinh.Value;
            txtEmail.Text       = vm.Email;
            txtSdt.Text         = vm.SoDienThoai;
            chkTrangThai.Checked = vm.TrangThai;
            RefreshDetailEditors();
        }

        private KhachTaiKhoanVM CurrentVM()
        {
            if (grid.CurrentRow == null) return null;
            return grid.CurrentRow.DataBoundItem as KhachTaiKhoanVM;
        }

        private void ClearForm()
        {
            txtMaKhach.Clear(); txtMaTaiKhoan.Clear(); txtTenDangNhap.Clear();
            txtHoTen.Clear(); txtCmnd.Clear(); txtDiaChi.Clear();
            txtEmail.Clear(); txtSdt.Clear();
            dtpNgaySinh.Value = DateTime.Today.AddYears(-18);
            chkTrangThai.Checked = false;
            UpdateActionButtons();
            RefreshDetailEditors();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void RefreshDetailEditors()
        {
            var controls = new Control[]
            {
                txtMaKhach, txtMaTaiKhoan, txtTenDangNhap, txtHoTen, txtCmnd,
                txtDiaChi, txtEmail, txtSdt, dtpNgaySinh, chkTrangThai
            };

            foreach (var control in controls)
            {
                control.Invalidate();
                control.Refresh();
            }

            if (!IsHandleCreated) return;
            BeginInvoke(new Action(() =>
            {
                foreach (var control in controls)
                {
                    control.Invalidate();
                    control.Refresh();
                }
            }));
        }

        private MaterialTextBox Txt(string hint, bool readOnly)
        {
            return new MaterialTextBox { Dock = DockStyle.Top, Hint = hint, ReadOnly = readOnly };
        }

        private void AddField(TableLayoutPanel parent, string label, Control control)
        {
            parent.Controls.Add(new Label
            {
                Text = label, AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 8, 0, 2)
            });
            parent.Controls.Add(control);
        }

        private MaterialButton Btn(string text, EventHandler handler)
        {
            var b = new MaterialButton { Text = text, AutoSize = true, Margin = new Padding(0, 4, 6, 4) };
            b.Click += handler;
            return b;
        }

    }
}
