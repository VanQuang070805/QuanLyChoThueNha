using System;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmQrThanhToan : MaterialForm
    {
        private readonly HoaDonThanhToan _hoaDon;
        private readonly decimal _soTienCanTra;
        private readonly PictureBox _picture = new PictureBox();
        private readonly Label _lblInfo = new Label();

        public frmQrThanhToan(HoaDonThanhToan hoaDon)
        {
            _hoaDon = hoaDon;
            _soTienCanTra = Math.Max(0, hoaDon.SoTienPhaiTra - hoaDon.SoTienDaTra);
            Text = "QR thanh toan hoa don";
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
                Padding = new Padding(16, 76, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _lblInfo.Dock = DockStyle.Fill;
            _lblInfo.Text = string.Format(
                "Hoa don: {0}\nKy: {1}\nSo tien can thanh toan: {2:N0}",
                _hoaDon.MaHoaDon, _hoaDon.KyThanhToan, _soTienCanTra);
            _lblInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            root.Controls.Add(_lblInfo, 0, 0);

            _picture.Dock = DockStyle.Fill;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            _picture.BackColor = Color.White;
            root.Controls.Add(_picture, 0, 1);

            var btnCopy = new MaterialButton { Text = "Copy thong tin", Dock = DockStyle.Left, AutoSize = true };
            btnCopy.Click += delegate
            {
                Clipboard.SetText(_lblInfo.Text + "\nNoi dung: " + NoiDungChuyenKhoan());
                MessageBox.Show("Da copy thong tin thanh toan.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            root.Controls.Add(btnCopy, 0, 2);
            Controls.Add(root);
        }

        private void LoadQr()
        {
            if (_soTienCanTra <= 0)
            {
                _lblInfo.Text += "\nHoa don nay da duoc thanh toan du.";
                return;
            }

            try
            {
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
                _lblInfo.Text += "\nKhong tai duoc anh QR. Vui long dung thong tin ben duoi de chuyen khoan.";
            }
        }

        private string TaoVietQrUrl()
        {
            var bank = Config("PaymentBankCode", "MB");
            var account = Config("PaymentAccountNo", "0000000000");
            var name = Config("PaymentAccountName", "QUAN LY CHO THUE NHA");
            var amount = decimal.ToInt64(_soTienCanTra);
            var addInfo = Uri.EscapeDataString(NoiDungChuyenKhoan());
            var accountName = Uri.EscapeDataString(name);
            return string.Format("https://img.vietqr.io/image/{0}-{1}-compact2.png?amount={2}&addInfo={3}&accountName={4}",
                bank, account, amount, addInfo, accountName);
        }

        private string NoiDungChuyenKhoan()
        {
            return string.Format("Thanh toan {0} {1}", _hoaDon.MaHoaDon, _hoaDon.KyThanhToan);
        }

        private string Config(string key, string fallback)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
    }
}
