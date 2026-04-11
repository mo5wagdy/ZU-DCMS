using AutoMapper;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    // __ This profile defines mappings related to Session entity for session-related DTOs. __ //
    public class SessionMappingProfile : Profile
    {
        public SessionMappingProfile()
        {
            // => Session -> SessionDto
            CreateMap<Session, SessionDto>()
                     // __ Format time as "hh:mm" and determine if session is full based on new and follow-up counts __ //
                     .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                     .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")))
                     // __ A session is considered full if both new and follow-up counts have reached their respective maximums __ //
                     .ForMember(dest => dest.IsFull, opt => opt.MapFrom(src => src.IsNewFull && src.IsFollowUpFull)); ;
        }
    }
}
