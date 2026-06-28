using AutoMapper;
using BettingSite.Application.DTOs;
using BettingSite.Domain.Betting;
using BettingSite.Infrastructure.Identity;

namespace BettingSite.Infrastructure.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<ApplicationUser, MemberDto>();
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, ApplicationUser>();
            CreateMap<RegisterDto, ApplicationUser>();
        }
    }
}
