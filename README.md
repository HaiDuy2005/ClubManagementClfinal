# ClubManagement

Website quản lý Câu lạc bộ Văn Nghệ Sinh Viên. Hệ thống hỗ trợ quản lý nhóm chuyên môn, thành viên, hoạt động, đăng ký tham gia, liên hệ và điểm danh kèm ảnh minh chứng. Bản triển khai demo chạy ba dịch vụ bằng Docker Compose: ứng dụng web, MySQL và MinIO.

**Nhóm 2 · Lớp 242101TH001 · Học phần Điện toán đám mây**  
Trường Đại học Bình Dương, Phân hiệu Cà Mau

## Thành viên và phân công

| Thành viên | MSSV | Phụ trách |
| --- | --- | --- |
| Trần Trọng Nghĩa | 24210501025 | Trưởng nhóm, điều phối công việc, theo dõi tiến độ, tổng hợp dự án và báo cáo cuối kỳ |
| Nguyễn Hải Duy | 24210501007 | Viết mã nguồn ban đầu và xây dựng nền tảng ứng dụng |
| Đoàn Hoàng Hảo | 24210501014 | Chỉnh sửa giao diện, biên tập video demo và hoàn thiện slide |
| Mã Triệu Phát | 24210501030 | Phân tích yêu cầu và hướng dẫn triển khai các quy trình nghiệp vụ |
| Trần Nguyễn Khánh Duy | 24210501010 | Kiểm thử hệ thống, ghi nhận và khắc phục lỗi |

## Công nghệ và môi trường

- **Ứng dụng web** dùng ASP.NET Core MVC trên .NET 10, C#, Razor Views, Bootstrap và Entity Framework Core.
- **Cơ sở dữ liệu** dùng MySQL 8.4. Named volume `mysql_data` giữ dữ liệu nghiệp vụ.
- **Lưu trữ ảnh** dùng MinIO. Named volume `minio_data` giữ object. Bucket mặc định là `clubmanagement`.
- **Triển khai** dùng Dockerfile nhiều giai đoạn và Docker Compose. Web kết nối tới `mysql:3306` và `minio:9000` trên mạng `clubmanagement-network`.

Máy chạy cần Docker Desktop ở chế độ Linux containers hoặc Docker Engine với Compose V2. Khởi động Docker và kiểm tra bằng `docker version` cùng `docker compose version`. Các cổng host 8080, 3307, 9001 và 9002 cần còn trống. Dockerfile tự tải SDK và runtime .NET 10 khi build.

## Chuẩn bị cấu hình và khởi chạy

Clone dự án và vào thư mục chứa `compose.yaml`. Các lệnh dưới đây dùng PowerShell.

```powershell
git clone https://github.com/HaiDuy2005/ClubManagementClfinal.git
cd ClubManagementClfinal/ClubManagement
Copy-Item .env.example .env
```

Nếu đã có `.env`, giữ tệp đó và đối chiếu các biến với `.env.example`. Điền giá trị cục bộ theo bảng dưới đây. Chỉ chia sẻ `.env.example` với giá trị mẫu. Giữ `.env` và thông tin đăng nhập thật trên máy demo.

| Biến | Ý nghĩa |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Môi trường ứng dụng, mặc định `Development` |
| `MYSQL_DATABASE` | Tên database, mặc định `ClubManagementDB` |
| `MYSQL_USER` | Tài khoản MySQL cho ứng dụng, mặc định `clubapp` |
| `MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` | Mật khẩu của tài khoản ứng dụng và tài khoản root MySQL |
| `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD` | Tài khoản và mật khẩu MinIO dùng trên máy demo |
| `MINIO_BUCKET_NAME` | Bucket chứa ảnh điểm danh, mặc định `clubmanagement` |

```powershell
docker compose config --services
docker compose up -d --build
docker compose ps
```

Danh sách dịch vụ gồm `web`, `mysql` và `minio`. Nếu ứng dụng chưa mở được, xem log bằng `docker compose logs --tail 100 web mysql minio`.

### Chuẩn bị dữ liệu khi chạy lần đầu

MySQL tạo database từ `MYSQL_DATABASE` khi volume còn trống. Bản Compose hiện tại chưa tự tạo đầy đủ bảng nghiệp vụ hoặc tài khoản quản trị demo. Nếu chạy trên máy mới, cần phục hồi bản SQL demo đã làm sạch và tương thích do nhóm cung cấp. Tệp dump không nằm trong repository tại commit được đối chiếu.

