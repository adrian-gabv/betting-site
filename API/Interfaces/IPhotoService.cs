using API.Helpers;

namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<PhotoUploadResult> AddPhotoAsync(IFormFile file);
        Task<PhotoDeleteResult> DeletePhotoAsync(string publicId);
    }
}
