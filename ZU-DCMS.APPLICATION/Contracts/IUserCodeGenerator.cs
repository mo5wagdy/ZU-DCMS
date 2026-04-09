using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    // _________________________ User Code Generator _________________________ //
    public interface IUserCodeGenerator
    {
        // __ Generate a unique code for a user based on the provided prefix and sequence name __ //
        Task<string> GenerateAsync(string prefix, string sequenceName);
    }
}
