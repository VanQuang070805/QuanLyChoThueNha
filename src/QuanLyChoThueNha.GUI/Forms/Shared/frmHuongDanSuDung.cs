using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using QuanLyChoThueNha.BLL;
using QuanLyChoThueNha.GUI.Controls;

namespace QuanLyChoThueNha.GUI.Forms.Shared
{
    public class frmHuongDanSuDung : MaterialForm
    {
        private readonly string _cheDo;

        public frmHuongDanSuDung(string cheDo = null)
        {
            _cheDo = string.IsNullOrWhiteSpace(cheDo) ? LayCheDoTheoSession() : cheDo;
            Text = "Hướng dẫn sử dụng SmartApart";
            Size = new Size(980, 720);
            MinimumSize = new Size(760, 560);
            StartPosition = FormStartPosition.CenterParent;

            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);

            BuildLayout();
        }

        private static string LayCheDoTheoSession()
        {
            if (SessionContext.LaAdmin) return "Admin";
            if (SessionContext.LaNhanVien) return "NhanVien";
            if (SessionContext.LaKhachThue) return "KhachThue";
            return "Public";
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(22, 18, 22, 18),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(CreateHeader(), 0, 0);

            var list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(2, 8, 12, 8),
                BackColor = Color.FromArgb(248, 250, 252)
            };

            foreach (var card in TaoNoiDung())
                list.Controls.Add(CreateGuideCard(card));

