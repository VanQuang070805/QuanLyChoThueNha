using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class GiaHanHopDongService : BaseService<GiaHanHopDong>
    {
        protected override IRepository<GiaHanHopDong> Repo => _uow.GiaHanHopDongs;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.GiaHanHopDongs.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaGiaHan, 2));
            return MaGenerator.Sinh("GH", max);
        }

        public bool YeuCauGiaHan(string maHopDong, DateTime ngayKetThucMoi, string maNhanVien, out string loi)
        {
            loi = string.Empty;
            var hd = _uow.HopDongs.GetById(maHopDong);
            if (hd == null) { loi = "Hop dong khong ton tai."; return false; }
            if (hd.TrangThai != "HieuLuc") { loi = "Chi gia han hop dong dang hieu luc."; return false; }
            if (ngayKetThucMoi <= hd.NgayKetThuc) { loi = "Ngay ket thuc moi phai sau ngay ket thuc hien tai."; return false; }

            var gh = new GiaHanHopDong
            {
                MaGiaHan = SinhMa(),
                MaHopDong = maHopDong,
                MaNhanVien = maNhanVien,
                MaNguoiThaoTac = SessionContext.DaXacThuc ? SessionContext.MaNguoiDung : null,
                VaiTroNguoiThaoTac = SessionContext.DaXacThuc ? SessionContext.VaiTro : null,
                NgayKetThucCu = hd.NgayKetThuc,
                NgayKetThucMoi = ngayKetThucMoi,
                TrangThai = "ChoXetDuyet",
                NgayYeuCau = DateTime.Now
            };
            base.Them(gh);
            return true;
        }

        public bool ChapThuan(string maGiaHan, out string loi)
        {
            loi = string.Empty;
            var gh = LayTheoMa(maGiaHan);
            if (gh == null) { loi = "Khong tim thay phieu gia han."; return false; }

            _uow.BeginTransaction();
            try
            {
                gh.TrangThai = "ChapThuan";
                gh.NgayDuyet = DateTime.Now;
                _uow.GiaHanHopDongs.Update(gh);

                var hd = _uow.HopDongs.GetById(gh.MaHopDong);
                if (hd != null)
                {
                    hd.NgayKetThuc = gh.NgayKetThucMoi;
                    hd.TrangThai = "HieuLuc";
                    _uow.HopDongs.Update(hd);
                }

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
    }

    public class HoaDonThanhToanService : BaseService<HoaDonThanhToan>
    {
        protected override IRepository<HoaDonThanhToan> Repo => _uow.HoaDonThanhToans;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.HoaDonThanhToans.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaHoaDon, 6));
            return MaGenerator.Sinh("HOADON", max, 6);
        }

        public bool TaoHoaDon(HoaDonThanhToan hoaDon, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(hoaDon.MaHopDong, "Hop dong", out loi)) return false;
            if (!ValidationHelper.KhongRong(hoaDon.MaLoaiHoaDon, "Loai hoa don", out loi)) return false;
            if (!ValidationHelper.KhongRong(hoaDon.KyThanhToan, "Ky thanh toan", out loi)) return false;
            if (!Regex.IsMatch(hoaDon.KyThanhToan, @"^(0[1-9]|1[0-2])/[0-9]{4}$"))
            {
                loi = "Ky thanh toan phai co dinh dang MM/yyyy.";
                return false;
            }
            if (!ValidationHelper.SoDuong(hoaDon.SoTienPhaiTra, "So tien phai tra", out loi)) return false;
            if (hoaDon.SoTienDaTra < 0) { loi = "So tien da tra khong duoc am."; return false; }
            if (_uow.HopDongs.GetById(hoaDon.MaHopDong) == null) { loi = "Hop dong khong ton tai."; return false; }
            if (_uow.LoaiHoaDons.GetById(hoaDon.MaLoaiHoaDon) == null) { loi = "Loai hoa don khong ton tai."; return false; }
            // BỔ SUNG: không cho lập 2 hóa đơn cùng KỲ + cùng LOẠI trên cùng 1 hợp đồng (tránh trùng).
            if (_uow.HoaDonThanhToans.Any(h => h.MaHopDong == hoaDon.MaHopDong
                    && h.MaLoaiHoaDon == hoaDon.MaLoaiHoaDon
                    && h.KyThanhToan == hoaDon.KyThanhToan))
            {
                loi = "Da ton tai hoa don cung loai cho ky thanh toan nay.";
                return false;
            }
            if (!string.IsNullOrWhiteSpace(hoaDon.MaViPham) && _uow.PhieuXuLyViPhams.GetById(hoaDon.MaViPham) == null)
            {
                loi = "Phieu vi pham khong ton tai.";
                return false;
            }

            hoaDon.MaHoaDon = SinhMa();
            hoaDon.TrangThai = hoaDon.SoTienDaTra >= hoaDon.SoTienPhaiTra ? "DaTra" : "ChuaTra";
            if (hoaDon.TrangThai == "DaTra" && hoaDon.NgayThanhToan == null) hoaDon.NgayThanhToan = DateTime.Now;
            AuditHelper.GanNguoiThaoTac(hoaDon);
            base.Them(hoaDon);
            return true;
        }

        public decimal TinhTienDienNuocDichVu(string maHopDong, decimal soDien, decimal soNuoc, bool tinhDichVuChung, out string loi)
        {
            loi = string.Empty;
            if (soDien < 0 || soNuoc < 0) { loi = "So dien/so nuoc khong duoc am."; return 0; }

            var hopDong = _uow.HopDongs.GetById(maHopDong);
            if (hopDong == null) { loi = "Hop dong khong ton tai."; return 0; }
            var canHo = _uow.CanHos.GetById(hopDong.MaCanHo);
            if (canHo == null) { loi = "Can ho cua hop dong khong ton tai."; return 0; }
            var gia = _uow.GiaDichVus.FirstOrDefault(g => g.MaToa == canHo.MaToa && g.DangApDung);
            if (gia == null) { loi = "Chua co bang gia dich vu dang ap dung cho toa nha nay."; return 0; }

            return soDien * gia.GiaDien + soNuoc * gia.GiaNuoc + (tinhDichVuChung ? gia.GiaDichVuChung : 0);
        }

        // ===== BỔ SUNG (Bước 3): lấy chỉ số điện/nước kỳ gần nhất của hợp đồng =====
        // Dùng làm chỉ số CŨ cho kỳ mới, tự động điền -> nhân viên chỉ cần nhập chỉ số MỚI.
        public void LayChiSoKyTruoc(string maHopDong, out double dienCu, out double nuocCu)
        {
            dienCu = 0;
            nuocCu = 0;
            var hoaDonGanNhat = _uow.HoaDonThanhToans
                .Find(h => h.MaHopDong == maHopDong && h.ChiSoDienMoi > 0)
                .OrderByDescending(h => h.KyThanhToan)
                .FirstOrDefault();
            if (hoaDonGanNhat != null)
            {
                dienCu = hoaDonGanNhat.ChiSoDienMoi;
                nuocCu = hoaDonGanNhat.ChiSoNuocMoi;
            }
        }

        // ===== BỔ SUNG (Bước 3): tính tiền dựa trên chỉ số cũ/mới và lưu lại chỉ số =====
        // Tự suy ra lượng tiêu thụ = (mới - cũ), tính tiền, đồng thời trả ra chỉ số để lưu vào hóa đơn.
        public decimal TinhTienTheoChiSo(string maHopDong, double dienCu, double dienMoi,
            double nuocCu, double nuocMoi, bool tinhDichVuChung, out string loi)
        {
            loi = string.Empty;
            if (dienMoi < dienCu) { loi = "Chi so dien moi phai >= chi so dien cu."; return 0; }
            if (nuocMoi < nuocCu) { loi = "Chi so nuoc moi phai >= chi so nuoc cu."; return 0; }

            decimal soDienTieuThu = (decimal)(dienMoi - dienCu);
            decimal soNuocTieuThu = (decimal)(nuocMoi - nuocCu);
            return TinhTienDienNuocDichVu(maHopDong, soDienTieuThu, soNuocTieuThu, tinhDichVuChung, out loi);
        }

        public bool ThanhToan(string maHoaDon, decimal soTienThanhToan, string phuongThuc, string maNhanVien, out string loi)
        {
            loi = string.Empty;
            var hd = LayTheoMa(maHoaDon);
            if (hd == null) { loi = "Khong tim thay hoa don."; return false; }
            if (hd.TrangThai == "DaTra") { loi = "Hoa don da duoc thanh toan."; return false; }
            if (soTienThanhToan <= 0) { loi = "So tien thanh toan phai lon hon 0."; return false; }

            hd.SoTienDaTra = soTienThanhToan;
            hd.NgayThanhToan = DateTime.Now;
            hd.PhuongThucThanhToan = phuongThuc;
            hd.MaNhanVienThu = maNhanVien;
            hd.TrangThai = soTienThanhToan >= hd.SoTienPhaiTra ? "DaTra" : "TraThieu";
            AuditHelper.GanNguoiThaoTac(hd);
            Sua(hd);
            return true;
        }

        public IEnumerable<HoaDonThanhToan> LayTheoHopDong(string maHopDong)
        {
            return Tim(h => h.MaHopDong == maHopDong);
        }

        public IEnumerable<HoaDonThanhToan> LayQuaHan()
        {
            return Tim(h => h.TrangThai == "ChuaTra" && h.NgayDaoHan < DateTime.Today);
        }

        // ===== BỔ SUNG: tự động chuyển hóa đơn "ChuaTra" đã quá ngày đáo hạn sang "QuaHan" =====
        // Trước đây chỉ tô màu ở GUI mà không cập nhật dữ liệu; nay trạng thái được lưu thật.
        private void CapNhatHoaDonQuaHan()
        {
            var quaHan = _uow.HoaDonThanhToans
                .Find(h => h.TrangThai == "ChuaTra" && h.NgayDaoHan < DateTime.Today)
                .ToList();
            if (quaHan.Count == 0) return;
            foreach (var h in quaHan)
            {
                h.TrangThai = "QuaHan";
                _uow.HoaDonThanhToans.Update(h);
            }
            _uow.Complete();
        }

        public override IEnumerable<HoaDonThanhToan> LayTatCa()
        {
            CapNhatHoaDonQuaHan();
            return base.LayTatCa();
        }
    }

    public class PhieuTraNhaService : BaseService<PhieuTraNha>
    {
        protected override IRepository<PhieuTraNha> Repo => _uow.PhieuTraNhas;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.PhieuTraNhas.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaPhieu, 3));
            return MaGenerator.Sinh("PTN", max, 3);
        }

        public bool LapPhieu(PhieuTraNha phieu, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(phieu.MaHopDong, "Hop dong", out loi)) return false;

            var hd = _uow.HopDongs.GetById(phieu.MaHopDong);
            if (hd == null) { loi = "Hop dong khong ton tai."; return false; }
            if (_uow.PhieuTraNhas.Any(p => p.MaHopDong == phieu.MaHopDong))
            {
                loi = "Hop dong nay da co phieu tra nha.";
                return false;
            }

            // ===== BỔ SUNG (Bước 5.2): tự động tổng hợp phí vi phạm "trừ vào cọc" =====
            // Lấy mọi phiếu vi phạm của hợp đồng có cờ TruVaoCoc = true và chưa khấu trừ.
            var dsViPham = _uow.PhieuXuLyViPhams
                .Find(v => v.MaHopDong == phieu.MaHopDong && v.TruVaoCoc && v.TinhTrang == "ChoXuLy")
                .ToList();
            decimal tongPhatTruCoc = dsViPham.Sum(v => v.PhiBoiThuong);

            // Tiền khấu trừ = phần nhân viên tự nhập (nếu có) + tổng phí vi phạm trừ cọc.
            decimal tienKhauTru = phieu.TienKhauTru + tongPhatTruCoc;

            // Tiền hoàn cọc tự tính = tiền cọc chốt trên HĐ - tiền khấu trừ (không âm).
            // -> Trường tự sinh, GUI không cần (và không nên) tự tính nữa.
            decimal tienHoanCoc = Math.Max(0, hd.TienCocChot - tienKhauTru);

            if (phieu.NgayTra == DateTime.MinValue) phieu.NgayTra = DateTime.Today;

            _uow.BeginTransaction();
            try
            {
                phieu.MaPhieu = SinhMa();
                phieu.TienKhauTru = tienKhauTru;
                phieu.TienHoanCoc = tienHoanCoc;
                AuditHelper.GanNguoiThaoTac(phieu);
                _uow.PhieuTraNhas.Add(phieu);

                // Đánh dấu các phiếu vi phạm đã được khấu trừ vào cọc + liên kết về phiếu trả nhà.
                foreach (var v in dsViPham)
                {
                    v.TinhTrang = "DaKhauTru";
                    v.MaPhieuTraNha = phieu.MaPhieu;
                    _uow.PhieuXuLyViPhams.Update(v);
                }

                hd.TrangThai = "HetHan";
                _uow.HopDongs.Update(hd);

                var canHo = _uow.CanHos.GetById(hd.MaCanHo);
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
    }

    public class PhieuXuLyViPhamService : BaseService<PhieuXuLyViPham>
    {
        protected override IRepository<PhieuXuLyViPham> Repo => _uow.PhieuXuLyViPhams;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.PhieuXuLyViPhams.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaViPham, 2));
            return MaGenerator.Sinh("VP", max);
        }

        public bool GhiNhan(PhieuXuLyViPham phieu, out string loi)
        {
            loi = string.Empty;
            if (!ValidationHelper.KhongRong(phieu.MaHopDong, "Hop dong", out loi)) return false;
            if (_uow.HopDongs.GetById(phieu.MaHopDong) == null) { loi = "Hop dong khong ton tai."; return false; }
            if (!ValidationHelper.KhongRong(phieu.LoaiViPham, "Loai vi pham", out loi)) return false;
            // BỔ SUNG: nếu vi phạm bị trừ vào cọc thì bắt buộc phí bồi thường > 0.
            if (phieu.TruVaoCoc && phieu.PhiBoiThuong <= 0)
            {
                loi = "Vi pham tru vao coc phai co phi boi thuong lon hon 0.";
                return false;
            }

            phieu.MaViPham = SinhMa();
            phieu.NgayGhiNhan = DateTime.Now;
            phieu.TinhTrang = "ChoXuLy";
            AuditHelper.GanNguoiThaoTac(phieu);
            base.Them(phieu);
            return true;
        }

        public IEnumerable<PhieuXuLyViPham> LayTheoHopDong(string maHopDong)
        {
            return Tim(p => p.MaHopDong == maHopDong);
        }
    }
}
