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
                     .ForMember(d => d.PaymentCode, o => o.MapFrom(s => s.Payment != null ? s.Payment.PaymentCode : null))
                     .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.Payment != null ? s.Payment.Status : Domain.Enums.PaymentStatus.Pending))
                     .ForMember(d => d.Amount, o => o.MapFrom(s => s.Payment != null ? s.Payment.Amount : 0));
        }
    }
}
