using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmDangKyDatTruoc : MaterialForm
    {
        private readonly CanHo _room;
        private readonly MaterialTextBox _txtHoTen = new MaterialTextBox();
        private readonly MaterialTextBox _txtCmnd = new MaterialTextBox();
        private readonly MaterialTextBox _txtDiaChi = new MaterialTextBox();
        private readonly MaterialTextBox _txtEmail = new MaterialTextBox();
        private readonly MaterialTextBox _txtSdt = new MaterialTextBox();
        private readonly DateTimePicker _dtpNgaySinh = new DateTimePicker();
        private readonly NumericUpDown _numTienCoc = new NumericUpDown();

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
            Size = new Size(520, 680);
            StartPosition = FormStartPosition.CenterParent;
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
            SetupText(_txtDiaChi, "Dia chi");
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
            AddField(root, "Dia chi", _txtDiaChi);
            AddField(root, "Email", _txtEmail);
            AddField(root, "So dien thoai", _txtSdt);
            AddField(root, "Tien coc dat truoc", _numTienCoc);

            var commands = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 16, 0, 0) };
            var btnOk = new MaterialButton { Text = "Tao tai khoan va dat truoc", AutoSize = true };
            var btnCancel = new MaterialButton { Text = "Huy", AutoSize = true };
            btnOk.Click += delegate { Submit(); };
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            commands.Controls.Add(btnOk);
            commands.Controls.Add(btnCancel);
            root.Controls.Add(commands);
            Controls.Add(root);
        }

        private decimal TienCocMacDinh()
        {
            return _room.TienCocNiemYet > 0 ? _room.TienCocNiemYet : _room.GiaThueNiemYet;
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
            if (string.IsNullOrWhiteSpace(_txtHoTen.Text) || string.IsNullOrWhiteSpace(_txtCmnd.Text))
            {
                MessageBox.Show("Vui long nhap ho ten va CMND/CCCD.", "Thieu thong tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_numTienCoc.Value <= 0)
            {
                MessageBox.Show("Tien coc phai lon hon 0.", "Thieu thong tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Khach = new KhachThue
            {
                HoTen = _txtHoTen.Text.Trim(),
                SoCMND = _txtCmnd.Text.Trim(),
                DiaChi = _txtDiaChi.Text.Trim(),
                NgaySinh = _dtpNgaySinh.Value.Date
            };
            TenDangNhap = TaoTenDangNhap(_txtHoTen.Text, _txtCmnd.Text);
            MatKhau = TaoMatKhauTam();
            DialogResult = DialogResult.OK;
            Close();
        }

        private string TaoTenDangNhap(string hoTen, string cmnd)
        {
            var letters = new string((hoTen ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
            if (letters.Length > 10) letters = letters.Substring(letters.Length - 10);
            var tail = new string((cmnd ?? string.Empty).Where(char.IsDigit).Take(4).ToArray());
            if (string.IsNullOrWhiteSpace(letters)) letters = "khach";
            if (string.IsNullOrWhiteSpace(tail)) tail = DateTime.Now.ToString("HHmm");
            return letters + tail;
        }

        private string TaoMatKhauTam()
        {
            return "Khach@" + DateTime.Now.ToString("HHmmss");
        }
    }
}
