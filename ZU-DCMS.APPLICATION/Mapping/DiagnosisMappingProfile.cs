using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class DiagnosisMappingProfile : Profile
    {
        // __ This profile defines mappings related to DiagnosisRecord and Booking entities for diagnosis-related DTOs. __ //
        public DiagnosisMappingProfile()
        {
            // => DiagnosisRecord → DiagnosisRecordDto
            CreateMap<DiagnosisRecord, DiagnosisRecordDto>()
                     .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Booking.Patient.FullName))
                     .ForMember(d => d.InternDoctorName, o => o.MapFrom(s => s.InternDoctor.FullName))
                     .ForMember(d => d.ClinicName, o => o.MapFrom(s => s.Clinic.Name))
                     .ForMember(d => d.DiagnosisTypeName, o => o.MapFrom(s => s.DiagnosisType.NameAr))
                     .ForMember(d => d.Complaint, o => o.MapFrom(s => s.Complaint));


            // => DiagnosisRecord → BookingForDiagnosisDto
            CreateMap<Booking, BookingForDiagnosisDto>()
                     .ForMember(d => d.BookingId, o => o.MapFrom(s => s.Id))
                     .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient.FullName))
                     .ForMember(d => d.PatientAge, o => o.MapFrom(s => s.Patient.Age))
                     .ForMember(d => d.PatientGender, o => o.MapFrom(s => s.Patient.Gender))
                     .ForMember(d => d.ChronicConditions, o => o.MapFrom(s => s.Patient.ChronicConditions))
                     .ForMember(d => d.OtherConditions, o => o.MapFrom(s => s.Patient.OtherConditions))
                     .ForMember(d => d.IsAssigned, o => o.MapFrom(s => s.DiagnosisRecord != null && s.DiagnosisRecord.IsAssigned));
        }
    }
}
