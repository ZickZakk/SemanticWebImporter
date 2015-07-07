#region Using

using System;

#endregion

namespace Common
{
    /// <summary>
    /// Class providing Helper Methods for Strings
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Converts a string to a well formatted string that can be used as an RDF id.
        /// </summary>
        /// <param name="id">String to be formatted</param>
        /// <returns>Formatted String that can be used as an RDF id</returns>
        public static string ToRdfId(this string id)
        {
            return Uri.EscapeDataString(id.Replace(' ', '_'));
        }
    }
}