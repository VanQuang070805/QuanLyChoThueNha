using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyChoThueNha.BLL.Helpers;
using QuanLyChoThueNha.DAL.Interfaces;
using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Services
{
    public class EmailLogService : BaseService<EmailLog>
    {
        protected override IRepository<EmailLog> Repo => _uow.EmailLogs;

        private string SinhMa()
        {
            int max = 0;
            foreach (var x in _uow.EmailLogs.GetAll())
                max = Math.Max(max, MaGenerator.LaySoThuTu(x.MaEmailLog, 3));
            return MaGenerator.Sinh("EML", max, 3);
        }

        public void GhiLogDatTruoc(string maPhieuDatTruoc, string maTaiKhoan, string emailNguoiNhan,
            string tieuDe, bool thanhCong, string thongBao)
        {
            var log = new EmailLog
            {
                MaEmailLog = SinhMa(),
                MaPhieuDatTruoc = maPhieuDatTruoc,
                MaTaiKhoan = maTaiKhoan,
                EmailNguoiNhan = string.IsNullOrWhiteSpace(emailNguoiNhan) ? "(khong co email)" : emailNguoiNhan.Trim(),
                LoaiEmail = "ThongTinDatTruoc",
                TieuDe = string.IsNullOrWhiteSpace(tieuDe) ? "Thong tin dat truoc" : tieuDe,
                TrangThai = thanhCong ? "ThanhCong" : "ThatBai",
                ThongBao = thongBao,
                NgayGui = DateTime.Now,
                MaNguoiThaoTac = SessionContext.MaNguoiDung,
                VaiTroNguoiThaoTac = SessionContext.VaiTro
            };
            Repo.Add(log);
            _uow.Complete();
        }

        public override IEnumerable<EmailLog> LayTatCa()
        {
            return base.LayTatCa().OrderByDescending(x => x.NgayGui).ToList();
        }
    }
}
