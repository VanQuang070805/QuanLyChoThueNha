using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class PhieuDatTruocService : BaseService<PhieuDatTruoc>
    {
        protected override IRepository<PhieuDatTruoc> Repo => _uow.PhieuDatTruocs;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.PhieuDatTruocs.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaPhieuDatTruoc, 3));
            return MaGenerator.Sinh("PDT", max, 3);
        }

        // ===== BỔ SUNG (Bước 1): quét và tự động cho hết hạn các phiếu giữ phòng quá hạn =====
        // Khách không vào ở trong thời hạn giữ phòng -> phiếu chuyển "HetHan", khách MẤT cọc trước,
        // căn hộ được giải phóng về "Trong" để cho người khác thuê.
        private void CapNhatPhieuHetHan()
        {
            var quaHan = _uow.PhieuDatTruocs
                .Find(p => p.TrangThai == "ChoKy" && p.NgayHetHan < DateTime.Today)
                .ToList();
            if (quaHan.Count == 0) return;

            foreach (var p in quaHan)
            {
                p.TrangThai = "HetHan";
                _uow.PhieuDatTruocs.Update(p);

                var canHo = _uow.CanHos.GetById(p.MaCanHo);
                // Chỉ trả căn hộ về Trong nếu nó vẫn đang ở trạng thái giữ chỗ (DaDatCoc)
                if (canHo != null && canHo.TinhTrang == "DaDatCoc")
                {
                    canHo.TinhTrang = "Trong";
                    _uow.CanHos.Update(canHo);
                }
            }
            _uow.Complete();
        }

        public override IEnumerable<PhieuDatTruoc> LayTatCa()
        {
            CapNhatPhieuHetHan();
            return base.LayTatCa();
        }

        public bool TaoPhieu(PhieuDatTruoc phieu, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(phieu.MaCanHo, "Can ho", out loi)) return false;
            if (!ValidationHelper.KhongRong(phieu.MaKhach, "Khach thue", out loi)) return false;
            if (!ValidationHelper.SoDuong(phieu.SoTienDatCoc, "So tien dat coc", out loi)) return false;
            if (phieu.NgayHetHan <= DateTime.Today) { loi = "Ngay het han phai sau hom nay."; return false; }

            var canHo = _uow.CanHos.GetById(phieu.MaCanHo);
            if (canHo == null) { loi = "Can ho khong ton tai. Vui long chon lai can ho."; return false; }
            if (canHo.TinhTrang != "Trong") { loi = "Can ho dang o trang thai: " + canHo.TinhTrang; return false; }

            var khach = _uow.KhachThues.GetById(phieu.MaKhach);
            if (khach == null) { loi = "Khach thue khong ton tai. Vui long chon lai khach thue."; return false; }

            _uow.BeginTransaction();
            try
            {
                phieu.MaPhieuDatTruoc = SinhMa();
                phieu.NgayDatCoc = DateTime.Now;
                phieu.TrangThai = "ChoKy";
                _uow.PhieuDatTruocs.Add(phieu);

                canHo.TinhTrang = "DaDatCoc";
                _uow.CanHos.Update(canHo);

                _uow.Complete();
                _uow.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _uow.RollbackTransaction();
                loi = "Loi he thong: " + ex.Message;
                return false;
            }
        }

        public bool HuyPhieu(string maPhieu, out string loi)
        {
            loi = string.Empty;
            var phieu = LayTheoMa(maPhieu);
            if (phieu == null) { loi = "Khong tim thay phieu."; return false; }
            if (phieu.TrangThai != "ChoKy") { loi = "Chi huy duoc phieu o trang thai ChoKy."; return false; }

            _uow.BeginTransaction();
            try
            {
                phieu.TrangThai = "Huy";
                _uow.PhieuDatTruocs.Update(phieu);

                var canHo = _uow.CanHos.GetById(phieu.MaCanHo);
                if (canHo != null) { canHo.TinhTrang = "Trong"; _uow.CanHos.Update(canHo); }

                _uow.Complete();
                _uow.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _uow.RollbackTransaction();
                loi = "Loi he thong: " + ex.Message;
                return false;
            }
        }
    }
}
