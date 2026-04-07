using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    // __ This interface defines the contract for an AI agent service. __ //
    public interface IAiAgentService
    {
        Task<List<int>> GetStudentPriorityListAsync(int clinicId, int termId); // => This method retrieves a list of student IDs based on their priority for a specific clinic and term.
    }
}
