// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Engine;

namespace Mzinga.Test
{
    public class TestUtils
    {
        public static Assembly TestAssembly => _testAssembly ??= typeof(TestUtils).GetTypeInfo().Assembly;
        private static Assembly _testAssembly;

        public static void AssertExceptionThrown<T>(Action action) where T : Exception
        {
            bool exceptionThrown = false;

            try
            {
                action();
            }
            catch (T)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        public static void AssertHaveEqualChildren<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count());

            foreach (T item in expected)
            {
                Assert.IsTrue(actual.Contains(item));
            }
        }

        public static void AssertCompareToLessThan<T>(T lesser, T greater) where T : IComparable<T>
        {
            Assert.IsTrue(lesser.CompareTo(greater) < 0);
        }

        public static void AssertCompareToGreaterThan<T>(T greater, T lesser) where T : IComparable<T>
        {
            Assert.IsTrue(greater.CompareTo(lesser) > 0);
        }

        public static void AssertCompareToEqualTo<T>(T object1, T object2) where T : IComparable<T>
        {
            Assert.IsTrue(object1.CompareTo(object2) == 0);
        }

        public static void LoadAndExecuteTestCases<T>(string fileName) where T : ITestCase, new()
        {
            IEnumerable<T> testCases = LoadTestCases<T>(fileName);
            ExecuteTestCases<T>(testCases);
        }

        public static IEnumerable<T> LoadTestCases<T>(string fileName) where T : ITestCase, new()
        {
            List<T> testCases = new List<T>();

            using (StreamReader sr = new StreamReader(GetEmbeddedResource(fileName)))
            {
                string line;
                while (null != (line = sr.ReadLine()))
                {
                    line = line.Trim();
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        T testCase = new T();
                        testCase.Parse(line);
                        testCases.Add(testCase);
                    }
                }
            }

            return testCases;
        }

        private static Stream GetEmbeddedResource(string fileName)
        {
            foreach (var name in TestAssembly.GetManifestResourceNames())
            {
                if (name.EndsWith(fileName))
                {
                    return TestAssembly.GetManifestResourceStream(name);
                }
            }

            throw new FileNotFoundException();
        }

        public static void ExecuteTestCases<T>(IEnumerable<T> testCases) where T : ITestCase, new()
        {
            List<T> failedTestCases = new List<T>();
            StringBuilder failMessages = new StringBuilder();

            int testCaseIndex = 0;
            foreach (T testCase in testCases)
            {
                try
                {
                    testCase.Execute();
                }
                catch (Exception ex)
                {
                    failedTestCases.Add(testCase);
                    failMessages.AppendLine(string.Format("Test case #{0} \"{1}\" failed:", testCaseIndex, testCase));
                    failMessages.AppendLine(ex.Message);
                }
                finally
                {
                    testCaseIndex++;
                }
            }

            if (failedTestCases.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("{0} test cases failed:", failedTestCases.Count));

                sb.Append(failMessages);

                Assert.Fail(sb.ToString());
            }
        }

        public static readonly string[] NullOrWhiteSpaceStrings = new string[]
        {
            null,
            string.Empty,
            " ",
            "\t",
            "  ",
            "\t\t",
            " \t ",
            "\t \t",
        };

        public static GameEngineConfig DefaultGameEngineConfig
        {
            get
            {
                return _defaultGameEngineConfig ??= GameEngineConfig.GetDefaultEngineConfig();
            }
        }
        private static GameEngineConfig _defaultGameEngineConfig;
    }

    public interface ITestCase
    {
        void Execute();
        void Parse(string s);
    }
}
