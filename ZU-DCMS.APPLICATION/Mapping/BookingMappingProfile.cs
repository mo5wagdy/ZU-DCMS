using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Booking;
using ZU_DCMS.APPLICATION.DTOs.Session;
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
                     .ForMember(d => d.PaymentCode, o => o.MapFrom(s => s.Payment != null? s.Payment.PaymentCode : null))
                     .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.Payment != null? s.Payment.Status : Domain.Enums.PaymentStatus.Pending))
                     .ForMember(d => d.Amount, o => o.MapFrom(s => s.Payment != null? s.Payment.Amount : 0));

            // => Session → SessionDto
            CreateMap<Session, SessionDto>()
                     .ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime.ToString(@"hh\:mm")))
                     .ForMember(d => d.EndTime, o => o.MapFrom(s => s.EndTime.ToString(@"hh\:mm")));

            // => Session → AvailableSlotDto
            CreateMap<Session, AvailableSlotDto>()
                     .ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime.ToString(@"hh\:mm")))
                     .ForMember(d => d.EndTime, o => o.MapFrom(s => s.EndTime.ToString(@"hh\:mm")))
                     .ForMember(d => d.AvailableNewSlots, o => o.MapFrom(s => s.MaxNewPatients - s.CurrentNewCount))
                     .ForMember(d => d.AvailableFollowUpSlots, o => o.MapFrom(s => s.MaxFollowUpPatients - s.CurrentFollowUpCount))
                     .ForMember(d => d.IsAvailable, o => o.MapFrom(s => !s.IsFull));
        }
    }
}
