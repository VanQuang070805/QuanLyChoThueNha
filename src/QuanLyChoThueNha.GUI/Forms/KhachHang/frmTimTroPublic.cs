using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Auth;
using QuanLyChoThueNha.GUI.Helpers;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmTimTroPublic : MaterialForm
    {
        private readonly CanHoService _canHoService = new CanHoService();
        private readonly KhuVucService _khuVucService = new KhuVucService();
        private readonly ToaService _toaService = new ToaService();
        private readonly LoaiCanHoService _loaiService = new LoaiCanHoService();
        private readonly KhachThueService _khachThueService = new KhachThueService();
        private readonly PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();

        private readonly FlowLayoutPanel _roomCards = new FlowLayoutPanel();
        private readonly ComboBox _cboKhuVuc = new ComboBox();
        private readonly ComboBox _cboToa = new ComboBox();
        private readonly ComboBox _cboLoai = new ComboBox();
        private readonly NumericUpDown _numBanKinh = new NumericUpDown();
        private readonly NumericUpDown _numGiaTu = new NumericUpDown();
        private readonly NumericUpDown _numGiaDen = new NumericUpDown();
        private readonly MaterialTextBox _txtTimKiem = new MaterialTextBox();
        private readonly MaterialLabel _lblCount = new MaterialLabel();

        public frmTimTroPublic()
        {
            Text = "Tim tro";
            Size = new Size(1220, 820);
            StartPosition = FormStartPosition.CenterScreen;

            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);

            BuildLayout();
            NapBoLoc();
            TaiDanhSachTro();
            Resize += delegate { ResizeCards(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(16, 76, 16, 16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            var title = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Tim tro dang trong",
                Font = new Font("Roboto", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var btnKhach = new MaterialButton { Text = "Tai khoan khach", Dock = DockStyle.Fill };
            btnKhach.Click += delegate { new frmLogin(new[] { "KhachThue" }, "Dang nhap khach hang").Show(); };
            var btnNoiBo = new MaterialButton { Text = "Cong noi bo", Dock = DockStyle.Fill };
            btnNoiBo.Click += delegate { new frmLogin(new[] { "Admin", "NhanVien" }, "Dang nhap noi bo").Show(); };
            var btnLamMoi = new MaterialButton { Text = "Lam moi", Dock = DockStyle.Fill };
            btnLamMoi.Click += delegate { TaiDanhSachTro(); };

            top.Controls.Add(title, 0, 0);
            top.Controls.Add(btnLamMoi, 1, 0);
            top.Controls.Add(btnKhach, 2, 0);
            top.Controls.Add(btnNoiBo, 3, 0);
            root.Controls.Add(top, 0, 0);

            var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            _txtTimKiem.Width = 280;
            _txtTimKiem.Hint = "Tim theo ma, toa, so can";
            _txtTimKiem.TextChanged += delegate { TaiDanhSachTro(); };

            SetupCombo(_cboToa, 180);
            SetupCombo(_cboKhuVuc, 210);
            SetupCombo(_cboLoai, 180);
            _cboKhuVuc.SelectedIndexChanged += delegate { TaiDanhSachTro(); };
            _cboToa.SelectedIndexChanged += delegate { TaiDanhSachTro(); };
            _cboLoai.SelectedIndexChanged += delegate { TaiDanhSachTro(); };

            _numBanKinh.Width = 90;
            _numBanKinh.Minimum = 1;
            _numBanKinh.Maximum = 100;
            _numBanKinh.Value = 3;
            _numBanKinh.ValueChanged += delegate { TaiDanhSachTro(); };

            SetupMoney(_numGiaTu);
            SetupMoney(_numGiaDen);
            _numGiaTu.ValueChanged += delegate { TaiDanhSachTro(); };
            _numGiaDen.ValueChanged += delegate { TaiDanhSachTro(); };

            filters.Controls.Add(_txtTimKiem);
            filters.Controls.Add(Label("Khu vuc"));
            filters.Controls.Add(_cboKhuVuc);
            filters.Controls.Add(Label("Ban kinh km"));
            filters.Controls.Add(_numBanKinh);
            filters.Controls.Add(Label("Toa"));
            filters.Controls.Add(_cboToa);
            filters.Controls.Add(Label("Loai"));
            filters.Controls.Add(_cboLoai);
            filters.Controls.Add(Label("Gia tu"));
            filters.Controls.Add(_numGiaTu);
            filters.Controls.Add(Label("Gia den"));
            filters.Controls.Add(_numGiaDen);
            filters.Controls.Add(_lblCount);
            root.Controls.Add(filters, 0, 1);

            _roomCards.Dock = DockStyle.Fill;
            _roomCards.AutoScroll = true;
            _roomCards.WrapContents = true;
            _roomCards.Padding = new Padding(2);
            _roomCards.BackColor = Color.White;
            root.Controls.Add(_roomCards, 0, 2);
            Controls.Add(root);
        }

        private Label Label(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(12, 15, 4, 0)
            };
        }

        private void SetupCombo(ComboBox combo, int width)
        {
            combo.Width = width;
            combo.Height = 32;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void SetupMoney(NumericUpDown number)
        {
            number.Width = 140;
            number.Maximum = 1000000000000;
            number.ThousandsSeparator = true;
        }

        private void NapBoLoc()
        {
            _cboKhuVuc.DisplayMember = "TenKhuVuc";
            _cboKhuVuc.ValueMember = "MaKhuVuc";
            _cboKhuVuc.DataSource = _khuVucService.LayTatCa().OrderBy(k => k.TenKhuVuc).ToList();
            _cboKhuVuc.SelectedIndex = -1;

            _cboToa.DisplayMember = "TenToa";
            _cboToa.ValueMember = "MaToa";
            _cboToa.DataSource = _toaService.LayTatCa().OrderBy(t => t.TenToa).ToList();
            _cboToa.SelectedIndex = -1;

            _cboLoai.DisplayMember = "TenLoai";
            _cboLoai.ValueMember = "MaLoai";
            _cboLoai.DataSource = _loaiService.LayTatCa().OrderBy(l => l.TenLoai).ToList();
            _cboLoai.SelectedIndex = -1;
        }

        private void TaiDanhSachTro()
        {
            if (_roomCards == null) return;
            _roomCards.Controls.Clear();

            var data = _canHoService.LayTheoTinhTrang("Trong");
            var kw = (_txtTimKiem.Text ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(kw))
            {
                data = data.Where(c =>
                    (c.MaCanHo ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                    (c.MaToa ?? string.Empty).ToLowerInvariant().Contains(kw) ||
                    c.SoCanHo.ToString().Contains(kw));
            }
            if (_cboToa.SelectedValue != null)
            {
                var maToa = _cboToa.SelectedValue.ToString();
                data = data.Where(c => c.MaToa == maToa);
            }
            if (_cboLoai.SelectedValue != null)
            {
                var maLoai = _cboLoai.SelectedValue.ToString();
                data = data.Where(c => c.MaLoai == maLoai);
            }
            if (_numGiaTu.Value > 0)
            {
                var giaTu = _numGiaTu.Value;
                data = data.Where(c => c.GiaThueNiemYet >= giaTu);
            }
            if (_numGiaDen.Value > 0)
            {
                var giaDen = _numGiaDen.Value;
                data = data.Where(c => c.GiaThueNiemYet <= giaDen);
            }

            var khuVucs = _khuVucService.LayTatCa().ToDictionary(k => k.MaKhuVuc);
            var toas = _toaService.LayTatCa().ToDictionary(t => t.MaToa);
            if (_cboKhuVuc.SelectedValue != null)
            {
                var selectedMaKhuVuc = _cboKhuVuc.SelectedValue.ToString();
                KhuVuc selectedKhuVuc;
                khuVucs.TryGetValue(selectedMaKhuVuc, out selectedKhuVuc);
                data = data.Where(c => TrongBanKinhKhuVuc(c, selectedMaKhuVuc, selectedKhuVuc, toas, khuVucs));
            }

            var rooms = data.OrderBy(c => c.MaToa).ThenBy(c => c.SoCanHo).ToList();
            _lblCount.Text = string.Format("{0:N0} phong", rooms.Count);

            if (rooms.Count == 0)
            {
                _roomCards.Controls.Add(new Label
                {
                    Text = "Khong co phong phu hop.",
                    AutoSize = true,
                    Padding = new Padding(16)
                });
                return;
            }

            foreach (var room in rooms)
                _roomCards.Controls.Add(CreateRoomCard(room));
            ResizeCards();
        }

        private Control CreateRoomCard(CanHo room)
        {
            var card = new Panel
            {
                Height = 330,
                Width = 330,
                Margin = new Padding(4, 4, 12, 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Tag = "room-card"
            };

            var image = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 160,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(236, 240, 244)
            };
            GanAnh(image, room.MaCanHo);
            card.Controls.Add(image);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                Padding = new Padding(10, 8, 10, 10)
            };
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            body.Controls.Add(new Label
            {
                Text = string.Format("{0} - Can {1}", room.MaToa, room.SoCanHo),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            }, 0, 0);
            body.Controls.Add(new Label
            {
                Text = string.Format("Loai {0} | Tang {1} | {2:N1} m2", room.MaLoai, room.TangSo, room.DienTich),
                Dock = DockStyle.Fill
            }, 0, 1);
            body.Controls.Add(new Label
            {
                Text = string.Format("Gia {0:N0} - Coc {1:N0}", room.GiaThueNiemYet, room.TienCocNiemYet),
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(0, 105, 92)
            }, 0, 2);
            body.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(room.MoTa) ? "Phong dang san sang cho thue." : room.MoTa,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            }, 0, 3);

            var btnDatTruoc = new MaterialButton
            {
                Text = "Lien he dat truoc",
                Dock = DockStyle.Left,
                AutoSize = true
            };
            btnDatTruoc.Click += delegate { DangKyVaDatTruoc(room); };
            body.Controls.Add(btnDatTruoc, 0, 4);
            card.Controls.Add(body);
            body.BringToFront();
            return card;
        }

        private bool TrongBanKinhKhuVuc(CanHo room, string selectedMaKhuVuc, KhuVuc selectedKhuVuc,
            System.Collections.Generic.Dictionary<string, Toa> toas,
            System.Collections.Generic.Dictionary<string, KhuVuc> khuVucs)
        {
            Toa toa;
            if (!toas.TryGetValue(room.MaToa, out toa)) return false;
            if (toa.MaKhuVuc == selectedMaKhuVuc) return true;
            if (selectedKhuVuc == null || !selectedKhuVuc.ViDo.HasValue || !selectedKhuVuc.KinhDo.HasValue)
                return false;

            KhuVuc roomKhuVuc;
            if (!khuVucs.TryGetValue(toa.MaKhuVuc, out roomKhuVuc)) return false;
            if (!roomKhuVuc.ViDo.HasValue || !roomKhuVuc.KinhDo.HasValue) return false;
            return TinhKhoangCachKm(selectedKhuVuc.ViDo.Value, selectedKhuVuc.KinhDo.Value,
                roomKhuVuc.ViDo.Value, roomKhuVuc.KinhDo.Value) <= (double)_numBanKinh.Value;
        }

        private double TinhKhoangCachKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private void GanAnh(PictureBox picture, string maCanHo)
        {
            var anh = _canHoService.LayAnhDaiDien(maCanHo);
            if (anh != null && File.Exists(anh.DuongDanAnh))
            {
                using (var temp = Image.FromFile(anh.DuongDanAnh))
                    picture.Image = new Bitmap(temp);
                return;
            }

            var bitmap = new Bitmap(420, 220);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(232, 238, 243));
                using (var brush = new SolidBrush(Color.FromArgb(80, 98, 112)))
                using (var font = new Font("Segoe UI", 16F, FontStyle.Bold))
                {
                    var text = "NHA TRO";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (bitmap.Width - size.Width) / 2, (bitmap.Height - size.Height) / 2);
                }
            }
            picture.Image = bitmap;
        }

        private void ResizeCards()
        {
            if (_roomCards.Width <= 0) return;
            var columns = Math.Max(1, _roomCards.ClientSize.Width / 340);
            var width = Math.Max(280, (_roomCards.ClientSize.Width - (columns * 18)) / columns);
            foreach (Control control in _roomCards.Controls)
            {
                if ((control.Tag as string) == "room-card")
                    control.Width = width;
            }
        }

        private void DangKyVaDatTruoc(CanHo room)
        {
            using (var dialog = new frmDangKyDatTruoc(room))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string loi;
                if (!_khachThueService.TaoKhachKemTaiKhoan(dialog.Khach, dialog.TenDangNhap,
                        dialog.MatKhau, dialog.Email, dialog.SoDienThoai, out loi))
                {
                    MessageBox.Show(loi, "Khong the tao tai khoan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var phieu = new PhieuDatTruoc
                {
                    MaCanHo = room.MaCanHo,
                    MaKhach = dialog.Khach.MaKhach,
                    SoTienDatCoc = dialog.TienCoc,
                    NgayHetHan = DateTime.Today.AddDays(3),
                    PhuongThucThanhToan = "VietQRDatCoc",
                    GhiChu = "Khach vang lai dang ky tu man hinh tim tro; cho xac nhan chuyen khoan dat coc"
                };

                if (!_phieuDatTruocService.TaoPhieu(phieu, out loi))
                {
                    MessageBox.Show(loi, "Khong the tao phieu dat truoc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var noiDungChuyenKhoan = NoiDungDatCoc(phieu);
                string emailStatus;
                EmailNotificationHelper.GuiThongTinDatTruoc(dialog.Email, dialog.Khach.HoTen,
                    dialog.TenDangNhap, dialog.MatKhau, phieu.MaPhieuDatTruoc, room.MaCanHo,
                    dialog.TienCoc, phieu.NgayHetHan, noiDungChuyenKhoan, out emailStatus);

                var message = new StringBuilder();
                message.AppendLine("Da tao tai khoan va phieu dat truoc.");
                message.AppendLine();
                message.AppendLine("Ten dang nhap: " + dialog.TenDangNhap);
                message.AppendLine("Mat khau tam: " + dialog.MatKhau);
                message.AppendLine("Ma khach: " + dialog.Khach.MaKhach);
                message.AppendLine("Ma phieu: " + phieu.MaPhieuDatTruoc);
                message.AppendLine("Ma phong: " + room.MaCanHo);
                message.AppendLine("Tien coc: " + dialog.TienCoc.ToString("N0"));
                message.AppendLine("Noi dung CK: " + noiDungChuyenKhoan);
                message.AppendLine(emailStatus);
                Clipboard.SetText(message.ToString());
                MessageBox.Show(message + "\nThong tin dang nhap va dat coc da duoc copy.",
                    "Dat truoc thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                using (var qr = new frmQrThanhToan("QR dat coc phong", phieu.MaPhieuDatTruoc,
                    "Phong " + room.MaCanHo, dialog.TienCoc, noiDungChuyenKhoan))
                {
                    qr.ShowDialog(this);
                }
                TaiDanhSachTro();
            }
        }

        private string NoiDungDatCoc(PhieuDatTruoc phieu)
        {
            return string.Format("DAT COC {0} PHONG {1}", phieu.MaPhieuDatTruoc, phieu.MaCanHo);
        }
    }
}
