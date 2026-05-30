using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.KhachHang
{
    public class frmKhachHangHome : MaterialForm
    {
        private readonly CanHoService _canHoService = new CanHoService();
        private readonly ToaService _toaService = new ToaService();
        private readonly LoaiCanHoService _loaiService = new LoaiCanHoService();
        private readonly PhieuDatTruocService _phieuDatTruocService = new PhieuDatTruocService();
        private readonly HopDongService _hopDongService = new HopDongService();
        private readonly HoaDonThanhToanService _hoaDonService = new HoaDonThanhToanService();

        private ComboBox cboToa;
        private ComboBox cboLoai;
        private NumericUpDown numGiaToiDa;
        private FlowLayoutPanel roomCards;
        private DataGridView gridHopDong;
        private DataGridView gridHoaDon;
        private MaterialLabel lblHeader;
        private MaterialButton btnQrThanhToan;

        public frmKhachHangHome()
        {
            Text = "Giao dien Khach hang";
            Size = new Size(1120, 720);
            StartPosition = FormStartPosition.CenterParent;
            BuildLayout();
            NapBoLoc();
            Load += delegate { TaiDuLieu(); };
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                Padding = new Padding(12, 76, 12, 12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            lblHeader = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Thong tin khach hang",
                Font = new Font("Roboto", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(lblHeader, 0, 0);

            var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            cboToa = CreateComboBox(190);
            cboLoai = CreateComboBox(190);
            numGiaToiDa = new NumericUpDown
            {
                Width = 170,
                Minimum = 0,
                Maximum = 1000000000000,
                ThousandsSeparator = true
            };
            var btnLamMoi = new MaterialButton { Text = "Lam moi", AutoSize = true };
            btnLamMoi.Click += delegate { TaiDuLieu(); };
            btnQrThanhToan = new MaterialButton { Text = "Hien QR hoa don", AutoSize = true };
            btnQrThanhToan.Click += BtnQrThanhToan_Click;
            cboToa.SelectedIndexChanged += delegate { TaiDuLieu(); };
            cboLoai.SelectedIndexChanged += delegate { TaiDuLieu(); };
            numGiaToiDa.ValueChanged += delegate { TaiDuLieu(); };

            filters.Controls.Add(new Label { Text = "Toa", AutoSize = true, Padding = new Padding(0, 9, 4, 0) });
            filters.Controls.Add(cboToa);
            filters.Controls.Add(new Label { Text = "Loai", AutoSize = true, Padding = new Padding(12, 9, 4, 0) });
            filters.Controls.Add(cboLoai);
            filters.Controls.Add(new Label { Text = "Gia toi da", AutoSize = true, Padding = new Padding(12, 9, 4, 0) });
            filters.Controls.Add(numGiaToiDa);
            filters.Controls.Add(btnLamMoi);
            filters.Controls.Add(btnQrThanhToan);
            root.Controls.Add(filters, 0, 1);

            roomCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(4),
                BackColor = Color.White
            };
            gridHopDong = CreateGrid();
            gridHoaDon = CreateGrid();

            root.Controls.Add(CreateGroup("Can ho dang trong", roomCards), 0, 2);
            root.Controls.Add(CreateGroup("Hop dong cua toi", gridHopDong), 0, 3);
            root.Controls.Add(CreateGroup("Hoa don cua toi", gridHoaDon), 0, 4);
            Controls.Add(root);
        }

        private void NapBoLoc()
        {
            cboToa.DisplayMember = "TenToa";
            cboToa.ValueMember = "MaToa";
            cboToa.DataSource = _toaService.LayTatCa().OrderBy(t => t.TenToa).ToList();
            cboToa.SelectedIndex = -1;

            cboLoai.DisplayMember = "TenLoai";
            cboLoai.ValueMember = "MaLoai";
            cboLoai.DataSource = _loaiService.LayTatCa().OrderBy(l => l.TenLoai).ToList();
            cboLoai.SelectedIndex = -1;
        }

        private ComboBox CreateComboBox(int width)
        {
            return new ComboBox
            {
                Width = width,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private GroupBox CreateGroup(string text, Control content)
        {
            var group = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(8)
            };
            content.Dock = DockStyle.Fill;
            group.Controls.Add(content);
            return group;
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
        }

        private void TaiDuLieu()
        {
            if (roomCards == null || gridHopDong == null || gridHoaDon == null) return;
            lblHeader.Text = string.Format("Xin chao {0} [{1}]", SessionContext.HoTen, SessionContext.MaNguoiDung);
            TaiPhongTrong();
            TaiHopDongVaHoaDon();
        }

        private void TaiPhongTrong()
        {
            roomCards.Controls.Clear();

            var data = _canHoService.LayTheoTinhTrang("Trong");
            if (cboToa.SelectedValue != null)
            {
                var maToa = cboToa.SelectedValue.ToString();
                data = data.Where(c => c.MaToa == maToa);
            }
            if (cboLoai.SelectedValue != null)
            {
                var maLoai = cboLoai.SelectedValue.ToString();
                data = data.Where(c => c.MaLoai == maLoai);
            }
            if (numGiaToiDa.Value > 0)
            {
                var giaToiDa = numGiaToiDa.Value;
                data = data.Where(c => c.GiaThueNiemYet <= giaToiDa);
            }

            var rooms = data.OrderBy(c => c.MaToa).ThenBy(c => c.SoCanHo).ToList();
            if (rooms.Count == 0)
            {
                roomCards.Controls.Add(new Label
                {
                    Text = "Khong co can ho phu hop.",
                    AutoSize = true,
                    Padding = new Padding(12)
                });
                return;
            }

            foreach (var room in rooms)
                roomCards.Controls.Add(CreateRoomCard(room));
        }

        private Control CreateRoomCard(CanHo room)
        {
            var card = new Panel
            {
                Width = 250,
                Height = 142,
                Margin = new Padding(4, 4, 10, 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            card.Controls.Add(new Label
            {
                Text = string.Format("{0} - Can {1}", room.MaToa, room.SoCanHo),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            });
            card.Controls.Add(new Label
            {
                Text = string.Format("Loai: {0} | Tang: {1}", room.MaLoai, room.TangSo),
                Location = new Point(10, 38),
                AutoSize = true
            });
            card.Controls.Add(new Label
            {
                Text = string.Format("Gia: {0:N0} | Coc: {1:N0}", room.GiaThueNiemYet, room.TienCocNiemYet),
                Location = new Point(10, 62),
                AutoSize = true
            });

            var btnDatPhong = new MaterialButton
            {
                Text = "Dat phong",
                AutoSize = true,
                Location = new Point(8, 92)
            };
            btnDatPhong.Click += delegate { DatPhong(room); };
            card.Controls.Add(btnDatPhong);
            return card;
        }

        private void DatPhong(CanHo room)
        {
            if (!SessionContext.LaKhachThue || string.IsNullOrWhiteSpace(SessionContext.MaNguoiDung))
            {
                MessageBox.Show("Chi khach hang moi duoc dat phong.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tienCoc = room.TienCocNiemYet > 0 ? room.TienCocNiemYet : room.GiaThueNiemYet;
            var confirm = MessageBox.Show(
                string.Format("Dat phong {0} voi tien coc {1:N0}?", room.MaCanHo, tienCoc),
                "Xac nhan dat phong",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var phieu = new PhieuDatTruoc
            {
                MaCanHo = room.MaCanHo,
                MaKhach = SessionContext.MaNguoiDung,
                SoTienDatCoc = tienCoc,
                NgayHetHan = DateTime.Today.AddDays(3),
                PhuongThucThanhToan = "KhachDatPhong",
                GhiChu = "Khach hang dat phong tu trang ca nhan"
            };

            string loi;
            if (!_phieuDatTruocService.TaoPhieu(phieu, out loi))
            {
                MessageBox.Show(loi, "Khong the dat phong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Da tao phieu dat phong. Nhan vien se tiep tuc xu ly ky hop dong.",
                "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
            TaiDuLieu();
        }

        private void TaiHopDongVaHoaDon()
        {
            var hopDongs = _hopDongService.LayTheoKhach(SessionContext.MaNguoiDung)
                .OrderByDescending(h => h.NgayTao)
                .ToList();
            gridHopDong.DataSource = new BindingList<object>(hopDongs.Select(h => new
            {
                h.MaHopDong,
                h.MaCanHo,
                h.NgayBatDau,
                h.NgayKetThuc,
                h.GiaThueChot,
                h.TienCocChot,
                CocTruocDaTru = h.TienCocTruocDaTru,
                CocConPhaiNop = h.TienCocConPhaiNop,
                h.TrangThai
            }).Cast<object>().ToList());

            var maHopDongs = hopDongs.Select(h => h.MaHopDong).ToList();
            var hoaDons = maHopDongs
                .SelectMany(ma => _hoaDonService.LayTheoHopDong(ma))
                .OrderByDescending(h => h.NgayDaoHan)
                .ToList();
            gridHoaDon.DataSource = new BindingList<HoaDonThanhToan>(hoaDons);
            foreach (DataGridViewColumn column in gridHoaDon.Columns)
            {
                if (column.Name == "MaNguoiThaoTac" || column.Name == "VaiTroNguoiThaoTac" ||
                    column.Name == "MaNhanVienThu" || column.Name == "MaViPham")
                    column.Visible = false;
            }
        }

        private void BtnQrThanhToan_Click(object sender, EventArgs e)
        {
            if (gridHoaDon.CurrentRow == null)
            {
                MessageBox.Show("Chon hoa don can thanh toan.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var hoaDon = gridHoaDon.CurrentRow.DataBoundItem as HoaDonThanhToan;
            if (hoaDon == null) return;
            if (hoaDon.SoTienPhaiTra <= hoaDon.SoTienDaTra)
            {
                MessageBox.Show("Hoa don nay da thanh toan du.", "Thong bao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            new frmQrThanhToan(hoaDon).ShowDialog(this);
        }
    }
}
