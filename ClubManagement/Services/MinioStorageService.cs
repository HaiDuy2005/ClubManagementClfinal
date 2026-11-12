using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace ClubManagement.Services
{
    public class MinioStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public MinioStorageService(
            IMinioClient minioClient,
            IConfiguration configuration)
        {
            _minioClient = minioClient;

            _bucketName = configuration["MinIO:BucketName"]
                ?? throw new InvalidOperationException("Chưa cấu hình MinIO:BucketName");
        }

        // Tải ảnh minh chứng lên MinIO (có tự động khởi tạo Bucket nếu chưa có)
        public async Task<string> UploadEvidenceAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Vui lòng chọn file ảnh hợp lệ.");
            }

            // --- TỰ ĐỘNG KIỂM TRA & TẠO BUCKET NẾU CHƯA TỒN TẠI ---
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs, cancellationToken);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs, cancellationToken);
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new ArgumentException("Ảnh minh chứng không được vượt quá 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };

            if (!allowedExtensions.Contains(extension) ||
                !allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new ArgumentException("Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            var objectKey = $"attendance/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";

            await using var stream = file.OpenReadStream();

            var args = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(args, cancellationToken);

            return objectKey;
        }

        // Tải ảnh minh chứng từ MinIO để Admin xem
        public async Task<byte[]> DownloadEvidenceAsync(
            string objectKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Đường dẫn ảnh minh chứng không hợp lệ.");
            }

            using var memoryStream = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(args, cancellationToken);

            return memoryStream.ToArray();
        }
    }
}