using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace ClubManagement.Infrastructure
{
    // Chỉ dùng để hiển thị thông báo kiểm tra dữ liệu bằng tiếng Việt.
    // Không thay đổi model, cơ sở dữ liệu hay nghiệp vụ.
    public class VietnameseDisplayNameProvider : IDisplayMetadataProvider
    {
        private static readonly Dictionary<string, string> Names = new()
        {
            ["FullName"] = "họ và tên",
            ["StudentCode"] = "mã sinh viên",
            ["ClassName"] = "lớp",
            ["Email"] = "email",
            ["Phone"] = "số điện thoại",
            ["Position"] = "chức vụ",
            ["TrainingPoints"] = "điểm rèn luyện",
            ["ClubId"] = "câu lạc bộ",
            ["ClubName"] = "tên câu lạc bộ",
            ["Description"] = "mô tả",
            ["CreatedDate"] = "ngày thành lập",
            ["ActivityName"] = "tên hoạt động",
            ["Location"] = "địa điểm",
            ["ActivityDate"] = "ngày tổ chức",
            ["Name"] = "tên",
            ["Message"] = "nội dung",
            ["Reason"] = "lý do",
            ["AttendanceDate"] = "ngày tham gia",
            ["ActivityId"] = "hoạt động"
        };

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind != ModelMetadataKind.Property ||
                context.Key.Name == null ||
                context.DisplayMetadata.DisplayName != null)
            {
                return;
            }

            if (Names.TryGetValue(context.Key.Name, out var display))
            {
                context.DisplayMetadata.DisplayName = () => display;
            }
        }
    }

    public class VietnameseValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly ValidationAttributeAdapterProvider _inner = new();

        public IAttributeAdapter? GetAttributeAdapter(
            ValidationAttribute attribute,
            IStringLocalizer? stringLocalizer)
        {
            if (attribute.ErrorMessage == null && attribute.ErrorMessageResourceName == null)
            {
                switch (attribute)
                {
                    case RequiredAttribute:
                        attribute.ErrorMessage = "Vui lòng nhập {0}.";
                        break;
                    case StringLengthAttribute:
                        attribute.ErrorMessage = "{0} không được vượt quá {1} ký tự.";
                        break;
                    case RangeAttribute:
                        attribute.ErrorMessage = "{0} phải nằm trong khoảng từ {1} đến {2}.";
                        break;
                    case EmailAddressAttribute:
                        attribute.ErrorMessage = "Email không hợp lệ.";
                        break;
                    case PhoneAttribute:
                        attribute.ErrorMessage = "Số điện thoại không hợp lệ.";
                        break;
                }
            }

            return _inner.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
