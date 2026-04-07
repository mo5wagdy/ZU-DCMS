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
                     .ForMember(d => d.ClinicName, o => o.MapFrom(s => s.Clinic.Name))

                      // => Calculate completion percentage with safe division and rounding
                     .ForMember(d => d.CompletionPercentage, o => o.MapFrom(s => s.RequiredCount == 0 ? 0 : Math.Round((double)(s.CompletedCount + s.TransferredCount) / s.RequiredCount * 100, 1)));
        }
    }
}
