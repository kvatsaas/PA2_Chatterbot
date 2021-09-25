using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Weighted_Randomizer;

namespace Chatterbot
{
    class Chatterbot
    {
        static Dictionary<string, int> tokenCounts;
        static int totalCount, n;

        static void ParseFile(string path)
        {
            using (var sr = new StreamReader(path))
            {
                while (sr.Peek() != -1) // not EOF
                {
                    var sentence = "";
                    char c;
                    while (true)
                    {
                        c = (char)sr.Read();
                        sentence += c;
                        if (c == '.' || c == '?' || c == '!')
                            break;
                    }
                    ParseSentence(sentence);
                }
            }
        }

        static void ParseSentence(string sentence)
        {
            sentence = Regex.Replace(sentence.ToLower(), @"(?<punc>[^a-z0-9\s])", @" ${punc} ", RegexOptions.Compiled);     // split punctuation marks into their own tokens
            sentence = Regex.Replace(sentence, @"\s+", @" ", RegexOptions.Compiled);    // turn substrings of whitespace characters into a single space to simplify splitting
            string[] tokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);      // split into individual tokens
            totalCount += tokens.Length;
            foreach (string token in tokens)
            {
                if (tokenCounts.ContainsKey(token))
                    tokenCounts[token]++;
                else
                    tokenCounts.Add(token, 1);
            }
        }

        static string GenerateSentence(IWeightedRandomizer<string> wr)
        {
            var sentence = "";
            string token;
            

            while (true)
            {
                token = wr.NextWithReplacement();
                sentence += token;
                if (Regex.Match(token, @"[\.\?\!]", RegexOptions.Compiled).Success)
                    break;
                else
                    sentence += " ";
            }

            return sentence;
        }

        static void Main(string[] args)
        {
            // move all args to local variables
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments!");
                return;
            }

            int n, sentences;
            var filepaths = new List<string>();
            tokenCounts = new Dictionary<string, int>();

            try
            {
                n = Int32.Parse(args[0]);
            }
            catch
            {
                Console.WriteLine("Invalid n value!");
                return;
            }
            try
            {
                sentences = Int32.Parse(args[1]);
            }
            catch
            {
                Console.WriteLine("Invalid sentences value!");
                return;
            }

            for (int i = 2; i < args.Length; i++)
            {
                if (!File.Exists(args[i]))
                {
                    Console.WriteLine("Filepath " + (i - 1) + " is invalid!");
                    return;
                }
                filepaths.Add(args[i]);
            }

            foreach (string filepath in filepaths)
                ParseFile(filepath);

            //foreach (string key in tokenCounts.Keys)
            //    Console.WriteLine(key + ": " + tokenCounts[key]);
            //Console.WriteLine("Total tokens: " + totalCount);
            //Console.WriteLine("Unique tokens: " + tokenCounts.Keys.Count);

            var wr = new StaticWeightedRandomizer<string>();
            foreach (string key in tokenCounts.Keys)
                wr.Add(key, tokenCounts[key]);

            for (int i = 1; i <= sentences; i++)
                Console.WriteLine(i + ". " + GenerateSentence(wr));
        }
    }
}
