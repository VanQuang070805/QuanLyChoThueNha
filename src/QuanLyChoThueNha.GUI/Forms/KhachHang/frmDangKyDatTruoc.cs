using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmDangKyDatTruoc : MaterialForm
    {
        private readonly CanHo _room;
        private readonly MaterialTextBox _txtHoTen = new MaterialTextBox();
        private readonly MaterialTextBox _txtCmnd = new MaterialTextBox();
        private readonly MaterialTextBox _txtEmail = new MaterialTextBox();
        private readonly MaterialTextBox _txtSdt = new MaterialTextBox();
        private readonly DateTimePicker _dtpNgaySinh = new DateTimePicker();
        private readonly NumericUpDown _numTienCoc = new NumericUpDown();
        private readonly ErrorProvider _errors = new ErrorProvider();
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();

        public KhachThue Khach { get; private set; }
        public string TenDangNhap { get; private set; }
        public string MatKhau { get; private set; }
        public string Email => _txtEmail.Text.Trim();
        public string SoDienThoai => _txtSdt.Text.Trim();
        public decimal TienCoc => _numTienCoc.Value;

        public frmDangKyDatTruoc(CanHo room)
        {
            _room = room;
            Text = "Dang ky dat truoc";
            Size = new Size(560, 640);
            MinimumSize = new Size(540, 620);
            StartPosition = FormStartPosition.CenterParent;
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            BuildLayout();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(18, 76, 18, 18)
            };

            root.Controls.Add(new Label
            {
                Text = string.Format("Phong {0} - tien coc de xuat {1:N0}", _room.MaCanHo, TienCocMacDinh()),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10)
            });

            SetupText(_txtHoTen, "Ho ten khach");
            SetupText(_txtCmnd, "CMND/CCCD");
            SetupText(_txtEmail, "Email nhan tai khoan");
            SetupText(_txtSdt, "So dien thoai");

            _dtpNgaySinh.Dock = DockStyle.Top;
            _dtpNgaySinh.Format = DateTimePickerFormat.Custom;
            _dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
            _dtpNgaySinh.MaxDate = DateTime.Today;
            _dtpNgaySinh.Value = DateTime.Today.AddYears(-18);

            _numTienCoc.Dock = DockStyle.Top;
            _numTienCoc.Maximum = 1000000000000;
            _numTienCoc.ThousandsSeparator = true;
            _numTienCoc.Value = TienCocMacDinh();

            AddField(root, "Ho ten *", _txtHoTen);
            AddField(root, "CMND/CCCD *", _txtCmnd);
            AddField(root, "Ngay sinh *", _dtpNgaySinh);
            AddField(root, "Email", _txtEmail);
            AddField(root, "So dien thoai *", _txtSdt);
            AddField(root, "Tien coc dat truoc (toi thieu 10% gia phong)", _numTienCoc);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 16, 0, 0) };
            var btnOk = new MaterialButton { Text = "Tao tai khoan va dat truoc", AutoSize = true };
            var btnCheckEmail = new MaterialButton { Text = "Kiem tra email", AutoSize = true };
            var btnCancel = new MaterialButton { Text = "Huy", AutoSize = true };
            btnOk.Click += delegate { Submit(); };
            btnCheckEmail.Click += delegate { KiemTraEmail(true); };
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            commands.Controls.Add(btnCheckEmail);
            commands.Controls.Add(btnOk);
            commands.Controls.Add(btnCancel);
            root.Controls.Add(commands);
            Controls.Add(root);
        }

        private decimal TienCocMacDinh()
        {
            var toiThieu = Math.Ceiling(_room.GiaThueNiemYet * 0.1m);
            var deXuat = _room.TienCocNiemYet > 0 ? _room.TienCocNiemYet : toiThieu;
            return Math.Max(toiThieu, deXuat);
        }

        private void SetupText(MaterialTextBox textbox, string hint)
        {
            textbox.Dock = DockStyle.Top;
            textbox.Hint = hint;
        }

        private void AddField(TableLayoutPanel root, string label, Control control)
        {
            root.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 8, 0, 2)
            });
            root.Controls.Add(control);
        }

        private void Submit()
        {
            _errors.Clear();
            if (string.IsNullOrWhiteSpace(_txtHoTen.Text) || string.IsNullOrWhiteSpace(_txtCmnd.Text))
            {
                if (string.IsNullOrWhiteSpace(_txtHoTen.Text)) _errors.SetError(_txtHoTen, "Vui long nhap ho ten.");
                if (string.IsNullOrWhiteSpace(_txtCmnd.Text)) _errors.SetError(_txtCmnd, "Vui long nhap CMND/CCCD.");
                MessageBox.Show("Vui long nhap ho ten va CMND/CCCD.", "Thieu thong tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string loiSdt;
            if (!ValidationHelper.KhongRong(SoDienThoai, "So dien thoai", out loiSdt) ||
                !ValidationHelper.SdtHopLe(SoDienThoai, out loiSdt))
            {
                _errors.SetError(_txtSdt, loiSdt);
                _txtSdt.Focus();
                MessageBox.Show(loiSdt, "So dien thoai khong hop le",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var tienCocToiThieu = Math.Ceiling(_room.GiaThueNiemYet * 0.1m);
            if (_numTienCoc.Value < tienCocToiThieu)
            {
                var loi = string.Format("Tien coc dat truoc phai toi thieu 10% gia phong ({0:N0}).", tienCocToiThieu);
                _errors.SetError(_numTienCoc, loi);
                MessageBox.Show(loi, "Tien coc khong hop le",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!KiemTraEmail(false))
                return;

            Khach = new KhachThue
            {
                HoTen = _txtHoTen.Text.Trim(),
                SoCMND = _txtCmnd.Text.Trim(),
                NgaySinh = _dtpNgaySinh.Value.Date
            };
            TenDangNhap = TaoTenDangNhap(_txtHoTen.Text, _txtCmnd.Text);
            MatKhau = TaoMatKhauTam();
            DialogResult = DialogResult.OK;
            Close();
        }

        private string TaoTenDangNhap(string hoTen, string cmnd)
        {
            var normalized = BoDauTiengViet(hoTen ?? string.Empty).ToLowerInvariant();
            var letters = new string(normalized
                .Where(c => (c >= 'a' && c <= 'z') || char.IsDigit(c))
                .ToArray());
            if (letters.Length > 12) letters = letters.Substring(letters.Length - 12);
            var tail = new string((cmnd ?? string.Empty).Where(char.IsDigit).Take(4).ToArray());
            if (string.IsNullOrWhiteSpace(letters)) letters = "khach";
            if (string.IsNullOrWhiteSpace(tail)) tail = DateTime.Now.ToString("HHmm");
            return (letters + tail).ToLowerInvariant();
        }

        private static string BoDauTiengViet(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark) continue;
                if (c == 'đ') builder.Append('d');
                else if (c == 'Đ') builder.Append('D');
                else builder.Append(c);
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private bool KiemTraEmail(bool hienThongBaoThanhCong)
        {
            var email = Email;
            string loi;
            if (!ValidationHelper.EmailHopLe(email, out loi))
            {
                _errors.SetError(_txtEmail, loi);
                MessageBox.Show(loi, "Email khong hop le", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmail.Focus();
                return false;
            }
            if (_taiKhoanService.EmailDaTon(email))
            {
                _errors.SetError(_txtEmail, "Email da duoc su dung.");
                MessageBox.Show("Email da duoc su dung.", "Email da ton tai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmail.Focus();
                return false;
            }
            if (hienThongBaoThanhCong)
            {
                MessageBox.Show("Email dung dinh dang va chua ton tai trong he thong. He thong se xac nhan thuc te khi gui mail.",
                    "Email hop le", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return true;
        }

        private string TaoMatKhauTam()
        {
            return "Khach@" + DateTime.Now.ToString("HHmmss");
        }
    }
}
