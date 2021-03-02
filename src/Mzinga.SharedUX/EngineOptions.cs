﻿// 
// EngineOptions.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Collections;
using System.Collections.Generic;

namespace Mzinga.SharedUX
{
    public class EngineOptions : IEnumerable<EngineOption>
    {
        public int Count
        {
            get
            {
                return _options.Count;
            }
        }

        public EngineOption this[string key]
        {
            get
            {
                return _options[key];
            }
            set
            {
                _options[key] = value;
            }
        }

        readonly Dictionary<string, EngineOption> _options = new Dictionary<string, EngineOption>();

        public EngineOptions Clone()
        {
            EngineOptions clone = new EngineOptions();

            foreach (EngineOption eo in this)
            {
                if (eo is BooleanEngineOption beo)
                {
                    clone._options[beo.Key] = new BooleanEngineOption()
                    {
                        Key = beo.Key,
                        Value = beo.Value,
                        DefaultValue = beo.DefaultValue,
                    };
                }
                else if (eo is IntegerEngineOption ieo)
                {
                    clone._options[ieo.Key] = new IntegerEngineOption()
                    {
                        Key = ieo.Key,
                        Value = ieo.Value,
                        DefaultValue = ieo.DefaultValue,
                        MinValue = ieo.MinValue,
                        MaxValue = ieo.MaxValue,
                    };
                }
                else if (eo is DoubleEngineOption deo)
                {
                    clone._options[eo.Key] = new DoubleEngineOption()
                    {
                        Key = eo.Key,
                        Value = deo.Value,
                        DefaultValue = deo.DefaultValue,
                        MinValue = deo.MinValue,
                        MaxValue = deo.MaxValue,
                    };
                }
                else if (eo is EnumEngineOption eeo)
                {
                    EnumEngineOption eeo2 = new EnumEngineOption()
                    {
                        Key = eo.Key,
                        Value = eeo.Value,
                        DefaultValue = eeo.DefaultValue,
                    };
                    string[] values = new string[eeo.Values.Length];
                    Array.Copy(eeo.Values, values, values.Length);
                    eeo2.Values = values;

                    clone._options[eeo.Key] = eeo2;
                }
            }

            return clone;
        }

        public void ParseEngineOptionLines(string [] optionLines)
        {
            foreach (string optionLine in optionLines)
            {
                try
                {
                    string[] split = optionLine.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    string key = split[0];
                    string type = split[1];
                    string value = split[2];
                    string defaultValue = split[3];

                    EngineOption eo;
                    
                    switch (type)
                    {
                        case "bool":
                            BooleanEngineOption beo = new BooleanEngineOption()
                            {
                                Value = bool.Parse(value),
                                DefaultValue = bool.Parse(defaultValue)
                            };
                            eo = beo;
                            break;
                        case "int":
                            IntegerEngineOption ieo = new IntegerEngineOption()
                            {
                                Value = int.Parse(value),
                                DefaultValue = int.Parse(defaultValue)
                            };
                            if (split.Length >= 6)
                            {
                                ieo.MinValue = int.Parse(split[4]);
                                ieo.MaxValue = int.Parse(split[5]);
                            }
                            eo = ieo;
                            break;
                        case "double":
                            DoubleEngineOption deo = new DoubleEngineOption()
                            {
                                Value = double.Parse(value),
                                DefaultValue = double.Parse(defaultValue)
                            };
                            if (split.Length >= 6)
                            {
                                deo.MinValue = double.Parse(split[4]);
                                deo.MaxValue = double.Parse(split[5]);
                            }
                            eo = deo;
                            break;
                        case "enum":
                            EnumEngineOption eeo = new EnumEngineOption
                            {
                                Value = value,
                                DefaultValue = defaultValue,
                                Values = new string[split.Length - 4]
                            };
                            Array.Copy(split, 4, eeo.Values, 0, eeo.Values.Length);
                            eo = eeo;
                            break;
                        default:
                            throw new Exception(string.Format("Unknown type \"{0}\"", type));
                    }

                    eo.Key = key;

                    _options[key] = eo;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to parse option line {0}", optionLine), ex);
                }
            }
        }

        public IEnumerator<EngineOption> GetEnumerator()
        {
            return _options.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public abstract class EngineOption 
    {
        public string Key;
    }

    public class BooleanEngineOption : EngineOption
    {
        public bool Value;
        public bool DefaultValue;
    }

    public class IntegerEngineOption : EngineOption
    {
        public int Value;
        public int DefaultValue;
        public int MinValue = int.MinValue;
        public int MaxValue = int.MaxValue;
    }

    public class DoubleEngineOption : EngineOption
    {
        public double Value;
        public double DefaultValue;
        public double MinValue = double.MinValue;
        public double MaxValue = double.MaxValue;
    }

    public class EnumEngineOption : EngineOption
    {
        public string Value;
        public string DefaultValue;
        public string[] Values;
    }
}
