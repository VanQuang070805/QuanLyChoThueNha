using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;
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
        private MaterialTextBox txtSearch;
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
            Text = "Quan ly Khach thue";
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
                Padding = new Padding(12, 76, 12, 12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            txtSearch = new MaterialTextBox
            {
                Dock = DockStyle.Fill,
                Hint = "Tim theo ho ten, CMND/CCCD, tai khoan"
            };
            txtSearch.TextChanged += delegate { ReloadData(); };
            left.Controls.Add(txtSearch, 0, 0);

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

            txtMaKhach    = Txt("(tu sinh)", true);
            txtMaTaiKhoan = Txt("(tu sinh)", true);
            txtHoTen      = Txt("Ho ten khach", false);
            txtCmnd       = Txt("9 hoac 12 so", false);
            txtDiaChi     = Txt("Dia chi", false);
            txtTenDangNhap = Txt("Ten dang nhap", false);
            txtMatKhau    = Txt("Mat khau", false);
            txtMatKhau.Password = true;
            txtEmail      = Txt("Email", false);
            txtSdt        = Txt("So dien thoai", false);

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

            AddField(right, "Ma khach",              txtMaKhach);
            AddField(right, "Ma tai khoan",           txtMaTaiKhoan);
            AddField(right, "Ho ten *",               txtHoTen);
            AddField(right, "CMND/CCCD *",            txtCmnd);
            AddField(right, "Dia chi",                txtDiaChi);
            AddField(right, "Ngay sinh",              dtpNgaySinh);
            AddField(right, "Ten dang nhap * (moi)",  txtTenDangNhap);
            AddField(right, "Mat khau * (moi)",       txtMatKhau);
            AddField(right, "Email",                  txtEmail);
            AddField(right, "So dien thoai",          txtSdt);

            var commands = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, WrapContents = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            btnThem   = Btn("Them khach",  BtnThem_Click);
            btnSua    = Btn("Sua thong tin", BtnSua_Click);
            btnLamMoi = Btn("Lam moi",      BtnLamMoi_Click);
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
            btnThem.Text   = "Luu";   btnThem.Enabled  = true;
            btnSua.Text    = "Sua thong tin"; btnSua.Enabled   = false;
            btnLamMoi.Text = "Lam moi";
            // Pre-populate ma tu sinh de nguoi dung thay duoc
            txtMaKhach.Text    = _service.LayMaKhachTiepTheo();
            txtMaTaiKhoan.Text = _service.LayMaTaiKhoanTiepTheo();
            grid.ClearSelection();
        }

        private void EnterViewMode()
        {
            _mode = FormMode.View;
            SetEditorsEnabled(false);
            btnThem.Text   = "Them khach"; btnThem.Enabled  = true;
            btnSua.Text    = "Sua thong tin"; btnSua.Enabled   = true;
            btnLamMoi.Text = "Lam moi";
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
            btnThem.Text   = "Them khach"; btnThem.Enabled  = false;
            btnSua.Text    = "Luu";        btnSua.Enabled   = true;
            btnLamMoi.Text = "Huy";
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
                if (string.IsNullOrWhiteSpace(txtMaKhach.Text)) { ShowError("Chon khach thue can sua."); return; }
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
            var kw = txtSearch.Text.Trim().ToLowerInvariant();
            var data = _service.LayTatCa();
            if (!string.IsNullOrEmpty(kw))
            {
                data = data.Where(k =>
                    (k.HoTen ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                    (k.SoCMND ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                    (k.MaTaiKhoan ?? string.Empty).ToLowerInvariant().Contains(kw));
            }
            grid.DataSource = new BindingList<KhachThue>(data.OrderBy(k => k.HoTen).ToList());
            grid.ClearSelection();
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
                loi = "Ho ten khong duoc de trong.";
                _errors.SetError(txtHoTen, loi);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCmnd.Text))
            {
                loi = "CMND/CCCD khong duoc de trong.";
                _errors.SetError(txtCmnd, loi);
                return false;
            }
            if (addMode && string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
            {
                loi = "Ten dang nhap khong duoc de trong.";
                _errors.SetError(txtTenDangNhap, loi);
                return false;
            }
            if (addMode && string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                loi = "Mat khau khong duoc de trong.";
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
