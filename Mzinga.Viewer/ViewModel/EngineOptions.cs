// 
// EngineOptions.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Mzinga.Viewer.ViewModel
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

        Dictionary<string, EngineOption> _options = new Dictionary<string, EngineOption>();

        public EngineOptions Clone()
        {
            EngineOptions clone = new EngineOptions();

            foreach (EngineOption eo in this)
            {
                if (eo is BooleanEngineOption)
                {
                    clone._options[eo.Key] = new BooleanEngineOption()
                    {
                        Key = eo.Key,
                        Value = ((BooleanEngineOption)eo).Value,
                    };
                }
                else if (eo is IntegerEngineOption)
                {
                    clone._options[eo.Key] = new IntegerEngineOption()
                    {
                        Key = eo.Key,
                        Value = ((IntegerEngineOption)eo).Value,
                        MinValue = ((IntegerEngineOption)eo).MinValue,
                        MaxValue = ((IntegerEngineOption)eo).MaxValue,
                    };
                }
                else if (eo is DoubleEngineOption)
                {
                    clone._options[eo.Key] = new DoubleEngineOption()
                    {
                        Key = eo.Key,
                        Value = ((DoubleEngineOption)eo).Value,
                        MinValue = ((DoubleEngineOption)eo).MinValue,
                        MaxValue = ((DoubleEngineOption)eo).MaxValue,
                    };
                }
                else if (eo is EnumEngineOption)
                {
                    EnumEngineOption eeo = new EnumEngineOption()
                    {
                        Key = eo.Key,
                        Value = ((EnumEngineOption)eo).Value,
                    };
                    string[] values = new string[((EnumEngineOption)eo).Values.Length];
                    Array.Copy(((EnumEngineOption)eo).Values, values, values.Length);
                    eeo.Values = values;

                    clone._options[eo.Key] = eeo;
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

                    EngineOption eo;
                    
                    switch (type)
                    {
                        case "bool":
                            BooleanEngineOption beo = new BooleanEngineOption();
                            beo.Value = bool.Parse(value);
                            eo = beo;
                            break;
                        case "int":
                            IntegerEngineOption ieo = new IntegerEngineOption();
                            ieo.Value = int.Parse(value);
                            if (split.Length >= 5)
                            {
                                ieo.MinValue = int.Parse(split[3]);
                                ieo.MaxValue = int.Parse(split[4]);
                            }
                            eo = ieo;
                            break;
                        case "double":
                            DoubleEngineOption deo = new DoubleEngineOption();
                            deo.Value = double.Parse(value);
                            if (split.Length >= 5)
                            {
                                deo.MinValue = double.Parse(split[3]);
                                deo.MaxValue = double.Parse(split[4]);
                            }
                            eo = deo;
                            break;
                        case "enum":
                            EnumEngineOption eeo = new EnumEngineOption();
                            eeo.Value = value;
                            eeo.Values = new string[split.Length - 3];
                            Array.Copy(split, 3, eeo.Values, 0, eeo.Values.Length);
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
    }

    public class IntegerEngineOption : EngineOption
    {
        public int Value;
        public int MinValue = int.MinValue;
        public int MaxValue = int.MaxValue;
    }

    public class DoubleEngineOption : EngineOption
    {
        public double Value;
        public double MinValue = double.MinValue;
        public double MaxValue = double.MaxValue;
    }

    public class EnumEngineOption : EngineOption
    {
        public string Value;
        public string[] Values;
    }
}
