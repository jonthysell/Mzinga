// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Text;

using GalaSoft.MvvmLight;

namespace Mzinga.SharedUX.ViewModel
{
    public abstract class ObservableEngineOption : ObservableObject
    {
        public string Key { get; private set; }

        public string FriendlyKey
        {
            get
            {
                return GetFriendly(Key);
            }
        }

        public ObservableEngineOption(EngineOption option)
        {
            Key = option.Key;
        }

        protected static string GetFriendly(string s)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                if (i > 0 && char.IsUpper(s[i]) && char.IsLower(s[i - 1]))
                {
                    sb.Append(' ');
                }

                sb.Append(s[i]);
            }

            return sb.ToString();
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
                RaisePropertyChanged(nameof(Value));
            }
        }
        private bool _value;

        public ObservableBooleanEngineOption(BooleanEngineOption option, bool resetToDefaults) : base(option)
        {
            _value = resetToDefaults ? option.DefaultValue : option.Value;
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
                try
                {
                    if (value < MinValue || value > MaxValue)
                    {
                        throw new ArgumentOutOfRangeException(Key, string.Format("Value must be between {0} and {1}.", MinValue, MaxValue));
                    }
                    _value = value;
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
                
                RaisePropertyChanged(nameof(Value));
            }
        }
        private int _value;

        public int MinValue { get; private set; }

        public int MaxValue { get; private set; }

        public ObservableIntegerEngineOption(IntegerEngineOption option, bool resetToDefaults) : base(option)
        {
            _value = resetToDefaults ? option.DefaultValue : option.Value;
            MinValue = option.MinValue;
            MaxValue = option.MaxValue;
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
                try
                {
                    if (value < MinValue || value > MaxValue)
                    {
                        throw new ArgumentOutOfRangeException(Key, string.Format("Value must be between {0} and {1}.", MinValue, MaxValue));
                    }
                    _value = value;
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
                RaisePropertyChanged(nameof(Value));
            }
        }
        private double _value;

        public double MinValue { get; private set; }

        public double MaxValue { get; private set; }

        public ObservableDoubleEngineOption(DoubleEngineOption option, bool resetToDefaults) : base(option)
        {
            _value = resetToDefaults ? option.DefaultValue : option.Value;
            MinValue = option.MinValue;
            MaxValue = option.MaxValue;
        }
    }

    public class ObservableEnumEngineOption : ObservableEngineOption
    {
        public int SelectedValueIndex
        {
            get
            {
                return _selectedValueIndex;
            }
            set
            {
                _selectedValueIndex = value;
                RaisePropertyChanged(nameof(SelectedValueIndex));
                RaisePropertyChanged(nameof(Value));
                RaisePropertyChanged(nameof(FriendlyValue));
            }
        }
        private int _selectedValueIndex = 0;

        public string Value
        {
            get
            {
                return Values[SelectedValueIndex];
            }
        }

        public string FriendlyValue
        {
            get
            {
                return FriendlyValues[SelectedValueIndex];
            }
        }

        public ObservableCollection<string> Values { get; private set; }  =  new ObservableCollection<string>();

        public ObservableCollection<string> FriendlyValues { get; private set; } = new ObservableCollection<string>();

        public ObservableEnumEngineOption(EnumEngineOption option, bool resetToDefaults) : base(option)
        {
            foreach (string value in option.Values)
            {
                Values.Add(value);
                FriendlyValues.Add(GetFriendly(value));
            }

            _selectedValueIndex = Array.IndexOf(option.Values, resetToDefaults ? option.DefaultValue : option.Value);
        }
    }
}
