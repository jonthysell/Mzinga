// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace Mzinga.Viewer
{
    public class Program
    {
        private static string configFile = null;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
#if PORTABLE
                // Hook into assembly resolution since the assemblies are embedded
                AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
#endif

                if (null != args && args.Length > 0)
                {
                    configFile = args[0];
                }

                App app = new App(configFile);
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                string message = string.Join(Environment.NewLine, string.Format("Unhandled Error: {0}", ex.Message), ex.StackTrace);
                MessageBox.Show(message, "Unhandled Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

#if PORTABLE
        // Adapted from http://www.digitallycreated.net/Blog/61/combining-multiple-assemblies-into-a-single-exe-for-a-wpf-application
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (null == stream)
                {
                    return null;
                }

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
#endif
    }
}
