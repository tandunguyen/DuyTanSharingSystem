using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FileService : IFileService
    {
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".mov", ".avi", ".wmv", ".flv", ".mkv" };
        private readonly string _webRootPath;
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt", ".ppt", ".pptx" }; // Thêm mảng cho document
        public FileService(IHostEnvironment env)
        {
            _webRootPath = Path.Combine(env.ContentRootPath, "wwwroot");
        }
        public async Task<string?> SaveFileAsync(IFormFile file, string folderName, bool isImage)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (isImage)
            {
                if (!_allowedImageExtensions.Contains(fileExtension))
                    throw new InvalidOperationException("Invalid image format! Only JPG, JPEG, PNG, GIF, BMP allowed.");
            }
            else
            {
                // Hỗ trợ video HOẶC document (linh hoạt)
                var allowedNonImageExtensions = _allowedVideoExtensions.Concat(_allowedDocumentExtensions).ToArray();
                if (!allowedNonImageExtensions.Contains(fileExtension))
                    throw new InvalidOperationException("Invalid file format! Only MP4, MOV, AVI, WMV, FLV, MKV, PDF, DOC, DOCX, TXT, PPT, PPTX allowed.");
            }

            var folderPath = Path.Combine(_webRootPath, folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folderName}/{fileName}"; // Trả về đường dẫn tương đối
        }
        public bool IsImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            return _allowedImageExtensions.Contains(extension);
        }

        public bool IsVideo(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            return _allowedVideoExtensions.Contains(extension);
        }
    }
}
