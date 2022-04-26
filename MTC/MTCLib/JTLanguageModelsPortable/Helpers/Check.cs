using System;
using System.Collections.Generic;
using System.Linq;

namespace JTLanguageModelsPortable.Helpers
{
    public static class Check
    {
        public static void IsNotNull(object argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        public static void IsNotEmpty(string argument, string argumentName)
        {
            if (String.IsNullOrEmpty((argument ?? string.Empty).Trim()))
                throw new ArgumentNullException(argumentName);
        }
    }
}
