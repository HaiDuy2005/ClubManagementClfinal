
using ClubManagement.Data;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// Cau hinh ket noi MySQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Chua cau hinh ConnectionStrings:DefaultConnection trong appsettings.json");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

builder.Services.AddDbContext<MySqlMigrationDbContext>(options =>
    options.UseMySQL(connectionString));

// Cau hinh dich vu MinIO
builder.Services.AddSingleton<IMinioClient>(serviceProvider =>
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var endpoint = configuration["MinIO:Endpoint"]
        ?? throw new InvalidOperationException(
            "Chua cau hinh MinIO:Endpoint");

    var accessKey = configuration["MinIO:AccessKey"]
        ?? throw new InvalidOperationException(
            "Chua cau hinh MinIO:AccessKey");

    var secretKey = configuration["MinIO:SecretKey"]
        ?? throw new InvalidOperationException(
            "Chua cau hinh MinIO:SecretKey");

    var useSSL = configuration.GetValue<bool>("MinIO:UseSSL");

    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(useSSL)
        .Build();
});

// Cau hinh MVC va Session
builder.Services.AddControllersWithViews(options =>
{
    // Thông báo kiểm tra dữ liệu bằng tiếng Việt (chỉ hiển thị).
    options.ModelMetadataDetailsProviders.Add(
        new ClubManagement.Infrastructure.VietnameseDisplayNameProvider());

    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập giá trị.");
    messages.SetMissingBindRequiredValueAccessor(field => $"Thiếu giá trị cho {field}.");
    messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"Giá trị '{value}' không hợp lệ cho {field}.");
    messages.SetValueIsInvalidAccessor(value => $"Giá trị '{value}' không hợp lệ.");
    messages.SetValueMustBeANumberAccessor(field => $"{field} phải là một số.");
});
builder.Services.AddSingleton<
    Microsoft.AspNetCore.Mvc.DataAnnotations.IValidationAttributeAdapterProvider,
    ClubManagement.Infrastructure.VietnameseValidationAttributeAdapterProvider>();
builder.Services.AddSession();

builder.Services.AddScoped<ClubManagement.Services.MinioStorageService>();

var app = builder.Build();

// Cau hinh middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
