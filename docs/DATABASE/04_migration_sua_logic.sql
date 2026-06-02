/* ============================================================================
   MIGRATION: Bổ sung cột phục vụ việc sửa lại logic nghiệp vụ.
   ----------------------------------------------------------------------------
   DÙNG KHI: bạn ĐÃ tạo database QuanLyChoThueNha từ trước và KHÔNG muốn
            tạo lại từ đầu. Script này chỉ THÊM cột, an toàn (idempotent),
            chạy lại nhiều lần không lỗi.

   NẾU bạn tạo database MỚI bằng 01_create_database.sql thì KHÔNG cần chạy file
   này (các cột đã có sẵn trong script tạo bảng).
   ============================================================================ */

USE QuanLyChoThueNha;
GO

/* ─── 1. HopDong: thêm cột TienCocTruocDaTru ──────────────────────────────────
   Ghi nhận phần tiền khách đã đặt cọc trước (PhieuDatTruoc) được trừ vào
   tiền cọc của hợp đồng. Mặc định 0 cho hợp đồng cũ. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('HopDong') AND name = 'TienCocTruocDaTru')
BEGIN
    ALTER TABLE HopDong ADD TienCocTruocDaTru DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Da them cot HopDong.TienCocTruocDaTru';
END
GO

/* ─── 2. HoaDonThanhToan: thêm 4 cột chỉ số điện/nước ────────────────────────
   Lưu chỉ số công-tơ cũ/mới để truy vết. Tiêu thụ = Moi - Cu. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('HoaDonThanhToan') AND name = 'ChiSoDienCu')
BEGIN
    ALTER TABLE HoaDonThanhToan ADD
        ChiSoDienCu  FLOAT NOT NULL DEFAULT 0,
        ChiSoDienMoi FLOAT NOT NULL DEFAULT 0,
        ChiSoNuocCu  FLOAT NOT NULL DEFAULT 0,
        ChiSoNuocMoi FLOAT NOT NULL DEFAULT 0;
    PRINT 'Da them 4 cot chi so dien/nuoc vao HoaDonThanhToan';
END
GO

PRINT 'Migration hoan tat.';
GO
