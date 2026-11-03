# Giao diện "Đêm diễn" (Taste Skill + Animate UI)

Chỉ thay đổi phần trình bày. Không đụng tới Controllers, Models, Data, Migrations, Services, Program.cs, route, form field, name/id hay logic.

Tệp thay đổi:
- wwwroot/css/site.css — hệ thiết kế mới (token, nút, form, bảng, shell admin, trang công khai, chuyển động)
- wwwroot/js/site.js — highlight trượt, đếm số, hiện dần, đèn rọi hero, menu admin mobile
- Views/Shared/_Layout.cshtml (+ .css) — khung công khai / admin
- Views/Home/Index.cshtml, Views/Account/Login.cshtml, Views/Account/Dashboard.cshtml — viết lại bố cục
- Views/Attendance/Create.cshtml — xóa 2 dòng ``` thừa hiển thị thành chữ
Các view còn lại giữ nguyên markup, được tạo kiểu qua CSS.

Phông chữ (Be Vietnam Pro, Bricolage Grotesque) nạp từ Google Fonts; offline sẽ dùng phông hệ thống.
Cần chạy `dotnet build` và thử các luồng trên máy có .NET (môi trường này không có .NET).

## Đợt sửa lỗi quản trị (Thêm / Sửa / Xóa / Chi tiết)
- ClubsController: tham số `clubid` → `id` (trước đây Sửa/Chi tiết/Xóa CLB luôn trả 404). Xóa CLB còn dữ liệu liên quan sẽ báo lỗi thân thiện thay vì lỗi 500.
- Members: form Thêm/Sửa bổ sung Mã sinh viên, Lớp, Điểm rèn luyện (thiếu các ô này nên Sửa không lưu được và làm mất điểm rèn luyện); Mã CLB chọn từ danh sách.
- ClubActivities: Mã CLB chọn từ danh sách; xóa an toàn khi còn dữ liệu liên quan.
- Việt hóa: các trang Thêm/Sửa/Chi tiết/Xóa, trang lỗi, chính sách riêng tư, thông báo kiểm tra dữ liệu (Infrastructure/VietnameseValidation.cs + Program.cs).
- Giao diện mới cho các trang bản ghi (.record-page, .form-grid, .detail-grid, .danger-card trong site.css).
