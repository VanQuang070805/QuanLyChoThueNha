using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;
using System.Windows.Forms;
using HopDongEntity = QuanLyChoThueNha.Model.Entities.HopDong;

namespace QuanLyChoThueNha.GUI.Forms.TraNha
{
    public class frmPhieuXuLyViPham : CrudFormBase<PhieuXuLyViPham>
    {
        private readonly PhieuXuLyViPhamService _service = new PhieuXuLyViPhamService();

        public frmPhieuXuLyViPham() : base("Quan ly Xu ly vi pham", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongOptions = new HopDongService().LayTatCa()
                .Where(h => h.TrangThai == "HieuLuc")
                .OrderBy(h => h.MaHopDong)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} | Khach {1} | Phong {2}", h.MaHopDong, h.MaKhach, h.MaCanHo)))
                .ToList();
            var phieuTraOptions = new List<ComboOption> { new ComboOption(string.Empty, "(Khong gan phieu tra nha)") };
            phieuTraOptions.AddRange(new PhieuTraNhaService().LayTatCa()
                .OrderBy(p => p.MaPhieu)
                .Select(p => new ComboOption(p.MaPhieu,
                    string.Format("{0} | Hop dong {1} | {2:dd/MM/yyyy}", p.MaPhieu, p.MaHopDong, p.NgayTra))));

            return new[]
            {
                new FieldDefinition("MaViPham", "Ma vi pham", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Ma hop dong", hopDongOptions),
                FieldDefinition.Lookup("MaPhieuTraNha", "Ma phieu tra nha", phieuTraOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("LoaiViPham", "Loai vi pham"),
                new FieldDefinition("MoTa", "Mo ta", typeof(string), false, null, true),
                new FieldDefinition("PhiBoiThuong", "Phi boi thuong", typeof(decimal)),
                new FieldDefinition("TruVaoCoc", "Tru vao coc", typeof(bool)),
                new FieldDefinition("TinhTrang", "Tinh trang", typeof(string), false,
                    new[] { "ChoXuLy", "DaThanhToan" }),
                new FieldDefinition("NgayGhiNhan", "Ngay ghi nhan", typeof(System.DateTime), true)
            };
        }

        protected override IEnumerable<PhieuXuLyViPham> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(PhieuXuLyViPham item, out string error)
        {
            item.MaNhanVien = SessionContext.LaNhanVien ? SessionContext.MaNguoiDung : null;
            item.MaPhieuTraNha = string.IsNullOrWhiteSpace(item.MaPhieuTraNha) ? null : item.MaPhieuTraNha;
            if (item.TruVaoCoc) item.TinhTrang = "ChoXuLy";
            return _service.GhiNhan(item, out error);
        }

        protected override bool UpdateItem(PhieuXuLyViPham item, out string error)
        {
            error = string.Empty;
            item.MaPhieuTraNha = string.IsNullOrWhiteSpace(item.MaPhieuTraNha) ? null : item.MaPhieuTraNha;
            if (item.TruVaoCoc) item.TinhTrang = "ChoXuLy";
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(PhieuXuLyViPham item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }

        protected override void AfterGridBound()
        {
            if (!Grid.Columns.Contains("TenCanHo"))
            {
                Grid.Columns.Insert(2, new DataGridViewTextBoxColumn
                {
                    Name = "TenCanHo",
                    HeaderText = "Tên căn hộ",
                    ReadOnly = true
                });
            }
            if (!Grid.Columns.Contains("TenToa"))
            {
                Grid.Columns.Insert(3, new DataGridViewTextBoxColumn
                {
                    Name = "TenToa",
                    HeaderText = "Tên tòa",
                    ReadOnly = true
                });
            }

            var hopDongService = new HopDongService();
            var canHoService = new CanHoService();
            var toaService = new ToaService();

            var canHos = canHoService.LayTatCa().ToDictionary(c => c.MaCanHo);
            var toas = toaService.LayTatCa().ToDictionary(t => t.MaToa);
            var hopDongs = hopDongService.LayTatCa().ToDictionary(h => h.MaHopDong);

            foreach (DataGridViewRow row in Grid.Rows)
            {
                var viPham = row.DataBoundItem as PhieuXuLyViPham;
                if (viPham == null) continue;

                HopDongEntity hopDong;
                if (hopDongs.TryGetValue(viPham.MaHopDong, out hopDong))
                {
                    CanHo canHo;
                    if (canHos.TryGetValue(hopDong.MaCanHo, out canHo))
                    {
                        row.Cells["TenCanHo"].Value = "Căn " + canHo.SoCanHo;
                        Toa toa;
                        row.Cells["TenToa"].Value = toas.TryGetValue(canHo.MaToa, out toa) ? toa.TenToa : canHo.MaToa;
                        continue;
                    }
                }
                row.Cells["TenCanHo"].Value = string.Empty;
                row.Cells["TenToa"].Value = string.Empty;
            }
        }
    }
}
