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
        private readonly KhachThueService _service = new KhachThueService();
        private readonly ErrorProvider _errors = new ErrorProvider();

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

        public frmKhachThue()
        {
            Text = "Quan ly Khach thue";
            Size = new Size(1180, 700);
            StartPosition = FormStartPosition.CenterParent;
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            BuildLayout();
            ReloadData();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12, 76, 12, 12) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            txtSearch = new MaterialTextBox { Dock = DockStyle.Fill, Hint = "Tim theo ho ten, CMND/CCCD, tai khoan" };
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

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 0, 0, 0) };
            txtMaKhach = TextBox("(tu sinh)", true);
            txtMaTaiKhoan = TextBox("(tu sinh)", true);
            txtHoTen = TextBox("Ho ten khach", false);
            txtCmnd = TextBox("9 hoac 12 so", false);
            txtDiaChi = TextBox("Dia chi", false);
            txtTenDangNhap = TextBox("Ten dang nhap khach", false);
            txtMatKhau = TextBox("Mat khau mac dinh/khach dat", false);
            txtMatKhau.Password = true;
            txtEmail = TextBox("Email", false);
            txtSdt = TextBox("So dien thoai", false);
            dtpNgaySinh = new DateTimePicker { Dock = DockStyle.Top, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
            // Mặc định trỏ về mốc 18 tuổi trước để nhắc người dùng (khách phải >= 18 tuổi).
            dtpNgaySinh.MaxDate = DateTime.Today;
            dtpNgaySinh.Value = DateTime.Today.AddYears(-18);

            AddField(right, "Ma khach", txtMaKhach);
            AddField(right, "Ma tai khoan", txtMaTaiKhoan);
            AddField(right, "Ho ten *", txtHoTen);
            AddField(right, "CMND/CCCD *", txtCmnd);
            AddField(right, "Dia chi", txtDiaChi);
            AddField(right, "Ngay sinh", dtpNgaySinh);
            AddField(right, "Ten dang nhap khach *", txtTenDangNhap);
            AddField(right, "Mat khau khach *", txtMatKhau);
            AddField(right, "Email", txtEmail);
            AddField(right, "So dien thoai", txtSdt);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Margin = new Padding(0, 12, 0, 0) };
            commands.Controls.Add(Button("Them khach", BtnAdd_Click));
            commands.Controls.Add(Button("Sua thong tin", BtnUpdate_Click));
            commands.Controls.Add(Button("Lam moi", delegate { ClearInputs(); ReloadData(); }));
            right.Controls.Add(commands);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);
            Controls.Add(root);
        }

        private MaterialTextBox TextBox(string hint, bool readOnly)
        {
            return new MaterialTextBox { Dock = DockStyle.Top, Hint = hint, ReadOnly = readOnly };
        }

        private void AddField(TableLayoutPanel parent, string label, Control control)
        {
            parent.Controls.Add(new Label { Text = label, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(0, 8, 0, 2) });
            parent.Controls.Add(control);
        }

        private MaterialButton Button(string text, EventHandler handler)
        {
            var btn = new MaterialButton { Text = text, AutoSize = true, Margin = new Padding(0, 4, 6, 4) };
            btn.Click += handler;
            return btn;
        }

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
            if (grid.CurrentRow == null) return;
            var khach = grid.CurrentRow.DataBoundItem as KhachThue;
            if (khach == null) return;

            txtMaKhach.Text = khach.MaKhach;
            txtMaTaiKhoan.Text = khach.MaTaiKhoan;
            txtHoTen.Text = khach.HoTen;
            txtCmnd.Text = khach.SoCMND;
            txtDiaChi.Text = khach.DiaChi;
            if (khach.NgaySinh.HasValue) dtpNgaySinh.Value = khach.NgaySinh.Value;
            txtTenDangNhap.Clear();
            txtMatKhau.Clear();
            txtEmail.Clear();
            txtSdt.Clear();
        }

        private bool ValidateInput(bool addMode, out string loi)
        {
            _errors.Clear();
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(txtHoTen.Text)) { loi = "Ho ten khong duoc de trong."; _errors.SetError(txtHoTen, loi); return false; }
            if (string.IsNullOrWhiteSpace(txtCmnd.Text)) { loi = "CMND/CCCD khong duoc de trong."; _errors.SetError(txtCmnd, loi); return false; }
            if (addMode && string.IsNullOrWhiteSpace(txtTenDangNhap.Text)) { loi = "Ten dang nhap khach khong duoc de trong."; _errors.SetError(txtTenDangNhap, loi); return false; }
            if (addMode && string.IsNullOrWhiteSpace(txtMatKhau.Text)) { loi = "Mat khau khach khong duoc de trong."; _errors.SetError(txtMatKhau, loi); return false; }
            // Khách phải đủ 18 tuổi — chặn ngay tại GUI để báo lỗi sớm (BLL vẫn kiểm tra lại).
            if (!QuanLyChoThueNha.BLL.Helpers.ValidationHelper.DuTuoiTroLen(dtpNgaySinh.Value.Date, 18, out loi))
            { _errors.SetError(dtpNgaySinh, loi); return false; }
            return true;
        }

        private KhachThue ReadKhach()
        {
            return new KhachThue
            {
                MaKhach = txtMaKhach.Text.Trim(),
                MaTaiKhoan = txtMaTaiKhoan.Text.Trim(),
                HoTen = txtHoTen.Text.Trim(),
                SoCMND = txtCmnd.Text.Trim(),
                DiaChi = txtDiaChi.Text.Trim(),
                NgaySinh = dtpNgaySinh.Value.Date
            };
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string loi;
            if (!ValidateInput(true, out loi)) { MessageBox.Show(loi, "Loi nhap lieu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!_service.TaoKhachKemTaiKhoan(ReadKhach(), txtTenDangNhap.Text, txtMatKhau.Text, txtEmail.Text, txtSdt.Text, out loi))
            {
                MessageBox.Show(loi, "Loi nhap lieu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ClearInputs();
            ReloadData();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKhach.Text)) { MessageBox.Show("Chon khach thue can sua."); return; }
            string loi;
            if (!ValidateInput(false, out loi)) { MessageBox.Show(loi, "Loi nhap lieu", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var current = _service.LayTheoMa(txtMaKhach.Text);
            if (current == null) return;
            var update = ReadKhach();
            current.HoTen = update.HoTen;
            current.SoCMND = update.SoCMND;
            current.DiaChi = update.DiaChi;
            current.NgaySinh = update.NgaySinh;
            _service.Sua(current);
            ReloadData();
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
    }
}
