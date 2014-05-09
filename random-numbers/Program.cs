﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splunk.ModularInputs;

namespace random_numbers
{
    class Program : ModularInput
    {
        static void Main(string[] args)
        {
        }

        public override Scheme Scheme
        {
            get {
                return new Scheme
                {
                    Title = "Random numbers",
                    Description = "Generate random numbers in the specified range",
                    Arguments = new List<Argument>
                    {
                            new Argument
                            {
                                Name = "min",
                                Description = "Generated value should be at least min",
                                DataType = DataType.Number,
                                RequiredOnCreate = true
                            },
                            new Argument
                            {
                                Name = "max",
                                Description = "Generated value should be less than max",
                                DataType = DataType.Number,
                                RequiredOnCreate = true
                            }
                    }
                };

            }
        }

        public override bool Validate(Validation validation, out string errorMessage)
        {
            double min = (double)validation.Parameters["min"];
            double max = (double)validation.Parameters["max"];

            if (max >= min)
            {
                errorMessage = "min must be less than max.";
                return false;
            }
            else
            {
                errorMessage = "";
                return true;
            }
        }

        public override async Task StreamEventsAsync(InputDefinition inputDefinition, EventWriter eventWriter)
        {
            double min = (double)inputDefinition.Parameters["min"];
            double max = (double)inputDefinition.Parameters["max"];

            while (true)
            {
                eventWriter.WriteEvent(new Event
                {
                    Stanza = inputDefinition.Name,
                    Data = "number=" + 1 * (max - min) + min
                });
                await Task.Delay(1000);
            }
        }
    }
}
