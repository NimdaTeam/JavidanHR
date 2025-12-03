using Microsoft.AspNetCore.Http;

namespace _0_Framework.FileUploader;

public interface IFileUploadService
{
    /// <summary>
    /// آپلود فایل با اعتبارسنجی کامل و ذخیره امن
    /// </summary>
    /// <param name="file">فایل ارسالی</param>
    /// <param name="folder">پوشه مقصد (مثال: organizations, users/avatars)</param>
    /// <param name="maxFileSizeMb">حداکثر حجم به مگابایت (پیش‌فرض ۵)</param>
    /// <param name="allowedExtensions">اکستنشن‌های مجاز (مثال: new[] { ".jpg", ".png" })</param>
    /// <returns>نام فایل ذخیره‌شده یا null در صورت خطا</returns>
    Task<string?> UploadFileAsync(
        IFormFile file,
        string folder,
        int maxFileSizeMb = 5,
        string[]? allowedExtensions = null,
        bool requireImageContentType = false,
        CancellationToken ct = default);

    /// <summary>
    /// حذف فایل از سرور
    /// </summary>
    bool DeleteFile(string? fileName, string folder);

    /// <summary>
    /// دریافت URL قابل دسترسی از مرورگر
    /// </summary>
    string GetFileUrl(string? fileName, string folder, string defaultImage = "/images/no-image.png");
}