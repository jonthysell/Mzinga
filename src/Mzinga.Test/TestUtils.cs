﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mzinga.Core;

namespace Mzinga.Test
{
    public static class TestUtils
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

        public static void LoadAndExecuteTestCases<T>(string fileName, params object[] args) where T : ITestCase, new()
        {
            var testCases = LoadTestCases<T>(fileName, args);
            ExecuteTestCases<T>(testCases);
        }

        public static IReadOnlyDictionary<int, T> LoadTestCases<T>(string fileName, params object[] args) where T : ITestCase, new()
        {
            Dictionary <int, T> testCases = new Dictionary<int, T>();

            using (StreamReader sr = new StreamReader(GetEmbeddedResource(fileName)))
            {
                string line;
                int lineNum = 0;
                while ((line = sr.ReadLine()) is not null)
                {
                    lineNum++;
                    line = line.Trim();
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        T testCase = new T
                        {
                            TestArgs = args
                        };
                        testCase.Parse(line);
                        testCases.Add(lineNum, testCase);
                    }
                }
            }

            return testCases;
        }

        public static Stream GetEmbeddedResource(string fileName)
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

        public static IEnumerable<KeyValuePair<string, Stream>> GetEmbeddedResources(string pattern)
        {
            foreach (var name in TestAssembly.GetManifestResourceNames())
            {
                if (Regex.IsMatch(name, pattern))
                {
                    yield return new KeyValuePair<string, Stream>(name, TestAssembly.GetManifestResourceStream(name));
                }
            }
        }

        public static void ProcessEmbeddedResources(string pattern, Action<string, Stream> action)
        {
            var resources = GetEmbeddedResources(pattern);
            Assert.IsNotNull(resources);
            Assert.AreNotEqual(0, resources.Count());

            var failedResources = new List<string>();
            StringBuilder failMessages = new StringBuilder();

            foreach (var kvp in resources)
            {
                try
                {
                    action(kvp.Key, kvp.Value);
                }
                catch (Exception ex)
                {
                    failedResources.Add(kvp.Key);
                    failMessages.AppendLine($"Exception processing \"{kvp.Key}\":");
                    failMessages.AppendLine(ex.ToString());
                }
            }

            if (failedResources.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{failedResources.Count} resources failed:");

                sb.Append(failMessages);

                Assert.Fail(sb.ToString());
            }
        }

        public static void ExecuteTestCases<T>(IReadOnlyDictionary<int, T> testCases) where T : ITestCase, new()
        {
            List<T> failedTestCases = new List<T>();
            StringBuilder failMessages = new StringBuilder();

            foreach (int lineNum in testCases.Keys)
            {
                var testCase = testCases[lineNum];
                var stopWatch = Stopwatch.StartNew();
                try
                {
                    testCase.Execute();
                }
                catch (Exception ex)
                {
                    failedTestCases.Add(testCase);
                    failMessages.AppendLine($"Test case on line #{lineNum} failed:");
                    failMessages.AppendLine(ex.Message);
                }
                Trace.TraceInformation($"Test case on line #{lineNum} finished in {stopWatch.ElapsedMilliseconds}ms");
            }

            if (failedTestCases.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{failedTestCases.Count} test cases failed:");

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
    }

    public interface ITestCase
    {
        object[] TestArgs { get; set; }

        void Execute();
        void Parse(string s);
    }
}
