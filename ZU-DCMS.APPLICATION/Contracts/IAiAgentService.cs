using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    // This interface defines the contract for an AI agent service.
    public interface IAiAgentService
    {
        // This method retrieves a list of student IDs based on their priority for a specific clinic and term.
        Task<List<int>> GetStudentPriorityListAsync(int clinicId, int termId);
    }
}
