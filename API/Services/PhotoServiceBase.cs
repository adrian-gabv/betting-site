using API.Helpers;
using API.Interfaces;

namespace API.Services
{
    public abstract class PhotoServiceBase : IPhotoService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        protected static string? ValidateFile(IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
                return "Invalid file type. Only image files are allowed.";
            if (file.Length == 0)
                return "The file is empty.";
            if (file.Length > MaxFileSizeBytes)
                return "The maximum allowed file size is 5 MB.";
            return null;
        }

        public abstract Task<PhotoUploadResult> AddPhotoAsync(IFormFile file);
        public abstract Task<PhotoDeleteResult> DeletePhotoAsync(string publicId);
    }
}
