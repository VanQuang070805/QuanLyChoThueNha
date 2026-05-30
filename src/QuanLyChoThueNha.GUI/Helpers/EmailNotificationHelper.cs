using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using QuanLyChoThueNha.BLL.Services;

namespace QuanLyChoThueNha.GUI.Helpers
{
    public static class EmailNotificationHelper
    {
        public static bool GuiThongTinDatTruoc(string email, string hoTen, string tenDangNhap,
            string matKhauTam, string maPhieu, string maPhong, decimal tienCoc, DateTime ngayHetHan,
            string noiDungChuyenKhoan, out string thongBao)
        {
            return GuiThongTinDatTruoc(email, hoTen, tenDangNhap, matKhauTam, maPhieu, maPhong,
                tienCoc, ngayHetHan, noiDungChuyenKhoan, null, out thongBao);
        }

        public static bool GuiThongTinDatTruoc(string email, string hoTen, string tenDangNhap,
            string matKhauTam, string maPhieu, string maPhong, decimal tienCoc, DateTime ngayHetHan,
            string noiDungChuyenKhoan, string maTaiKhoan, out string thongBao)
        {
            thongBao = string.Empty;
            var subject = "Thong tin tai khoan va dat coc phong";
            if (string.IsNullOrWhiteSpace(email))
            {
                thongBao = "Khach chua nhap email.";
                GhiLog(maPhieu, maTaiKhoan, email, subject, false, thongBao);
                return false;
            }

            if (!DocBool("EmailEnabled", false))
            {
                thongBao = "Chua bat gui email trong App.config.";
                GhiLog(maPhieu, maTaiKhoan, email, subject, false, thongBao);
                return false;
            }

            var host = Config("SmtpHost", string.Empty);
            var user = Config("SmtpUser", string.Empty);
            var password = Config("SmtpPassword", string.Empty);
            var from = Config("SmtpFrom", user);
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                thongBao = "Thieu cau hinh SMTP trong App.config.";
                GhiLog(maPhieu, maTaiKhoan, email, subject, false, thongBao);
                return false;
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(from, Config("SmtpFromName", "Quan ly cho thue nha"));
                    message.To.Add(email);
                    message.Subject = subject;
                    message.BodyEncoding = Encoding.UTF8;
                    message.SubjectEncoding = Encoding.UTF8;
                    message.Body = TaoNoiDung(hoTen, tenDangNhap, matKhauTam, maPhieu,
                        maPhong, tienCoc, ngayHetHan, noiDungChuyenKhoan);

                    using (var client = new SmtpClient(host, DocInt("SmtpPort", 587)))
                    {
                        client.EnableSsl = DocBool("SmtpEnableSsl", true);
                        if (!string.IsNullOrWhiteSpace(user))
                            client.Credentials = new NetworkCredential(user, password);

                        client.Send(message);
                    }
                }

                thongBao = "Da gui email cho khach.";
                GhiLog(maPhieu, maTaiKhoan, email, subject, true, thongBao);
                return true;
            }
            catch (Exception ex)
            {
                thongBao = "Khong gui duoc email: " + ex.Message;
                GhiLog(maPhieu, maTaiKhoan, email, subject, false, thongBao);
                return false;
            }
        }

        private static string TaoNoiDung(string hoTen, string tenDangNhap, string matKhauTam,
            string maPhieu, string maPhong, decimal tienCoc, DateTime ngayHetHan, string noiDungChuyenKhoan)
        {
            var body = new StringBuilder();
            body.AppendLine("Xin chao " + (string.IsNullOrWhiteSpace(hoTen) ? "quy khach" : hoTen) + ",");
            body.AppendLine();
            body.AppendLine("He thong da tao phieu dat truoc phong cho ban.");
            body.AppendLine("Ma phieu: " + maPhieu);
            body.AppendLine("Ma phong: " + maPhong);
            body.AppendLine("So tien dat coc: " + tienCoc.ToString("N0") + " VND");
            body.AppendLine("Han giu phong: " + ngayHetHan.ToString("dd/MM/yyyy"));
            body.AppendLine("Ngan hang: " + Config("PaymentBankCode", "MB"));
            body.AppendLine("So tai khoan: " + Config("PaymentAccountNo", "0000000000"));
            body.AppendLine("Chu tai khoan: " + Config("PaymentAccountName", "QUAN LY CHO THUE NHA"));
            body.AppendLine("Noi dung chuyen khoan: " + noiDungChuyenKhoan);
            body.AppendLine();
            body.AppendLine("Tai khoan dang nhap:");
            body.AppendLine("Ten dang nhap: " + tenDangNhap);
            body.AppendLine("Mat khau tam: " + matKhauTam);
            body.AppendLine();
            body.AppendLine("Vui long doi mat khau sau khi dang nhap.");
            return body.ToString();
        }

        private static string Config(string key, string fallback)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static int DocInt(string key, int fallback)
        {
            int value;
            return int.TryParse(ConfigurationManager.AppSettings[key], out value) ? value : fallback;
        }

        private static bool DocBool(string key, bool fallback)
        {
            bool value;
            return bool.TryParse(ConfigurationManager.AppSettings[key], out value) ? value : fallback;
        }

        private static void GhiLog(string maPhieu, string maTaiKhoan, string email,
            string subject, bool thanhCong, string thongBao)
        {
            try
            {
                new EmailLogService().GhiLogDatTruoc(maPhieu, maTaiKhoan, email, subject, thanhCong, thongBao);
            }
            catch
            {
                // Khong de loi ghi log lam hong luong tao phieu/gui QR cho khach.
            }
        }
    }
}
