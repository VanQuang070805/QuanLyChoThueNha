using QuanLyChoThueNha.Model.Entities;

namespace QuanLyChoThueNha.BLL.Helpers
{
    public static class AuditHelper
    {
        public static void GanNguoiThaoTac(CanHo item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        public static void GanNguoiThaoTac(PhieuDatTruoc item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        public static void GanNguoiThaoTac(HopDong item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        public static void GanNguoiThaoTac(HoaDonThanhToan item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        public static void GanNguoiThaoTac(GiaHanHopDong item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        public static void GanNguoiThaoTac(PhieuTraNha item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        public static void GanNguoiThaoTac(PhieuXuLyViPham item)
        {
            if (!CoNguoiDung(item)) return;
            item.MaNguoiThaoTac = SessionContext.MaNguoiDung;
            item.VaiTroNguoiThaoTac = SessionContext.VaiTro;
        }

        private static bool CoNguoiDung(object item)
        {
            return item != null && SessionContext.DaXacThuc;
        }
    }
}
