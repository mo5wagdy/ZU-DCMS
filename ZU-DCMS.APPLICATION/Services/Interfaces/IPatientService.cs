using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.APPLICATION.Services.Interfaces
{
    // __ Interface for patient service to handle patient-related operations such as retrieving patient information, updating profiles, and fetching paginated lists of patients __ //
    public interface IPatientService
    {
        Task<PatientResult> GetByIdAsync(int id); // => Method to retrieve patient information by patient ID, returning a data transfer object if found
        Task<PatientResult> GetByUserIdAsync(string userId); // => Method to retrieve patient information by associated user ID, returning a data transfer object if found
        Task<PatientResult> UpdateProfileAsync(int id, UpdatePatientDto dto); // => Method to update a patient's profile using the provided patient ID and update data transfer object
        Task<PagedResult<PatientDto>> GetAllAsync(PagedRequest request); // Method to retrieve a paginated list of patients based on the provided pagination request, returning a paged result containing patient data transfer objects
    }
}
