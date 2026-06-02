using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Controls;
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
        private readonly NumericUpDown _numTienCoc = new KeyboardOnlyNumericUpDown();
        private readonly ErrorProvider _errors = new ErrorProvider();
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();
        private bool _dangLocSo;

        public KhachThue Khach { get; private set; }
        public string TenDangNhap { get; private set; }
        public string MatKhau { get; private set; }
        public string Email => _txtEmail.Text.Trim();
        public string SoDienThoai => _txtSdt.Text.Trim();
        public decimal TienCoc => _numTienCoc.Value;

        public frmDangKyDatTruoc(CanHo room)
        {
            _room = room;
            Text = "Đăng ký đặt trước";
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
                ColumnCount = 1,
                AutoScroll = true,
                Padding = new Padding(18, 18, 18, 18),
                BackColor = Color.White
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            root.Controls.Add(new Label
            {
                Text = string.Format("Phong {0} - tien coc de xuat {1:N0}", _room.MaCanHo, TienCocMacDinh()),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10)
            });

            SetupText(_txtHoTen, "Họ tên khách");
            SetupText(_txtCmnd, "CMND/CCCD");
            SetupDigitsOnly(_txtCmnd);
            SetupText(_txtEmail, "Email nhận tài khoản");
            SetupText(_txtSdt, "Số điện thoại");
            SetupDigitsOnly(_txtSdt);

            _dtpNgaySinh.Dock = DockStyle.Top;
            _dtpNgaySinh.Format = DateTimePickerFormat.Custom;
            _dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
            _dtpNgaySinh.MaxDate = DateTime.Today;
            _dtpNgaySinh.Value = DateTime.Today.AddYears(-18);

            _numTienCoc.Dock = DockStyle.Top;
            _numTienCoc.Maximum = 1000000000000;
            _numTienCoc.ThousandsSeparator = true;
            _numTienCoc.Value = TienCocMacDinh();

            AddField(root, "Họ tên *", _txtHoTen);
            AddField(root, "CMND/CCCD *", _txtCmnd);
            AddField(root, "Ngày sinh *", _dtpNgaySinh);
            AddField(root, "Email", _txtEmail);
            AddField(root, "Số điện thoại *", _txtSdt);
            AddField(root, "Tiền cọc đặt trước (tối thiểu 10% giá phòng)", _numTienCoc);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 16, 0, 0) };
            var btnOk = new RoundedButton
            {
                Text = "Tạo tài khoản và đặt trước",
                Width = 200,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(16, 185, 129),
                BorderColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White
            };
            var btnCheckEmail = new RoundedButton
            {
                Text = "Kiểm tra email",
                Width = 130,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(6, 182, 212),
                BorderColor = Color.FromArgb(8, 145, 178),
                ForeColor = Color.White
            };
            var btnCancel = new RoundedButton
            {
                Text = "Hủy",
                Width = 80,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(239, 68, 68),
                BorderColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White
            };
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
                if (string.IsNullOrWhiteSpace(_txtHoTen.Text)) _errors.SetError(_txtHoTen, "Vui lòng nhập họ tên.");
                if (string.IsNullOrWhiteSpace(_txtCmnd.Text)) _errors.SetError(_txtCmnd, "Vui lòng nhập CMND/CCCD.");
                MessageBox.Show("Vui lòng nhập họ tên và CMND/CCCD.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string loiSdt;
            if (!ValidationHelper.KhongRong(SoDienThoai, "Số điện thoại", out loiSdt) ||
                !ValidationHelper.SdtHopLe(SoDienThoai, out loiSdt))
            {
                _errors.SetError(_txtSdt, loiSdt);
                _txtSdt.Focus();
                MessageBox.Show(loiSdt, "Số điện thoại không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string loiCmnd;
            if (!ValidationHelper.CmndHopLe(_txtCmnd.Text.Trim(), out loiCmnd))
            {
                _errors.SetError(_txtCmnd, loiCmnd);
                _txtCmnd.Clear();
                _txtCmnd.Focus();
                MessageBox.Show(loiCmnd, "CMND/CCCD không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tienCocToiThieu = Math.Ceiling(_room.GiaThueNiemYet * 0.1m);
            if (_numTienCoc.Value < tienCocToiThieu)
            {
                var loi = string.Format("Tiền cọc đặt trước phải tối thiểu 10% giá phòng ({0:N0}).", tienCocToiThieu);
                _errors.SetError(_numTienCoc, loi);
                MessageBox.Show(loi, "Tiền cọc không hợp lệ",
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

        private void SetupDigitsOnly(MaterialTextBox textbox)
        {
            textbox.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };
            textbox.TextChanged += delegate
            {
                if (_dangLocSo) return;

                var digits = new string((textbox.Text ?? string.Empty).Where(char.IsDigit).ToArray());
                if (digits == textbox.Text) return;

                _dangLocSo = true;
                var selectionStart = Math.Min(digits.Length, textbox.SelectionStart);
                textbox.Text = digits;
                textbox.SelectionStart = selectionStart;
                _dangLocSo = false;
            };
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
                MessageBox.Show(loi, "Email không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmail.Focus();
                return false;
            }
            if (_taiKhoanService.EmailDaTon(email))
            {
                _errors.SetError(_txtEmail, "Email đã được sử dụng.");
                MessageBox.Show("Email đã được sử dụng.", "Email đã tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmail.Focus();
                return false;
            }
            if (hienThongBaoThanhCong)
            {
                MessageBox.Show("Email đúng định dạng và chưa tồn tại trong hệ thống. Hệ thống sẽ xác nhận thực tế khi gửi mail.",
                    "Email hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return true;
        }

        private string TaoMatKhauTam()
        {
            return "Khach@" + DateTime.Now.ToString("HHmmss");
        }
    }
}
