using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.HopDong
{
    public class frmKhachThue : MaterialForm
    {
        private enum FormMode { View, Adding, Editing }

        private readonly KhachThueService _service = new KhachThueService();
        private readonly ErrorProvider _errors = new ErrorProvider();
        private FormMode _mode = FormMode.View;

        private DataGridView grid;
        private PlaceholderTextBox txtSearch;
        private MaterialTextBox txtMaKhach;
        private MaterialTextBox txtMaTaiKhoan;
        private MaterialTextBox txtHoTen;
        private MaterialTextBox txtCmnd;
        private MaterialTextBox txtDiaChi;
        private DateTimePicker dtpNgaySinh;
        private MaterialTextBox txtTenDangNhap;
        private MaterialTextBox txtMatKhau;
        private MaterialTextBox txtEmail;
        private MaterialTextBox txtSdt;

        private MaterialButton btnThem;
        private MaterialButton btnSua;
        private MaterialButton btnLamMoi;
        private Label _lblMsg;

        public frmKhachThue()
        {
            Text = "Quản lý khách thuê";
            Size = new Size(1180, 700);
            StartPosition = FormStartPosition.CenterParent;
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            BuildLayout();
            ReloadData();
            EnterAddMode();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2,
                Padding = new Padding(12, 18, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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
                Placeholder = "Tìm theo họ tên, CMND/CCCD, tài khoản"
            };
            txtSearch.TextChanged += delegate { ReloadData(); };
            searchLayout.Controls.Add(txtSearch, 0, 1);
            searchPanel.Controls.Add(searchLayout);
            left.Controls.Add(searchPanel, 0, 0);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            grid.SelectionChanged += Grid_SelectionChanged;
            left.Controls.Add(grid, 0, 1);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                Padding = new Padding(12, 0, 0, 0)
            };

            txtMaKhach    = Txt("(tự sinh)", true);
            txtMaTaiKhoan = Txt("(tự sinh)", true);
            txtHoTen      = Txt("Họ tên khách", false);
            txtCmnd       = Txt("9 hoặc 12 số", false);
            txtDiaChi     = Txt("Địa chỉ", false);
            txtTenDangNhap = Txt("Tên đăng nhập", false);
            txtMatKhau    = Txt("Mật khẩu", false);
            txtMatKhau.Password = true;
            txtEmail      = Txt("Email", false);
            txtSdt        = Txt("Số điện thoại", false);

            dtpNgaySinh = new DateTimePicker
            {
                Dock = DockStyle.Top,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                MaxDate = DateTime.Today,
                Value = DateTime.Today.AddYears(-18)
            };

            // Bat dau tat ca truong co the sua (se duoc bat khi vao Add/Edit mode)
            SetEditorsEnabled(false);

            AddField(right, "Mã khách",              txtMaKhach);
            AddField(right, "Mã tài khoản",           txtMaTaiKhoan);
            AddField(right, "Họ tên *",               txtHoTen);
            AddField(right, "CMND/CCCD *",            txtCmnd);
            AddField(right, "Địa chỉ",                txtDiaChi);
            AddField(right, "Ngày sinh",              dtpNgaySinh);
            AddField(right, "Tên đăng nhập * (mới)",  txtTenDangNhap);
            AddField(right, "Mật khẩu * (mới)",       txtMatKhau);
            AddField(right, "Email",                  txtEmail);
            AddField(right, "Số điện thoại",          txtSdt);

            var commands = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            btnThem   = Btn("Thêm khách",  BtnThem_Click);
            btnSua    = Btn("Sửa thông tin", BtnSua_Click);
            btnLamMoi = Btn("Làm mới",      BtnLamMoi_Click);
            commands.Controls.Add(btnThem);
            commands.Controls.Add(btnSua);
            commands.Controls.Add(btnLamMoi);
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

        private void ShowError(string msg)
        {
            _lblMsg.Text = msg; _lblMsg.BackColor = Color.MistyRose;
            _lblMsg.ForeColor = Color.DarkRed; _lblMsg.Visible = true;
        }

        private void HideMsg() { _lblMsg.Visible = false; }

        // ── Mode management ──────────────────────────────────────────────────────

        private void EnterAddMode()
        {
            _mode = FormMode.Adding;
            ClearInputs();
            SetEditorsEnabled(true);
            // Tat cac truong chi dung khi them moi
            txtTenDangNhap.ReadOnly = false;
            txtMatKhau.ReadOnly     = false;
            btnThem.Text   = "Lưu";   btnThem.Enabled  = true;
            btnSua.Text    = "Sửa thông tin"; btnSua.Enabled   = false;
            btnLamMoi.Text = "Làm mới";
            // Pre-populate ma tu sinh de nguoi dung thay duoc
            txtMaKhach.Text    = _service.LayMaKhachTiepTheo();
            txtMaTaiKhoan.Text = _service.LayMaTaiKhoanTiepTheo();
            grid.ClearSelection();
        }

        private void EnterViewMode()
        {
            _mode = FormMode.View;
            SetEditorsEnabled(false);
            btnThem.Text   = "Thêm khách"; btnThem.Enabled  = true;
            btnSua.Text    = "Sửa thông tin"; btnSua.Enabled   = true;
            btnLamMoi.Text = "Làm mới";
        }

        private void EnterEditMode()
        {
            _mode = FormMode.Editing;
            // Chi bat cac truong thong tin khach (KHONG cho sua TenDangNhap/MatKhau o day)
            txtHoTen.ReadOnly  = false;
            txtCmnd.ReadOnly   = false;
            txtDiaChi.ReadOnly = false;
            dtpNgaySinh.Enabled = true;
            // Ten dang nhap va mat khau khong doi qua form nay
            txtTenDangNhap.ReadOnly = true;
            txtMatKhau.ReadOnly     = true;
            btnThem.Text   = "Thêm khách"; btnThem.Enabled  = false;
            btnSua.Text    = "Lưu";        btnSua.Enabled   = true;
            btnLamMoi.Text = "Hủy";
        }

        private void SetEditorsEnabled(bool enabled)
        {
            txtHoTen.ReadOnly       = !enabled;
            txtCmnd.ReadOnly        = !enabled;
            txtDiaChi.ReadOnly      = !enabled;
            txtTenDangNhap.ReadOnly = !enabled;
            txtMatKhau.ReadOnly     = !enabled;
            txtEmail.ReadOnly       = !enabled;
            txtSdt.ReadOnly         = !enabled;
            dtpNgaySinh.Enabled     = enabled;
        }

        // ── Button handlers ──────────────────────────────────────────────────────

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View || _mode == FormMode.Editing)
            {
                EnterAddMode();
                return;
            }
            // Adding -> luu khach moi
            string loi;
            if (!ValidateInput(true, out loi)) { ShowError(loi); return; }
            if (!_service.TaoKhachKemTaiKhoan(
                    ReadKhach(), txtTenDangNhap.Text, txtMatKhau.Text,
                    txtEmail.Text, txtSdt.Text, out loi)) { ShowError(loi); return; }
            ReloadData();
            EnterAddMode();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.View)
            {
                if (string.IsNullOrWhiteSpace(txtMaKhach.Text)) { ShowError("Chọn khách thuê cần sửa."); return; }
                EnterEditMode();
                return;
            }
            if (_mode == FormMode.Editing)
            {
                string loi;
                if (!ValidateInput(false, out loi)) { ShowError(loi); return; }
                var current = _service.LayTheoMa(txtMaKhach.Text);
                if (current == null) return;
                var upd = ReadKhach();
                current.HoTen    = upd.HoTen;
                current.SoCMND   = upd.SoCMND;
                current.DiaChi   = upd.DiaChi;
                current.NgaySinh = upd.NgaySinh;
                try { _service.Sua(current); }
                catch (Exception ex)
                {
                    var inner = ex; while (inner.InnerException != null) inner = inner.InnerException;
                    ShowError(inner.Message); return;
                }
                ReloadData();
                EnterAddMode();
            }
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            if (_mode == FormMode.Editing)
            {
                // Huy sua - khoi phuc du lieu goc tu grid
                if (grid.CurrentRow != null)
                {
                    var kh = grid.CurrentRow.DataBoundItem as KhachThue;
                    if (kh != null) { BindToForm(kh); EnterViewMode(); return; }
                }
            }
            ReloadData();
            EnterAddMode();
        }

        // ── Data ─────────────────────────────────────────────────────────────────

        private void ReloadData()
        {
            var kw = TextFormatHelper.NormalizeSearch(txtSearch.Text);
            var data = _service.LayTatCa();
            if (!string.IsNullOrEmpty(kw))
            {
                data = data.Where(k =>
                    TextFormatHelper.ContainsNormalized(k.HoTen, kw) ||
                    TextFormatHelper.ContainsNormalized(k.SoCMND, kw) ||
                    TextFormatHelper.ContainsNormalized(k.MaTaiKhoan, kw));
            }
            grid.DataSource = new BindingList<KhachThue>(data.OrderBy(k => k.HoTen).ToList());
            RenameColumns();
            grid.ClearSelection();
        }

        private void RenameColumns()
        {
            Rename("MaKhach", "Mã khách");
            Rename("MaTaiKhoan", "Mã tài khoản");
            Rename("HoTen", "Họ tên");
            Rename("SoCMND", "CMND/CCCD");
            Rename("DiaChi", "Địa chỉ");
            Rename("NgaySinh", "Ngày sinh");
        }

        private void Rename(string columnName, string header)
        {
            if (grid.Columns.Contains(columnName)) grid.Columns[columnName].HeaderText = header;
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_mode == FormMode.Adding) return;
            if (grid.CurrentRow == null) return;
            var khach = grid.CurrentRow.DataBoundItem as KhachThue;
            if (khach == null) return;
            BindToForm(khach);
            EnterViewMode();
        }

        private void BindToForm(KhachThue khach)
        {
            txtMaKhach.Text    = khach.MaKhach;
            txtMaTaiKhoan.Text = khach.MaTaiKhoan;
            txtHoTen.Text      = khach.HoTen;
            txtCmnd.Text       = khach.SoCMND;
            txtDiaChi.Text     = khach.DiaChi;
            if (khach.NgaySinh.HasValue) dtpNgaySinh.Value = khach.NgaySinh.Value;
            // Ten dang nhap / mat khau khong hien thi khi xem (bao mat)
            txtTenDangNhap.Clear();
            txtMatKhau.Clear();
            txtEmail.Clear();
            txtSdt.Clear();
        }

        // ── Validation / Read ────────────────────────────────────────────────────

        private bool ValidateInput(bool addMode, out string loi)
        {
            _errors.Clear();
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                loi = "Họ tên không được để trống.";
                _errors.SetError(txtHoTen, loi);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCmnd.Text))
            {
                loi = "CMND/CCCD không được để trống.";
                _errors.SetError(txtCmnd, loi);
                return false;
            }
            if (addMode && string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
            {
                loi = "Tên đăng nhập không được để trống.";
                _errors.SetError(txtTenDangNhap, loi);
                return false;
            }
            if (addMode && string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                loi = "Mật khẩu không được để trống.";
                _errors.SetError(txtMatKhau, loi);
                return false;
            }
            if (!QuanLyChoThueNha.BLL.Helpers.ValidationHelper.DuTuoiTroLen(
                    dtpNgaySinh.Value.Date, 18, out loi))
            {
                _errors.SetError(dtpNgaySinh, loi);
                return false;
            }
            return true;
        }

        private KhachThue ReadKhach()
        {
            return new KhachThue
            {
                MaKhach    = txtMaKhach.Text.Trim(),
                MaTaiKhoan = txtMaTaiKhoan.Text.Trim(),
                HoTen      = txtHoTen.Text.Trim(),
                SoCMND     = txtCmnd.Text.Trim(),
                DiaChi     = txtDiaChi.Text.Trim(),
                NgaySinh   = dtpNgaySinh.Value.Date
            };
        }

        private void ClearInputs()
        {
            txtMaKhach.Clear();
            txtMaTaiKhoan.Clear();
            txtHoTen.Clear();
            txtCmnd.Clear();
            txtDiaChi.Clear();
            txtTenDangNhap.Clear();
            txtMatKhau.Clear();
            txtEmail.Clear();
            txtSdt.Clear();
            dtpNgaySinh.Value = DateTime.Today.AddYears(-18);
            _errors.Clear();
            grid.ClearSelection();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

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
