using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class PhotoService(IOptions<CloudinarySettings> config) : IPhotoService
    {
        private readonly Cloudinary _cloudinary = new Cloudinary(new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        ));

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            if (!file.ContentType.StartsWith("image/"))
            {
                uploadResult.Error = new Error
                {
                    Message = "Invalid file type. Only image files are allowed."
                };
            }
            else if (file.Length > 5 * 1024 * 1024)
            {
                uploadResult.Error = new Error
                {
                    Message = "The maximum allowed file size is 5 MB."
                };
            }
            else if (file.Length == 0)
            {
                uploadResult.Error = new Error
                {
                    Message = "The file is empty."
                };
            }
            else
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            return uploadResult;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result;
        }
    }
}
