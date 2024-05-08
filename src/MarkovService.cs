using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace RenBot
{
    public class Markov
    {
        public Markov()
        {
            try
            {
                _DataSet_ = File.ReadAllText("./dataset.txt").Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                _DataSet_ = new string[1] { "null" };
            }
        }
        private readonly ReadOnlyMemory<string> _DataSet_;
        private readonly string EndingCharacters = ".?>~!]|})-+*;'\"";

        public string Query(string Input = "", int Length = 0)
        {
            if (String.IsNullOrEmpty(Input))
            {
                Input = _DataSet_.Slice(RandomNumberGenerator.GetInt32(0, _DataSet_.Length), 1).Span[0];
            }
            if (Length == 0)
            {
                Length = RandomNumberGenerator.GetInt32(1, 6) + RandomNumberGenerator.GetInt32(0, 6);
            }

            List<string> InputData = Input.Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            string LastInput = InputData[^1];
            bool End = false;
            int UntilEnd = Length;
            List<string> Guesses;

            while (End == false)
            {
                try
                {
                    Guesses = Enumerable.Range(0, _DataSet_.Length - 1).AsParallel().Where(i => string.Equals(_DataSet_.Slice(i, 1).Span[0], LastInput, StringComparison.OrdinalIgnoreCase)).Select(i => _DataSet_.Slice(i + 1, 1).Span[0]).ToList();

                    LastInput = Guesses[RandomNumberGenerator.GetInt32(0, Guesses.Count())];

                    if (LastInput.Contains("://"))
                    {
                        LastInput = Guesses[RandomNumberGenerator.GetInt32(0, Guesses.Count())];
                    }

                    InputData.Add(LastInput);

                    LastInput = InputData[^1];

                    if (UntilEnd < 1)
                    {
                        if (EndingCharacters.Contains(LastInput[^1]))
                        {
                            End = true;
                        }
                    }
                    UntilEnd--;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "There was an error generating output...";
                }
            }
            return String.Join(' ', InputData);
        }
    }
}
