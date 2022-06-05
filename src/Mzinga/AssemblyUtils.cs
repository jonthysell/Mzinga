// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.


using System;
using System.IO;
using System.Reflection;

namespace Mzinga
{
    public static class AssemblyUtils
    {
        public static Stream GetEmbeddedResource<T>(string filename)
        {
            var assembly = Assembly.GetAssembly(typeof(T));
            if (assembly is not null)
            {
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName.EndsWith(filename))
                    {
                        Stream? inputStream = assembly.GetManifestResourceStream(resourceName);
                        if (inputStream is not null)
                        {
                            return inputStream;
                        }
                    }
                }
            }

            throw new Exception($"Unable to load embedded resource \"{ filename }\".");
        }

        public static string GetEmbeddedMarkdownText<T>(string filename, bool stripHeader = false)
        {
            using Stream inputStream = GetEmbeddedResource<T>(filename);

            using StreamReader reader = new StreamReader(inputStream);
            string text = reader.ReadToEnd();

            if (stripHeader && text.StartsWith("# "))
            {
                text = text.Substring(text.IndexOf(" #") + " #".Length);
            }

            return text;
        }
    }
}
