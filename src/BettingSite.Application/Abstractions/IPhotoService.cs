using BettingSite.Application.Common;
using Microsoft.AspNetCore.Http;

namespace BettingSite.Application.Abstractions;

public interface IPhotoService
{
    Task<PhotoUploadResult> AddPhotoAsync(IFormFile file);
    Task<PhotoDeleteResult> DeletePhotoAsync(string publicId);
}
