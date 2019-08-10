using System.Collections.Generic;

namespace Plugin.FluentValidationRules.Interfaces
{
    public interface IValidity
    {
        string ClassPropertyName { get; set; }
        bool IsValid { get; set; }
        List<string> Errors { get; set; }
        void Clear();
    }
}
