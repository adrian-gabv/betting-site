
using AutoMapper;
using BettingSite.Application.Abstractions;
using BettingSite.Application.DTOs;
using BettingSite.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BettingSite.API.Controllers
{
    public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IMapper mapper) : BaseApiController
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMapper _mapper = mapper;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is already taken!");

            var user = _mapper.Map<ApplicationUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user.Id, user.UserName, roles),
                Money = user.Money
            };
        }


        private async Task<bool> UserExists(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null;
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var normalizedUsername = _userManager.NormalizeName(loginDto.Username);
            var user = await _userManager.Users
                .Include(a => a.Avatar)
                .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUsername);

            if (user == null) return Unauthorized("Invalid username");
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut) return Unauthorized("Account locked due to multiple failed attempts. Try again later.");
            if (!result.Succeeded) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Username = user.UserName!,
                Token = await _tokenService.CreateToken(user.Id, user.UserName!, roles),
                PhotoUrl = user.Avatar?.Url,
                Money = user.Money
            };
        }
    }
}
