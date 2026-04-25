
using AutoMapper;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class TermMappingProfile : Profile
    {
        public TermMappingProfile()
        {
            CreateMap<Term, TermDto>().ReverseMap();
        }
    }
}
