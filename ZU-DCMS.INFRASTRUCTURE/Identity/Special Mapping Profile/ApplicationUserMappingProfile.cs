
using AutoMapper;
using ZU_DCMS.APPLICATION.DTOs.Admin;

namespace ZU_DCMS.INFRASTRUCTURE.Identity.Special_Mapping_Profile
{
    public class ApplicationUserMappingProfile : Profile
    {
        public ApplicationUserMappingProfile()
        {
            CreateMap<ApplicationUser, StaffUsersDto>().ReverseMap();
        }
    }
}
