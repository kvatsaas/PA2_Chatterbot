/*
 * Sentence Generation with n-grams and a Corpus of Literature
 * Created by Kristian Vatsaas
 * September 2021
 * 
 * One model of language is the n-gram. An n-gram is simply 'n' tokens (words, numbers, or punctuation marks) that are
 * together in a sentence. They are in the Markov class of language models, meaning that the probability of the next
 * token can be predicted by the ones preceding it. So, given some kind of pre-existing probability, n-grams can predict
 * the next token by using the previous n-1 tokens and "completing" the n-gram.
 * 
 * For this implementation, we parse one or more text files - in this case, digitized literature from Project Gutenberg,
 * stripped of the licensing information and "unnatural" language such as tables of contents - and count the number of
 * occurrences for each token given the previous 'n-1'-gram. Then, we regenerate a new sentence by doing the same thing,
 * only instead we draw on the stored information as weighted probabilities. A larger n results in more intelligible
 * output, but also lowers variance. Some examples using a large corpus of about 2.5 million tokens:
 * 
 * 1-gram (unigram)
 *  labourers he carried undecipherable , was but benny had " is arise , in the then escape from neighbor long and me the be office ; that " shrouded .
 *  , a his my here were which " about and the port go old be her , .
 *  
 * 
 * 3-gram (trigram)
 *  length sure he will be a more desperate hunters were impelled , flaming hearts , on which ran deep between its horns .
 *  " she rose without a word for word , held her .
 * 
 * 5-gram
 *  and " we sat in silence for some little time after listening to this extraordinary narrative .
 *  the interest rather touched me and made them less remotely rich - nevertheless , i was confused and a little disgusted as i drove away .
 *  the society of equal workingmen which was divided into three fractions , the levellers , the communists , the reformers .
 * 
 * The unigrams are unintelligible, as they are essentially context-free. It should be evident as n increases that the sentences
 * make more sense, but entire sentences may not be unoriginal, such as the last 5-gram. It is also obvious that this implementation
 * is naive with its use of punctuation - for example, "open" and "close" punctuation is used without context.
 * 
 * There are two separate loops, though they are in practice very similar. First is file parsing:
 *  1. Open file using StreamReader.
 *  2. Read a character and add it to a string (sentence in progress). If it is end punctuation ('.', '!', or '?') or EOF, go to step 3. Otherwise, repeat.
 *  3. Change all letters to lowercase.
 *  4. Replace any whitespace characters (or strings of whitespace characters) with a single space.
 *  5. Split into individual tokens, using the single spaces as markers.
 *  6. Begin sentence parsing loop:
 *      a. Get the last n-1 tokens from the list of previous tokens, or all previous tokens if n-1 tokens have not yet been parsed.
 *      b. If a dictionary entry does not yet exist for these tokens, create one with an empty dictionary as the value.
 *      c. Get the next token.
 *      d. If the aforementioned dictionary does not have an entry for the token, create one with 1 as the value. Otherwise, increment its value by 1.
 *      e. Add the token to the list of previous tokens.
 *      f. If there are no more tokens, return to step 2. Otherwise, return to step a.
 *  
 *  Then, sentence generation:
 *  1. Get the last n-1 tokens from the list of previous tokens, or all previous tokens if n-1 tokens have not yet been generated.
 *  2. Using the weighted probability for these tokens in the dictionary, randomly choose the next token.
 *  3. Add the token to the list of previous tokens.
 *  4. If the token is end punctuation ('.', '!', or '?'), sentence generation is finished. Otherwise, return to step 1.
 *  
 *  Weighted probability for a large number of options is difficult to do efficiently, and so this implementation uses an open-source solution,
 *  found here: https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp. Thanks to this easy-to-use class, sentence generation is
 *  very fast - around 10 ms per sentence for a corpus of 2.5 million tokens - so hats off to BlueRaja!
 * 
 * This program was created for CS 4242 Natural Language Processing at the University of Minnesota-Duluth.
 * Last updated 9/26/21
 */

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

        /// <summary>
        /// Given a file, finds all tokens one sentence at a time and adds them to the given ngram dictionary.
        /// </summary>
        /// <param name="filepath">The path of the file to parse</param>
        /// <param name="n">The ngram size being used</param>
        /// <param name="ngrams">The ngram dictionary in use</param>
        /// <returns>The number of tokens in the file, not including sentences shorter than the ngram size.</returns>
        static int ParseFile(string filepath, int n, Dictionary<string, Dictionary<string, int>> ngrams)
        {
            var total = 0;
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
                        total += ParseSentence(sentence, n, ngrams); // add to token count
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Something went wrong while reading " + filepath + ": " + e.Message);
            }
            return total;
        }

        /// <summary>
        /// Helper function for ParseFile. Finds each token in a sentence and adds them to the ngram dictionary according to the previous ngram.
        /// Does not add sentences if they are shorter than the ngram length.
        /// </summary>
        /// <param name="sentence">The sentence to be parsed</param>
        /// <param name="n">The ngram size being used</param>
        /// <param name="ngrams">The ngram dictionary in use</param>
        /// <returns>The number of tokens in the sentence, or 0 if the sentence is shorter than the ngram size</returns>
        static int ParseSentence(string sentence, int n, Dictionary<string, Dictionary<string, int>> ngrams)
        {
            
            sentence = Regex.Replace(sentence.ToLower(), @"(?<punc>[^a-z0-9\s])", @" ${punc} ", RegexOptions.Compiled);     // split punctuation marks into their own tokens
            sentence = Regex.Replace(sentence, @"\s+", @" ", RegexOptions.Compiled);    // turn substrings of whitespace characters into a single space to simplify splitting
            sentence = Regex.Replace(sentence, @"\r", @"", RegexOptions.Compiled);
            MatchCollection matches = Regex.Matches(sentence, @"[^ ]+", RegexOptions.Compiled);     // split into individual tokens

            if (matches.Count < n)  // skip sentence if it is smaller than ngram size
                return 0;

            string ngram;
            var tokens = new List<string>();
            foreach (Match match in matches)
            {
                // get last ngram
                if (tokens.Count <= n - 1)
                    ngram = ConcatenateLastN(tokens, tokens.Count - 1);
                else
                    ngram = ConcatenateLastN(tokens, n - 1);

                // add next token to dictionary of current ngram
                if (!ngrams.ContainsKey(ngram))
                    ngrams.Add(ngram, new Dictionary<string, int>());
                if (ngrams[ngram].ContainsKey(match.Value))
                    ngrams[ngram][match.Value]++;
                else
                    ngrams[ngram].Add(match.Value, 1);

                // add token to list of previous tokens
                tokens.Add(match.Value);
            }
            
            return matches.Count;
        }

        /// <summary>
        /// Create sentence from given ngram dictionary using given ngram size.
        /// </summary>
        /// <param name="ngrams">The ngram dictionary in use</param>
        /// <param name="n">The ngram size being used</param>
        /// <returns>The sentence generated</returns>
        static string GenerateSentence(Dictionary<string, Dictionary<string, int>> ngrams, int n)
        {
            string token, ngram;
            
            var tokens = new List<string>();
            while (true)
            {
                // get last ngram
                if (tokens.Count <= n - 1)
                    ngram = ConcatenateLastN(tokens, tokens.Count - 1);
                else
                    ngram = ConcatenateLastN(tokens, n - 1);

                // this shouldn't happen, but better check for it
                if (!ngrams.ContainsKey(ngram))
                    throw new KeyNotFoundException("Missing ngram during sentence generation");

                // set up weighted randomizer
                var wr = new StaticWeightedRandomizer<string>();
                foreach (string key in ngrams[ngram].Keys)
                    wr.Add(key, ngrams[ngram][key]);

                // choose next token and add to token list
                token = wr.NextWithReplacement();
                tokens.Add(token);
                if (Regex.Match(token, @"[\.\?\!]", RegexOptions.Compiled).Success)
                    break;
            }

            return ConcatenateLastN(tokens, tokens.Count);  // concatenate the tokens and return the sentence
        }

        /// <summary>
        /// Concatenate the last n strings in a given list, separated by ' '
        /// </summary>
        /// <param name="strings">The list of strings to concatenate</param>
        /// <param name="n">The number of strings to use. Must not exceed the count of strings.</param>
        /// <returns>The concatenated strings</returns>
        static string ConcatenateLastN(List<string> strings, int n)
        {
            if (n <= 0)
                return "";
            else
            {
                var result = strings[strings.Count - n];
                for (int i = strings.Count - n + 1; i < strings.Count; i++)
                    result += " " + strings[i];
                return result;
            }
        }

        /// <summary>
        /// Gets arguments, makes calls to parse each file, and makes calls to generate sentences.
        /// </summary>
        /// <param name="args">ngram size, number of sentences, and filepaths to parse</param>
        static void Main(string[] args)
        {
            // check number of arguments
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments!");
                return;
            }

            int n, sentences;
            var filepaths = new List<string>();
            var ngrams = new Dictionary<string, Dictionary<string, int>>();
            var totalCount = 0;

            // give args nicer names and check validity
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

            // variables for time tracking, just for fun
            long time;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var startTime = stopwatch.ElapsedMilliseconds;
            var lastTime = startTime;

            // parse each file and report tokens found and time taken
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

            // report total tokens
            Console.WriteLine("Total tokens: " + totalCount);

            // generate and print sentences, and final time results
            Console.WriteLine("\n\nGenerating " + sentences + " sentences . . .\n");
            lastTime = stopwatch.ElapsedMilliseconds;
            for (int i = 1; i <= sentences; i++)
                Console.WriteLine(i + ". " + GenerateSentence(ngrams, n));
            stopwatch.Stop();
            Console.WriteLine("\nSentence generation time: " + (stopwatch.ElapsedMilliseconds - lastTime) + " ms");
            Console.WriteLine("Total process time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
    }
}
