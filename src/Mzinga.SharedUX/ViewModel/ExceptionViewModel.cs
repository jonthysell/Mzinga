﻿// 
// ExceptionViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.SharedUX.ViewModel
{
    public class ExceptionViewModel : InformationViewModel
    {
        public override bool ShowDetails => !(Exception is EngineInvalidMoveException);

        public Exception Exception
        {
            get
            {
                return _exception;
            }
            private set
            {
                _exception = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Exception _exception;

        public ExceptionViewModel(Exception exception) : base(exception?.Message, exception is EngineInvalidMoveException ? "Invalid Move" : "Error")
        {
            Exception = exception;
            Details = Exception is EngineErrorException ee ? string.Format(string.Join(Environment.NewLine, ee.OutputLines)) : string.Format(Exception.ToString());
        }
    }
}
