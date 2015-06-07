#region Using

using System;

#endregion

namespace Common
{
    public static class StringHelper
    {
        public static string ToRdfId(this string id)
        {
            return Uri.EscapeDataString(id.Replace(' ', '_'));
        }
    }
}