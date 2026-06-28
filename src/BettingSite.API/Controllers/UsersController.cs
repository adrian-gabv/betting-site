using AutoMapper;
using BettingSite.API.Extensions;
using BettingSite.Application.Abstractions;
using BettingSite.Application.DTOs;
using BettingSite.Domain.Betting;
using BettingSite.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BettingSite.API.Controllers
{
    [Authorize]
    public class UsersController(
        IUserRepository userRepository,
        IMapper mapper,
        IPhotoService photoService,
        UserManager<ApplicationUser> userManager) : BaseApiController
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IPhotoService _photoService = photoService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }

        // api/users/1
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var member = await _userRepository.GetMemberAsync(username);
            if (member == null) return NotFound();
            return member;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userManager.FindByNameAsync(User.GetUsername());
            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userManager.FindByNameAsync(User.GetUsername());
            if (user == null) return NotFound();
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error);

            var photo = new Photo
            {
                Url = result.Url!,
                PublicId = result.PublicId
            };

            if (user.Avatar?.PublicId != null)
                await _photoService.DeletePhotoAsync(user.Avatar.PublicId);

            user.Avatar = photo;

            if (await _userRepository.SaveAllAsync())
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));

            return BadRequest("Error uploading");
        }

        [HttpDelete("delete-photo")]
        public async Task<ActionResult> DeletePhoto()
        {
            var user = await _userManager.FindByNameAsync(User.GetUsername());
            if (user == null) return NotFound();
            var photo = user.Avatar;

            if (photo == null) return NotFound();
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error);
            }

            user.Avatar = null;
            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete avatar!");
        }
    }
}
