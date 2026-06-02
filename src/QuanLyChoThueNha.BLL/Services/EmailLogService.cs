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
            if (!thanhCong)
                XoaEmailTaiKhoanNeuLoiNguoiNhan(maTaiKhoan, emailNguoiNhan, thongBao);
            _uow.Complete();
        }

        public override IEnumerable<EmailLog> LayTatCa()
        {
            return base.LayTatCa().OrderByDescending(x => x.NgayGui).ToList();
        }

        private void XoaEmailTaiKhoanNeuLoiNguoiNhan(string maTaiKhoan, string emailNguoiNhan, string thongBao)
        {
            if (string.IsNullOrWhiteSpace(maTaiKhoan) || string.IsNullOrWhiteSpace(emailNguoiNhan))
                return;
            if (!LaLoiEmailNguoiNhan(thongBao))
                return;

            var taiKhoan = _uow.TaiKhoans.GetById(maTaiKhoan);
            if (taiKhoan == null || string.IsNullOrWhiteSpace(taiKhoan.Email))
                return;
            if (!string.Equals(taiKhoan.Email.Trim(), emailNguoiNhan.Trim(), StringComparison.OrdinalIgnoreCase))
                return;

            taiKhoan.Email = null;
            _uow.TaiKhoans.Update(taiKhoan);
        }

        private static bool LaLoiEmailNguoiNhan(string thongBao)
        {
            if (string.IsNullOrWhiteSpace(thongBao))
                return false;

            var value = thongBao.ToLowerInvariant();
            return value.Contains("recipient") ||
                   value.Contains("mailbox") ||
                   value.Contains("user unknown") ||
                   value.Contains("unknown user") ||
                   value.Contains("no such user") ||
                   value.Contains("does not exist") ||
                   value.Contains("not exist") ||
                   value.Contains("invalid address") ||
                   value.Contains("bad recipient") ||
                   value.Contains("undeliverable") ||
                   value.Contains("550") ||
                   value.Contains("551") ||
                   value.Contains("553");
        }
    }
}
