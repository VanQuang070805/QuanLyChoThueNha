using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.DAL.Repositories;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class ChucNangNhanVienDto
    {
        public string MaChucNang { get; set; }
        public string TenChucNang { get; set; }
        public string MoTa { get; set; }
        public bool MacDinh { get; set; }
    }

    public class NhanVienPhanQuyenService
    {
        public const string Dashboard = "Dashboard";
        public const string TaiSan = "TaiSan";
        public const string HopDong = "HopDong";
        public const string ThanhToan = "ThanhToan";
        public const string BaoCao = "BaoCao";

        private readonly IUnitOfWork _uow;

        public NhanVienPhanQuyenService()
        {
            _uow = new UnitOfWork();
        }

        public static List<ChucNangNhanVienDto> DanhSachChucNang()
        {
            return new List<ChucNangNhanVienDto>
            {
                new ChucNangNhanVienDto
                {
                    MaChucNang = Dashboard,
                    TenChucNang = "Tong quan cong viec",
                    MoTa = "Xem trang cong viec nhan vien, phieu cho coc, hop dong sap het han, hoa don can xu ly.",
                    MacDinh = true
                },
                new ChucNangNhanVienDto
                {
                    MaChucNang = TaiSan,
                    TenChucNang = "Tai san va danh muc",
                    MoTa = "Quan ly khu vuc, toa nha, loai can ho, can ho, tien nghi va gia dich vu.",
                    MacDinh = true
                },
                new ChucNangNhanVienDto
                {
                    MaChucNang = HopDong,
                    TenChucNang = "Khach thue va hop dong",
                    MoTa = "Tao khach thue, xu ly phieu dat truoc, ky hop dong va gia han.",
                    MacDinh = true
                },
                new ChucNangNhanVienDto
                {
                    MaChucNang = ThanhToan,
                    TenChucNang = "Thanh toan va tra nha",
                    MoTa = "Lap hoa don, ghi nhan thanh toan, xu ly tra nha va vi pham.",
                    MacDinh = true
                },
                new ChucNangNhanVienDto
                {
                    MaChucNang = BaoCao,
                    TenChucNang = "Bao cao thong ke",
                    MoTa = "Xem bao cao doanh thu, cong no, tinh trang can ho va bieu do quan tri.",
                    MacDinh = false
                }
            };
        }

        public IEnumerable<NhanVienQuanLy> LayNhanVien()
        {
            return _uow.NhanVienQuanLys.GetAll().OrderBy(n => n.HoTen).ToList();
        }

        public Dictionary<string, bool> LayQuyen(string maNhanVien)
        {
            var result = DanhSachChucNang().ToDictionary(x => x.MaChucNang, x => x.MacDinh);
            if (string.IsNullOrWhiteSpace(maNhanVien)) return result;

            foreach (var quyen in _uow.NhanVienQuyens.Find(q => q.MaNhanVien == maNhanVien))
                result[quyen.MaChucNang] = quyen.DuocTruyCap;

            return result;
        }

        public bool CoQuyen(string maNhanVien, string maChucNang)
        {
            if (string.IsNullOrWhiteSpace(maNhanVien)) return false;
            var quyen = _uow.NhanVienQuyens.FirstOrDefault(q =>
                q.MaNhanVien == maNhanVien && q.MaChucNang == maChucNang);
            if (quyen != null) return quyen.DuocTruyCap;

            var chucNang = DanhSachChucNang().FirstOrDefault(x => x.MaChucNang == maChucNang);
            return chucNang != null && chucNang.MacDinh;
        }

        public bool LuuQuyen(string maNhanVien, IDictionary<string, bool> quyenMoi, out string loi)
        {
            loi = string.Empty;
            if (string.IsNullOrWhiteSpace(maNhanVien))
            {
                loi = "Chon nhan vien can phan quyen.";
                return false;
            }
            if (_uow.NhanVienQuanLys.GetById(maNhanVien) == null)
            {
                loi = "Nhan vien khong ton tai.";
                return false;
            }

            var hopLe = new HashSet<string>(DanhSachChucNang().Select(x => x.MaChucNang));
            _uow.BeginTransaction();
            try
            {
                foreach (var pair in quyenMoi.Where(x => hopLe.Contains(x.Key)))
                {
                    var item = _uow.NhanVienQuyens.FirstOrDefault(q =>
                        q.MaNhanVien == maNhanVien && q.MaChucNang == pair.Key);

                    if (item == null)
                    {
                        item = new NhanVienQuyen
                        {
                            MaNhanVien = maNhanVien,
                            MaChucNang = pair.Key,
                            DuocTruyCap = pair.Value,
                            NgayCapNhat = DateTime.Now
                        };
                        _uow.NhanVienQuyens.Add(item);
                    }
                    else
                    {
                        item.DuocTruyCap = pair.Value;
                        item.NgayCapNhat = DateTime.Now;
                        _uow.NhanVienQuyens.Update(item);
                    }
                }

                _uow.Complete();
                _uow.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _uow.RollbackTransaction();
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                loi = inner.Message;
                return false;
            }
        }
    }
}
