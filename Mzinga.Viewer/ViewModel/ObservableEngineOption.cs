// 
// ObservableEngineOption.cs
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
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;

namespace Mzinga.Viewer.ViewModel
{
    public abstract class ObservableEngineOption : ObservableObject
    {
        public string Key { get; private set; }

        public ObservableEngineOption(EngineOption option)
        {
            Key = option.Key;
        }
    }

    public class ObservableBooleanEngineOption : ObservableEngineOption
    {
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }
        private bool _value;

        public ObservableBooleanEngineOption(BooleanEngineOption option) : base(option)
        {
            _value = option.Value;
        }
    }

    public class ObservableIntegerEngineOption : ObservableEngineOption
    {
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value < _minValue || value > _maxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _value = value;
                RaisePropertyChanged("Value");
            }
        }
        private int _value;

        public int MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
                RaisePropertyChanged("MinValue");
            }
        }
        private int _minValue;

        public int MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
                RaisePropertyChanged("MaxValue");
            }
        }
        private int _maxValue;

        public ObservableIntegerEngineOption(IntegerEngineOption option) : base(option)
        {
            _value = option.Value;
            _minValue = option.MinValue;
            _maxValue = option.MaxValue;
        }
    }

    public class ObservableDoubleEngineOption : ObservableEngineOption
    {
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value < _minValue || value > _maxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _value = value;
                RaisePropertyChanged("Value");
            }
        }
        private double _value;

        public double MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
                RaisePropertyChanged("MinValue");
            }
        }
        private double _minValue;

        public double MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
                RaisePropertyChanged("MaxValue");
            }
        }
        private double _maxValue;

        public ObservableDoubleEngineOption(DoubleEngineOption option) : base(option)
        {
            _value = option.Value;
            _minValue = option.MinValue;
            _maxValue = option.MaxValue;
        }
    }

    public class ObservableEnumEngineOption : ObservableEngineOption
    {
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }
        private string _value;

        public ObservableCollection<string> Values
        {
            get
            {
                return _values;
            }
        }
        private ObservableCollection<string> _values = new ObservableCollection<string>();

        public ObservableEnumEngineOption(EnumEngineOption option) : base(option)
        {
            _value = option.Value;
            
            foreach (string value in option.Values)
            {
                _values.Add(value);
            }
        }
    }
}
