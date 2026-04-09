using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    public interface IUserCodeGenerator
    {
        Task<string> GenerateAsync(string prefix, string sequenceName);
    }
}
