using System.Collections.Generic;

namespace T9Backend.Services
{
    public interface IWordService
    {
        List<string> Match(string input, bool strict = false);
    }
}