# Cau hinh email SmartApart

Ung dung dung `System.Net.Mail` cua .NET Framework de gui email qua SMTP, khong can cai them package NuGet.

## Cau hinh hien tai

File `src/QuanLyChoThueNha.GUI/App.config` da co:

- `EmailEnabled`: bat/tat gui email.
- `SmtpHost`: `smtp.gmail.com`.
- `SmtpPort`: `587`.
- `SmtpEnableSsl`: `true`.
- `SmtpUser`: email gui.
- `SmtpFrom`: email gui.
- `SmtpFromName`: `SmartApart`.

## Bao mat app password

Khong luu Google app password vao git. Dat bien moi truong tren may chay app:

```powershell
[Environment]::SetEnvironmentVariable("SMARTAPART_SMTP_PASSWORD", "<google-app-password>", "User")
```

Sau do doi `EmailEnabled` thanh `true` trong file config cua ung dung dang chay.

## Dung QR ngan hang co dinh

Neu muon dung anh QR rieng thay vi VietQR tu dong, luu anh QR tren may va dien duong dan vao:

```xml
<add key="PaymentQrImagePath" value="C:\DuongDan\qr-thanh-toan.png" />
```

Khong commit anh QR hoac thong tin ngan hang vao repo. App se:
- Hien anh QR nay tren man hinh thanh toan/dat coc.
- Nhung anh QR nay vao email gui cho khach.
- Van yeu cau khach chuyen dung so tien va noi dung chuyen khoan duoc hien trong app/email.

## Kiem tra

Khi gui email dat truoc, he thong se ghi vao bang `EmailLog`:

- `ThanhCong` neu gui duoc.
- `ThatBai` neu Gmail/SMTP tu choi hoac thieu cau hinh.

Nhan vien co the xem log trong man hinh `Trang nhan vien` bang nut `Lich su email`.
