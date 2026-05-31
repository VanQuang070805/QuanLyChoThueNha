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
        public const string ChoThanhToanCoc = "ChoThanhToanCoc";
        public const string DaThanhToanCoc = "DaThanhToanCoc";
        public const string ChoKy = "ChoKy";
        public const string DaKyHD = "DaKyHD";
        public const string Huy = "Huy";
        public const string HetHan = "HetHan";

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
        public List<PhieuDatTruoc> XuLyPhieuChoCocQuaHan24h()
        {
            var mocQuaHan = DateTime.Now.AddHours(-24);
            var quaHan = _uow.PhieuDatTruocs
                .Find(p => p.TrangThai == ChoThanhToanCoc &&
                           p.NgayDatCoc <= mocQuaHan)
                .ToList();

            if (quaHan.Count == 0) return quaHan;

            foreach (var p in quaHan)
            {
                p.TrangThai = HetHan;
                p.GhiChu = NoiGhiChu(p.GhiChu,
                    string.Format("Tu dong het han luc {0:dd/MM/yyyy HH:mm} do chua xac nhan coc sau 24h.", DateTime.Now));
                _uow.PhieuDatTruocs.Update(p);

                var canHo = _uow.CanHos.GetById(p.MaCanHo);
                if (canHo != null && canHo.TinhTrang == "DaDatCoc")
                {
                    canHo.TinhTrang = "Trong";
                    _uow.CanHos.Update(canHo);
                }
            }

            _uow.Complete();
            return quaHan;
        }

        private void CapNhatPhieuHetHan()
        {
            XuLyPhieuChoCocQuaHan24h();

            var quaHan = _uow.PhieuDatTruocs
                .Find(p => (p.TrangThai == DaThanhToanCoc ||
                            p.TrangThai == ChoKy) &&
                           p.NgayHetHan < DateTime.Today)
                .ToList();
            if (quaHan.Count == 0) return;

            foreach (var p in quaHan)
            {
                p.TrangThai = HetHan;
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
            if (phieu.NgayHetHan <= DateTime.Now) { loi = "Han thanh toan coc phai lon hon thoi diem hien tai."; return false; }

            var canHo = _uow.CanHos.GetById(phieu.MaCanHo);
            if (canHo == null) { loi = "Can ho khong ton tai. Vui long chon lai can ho."; return false; }
            if (canHo.TinhTrang != "Trong") { loi = "Can ho dang o trang thai: " + canHo.TinhTrang; return false; }

            var daCoPhieuMo = _uow.PhieuDatTruocs.Any(p =>
                p.MaCanHo == phieu.MaCanHo &&
                (p.TrangThai == ChoThanhToanCoc ||
                 p.TrangThai == DaThanhToanCoc ||
                 p.TrangThai == ChoKy));
            if (daCoPhieuMo)
            {
                loi = "Can ho da co phieu dat truoc dang xu ly. Vui long chon can ho khac.";
                return false;
            }

            var khach = _uow.KhachThues.GetById(phieu.MaKhach);
            if (khach == null) { loi = "Khach thue khong ton tai. Vui long chon lai khach thue."; return false; }

            _uow.BeginTransaction();
            try
            {
                phieu.MaPhieuDatTruoc = SinhMa();
                phieu.NgayDatCoc = DateTime.Now;
                phieu.TrangThai = ChoThanhToanCoc;
                AuditHelper.GanNguoiThaoTac(phieu);
                _uow.PhieuDatTruocs.Add(phieu);

                _uow.Complete();
                _uow.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _uow.RollbackTransaction();
                var _inner = ex; while (_inner.InnerException != null) _inner = _inner.InnerException;
                loi = _inner.Message;
                return false;
            }
        }

        public bool HuyPhieu(string maPhieu, out string loi)
        {
            loi = string.Empty;
            var phieu = LayTheoMa(maPhieu);
            if (phieu == null) { loi = "Khong tim thay phieu."; return false; }
            if (phieu.TrangThai != ChoThanhToanCoc &&
                phieu.TrangThai != DaThanhToanCoc &&
                phieu.TrangThai != ChoKy)
            {
                loi = "Chi huy duoc phieu dang cho coc hoac cho ky.";
                return false;
            }

            _uow.BeginTransaction();
            try
            {
                phieu.TrangThai = Huy;
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
                var _inner = ex; while (_inner.InnerException != null) _inner = _inner.InnerException;
                loi = _inner.Message;
                return false;
            }
        }

        public bool XacNhanDaNhanCoc(string maPhieu, out string loi)
        {
            loi = string.Empty;
            var phieu = LayTheoMa(maPhieu);
            if (phieu == null) { loi = "Khong tim thay phieu."; return false; }
            if (phieu.TrangThai != ChoThanhToanCoc && phieu.TrangThai != DaThanhToanCoc)
            {
                loi = "Chi xac nhan coc cho phieu dang cho thanh toan coc.";
                return false;
            }

            var canHo = _uow.CanHos.GetById(phieu.MaCanHo);
            if (canHo == null) { loi = "Khong tim thay can ho cua phieu."; return false; }
            if (canHo.TinhTrang != "Trong" && canHo.TinhTrang != "DaDatCoc")
            {
                loi = "Phong khong con trong hoac dang giu coc.";
                return false;
            }

            phieu.TrangThai = ChoKy;
            phieu.PhuongThucThanhToan = string.IsNullOrWhiteSpace(phieu.PhuongThucThanhToan)
                ? "DaNhanCoc"
                : phieu.PhuongThucThanhToan;
            phieu.GhiChu = NoiGhiChu(phieu.GhiChu,
                string.Format("Da xac nhan nhan coc luc {0:dd/MM/yyyy HH:mm}.", DateTime.Now));
            _uow.PhieuDatTruocs.Update(phieu);
            canHo.TinhTrang = "DaDatCoc";
            _uow.CanHos.Update(canHo);
            _uow.Complete();
            return true;
        }

        private static string NoiGhiChu(string current, string note)
        {
            if (string.IsNullOrWhiteSpace(current)) return note;
            return current + Environment.NewLine + note;
        }
    }
}
