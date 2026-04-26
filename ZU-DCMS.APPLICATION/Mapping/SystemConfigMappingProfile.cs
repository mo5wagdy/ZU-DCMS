
using AutoMapper;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class SystemConfigMappingProfile : Profile
    {
        public SystemConfigMappingProfile()
        {
            CreateMap<SystemConfig, SystemConfigDto>().ReverseMap();
            CreateMap<Clinic, ClinicDto>();
        }
    }
}
