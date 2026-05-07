using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.APPLICATION.Mapping
{
    public class StudentMappingProfile : Profile
    {
        // __ This profile defines mappings related to Student and TermRequirement entities for student-related DTOs. __ //
        public StudentMappingProfile()
        {
            // => Student → StudentDto
            CreateMap<Student, StudentDto>();

            // => Student → StudentPriorityDto
            CreateMap<Student, StudentPriorityDto>()
                     .ForMember(d => d.StudentId, o => o.MapFrom(s => s.Id))
                     .ForMember(d => d.CompletedCases, o => o.MapFrom(s => s.TermRequirements.Sum(r => r.CompletedCount)))
                     .ForMember(d => d.RequiredCases, o => o.MapFrom(s => s.TermRequirements.Sum(r => r.RequiredCount)))
                     .ForMember(d => d.IsComplete, o => o.MapFrom(s => s.TermRequirements.All(r => r.IsSatisfied)))
                     .ForMember(d => d.Priority, o => o.MapFrom(s => s.TermRequirements.Min(r => r.Priority)));

            // => TermRequirement → StudentRequirementDto
            CreateMap<TermRequirement, StudentRequirementDto>()
                     .ForMember(d => d.ClinicName,   o => o.MapFrom(s => s.Clinic.NameAr != "" ? s.Clinic.NameAr : s.Clinic.Name))
                     .ForMember(d => d.ClinicNameEn, o => o.MapFrom(s => s.Clinic.NameEn != "" ? s.Clinic.NameEn : s.Clinic.Name))
                     .ForMember(d => d.RequirementTypeName, o => o.MapFrom(s =>
                        s.Clinic.Code == "REST" ? "Fillings" :
                        s.Clinic.Code == "SURG" ? "Extraction" :
                        s.Clinic.Code == "PERIO" ? "Cleaning" :
                        s.Clinic.Code == "ENDO" ? "Endodontics" :
                        s.Clinic.Code == "PED" ? "Pediatrics" :
                        s.Clinic.Code == "FIX" ? "Fixed Prosthodontics" :
                        s.Clinic.Code == "REM" ? "Removable Prosthodontics" :
                        s.Clinic.NameAr != "" ? s.Clinic.NameAr : s.Clinic.Name))
                     .ForMember(d => d.RequirementTypeNameEn, o => o.MapFrom(s =>
                        s.Clinic.Code == "REST" ? "Fillings" :
                        s.Clinic.Code == "SURG" ? "Extractions" :
                        s.Clinic.Code == "PERIO" ? "Cleanings" :
                        s.Clinic.Code == "ENDO" ? "Root Canals" :
                        s.Clinic.Code == "PED" ? "Pediatrics" :
                        s.Clinic.Code == "FIX" ? "Fixed" :
                        s.Clinic.Code == "REM" ? "Removable" :
                        s.Clinic.NameEn != "" ? s.Clinic.NameEn : s.Clinic.Name))
                     // => Calculate completion percentage with safe division and rounding
                     .ForMember(d => d.CompletionPercentage, o => o.MapFrom(s => s.RequiredCount == 0 ? 0 : Math.Round((double)(s.CompletedCount + s.TransferredCount) / s.RequiredCount * 100, 1)));
        }
    }
}
