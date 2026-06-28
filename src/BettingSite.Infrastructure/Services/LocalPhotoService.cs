
using BettingSite.Application.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BettingSite.Infrastructure.Services
{
    public class LocalPhotoService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : PhotoServiceBase
    {
        private const string UploadsFolder = "uploads";

        public override async Task<PhotoUploadResult> AddPhotoAsync(IFormFile file)
        {
            var error = ValidateFile(file);
            if (error != null) return new PhotoUploadResult { Error = error };

            var uploadsDir = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), UploadsFolder);
            Directory.CreateDirectory(uploadsDir);

            var filename = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsDir, filename);

            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var request = httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return new PhotoUploadResult
            {
                Url = $"{baseUrl}/{UploadsFolder}/{filename}",
                PublicId = filename
            };
        }

        public override Task<PhotoDeleteResult> DeletePhotoAsync(string publicId)
        {
            var uploadsDir = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), UploadsFolder);
            var filePath = Path.Combine(uploadsDir, publicId);

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.FromResult(new PhotoDeleteResult());
        }
    }
}
