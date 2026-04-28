using AutoMapper;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class BookingMappingProfile : Profile
    {
        // __ This profile defines mappings related to Booking and Session entities. __ //
        public BookingMappingProfile()
        {
            //=> Booking → BookingDto
            CreateMap<Booking, BookingDto>()
                     .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.FullName))
                     .ForMember(d => d.SessionDate, o => o.MapFrom(s => s.Session.Date))
                     .ForMember(d => d.SessionTime, o => o.MapFrom(s => $"{s.Session.StartTime:hh\\:mm} - {s.Session.EndTime:hh\\:mm}"))
                     .ForMember(d => d.ClinicName, o => o.MapFrom(s => s.Clinic != null ? s.Clinic.Name : null));
        }
    }
}
