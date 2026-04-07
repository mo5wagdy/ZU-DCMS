using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class PatientMappingProfile : Profile
    {
        // __ This profile defines mappings related to Patient entity for patient-related DTOs. __ //
        public PatientMappingProfile()
        {
            // => Patient → PatientDto
            CreateMap<Patient, PatientDto>()
                     .ForMember(d => d.Age, o => o.MapFrom(s => s.Age)); // => Calculate age from DateOfBirth in the Patient entity

            // => RegisterPatientDto → Patient
            CreateMap<RegisterPatientDto, Patient>()
                     .ForMember(d => d.Id, o => o.Ignore())
                     .ForMember(d => d.PatientCode, o => o.Ignore()) // => PatientCode will be auto generated in the service layer
                     .ForMember(d => d.CreatedAt, o => o.Ignore())
                     .ForMember(d => d.IsDeleted, o => o.Ignore())
                     .ForMember(d => d.Bookings, o => o.Ignore())
                     .ForMember(d => d.Payments, o => o.Ignore());
        }
    }
}
