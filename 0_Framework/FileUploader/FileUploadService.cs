using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _0_Framework.FileUploader
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileUploadService> _logger;

        // اکستنشن‌های پیش‌فرض برای تصاویر
        private static readonly string[] DefaultImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

        public FileUploadService(IHostingEnvironment env, ILogger<FileUploadService> logger)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> UploadFileAsync(
            IFormFile file,
            string folder,
            int maxFileSizeMb = 5,
            string[]? allowedExtensions = null,
            bool requireImageContentType = false,
            CancellationToken ct = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.Length == 0) return null;

            allowedExtensions ??= requireImageContentType
                ? DefaultImageExtensions
                : [".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".zip"]; // پیش‌فرض عمومی

            var cleanFolder = NormalizeFolderPath(folder);

            var validation = ValidateFile(file, maxFileSizeMb, allowedExtensions, requireImageContentType);
            if (!validation.IsValid)
            {
                _logger.LogWarning("آپلود رد شد: {Error} | فایل: {FileName}", validation.ErrorMessage, file.FileName);
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullFolderPath = Path.Combine(_env.WebRootPath, "uploads", cleanFolder);
            var fullFilePath = Path.Combine(fullFolderPath, fileName);

            try
            {
                if (!Directory.Exists(fullFolderPath))
                    Directory.CreateDirectory(fullFolderPath);

                await using var stream = new FileStream(fullFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream, ct);

                _logger.LogInformation("فایل با موفقیت آپلود شد: {FileName} در پوشه {Folder}", fileName, cleanFolder);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ذخیره فایل: {FileName} | مسیر: {Path}", file.FileName, fullFilePath);
                return null;
            }
        }


        public bool DeleteFile(string? fileName, string folder)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return true;

            var cleanFolder = NormalizeFolderPath(folder);
            var fullPath = Path.Combine(_env.WebRootPath, "uploads", cleanFolder, fileName);

            if (!File.Exists(fullPath)) return true;

            try
            {
                File.Delete(fullPath);
                _logger.LogInformation("فایل حذف شد: {FilePath}", fullPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف فایل: {FilePath}", fullPath);
                return false;
            }
        }

        public string GetFileUrl(string? fileName, string folder, string defaultImage = "/images/no-image.png")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return defaultImage;

            var cleanFolder = NormalizeFolderPath(folder);
            return $"/uploads/{cleanFolder}/{fileName}";
        }

        // متدهای کمکی
        private static string NormalizeFolderPath(string folder)
        {
            return folder.Trim('/').Replace("\\", "/");
        }

        private (bool IsValid, string ErrorMessage) ValidateFile(
            IFormFile file,
            int maxSizeMb,
            string[] allowedExtensions,
            bool requireImageContentType = false)  // ← پارامتر جدید
        {
            // ۱. حجم
            if (file.Length > maxSizeMb * 1024L * 1024L)
                return (false, $"حجم فایل بیشتر از {maxSizeMb} مگابایت است.");

            // ۲. اکستنشن
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                return (false, "فرمت فایل مجاز نیست.");

            // ۳. فقط اگه requireImageContentType=true بود، چک کن که تصویر باشه
            if (requireImageContentType && !file.ContentType.StartsWith("image/"))
                return (false, "فایل ارسالی باید تصویر باشد.");

            // برای فایل‌های غیرتصویری (مثل PDF) چک MIME دقیق‌تر (اختیاری)
            if (!requireImageContentType)
            {
                var safeMimeTypes = new Dictionary<string, string>
                {
                    { ".pdf", "application/pdf" },
                    { ".doc", "application/msword" },
                    { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                    { ".xls", "application/vnd.ms-excel" },
                    { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                    { ".zip", "application/zip" },
                    { ".rar", "application/x-rar-compressed" }
                };

                if (safeMimeTypes.TryGetValue(ext, out var expectedMime) &&
                    file.ContentType != expectedMime &&
                    !file.ContentType.StartsWith("application/octet-stream")) // octet-stream گاهی برای فایل‌های ناشناخته میاد
                {
                    return (false, "نوع فایل با پسوند مطابقت ندارد.");
                }
            }

            return (true, string.Empty);
        }
    }
}