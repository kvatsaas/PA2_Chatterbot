using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Weighted_Randomizer;

namespace Chatterbot
{
    class Chatterbot
    {

        static int ParseFile(string filepath, int n, Dictionary<string, Dictionary<string, int>> ngrams)
        {
            var total = 0;
            if (n == 1)
                ngrams.Add("", new Dictionary<string, int>());
            try
            {
                using (var sr = new StreamReader(filepath))
                {
                    // read and parse one sentence at a time
                    while (sr.Peek() != -1) // not EOF
                    {
                        var sentence = "";
                        char c;

                        // read in one character at a time and stop after reading end punctuation
                        while (sr.Peek() != -1) // not EOF
                        {
                            c = (char)sr.Read();
                            sentence += c;
                            if (c == '.' || c == '?' || c == '!')
                                break;
                        }
                        total += ParseSentence(sentence, n, ngrams);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while reading " + filepath + ": " + e.Message);
            }
            return total;
        }

        static int ParseSentence(string sentence, int n, Dictionary<string, Dictionary<string, int>> ngrams)
        {
            sentence = Regex.Replace(sentence.ToLower(), @"(?<punc>[^a-z0-9\s])", @" ${punc} ", RegexOptions.Compiled);     // split punctuation marks into their own tokens
            sentence = Regex.Replace(sentence, @"\s+", @" ", RegexOptions.Compiled);    // turn substrings of whitespace characters into a single space to simplify splitting
            sentence = Regex.Replace(sentence, @"\r", @"", RegexOptions.Compiled);
            MatchCollection matches = Regex.Matches(sentence, @"[^ ]+", RegexOptions.Compiled);     // split into individual tokens

            if (n == 1)
            {
                foreach (Match match in matches)
                {
                    if (ngrams[""].ContainsKey(match.Value))
                        ngrams[""][match.Value]++;
                    else
                        ngrams[""].Add(match.Value, 1);
                }
            }
            else
            {
                if (matches.Count < n)  // skip sentence if it is smaller than ngram size
                    return 0;

                var prevTokens = new List<string>();
                foreach (Match match in matches)
                {
                    var ngram = "";
                    if (prevTokens.Count > 0)
                    {
                        ngram = prevTokens[0];
                        for (int i = 1; i < prevTokens.Count; i++)
                            ngram += " " + prevTokens[i];
                    }

                    if (!ngrams.ContainsKey(ngram))
                        ngrams.Add(ngram, new Dictionary<string, int>());
                    if (ngrams[ngram].ContainsKey(match.Value))
                        ngrams[ngram][match.Value]++;
                    else
                        ngrams[ngram].Add(match.Value, 1);

                    prevTokens.Add(match.Value);
                    if (prevTokens.Count > n - 1)
                        prevTokens.RemoveAt(0);
                }
            }
            
            return matches.Count;
        }

        static string GenerateSentence(Dictionary<string, Dictionary<string, int>> ngrams, int n)
        {
            var sentence = "";
            string token;
            
            if (n == 1)
            {
                var wr = new StaticWeightedRandomizer<string>();
                foreach (string key in ngrams[""].Keys)
                    wr.Add(key, ngrams[""][key]);
                while (true)
                {
                    token = wr.NextWithReplacement();
                    sentence += token;
                    if (Regex.Match(token, @"[\.\?\!]", RegexOptions.Compiled).Success)
                        break;
                    else
                        sentence += " ";
                }
            }
            else
            {
                var prevTokens = new List<string>();
                while (true)
                {
                    var ngram = "";
                    if (prevTokens.Count > 0)
                    {
                        ngram = prevTokens[0];
                        for (int i = 1; i < prevTokens.Count; i++)
                            ngram += " " + prevTokens[i];
                    }

                    if (!ngrams.ContainsKey(ngram))
                        throw new KeyNotFoundException("Missing ngram during sentence generation");
                    var wr = new StaticWeightedRandomizer<string>();
                    foreach (string key in ngrams[ngram].Keys)
                        wr.Add(key, ngrams[ngram][key]);
                    token = wr.NextWithReplacement();
                    sentence += token;
                    if (Regex.Match(token, @"[\.\?\!]", RegexOptions.Compiled).Success)
                        break;
                    else
                        sentence += " ";

                    prevTokens.Add(token);
                    if (prevTokens.Count > n - 1)
                        prevTokens.RemoveAt(0);
                }
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
            var tokenCounts = new Dictionary<string, int>();
            var ngrams = new Dictionary<string, Dictionary<string, int>>();
            var totalCount = 0;

            try
            {
                n = Int32.Parse(args[0]);
                if (n < 1)
                    Console.WriteLine("Invalid n value - must be at least one!");
            }
            catch
            {
                Console.WriteLine("Invalid n value - must be an integer!");
                return;
            }
            try
            {
                sentences = Int32.Parse(args[1]);
                if (sentences < 1)
                    Console.WriteLine("Invalid sentences value - must be at least one!");
            }
            catch
            {
                Console.WriteLine("Invalid sentences value - must be an integer!");
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

            long time;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var startTime = stopwatch.ElapsedMilliseconds;
            var lastTime = startTime;
            foreach (string filepath in filepaths)
            {
                Console.WriteLine("Parsing " + filepath + " . . .");
                var count = ParseFile(filepath, n, ngrams);
                time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine(count + " total tokens");
                totalCount += count;
                Console.WriteLine("Parse time: " + (time - lastTime) + "ms\n");
                lastTime = time;
            }

            Console.WriteLine("Total tokens: " + totalCount);

            Console.WriteLine("\n\nGenerating " + sentences + " sentences . . .\n");
            lastTime = stopwatch.ElapsedMilliseconds;
            for (int i = 1; i <= sentences; i++)
                Console.WriteLine(i + ". " + GenerateSentence(ngrams, n));
            stopwatch.Stop();
            Console.WriteLine("\nSentence generation time: " + (stopwatch.ElapsedMilliseconds - lastTime) + "ms");
            Console.WriteLine("Total process time: " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}
