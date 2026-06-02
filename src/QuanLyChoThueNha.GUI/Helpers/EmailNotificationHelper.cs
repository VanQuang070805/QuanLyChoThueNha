using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
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
            var subject = "SmartApart - Thong tin dat coc phong " + maPhong;
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
            var password = Environment.GetEnvironmentVariable("SMARTAPART_SMTP_PASSWORD");
            if (string.IsNullOrWhiteSpace(password))
                password = Config("SmtpPassword", string.Empty);
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
                    var qrImagePath = Config("PaymentQrImagePath", string.Empty);
                    var qrCid = File.Exists(qrImagePath) ? "smartapart_payment_qr" : null;
                    var body = TaoNoiDung(hoTen, tenDangNhap, matKhauTam, maPhieu,
                        maPhong, tienCoc, ngayHetHan, noiDungChuyenKhoan, qrCid);
                    message.IsBodyHtml = true;
                    if (qrCid == null)
                    {
                        message.Body = body;
                    }
                    else
                    {
                        var view = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Html);
                        var resource = new LinkedResource(qrImagePath, MediaTypeNames.Image.Jpeg)
                        {
                            ContentId = qrCid,
                            TransferEncoding = TransferEncoding.Base64
                        };
                        view.LinkedResources.Add(resource);
                        message.AlternateViews.Add(view);
                    }

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

        public static bool GuiThongBaoHetHanDatCoc(string email, string hoTen, string maPhieu,
            string maPhong, string maTaiKhoan, out string thongBao)
        {
            thongBao = string.Empty;
            var subject = "SmartApart - Phieu dat coc het han " + maPhieu;
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
            var password = Environment.GetEnvironmentVariable("SMARTAPART_SMTP_PASSWORD");
            if (string.IsNullOrWhiteSpace(password))
                password = Config("SmtpPassword", string.Empty);
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
                    message.IsBodyHtml = true;
                    message.Body = TaoNoiDungHetHan(hoTen, maPhieu, maPhong);

                    using (var client = new SmtpClient(host, DocInt("SmtpPort", 587)))
                    {
                        client.EnableSsl = DocBool("SmtpEnableSsl", true);
                        if (!string.IsNullOrWhiteSpace(user))
                            client.Credentials = new NetworkCredential(user, password);

                        client.Send(message);
                    }
                }

                thongBao = "Da gui email thong bao het han dat coc.";
                GhiLog(maPhieu, maTaiKhoan, email, subject, true, thongBao);
                return true;
            }
            catch (Exception ex)
            {
                thongBao = "Khong gui duoc email het han: " + ex.Message;
                GhiLog(maPhieu, maTaiKhoan, email, subject, false, thongBao);
                return false;
            }
        }

        private static string TaoNoiDung(string hoTen, string tenDangNhap, string matKhauTam,
            string maPhieu, string maPhong, decimal tienCoc, DateTime ngayHetHan,
            string noiDungChuyenKhoan, string qrCid)
        {
            var bankCode = Config("PaymentBankCode", "MB");
            var accountNo = Config("PaymentAccountNo", "0000000000");
            var accountName = Config("PaymentAccountName", "QUAN LY CHO THUE NHA");
            var qrUrl = string.IsNullOrWhiteSpace(qrCid)
                ? TaoVietQrUrl(bankCode, accountNo, accountName, tienCoc, noiDungChuyenKhoan)
                : "cid:" + qrCid;
            var ten = string.IsNullOrWhiteSpace(hoTen) ? "quy khach" : hoTen;

            return @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"">
  <style>
    body { margin:0; background:#f6faf7; font-family:Segoe UI, Arial, sans-serif; color:#1f2937; }
    .wrap { max-width:860px; margin:24px auto; background:#fff; border:1px solid #dbe7df; border-radius:14px; overflow:hidden; }
    .hero { padding:28px 32px; background:linear-gradient(90deg,#ffffff,#eef8f1); border-bottom:1px solid #e5efe8; }
    .brand { font-size:28px; font-weight:800; color:#1b8f3a; }
    .brand span { color:#111827; }
    .tag { color:#6b7280; font-size:13px; margin-top:4px; }
    .content { padding:28px 32px; }
    h2 { margin:0 0 8px; color:#11863a; font-size:22px; }
    .grid { display:grid; grid-template-columns:1fr 1fr; gap:16px; margin:22px 0; }
    .card { border:1px solid #dbe7df; background:#fbfffc; border-radius:12px; padding:18px; }
    .row { border-bottom:1px solid #e5e7eb; padding:10px 0; }
    .row:last-child { border-bottom:none; }
    .label { color:#6b7280; font-size:13px; }
    .value { font-weight:700; margin-top:4px; }
    .money { color:#11863a; font-size:18px; }
    .qr { display:flex; gap:18px; align-items:center; border:1px solid #e5e7eb; border-radius:12px; padding:16px; margin-top:18px; }
    .qr img { width:120px; height:120px; border:1px solid #e5e7eb; border-radius:8px; }
    .note { background:#fff8e7; border:1px solid #f6d58b; border-radius:10px; padding:14px; margin-top:18px; color:#7a4d00; }
    .footer { border-top:1px solid #e5e7eb; color:#6b7280; font-size:13px; padding-top:18px; margin-top:24px; }
    @media(max-width:720px){ .grid{grid-template-columns:1fr;} .qr{display:block;} .qr img{margin-bottom:12px;} }
  </style>
</head>
<body>
  <div class=""wrap"">
    <div class=""hero"">
      <div class=""brand""><span>Smart</span>Apart</div>
      <div class=""tag"">Smart living, simple choice</div>
    </div>
    <div class=""content"">
      <h2>Xin chao, " + Html(ten) + @"</h2>
      <p>SmartApart da tao phieu dat coc phong cho ban. Vui long thanh toan truoc han giu phong.</p>
      <div class=""grid"">
        <div class=""card"">
          <div class=""row""><div class=""label"">Ma phieu</div><div class=""value"">" + Html(maPhieu) + @"</div></div>
          <div class=""row""><div class=""label"">Ma phong</div><div class=""value"">" + Html(maPhong) + @"</div></div>
          <div class=""row""><div class=""label"">So tien dat coc</div><div class=""value money"">" + tienCoc.ToString("N0") + @" VND</div></div>
          <div class=""row""><div class=""label"">Han giu phong</div><div class=""value"">" + ngayHetHan.ToString("dd/MM/yyyy HH:mm") + @"</div></div>
        </div>
        <div class=""card"">
          <div class=""row""><div class=""label"">Noi dung chuyen khoan</div><div class=""value"">" + Html(noiDungChuyenKhoan) + @"</div></div>
          <div class=""row""><div class=""label"">Tai khoan nhan</div><div class=""value"">" + Html(accountNo) + @" - " + Html(accountName) + @"</div></div>
          <div class=""row""><div class=""label"">Ten dang nhap</div><div class=""value"">" + Html(tenDangNhap) + @"</div></div>
          <div class=""row""><div class=""label"">Mat khau tam</div><div class=""value"">" + Html(matKhauTam) + @"</div></div>
        </div>
      </div>
      <div class=""qr"">
        <img src=""" + Html(qrUrl) + @""" alt=""QR thanh toan"">
        <div>
          <h3>Quet ma QR de thanh toan</h3>
          <p>Su dung ung dung ngan hang/vi dien tu de quet QR va chuyen khoan dung so tien, dung noi dung.</p>
          " + (string.IsNullOrWhiteSpace(qrCid) ? "<p><a href=\"" + Html(qrUrl) + "\">Mo/tai ma QR</a></p>" : string.Empty) + @"
        </div>
      </div>
      <div class=""note"">Sau khi thanh toan, nhan vien se xac nhan tien coc va chuyen phieu sang trang thai cho ky hop dong.</div>
      <div class=""footer"">Tran trong,<br><b>SmartApart - Giai phap quan ly can ho thong minh</b><br>support@smartapart.vn</div>
    </div>
  </div>
</body>
</html>";
        }

        private static string TaoVietQrUrl(string bankCode, string accountNo, string accountName,
            decimal amount, string addInfo)
        {
            return string.Format("https://img.vietqr.io/image/{0}-{1}-compact2.png?amount={2}&addInfo={3}&accountName={4}",
                Uri.EscapeDataString(bankCode),
                Uri.EscapeDataString(accountNo),
                decimal.ToInt64(amount),
                Uri.EscapeDataString(addInfo ?? string.Empty),
                Uri.EscapeDataString(accountName ?? string.Empty));
        }

        private static string TaoNoiDungHetHan(string hoTen, string maPhieu, string maPhong)
        {
            var ten = string.IsNullOrWhiteSpace(hoTen) ? "quy khach" : hoTen;
            return @"<!doctype html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""margin:0;background:#f6faf7;font-family:Segoe UI,Arial,sans-serif;color:#1f2937"">
  <div style=""max-width:720px;margin:24px auto;background:#fff;border:1px solid #dbe7df;border-radius:14px;padding:28px"">
    <h2 style=""margin-top:0;color:#11863a"">SmartApart - Thong bao het han dat coc</h2>
    <p>Xin chao " + Html(ten) + @",</p>
    <p>Phieu dat coc <b>" + Html(maPhieu) + @"</b> cho phong <b>" + Html(maPhong) + @"</b> da het han vi he thong chua xac nhan nhan coc trong 24 gio.</p>
    <p>Phong da duoc mo lai tren danh sach phong trong. Neu ban da chuyen khoan, vui long lien he nhan vien quan ly de duoc kiem tra lai.</p>
    <p style=""margin-top:24px;color:#6b7280"">Tran trong,<br><b>SmartApart</b></p>
  </div>
</body>
</html>";
        }

        private static string Html(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return WebUtility.HtmlEncode(value);
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
