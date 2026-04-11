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
            CreateMap<Session, SessionDto>();

            // => SessionDto -> Session
            CreateMap<SessionDto, Session>();
        }
    }
}
