// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("MzingaTest")]

namespace Mzinga
{
    public class AppInfo
    {
        public static Assembly Assembly => _assembly ??= Assembly.GetExecutingAssembly();
        private static Assembly? _assembly = null;

        public static string Name => _name ??= Assembly.GetName().Name ?? nameof(Name);
        private static string? _name = null;

        public static string Version
        {
            get
            {
                if (null == _version)
                {
                    Version? vers = Assembly.GetName().Version;
                    _version = vers is null ? "0" : (vers.Build == 0 ? $"{vers.Major}.{vers.Minor}" : $"{vers.Major}.{vers.Minor}.{vers.Build}");
                }
                return _version;
            }
        }

        private static string? _version = null;

        public static ulong LongVersion
        {
            get
            {
                if (!_longVersion.HasValue)
                {
                    _longVersion = VersionUtils.ParseLongVersion(Version);
                }
                return _longVersion.Value;
            }
        }
        private static ulong? _longVersion;

        public static string Product => _product ??= Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? nameof(Product);
        private static string? _product = null;

        public static string Copyright => _copyright ??= Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? nameof(Copyright);
        private static string? _copyright = null;

        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static readonly string HiveProduct = "Hive";

        public static readonly string HiveCopyright = "Copyright © 2016 Gen42 Games";

        public static readonly string HiveLicense = "Mzinga is in no way associated with or endorsed by Gen42 Games.";

        public static readonly string MitLicenseName = "The MIT License (MIT)";

        public static string MitLicenseBody => string.Join(Environment.NewLine + Environment.NewLine, _mitLicense);

        private static readonly string[] _mitLicense = {
            @"Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:",
            @"The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.",
            @"THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
        };
    }
}