Ví dụ phục hồi từ thư mục có `ClubManagementDB.sql`, sau khi MySQL đã báo `healthy`:

```powershell
docker compose cp ./ClubManagementDB.sql mysql:/tmp/ClubManagementDB.sql
docker compose exec mysql mysql -u root -p -D ClubManagementDB
```

Nhập `MYSQL_ROOT_PASSWORD` khi được hỏi. Nếu đã đổi `MYSQL_DATABASE`, thay `ClubManagementDB` trong lệnh trên bằng tên đã cấu hình. Tại dấu nhắc MySQL, chạy:

```sql
SOURCE /tmp/ClubManagementDB.sql;
SHOW COLUMNS FROM ActivityRegistrations LIKE 'ClubId';
EXIT;
```

`ActivityRegistrations.ClubId` phải có trước khi thử luồng chọn nhóm khi đăng ký hoạt động. Nếu truy vấn không trả dòng nào, cần dùng bản SQL tương thích hoặc áp dụng bản cập nhật schema do nhóm xác nhận. Sau khi phục hồi, chạy `docker compose restart web` và thử mở trang chủ, đăng nhập Admin và đăng ký hoạt động. Quy trình dựng sạch từ commit `a04f6ad` chưa có biên bản kiểm thử.

## Địa chỉ truy cập và tài khoản demo

| Thành phần | Địa chỉ |
| --- | --- |
| Trang chủ | [http://localhost:8080](http://localhost:8080) |
| Đăng nhập quản trị | [http://localhost:8080/Account/Login](http://localhost:8080/Account/Login) |
| MinIO Console | [http://localhost:9001](http://localhost:9001) |
| MinIO API | `http://localhost:9002` |

- **Quản trị website.** Bản demo dùng tên đăng nhập `admin`. Tài khoản phải tồn tại trong bảng `Admins` của database demo. Nhận mật khẩu từ người giữ máy demo.
- **MinIO Console.** Đăng nhập bằng `MINIO_ROOT_USER` và `MINIO_ROOT_PASSWORD` đã đặt trong `.env`.
- **Người dùng công khai.** Có thể xem hoạt động và dùng các biểu mẫu công khai từ trang chủ.

## Kiểm tra HTTP bằng Postman

Bản hiện tại cung cấp các route MVC trả HTML. REST API cho ba thực thể, API upload/download theo hợp đồng REST và Swagger vẫn chưa được triển khai. Để kiểm tra định tuyến bằng Postman, tạo yêu cầu mới, chọn phương thức `GET`, nhập URL rồi bấm **Send**.

| Yêu cầu | Phản hồi dự kiến nếu dịch vụ và dữ liệu sẵn sàng |
| --- | --- |
| `GET http://localhost:8080/` | `200 OK`, trang chủ |
| `GET http://localhost:8080/Clubs` | `200 OK`, danh sách nhóm |
| `GET http://localhost:8080/Clubs/Details/{id}` | `200 OK` với ID có trong database demo, `404 Not Found` với ID không tồn tại |

Để minh họa luồng tệp, gửi ảnh điểm danh hợp lệ từ web, duyệt yêu cầu bằng tài khoản quản trị và bấm **Xem ảnh**. Trong MinIO Console, đối chiếu object trong bucket đã cấu hình. Bản demo đã kiểm tra ảnh đã duyệt cùng bản ghi mẫu `DEMO-PERSIST` sau khi tạo lại container. Cơ chế phân quyền quản trị và băm mật khẩu vẫn là hạn chế của bản hiện tại.

## Dừng và đặt lại môi trường demo

Dừng dịch vụ và giữ dữ liệu để chạy tiếp:

```powershell
docker compose down
```

Chỉ dùng lệnh sau khi cần **xóa toàn bộ dữ liệu demo** và đã lưu bản sao cần thiết. Tùy chọn `-v` xóa hai named volume, gồm dữ liệu MySQL và ảnh trong MinIO:

```powershell
docker compose down -v
```

Khi kiểm tra dữ liệu bền vững, dùng `docker compose down` rồi `docker compose up -d`. Mở lại cùng bản ghi và kiểm tra cùng object trong MinIO.

## Mã nguồn đối chiếu

README dựa trên nhánh `main` tại [commit a04f6ad](https://github.com/HaiDuy2005/ClubManagementClfinal/commit/a04f6ada8c1e3b6521b85de8e3028006fc83edd1) và báo cáo cuối kỳ của Nhóm 2.
