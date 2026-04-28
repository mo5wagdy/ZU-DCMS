using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class CaseMappingProfile : Profile
    {
        // __ This profile defines mappings related to CaseAssignment and CaseSession entities. __ //
        public CaseMappingProfile()
        {
            // => CaseAssignment → CaseAssignmentDto
            CreateMap<CaseAssignment, CaseAssignmentDto>()
                     .ForMember(d => d.PatientName, o => o.MapFrom(s => s.DiagnosisRecord!.Booking!.Patient!.FullName))
                     .ForMember(d => d.ClinicName, o => o.MapFrom(s => s.Clinic!.Name))
                     .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student!.FullName))
                     .ForMember(d => d.AssignedByInternName, o => o.MapFrom(s => s.AssignedByIntern!.FullName))
                     .ForMember(d => d.Diagnosis, o => o.MapFrom(s => s.DiagnosisRecord!.DiagnosisType!.NameAr));

            // => CaseSession → CaseSessionDto
            CreateMap<CaseSession, CaseSessionDto>()
                     .ForMember(d => d.ProceduresNames, o => o.MapFrom(s => s.Procedures.Select(p => p.Procedure!.NameAr).ToList()));
        }
    }
}
