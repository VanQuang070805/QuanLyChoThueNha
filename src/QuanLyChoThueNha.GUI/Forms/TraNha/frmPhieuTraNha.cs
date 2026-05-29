using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.BLL.Services;
using QuanLyChoThueNha.GUI.Forms.Shared;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.GUI.Forms.TraNha
{
    public class frmPhieuTraNha : CrudFormBase<PhieuTraNha>
    {
        private readonly PhieuTraNhaService _service = new PhieuTraNhaService();

        public frmPhieuTraNha() : base("Quan ly Phieu tra nha", Fields())
        {
        }

        private static IEnumerable<FieldDefinition> Fields()
        {
            var hopDongService = new HopDongService();
            var hopDongOptions = hopDongService.LayTatCa()
                .OrderBy(h => h.MaHopDong)
                .Select(h => new ComboOption(h.MaHopDong,
                    string.Format("{0} - {1} - {2}", h.MaHopDong, h.MaCanHo, h.TrangThai)))
                .ToList();

            return new[]
            {
                new FieldDefinition("MaPhieu", "Ma phieu", typeof(string), true),
                FieldDefinition.Lookup("MaHopDong", "Hop dong", hopDongOptions),
                new FieldDefinition("MaNhanVien", "Ma nhan vien", typeof(string), true),
                new FieldDefinition("NgayTra", "Ngay tra", typeof(DateTime)),
                new FieldDefinition("TinhTrangNha", "Tinh trang nha", typeof(string), false, null, true),
                new FieldDefinition("TienHoanCoc", "Tien hoan coc (tu tinh)", typeof(decimal), true),
                new FieldDefinition("TienKhauTru", "Tien khau tru", typeof(decimal)),
                new FieldDefinition("GhiChu", "Ghi chu", typeof(string), false, null, true)
            };
        }

        protected override IEnumerable<PhieuTraNha> GetItems() { return _service.LayTatCa(); }

        protected override bool AddItem(PhieuTraNha item, out string error)
        {
            // Gán mã người lập phiếu (NhanVien hoặc Admin) từ phiên đăng nhập.
            item.MaNhanVien = SessionContext.MaNguoiDung;
            if (item.NgayTra == DateTime.MinValue) item.NgayTra = DateTime.Today;
            // KHÔNG tự tính TienHoanCoc ở GUI nữa — toàn bộ logic hoàn cọc/khấu trừ vi phạm
            // đã được dồn về PhieuTraNhaService.LapPhieu để đảm bảo nhất quán.
            return _service.LapPhieu(item, out error);
        }

        protected override bool UpdateItem(PhieuTraNha item, out string error)
        {
            error = string.Empty;
            _service.Sua(item);
            return true;
        }

        protected override bool DeleteItem(PhieuTraNha item, out string error)
        {
            error = string.Empty;
            _service.Xoa(item);
            return true;
        }
    }
}
