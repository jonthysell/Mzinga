// 
// ObservableLicense.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2019 Jon Thysell <http://jonthysell.com>
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

using GalaSoft.MvvmLight;

namespace Mzinga.SharedUX.ViewModel
{
    public class ObservableLicense : ObservableObject
    {
        public string Header
        {
            get
            {
                return ProductName;
            }
        }

        public string Body
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LicenseName))
                {
                    return string.Join(Environment.NewLine + Environment.NewLine, Copyright, LicenseBody);
                }

                return string.Join(Environment.NewLine + Environment.NewLine, LicenseName, Copyright, LicenseBody);
            }
        }

        public string ProductName { get; private set; }

        public string Copyright { get; private set; }

        public string LicenseName { get; private set; }

        public string LicenseBody { get; private set; }

        public ObservableLicense(string productName, string copyright, string licenseName, string licenseBody)
        {
            ProductName = string.IsNullOrWhiteSpace(productName) ? throw new ArgumentNullException(nameof(productName)) : productName.Trim();
            Copyright = string.IsNullOrWhiteSpace(copyright) ? throw new ArgumentNullException(nameof(copyright)) : copyright.Trim();
            LicenseName = licenseName?.Trim();
            LicenseBody = string.IsNullOrWhiteSpace(licenseBody) ? throw new ArgumentNullException(nameof(licenseBody)) : licenseBody.Trim();
        }
    }
}