            root.Controls.Add(list, 0, 1);
            Controls.Add(root);
        }

        private Control CreateHeader()
        {
            var panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                BorderColor = Color.FromArgb(191, 219, 254),
                BackColor = Color.White,
                Padding = new Padding(18, 10, 18, 10)
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.White };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.Controls.Add(new Label
            {
                Text = "Chọn đúng thẻ hướng dẫn theo việc cần làm, rồi thao tác theo thứ tự từ trên xuống.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var btnDong = new RoundedButton
            {
                Text = "Đóng",
                Dock = DockStyle.Fill,
                Radius = 10,
                BackColor = Color.FromArgb(37, 99, 235),
                BorderColor = Color.FromArgb(37, 99, 235),
                Margin = new Padding(12, 8, 0, 8)
            };
            btnDong.Click += delegate { Close(); };
            layout.Controls.Add(btnDong, 1, 0);
            panel.Controls.Add(layout);
            return panel;
        }

        private Control CreateGuideCard(GuideCard item)
        {
            var card = new RoundedPanel
            {
                Width = 430,
                Height = item.Height,
                Margin = new Padding(6, 6, 14, 14),
                Radius = 14,
                BorderThickness = 1,
                BorderColor = item.BorderColor,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label
            {
                Text = item.Title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            layout.Controls.Add(new Label
            {
                Text = item.Message,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            var steps = new Label
            {
                Text = TaoDanhSachBuoc(item.Steps),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                TextAlign = ContentAlignment.TopLeft
            };
            layout.Controls.Add(steps, 0, 2);
            card.Controls.Add(layout);
            return card;
        }

        private static string TaoDanhSachBuoc(IList<string> steps)
        {
            var lines = new List<string>();
            for (var i = 0; i < steps.Count; i++)
                lines.Add(string.Format("{0}. {1}", i + 1, steps[i]));
            return string.Join(Environment.NewLine, lines);
        }

        private IEnumerable<GuideCard> TaoNoiDung()
        {
            if (_cheDo == "Admin") return HuongDanAdmin();
            if (_cheDo == "NhanVien") return HuongDanNhanVien();
            if (_cheDo == "KhachThue") return HuongDanKhachDaDangNhap();
            return HuongDanCongKhai();
        }

        private static IEnumerable<GuideCard> HuongDanCongKhai()
        {
            return new[]
            {
                new GuideCard("Tìm phòng phù hợp", "Dành cho khách chưa có tài khoản.",
                    new[]
                    {
                        "Ở màn Tìm trọ, nhập địa chỉ, khu vực, tòa hoặc mã phòng vào ô Tìm kiếm.",
                        "Chọn Khu vực và chỉnh Bán kính nếu muốn tìm quanh khu vực đó.",
                        "Lọc thêm theo Tòa, Loại phòng, Giá từ và Giá đến.",
                        "Xem trạng thái trên từng thẻ: Còn trống, Đang chờ cọc hoặc Đang cho thuê.",
                        "Bấm Chi tiết để xem thông tin phòng và bản đồ vị trí."
                    }, Color.FromArgb(59, 130, 246), 236),
                new GuideCard("Đặt trước phòng", "Chỉ cần tạo tài khoản khi bắt đầu đặt phòng.",
                    new[]
                    {
                        "Chọn phòng còn trống rồi bấm Đặt nhanh hoặc Chi tiết.",
                        "Nhập họ tên, CMND/CCCD, email, số điện thoại và ngày sinh.",
                        "Hệ thống tự tạo tài khoản khách và phiếu đặt trước.",
                        "Quét QR hoặc chuyển khoản đúng nội dung cọc được hiển thị.",
                        "Kiểm tra email để nhận tài khoản đăng nhập và thông tin đặt cọc."
                    }, Color.FromArgb(16, 185, 129), 236),
                new GuideCard("Đăng nhập khách hàng", "Dùng sau khi hệ thống đã tạo tài khoản.",
                    new[]
                    {
                        "Bấm Tài khoản khách ở màn tìm trọ.",
                        "Nhập tên đăng nhập và mật khẩu tạm trong email.",
                        "Vào trang khách hàng để xem phiếu đặt, hợp đồng và hóa đơn.",
                        "Khi có hóa đơn, bấm xem QR để thanh toán đúng số tiền."
                    }, Color.FromArgb(245, 158, 11), 214)
            };
        }

        private static IEnumerable<GuideCard> HuongDanKhachDaDangNhap()
        {
            return new[]
            {
                new GuideCard("Theo dõi phòng đã đặt", "Luồng dành cho khách đã đăng nhập.",
                    new[]
                    {
                        "Vào Trang khách hàng sau khi đăng nhập.",
                        "Kiểm tra Phiếu đặt trước để biết trạng thái cọc.",
                        "Nếu phiếu còn chờ cọc, thanh toán đúng nội dung chuyển khoản.",
                        "Sau khi nhân viên xác nhận, phiếu chuyển sang trạng thái chờ ký hợp đồng."
                    }, Color.FromArgb(59, 130, 246), 218),
                new GuideCard("Xem hợp đồng và hóa đơn", "Chỉ dữ liệu của chính khách đang đăng nhập được hiển thị.",
                    new[]
                    {
                        "Mở tab Hợp đồng để xem phòng đang thuê và thời hạn.",
                        "Mở tab Hóa đơn để xem kỳ thanh toán, số tiền và trạng thái.",
                        "Bấm nút QR thanh toán nếu hóa đơn còn phải trả.",
                        "Sau khi chuyển khoản, chờ nhân viên ghi nhận thanh toán."
                    }, Color.FromArgb(16, 185, 129), 218)
            };
        }

        private static IEnumerable<GuideCard> HuongDanNhanVien()
        {
            return new[]
            {
                new GuideCard("Xử lý phiếu đặt cọc", "Luồng thường dùng nhất của nhân viên.",
                    new[]
                    {
                        "Vào Trang nhân viên hoặc menu Hợp đồng > Đặt trước.",
                        "Lọc phiếu đang chờ thanh toán cọc.",
                        "Đối chiếu giao dịch chuyển khoản với mã phiếu/nội dung CK.",
                        "Bấm Xác nhận đã nhận cọc để chuyển phiếu sang chờ ký.",
                        "Nếu cần, gửi lại email tài khoản hoặc QR cho khách."
                    }, Color.FromArgb(37, 99, 235), 250),
                new GuideCard("Ký hợp đồng", "Chỉ ký khi phiếu đặt đã đủ điều kiện.",
                    new[]
                    {
                        "Vào Hợp đồng > Hợp đồng.",
                        "Chọn Phiếu đặt trước ở trạng thái chờ ký.",
                        "Kiểm tra căn hộ, khách thuê, ngày bắt đầu/kết thúc, giá thuê và tiền cọc.",
                        "Lưu hợp đồng để phòng chuyển sang đang thuê.",
                        "Kiểm tra lại danh sách hợp đồng và trạng thái căn hộ."
                    }, Color.FromArgb(16, 185, 129), 250),
                new GuideCard("Lập và thu hóa đơn", "Dùng cho tiền thuê, điện nước, dịch vụ và vi phạm.",
                    new[]
                    {
                        "Vào Thanh toán > Hóa đơn.",
                        "Chọn hợp đồng đang hiệu lực, kỳ thanh toán và loại hóa đơn.",
                        "Nhập số tiền phải trả hoặc dùng nút tính điện/nước/dịch vụ.",
                        "Lưu hóa đơn để khách nhìn thấy trong tài khoản.",
                        "Khi khách thanh toán, bấm Ghi nhận thanh toán."
                    }, Color.FromArgb(245, 158, 11), 250),
                new GuideCard("Trả nhà và vi phạm", "Dùng khi kết thúc hợp đồng hoặc phát sinh bồi thường.",
                    new[]
                    {
                        "Vào Thanh toán > Xử lý vi phạm nếu có khoản phạt.",
                        "Đánh dấu khoản phạt trừ vào cọc nếu nghiệp vụ yêu cầu.",
                        "Vào Thanh toán > Phiếu trả nhà để lập biên bản trả.",
                        "Hệ thống tính tiền hoàn cọc sau khấu trừ.",
                        "Kiểm tra căn hộ đã được mở lại đúng trạng thái."
                    }, Color.FromArgb(239, 68, 68), 250)
            };
        }

        private static IEnumerable<GuideCard> HuongDanAdmin()
        {
            return new[]
            {
                new GuideCard("Khởi tạo dữ liệu cho thuê", "Thực hiện trước khi khách có thể tìm thấy phòng.",
                    new[]
                    {
                        "Vào Tài sản > Khu vực để thêm khu vực và tọa độ.",
                        "Vào Tài sản > Tòa nhà để thêm tòa thuộc khu vực.",
                        "Vào Tài sản > Loại căn hộ để khai báo loại phòng.",
                        "Vào Tài sản > Căn hộ để thêm phòng, giá thuê, tiền cọc và trạng thái Trống.",
                        "Bấm Làm mới ở cổng tìm trọ để kiểm tra phòng đã hiển thị."
                    }, Color.FromArgb(37, 99, 235), 250),
                new GuideCard("Quản trị tài khoản và phân quyền", "Dùng để tách quyền Admin/Nhân viên/Khách.",
                    new[]
                    {
                        "Vào Quản trị > Tài khoản để quản lý tài khoản nội bộ.",
                        "Vào Quản trị > Tài khoản khách để khóa/mở tài khoản khách.",
                        "Vào Quản trị > Phân quyền NV để chọn nhân viên.",
                        "Tick các nhóm chức năng nhân viên được truy cập.",
                        "Nhân viên đăng nhập lại để nhận quyền mới."
                    }, Color.FromArgb(16, 185, 129), 250),
                new GuideCard("Theo dõi báo cáo", "Dùng để kiểm tra tình hình vận hành.",
                    new[]
                    {
                        "Vào Tổng quan > Dashboard để xem chỉ số nhanh.",
                        "Vào Báo cáo > Báo cáo & thống kê để xem doanh thu, công nợ, tình trạng căn hộ.",
                        "Dùng bộ lọc năm và thanh zoom khi cần xem kỹ biểu đồ.",
                        "Bấm Xuất Excel để gửi file báo cáo.",
                        "Rê chuột vào donut chart để xem tên loại và số liệu."
                    }, Color.FromArgb(245, 158, 11), 250),
                new GuideCard("Kiểm tra audit thao tác", "Dùng khi cần truy vết người xử lý nghiệp vụ.",
                    new[]
                    {
                        "Các bảng nghiệp vụ có MaNguoiThaoTac và VaiTroNguoiThaoTac.",
                        "Nhân viên thao tác vẫn ghi MaNhanVien khi cột đó là khóa ngoại nhân viên.",
                        "Admin thao tác không ghi MaAdmin vào cột MaNhanVien để tránh lỗi FK.",
                        "Xem bảng tương ứng trong SQL hoặc grid có hiển thị cột audit."
                    }, Color.FromArgb(239, 68, 68), 236)
            };
        }

        private class GuideCard
        {
            public GuideCard(string title, string message, IList<string> steps, Color borderColor, int height)
            {
                Title = title;
                Message = message;
                Steps = steps;
                BorderColor = borderColor;
                Height = height;
            }

            public string Title { get; private set; }
            public string Message { get; private set; }
            public IList<string> Steps { get; private set; }
            public Color BorderColor { get; private set; }
            public int Height { get; private set; }
        }
    }
}
