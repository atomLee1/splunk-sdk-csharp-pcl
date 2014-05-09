﻿// 
// Copyright 2013 Splunk, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License"): you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations
// under the License.

namespace Splunk.ModularInputs.UnitTesting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Splunk.ModularInputs;

    using Xunit;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Test classes in Splunk.ModularInputs namespace
    /// </summary>
    public class TestModularInputs
    {
        /// <summary>
        /// Input file folder
        /// </summary>
        static readonly string TestDataFolder = Path.Combine("Data", "ModularInputs");

        /// <summary>
        /// Input file containing input definition
        /// </summary>
        const string InputDefinitionFilePath = "InputDefinition.xml";

        /// <summary>
        /// Input file containing validation items
        /// </summary>
        const string ValidationItemsFilePath = "ValidationItems.xml";

        /// <summary>
        /// Input file containing expected validation error message.
        /// </summary>
        const string ValidationErrorMessageFilePath = "ValidationErrorMessage.xml";

        /// <summary>
        /// Input file containing scheme
        /// </summary>
        const string SchemeFilePath = "Scheme.xml";

        /// <summary>
        /// Input file containing events
        /// </summary>
        const string EventsFilePath = "Events.xml";

        /// <summary>
        /// Test returning scheme through stdout
        /// </summary>
        [Trait("class", "Splunk.ModularInputs")]
        [Fact]
        public async Task OutputScheme()
        {
            using (var consoleOut = new StringWriter())
            {
                Console.SetOut(consoleOut);
                await ModularInput.RunAsync<TestScript>(new[] { "--scheme" });
                AssertEqualWithExpectedFile(SchemeFilePath, consoleOut.ToString());
            }
        }

        [Trait("class", "Splunk.ModularInputs.SingleValueParameter")]
        [Fact]
        public void SingleValueParameterConversions()
        {
            SingleValueParameter parameter = new SingleValueParameter();

            parameter.Value = "abc";
            Assert.Equal("abc", (string)parameter);

            parameter.Value = "52";
            Assert.Equal(52, (int)parameter);

            parameter.Value = "52";
            Assert.Equal((double)52, (double)parameter);

            parameter.Value = "1";
            Assert.True((bool)parameter);

            parameter.Value = "52";
            Assert.Equal((long)52, (long)parameter);
        }

        [Trait("class", "Splunk.ModularInputs.SingleValueParameter")]
        [Fact]
        public void SingleValueParameterParsing()
        {
            using (TextReader reader = new StringReader("<param name=\"param1\">value1</param>"))
            {
                SingleValueParameter parameter =
                    (SingleValueParameter)new XmlSerializer(typeof(SingleValueParameter)).Deserialize(reader);
                Assert.Equal("param1", parameter.Name);
                Assert.Equal("value1", parameter.Value);
            }

        }

        [Trait("class", "Splunk.ModularInputs.MultiValueParameter")]
        [Fact]
        public void MultiValueParameterParsing()
        {
            using (TextReader reader = new StringReader("<param_list name=\"multiValue\"><value>value3</value>" +
                "<value>value4</value></param_list>"))
            {
                MultiValueParameter parameter =
                    (MultiValueParameter)new XmlSerializer(typeof(MultiValueParameter)).Deserialize(reader);
                Assert.Equal("multiValue", parameter.Name);
                Assert.Equal(new List<string> { "value3", "value4" }, parameter.Values);
            }
        }

        [Trait("class", "Splunk.ModularInputs.MultiValueParameter")]
        [Fact]
        public void MultiValueParameterConversions()
        {
            MultiValueParameter parameter = new MultiValueParameter
            {
                Name = "some_name",
                Values = new List<string> { "abc", "def" }
            };
            Assert.Equal(new List<string> { "abc", "def" }, (List<string>)parameter);

            parameter.Values = new List<string> { "true", "0" };
            Assert.Equal(new List<bool> { true, false }, (List<bool>)parameter);

            parameter.Values = new List<string> { "52", "42" };
            Assert.Equal(new List<double> { 52.0, 42.0 }, (List<double>)parameter);

            parameter.Values = new List<string> { "52", "42" };
            Assert.Equal(new List<float> { (float)52, (float)42 }, (List<float>)parameter);

            parameter.Values = new List<string> { "52", "42" };
            Assert.Equal(new List<int> { 52, 42 }, (List<int>)parameter);

            parameter.Values = new List<string> { "52", "42" };
            Assert.Equal(new List<long> { 52, 42 }, (List<long>)parameter);
        }

        /// <summary>
        /// Test getting validation info from stdin and return validation error through stdout.
        /// </summary>
        [Trait("class", "Splunk.ModularInputs")]
        [Fact]
        public async Task ExternalValidation()
        {
            using (var consoleIn = ReadFileFromDataFolderAsReader(ValidationItemsFilePath))
            using (var consoleOut = new StringWriter())
            {
                SetConsoleIn(consoleIn);
                Console.SetOut(consoleOut);

                int exitCode = await ModularInput.RunAsync<TestScript>(new[] { "--validate-arguments" });

                AssertEqualWithExpectedFile(ValidationErrorMessageFilePath, consoleOut.ToString());
                Assert.NotEqual(0, exitCode);
            }
        }

        /// <summary>
        /// Test getting validation info from stdin
        /// </summary>
        [Trait("class", "Splunk.ModularInputs")]
        [Fact]
        public async Task StreamEvents()
        {
            using (var consoleIn = ReadFileFromDataFolderAsReader(InputDefinitionFilePath))
            {
                using (var consoleOut = new StringWriter())
                {
                    SetConsoleIn(consoleIn);
                    Console.SetOut(consoleOut);
                    Assert.Equal(0, await ModularInput.RunAsync<TestScript>(new string[] { }));
                    AssertEqualWithExpectedFile(EventsFilePath, consoleOut.ToString());
                }
            }
        }

        /// <summary>
        /// Test error handling and logging
        /// </summary>
        [Trait("class", "Splunk.ModularInputs")]
        [Fact]
        public async Task ErrorHandling()
        {
            using (var consoleIn = new StringReader(string.Empty))
            {
                using (var consoleError = new StringWriter())
                {
                    SetConsoleIn(consoleIn);
                    Console.SetError(consoleError);
                    int exitCode = await ModularInput.RunAsync<TestScript>(new string[] { });

                    // There will be an exception due to missing input definition in 
                    // (redirected) console stdin.      
                    var error = consoleError.ToString();

                    // Verify that an exception is logged with level FATAL.
                    Assert.Contains("FATAL Script.Run: Unhandled exception:", error);

                    // Verify that the exception is what we expect.
                    Assert.Contains("No input definitions could be read from the standard input stream.", error);

                    // Verify that an info level message is logged properly.
                    Assert.Contains("INFO Script.Run: Reading input definitions.", error);

                    // Verify that the logged exception does not span more than one line
                    // Splunk breaks up events using new lines for splunkd log.
                    var lines = error.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries);

                    Assert.Equal(2, lines.Length);
                    Assert.NotEqual(0, exitCode);
                }
            }
        }

        [Trait("class", "Splunk.ModularInputs.Event")]
        [Fact]
        public void SerializeEvent()
        {
            Event e = new Event {
                Time = System.DateTime.Now,
                Source = "hilda",
                SourceType = "misc",
                Index = "main",
                Host = "localhost",
                Data = "This is a test of the emergency broadcast system.",
                Done = false,
                Unbroken = true
            };
            using (StringWriter writer = new StringWriter()) {
                XmlSerializer serializer = new XmlSerializer(typeof(Event));
                serializer.Serialize(writer, e);
                Assert.Equal("", writer.ToString());
            }
        }

        /// <summary>
        /// Assert equal with expected file content.
        /// </summary>
        /// <param name="expectedFilePath">Relative file path</param>
        /// <param name="actual">Data to check</param>
        static void AssertEqualWithExpectedFile(
            string expectedFilePath,
            string actual)
        {
            var expected = ReadFileFromDataFolderAsString(expectedFilePath);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Read file from data directory as a string
        /// </summary>
        /// <param name="relativePath">Relative path to the resource</param>
        /// <returns>Resource content</returns>
        static string ReadFileFromDataFolderAsString(string relativePath)
        {
            return File.ReadAllText(GetDataFilePath(relativePath));
        }

        /// <summary>
        /// Read file from data directory as a test reader
        /// </summary>
        /// <param name="relativePath">Relative path to the resource</param>
        /// <returns>Resource content</returns>
        static TextReader ReadFileFromDataFolderAsReader(string relativePath)
        {
            return File.OpenText(GetDataFilePath(relativePath));
        }

        /// <summary>
        /// Get full path to the data file.
        /// </summary>
        /// <param name="relativePath">Relative path to the data folder.</param>
        /// <returns>A full path</returns>
        static string GetDataFilePath(string relativePath)
        {
            return Path.Combine(TestDataFolder, relativePath);
        }



       
        /// <summary>
        /// Redirect console in
        /// </summary>
        /// <param name="target">Destination of the redirection</param>
        static void SetConsoleIn(TextReader target)
        {
            // Must set Console encoding to be UTF8. Otherwise, Script.Run
            // will call the setter of OutputEncoding which results in
            // resetting Console.In (which should be a System.Console bug).
            Console.InputEncoding = Encoding.UTF8;
            Console.SetIn(target);
        }

        /// <summary>
        /// Run the scripts and validate the results.
        /// </summary>
        class TestScript : ModularInput
        {
            /// <summary>
            /// Scheme used by the test
            /// </summary>
            public override Scheme Scheme
            {
                get
                {
                    return new Scheme
                    {
                        Title = "Test Example",
                        Description = "This is a test modular input that handles all the appropriate functionality",
                        StreamingMode = StreamingMode.Xml,
                        Endpoint =
                        {
                            Arguments = new List<Argument>
                            {
                                new Argument
                                {
                                    Name = "interval",
                                    Description = "Polling Interval",
                                    DataType = DataType.Number,
                                    Validation = "is_pos_int('interval')"
                                },
                                new Argument
                                {
                                    Name = "username",
                                    Description = "Admin Username",
                                    DataType = DataType.String,
                                    RequiredOnCreate = false
                                },
                                new Argument
                                {
                                    Name = "password",
                                    Description = "Admin Password",
                                    DataType = DataType.String,
                                    RequiredOnEdit = true
                                }
                            }
                        }
                    };
                }
            }

            /// <summary>
            /// Perform test verifications and stream events.
            /// </summary>
            /// <param name="inputDefinition">Input definition</param>
            public override Task StreamEventsAsync(InputDefinition inputDefinition, EventWriter eventWriter)
            {
                // Verify every part of the input definition is received 
                // parsed, and later recontructed correctly.
                var reconstructed = Serialize(inputDefinition);
                AssertEqualWithExpectedFile(InputDefinitionFilePath, reconstructed);


                return Task.FromResult(false);
            }

            /// <summary>
            /// Validate and return an error message.
            /// </summary>
            /// <param name="validation">Configuration data to validate</param>
            /// <param name="errorMessage">Message to display in UI when validation fails</param>
            /// <returns>Whether the validation succeeded</returns>
            public override bool Validate(Validation inputDefinition, out string errorMessage)
            {
                // Test the dictionary for single value parameter.
                Assert.False((bool)inputDefinition.Parameters["disabled"]);

                var reconstructed = Serialize(inputDefinition);
                AssertEqualWithExpectedFile(ValidationItemsFilePath, reconstructed);

                errorMessage = "test message";
                return false;
            }
        }
    }
}