using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBookStore.Areas.Admin.Helper
{
    public class StringHelper
    {
        public static string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;


            return new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower();
        }
    }
}