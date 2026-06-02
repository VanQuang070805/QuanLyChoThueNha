using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.GUI.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmQrThanhToan : MaterialForm
    {
        private readonly HoaDonThanhToan _hoaDon;
        private readonly decimal _soTienCanTra;
        private readonly string _maThanhToan;
        private readonly string _kyThanhToan;
        private readonly string _tieuDe;
        private readonly string _noiDungChuyenKhoan;
        private readonly PictureBox _picture = new PictureBox();
        private readonly Label _lblInfo = new Label();

        public static string SoTaiKhoanNhan
        {
            get
            {
                var value = ConfigurationManager.AppSettings["PaymentAccountNo"];
                return string.IsNullOrWhiteSpace(value) ? "0000000000" : value;
            }
        }

        public frmQrThanhToan(HoaDonThanhToan hoaDon)
        {
            _hoaDon = hoaDon;
            _soTienCanTra = Math.Max(0, hoaDon.SoTienPhaiTra - hoaDon.SoTienDaTra);
            _maThanhToan = hoaDon.MaHoaDon;
            _kyThanhToan = hoaDon.KyThanhToan;
            _tieuDe = "QR thanh toán hóa đơn";
            _noiDungChuyenKhoan = string.Format("Thanh toán {0} {1}", hoaDon.MaHoaDon, hoaDon.KyThanhToan);
            Text = _tieuDe;
            Size = new Size(520, 680);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { LoadQr(); };
        }

        public frmQrThanhToan(string tieuDe, string maThanhToan, string kyThanhToan, decimal soTienCanTra, string noiDungChuyenKhoan)
        {
            _soTienCanTra = Math.Max(0, soTienCanTra);
            _maThanhToan = maThanhToan;
            _kyThanhToan = kyThanhToan;
            _tieuDe = tieuDe;
            _noiDungChuyenKhoan = noiDungChuyenKhoan;
            Text = _tieuDe;
            Size = new Size(520, 680);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            Load += delegate { LoadQr(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(16, 16, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _lblInfo.Dock = DockStyle.Fill;
            _lblInfo.Text = string.Format(
                "{0}: {1}\nThông tin: {2}\nSTK nhận: {3}\nSố tiền cần thanh toán: {4:N0}",
                _hoaDon == null ? "Mã thanh toán" : "Hóa đơn",
                _maThanhToan,
                _kyThanhToan,
                SoTaiKhoanNhan,
                _soTienCanTra);
            _lblInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            root.Controls.Add(_lblInfo, 0, 0);

            _picture.Dock = DockStyle.Fill;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            _picture.BackColor = Color.White;
            root.Controls.Add(_picture, 0, 1);

            var btnCopy = new RoundedButton
            {
                Text = "Sao chép thông tin",
                Dock = DockStyle.Left,
                Width = 150,
                Height = 36,
                Radius = 10,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(29, 78, 216),
                ForeColor = Color.White
            };
            btnCopy.Click += delegate
            {
                Clipboard.SetText(_lblInfo.Text + "\nNội dung: " + NoiDungChuyenKhoan());
                MessageBox.Show("Đã sao chép thông tin thanh toán.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            root.Controls.Add(btnCopy, 0, 2);
            Controls.Add(root);
        }

        private void LoadQr()
        {
            if (_soTienCanTra <= 0)
            {
                _lblInfo.Text += "\nHóa đơn này đã được thanh toán đủ.";
                return;
            }

            try
            {
                var customQrPath = Config("PaymentQrImagePath", string.Empty);
                if (!string.IsNullOrWhiteSpace(customQrPath) && File.Exists(customQrPath))
                {
                    using (var temp = Image.FromFile(customQrPath))
                        _picture.Image = new Bitmap(temp);
                    _lblInfo.Text += "\nĐang dùng mã QR ngân hàng cố định. Vui lòng nhập đúng số tiền và nội dung chuyển khoản.";
                    return;
                }

                var url = TaoVietQrUrl();
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                {
                    if (stream != null)
                        _picture.Image = Image.FromStream(stream);
                }
            }
            catch
            {
                _lblInfo.Text += "\nKhông tải được ảnh QR. Vui lòng dùng thông tin bên dưới để chuyển khoản.";
            }
        }

        private string TaoVietQrUrl()
        {
            var bank = Config("PaymentBankCode", "MB");
            var account = SoTaiKhoanNhan;
            var name = Config("PaymentAccountName", "QUAN LY CHO THUE NHA");
            var amount = decimal.ToInt64(_soTienCanTra);
            var addInfo = Uri.EscapeDataString(NoiDungChuyenKhoan());
            var accountName = Uri.EscapeDataString(name);
            return string.Format("https://img.vietqr.io/image/{0}-{1}-compact2.png?amount={2}&addInfo={3}&accountName={4}",
                bank, account, amount, addInfo, accountName);
        }

        private string NoiDungChuyenKhoan()
        {
            return string.IsNullOrWhiteSpace(_noiDungChuyenKhoan)
                ? string.Format("Thanh toán {0} {1}", _maThanhToan, _kyThanhToan)
                : _noiDungChuyenKhoan;
        }

        private string Config(string key, string fallback)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
    }
}
