using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    using System.Net;

    public static class StringHelper
    {
        public static string ToRdfId(this string id)
        {
            return Uri.EscapeDataString(id.Replace(' ', '_'));
        }
    }
}
