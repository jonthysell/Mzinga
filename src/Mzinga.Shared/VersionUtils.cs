// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga
{
    public class VersionUtils
    {
        public static ulong ParseLongVersion(string s)
        {
            ulong vers = 0;

            string[] parts = s.TrimStart('v').Trim().Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                vers |= (ulong.Parse(parts[i]) << ((4 - (i + 1)) * 16));
            }

            return vers;
        }

        public static bool TryParseLongVersion(string s, out ulong result)
        {
            try
            {
                result = ParseLongVersion(s);
                return true;
            }
            catch (Exception) { }

            result = 0;
            return false;
        }
    }
}
