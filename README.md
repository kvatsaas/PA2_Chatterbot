# PA2_Chatterbot

Below is the text of the original assignment description from CS 4242 Natural Language Processing at the Universeity of Minnesota - Duluth. The source code includes further commentary on the implementation, and the textbook referenced can be found here: https://web.stanford.edu/~jurafsky/slp3/

---

Design and implement a command-line program in the language of your choice that will learn an Ngram language model from an arbitrary number of plain text files specified on the command line. Your program should generate a specified number of sentences based on that N-gram model. Your program should have the following command line arguments : 

n, which specifies if your model is a Unigram (1), Bigram (2), or Trigram (3) model, and 

m, which specifies the number of random sentences to generate from this model. 

After n and m your program must accept one or more file names, which will specify the input text for your model. For example :

Chatterbot 2 10 book1.txt book2.txt

Would mean that 10 random sentences should be generated from a bigram model (2) based on the combined contents of book1.txt and book2.txt. Your random sentences should be written to STDOUT.

Before learning the N-gram model, convert all text to lower case, and separate punctuation marks from words so they can be treated as separate tokens. Also, treat numeric data as tokens. 

The following sentence has 9 tokens (since tokens are just space separate string - eg. my, is a single token).

my, oh my, i wish i had 100 dollars. 

You should tokenize so that you end up with 12 tokens, where each punctuation mark is a separate token :

my , oh my , i wish i had 100 dollars . 

Your program will need to identify sentence boundaries, and your ngrams should *not* cross these boundaries. For example, if your input text is :

He went down the stairs

and then out the side door. 

My mother and brother 

followed him.

You would treat this as two sentences, as in:

He went down the stairs and then out the side door . 

My mother and brother followed him .

To identify sentence boundaries, you may assume that any period, question mark, or exclamation point represents the end of a sentence. This assumption is not always correct, but is perfectly adequate for purposes of building a language model. You should identify sentence boundaries using a regular expression/s of your own creation. 

When randomly generating a sentence, keep going until you generate a terminating punctuation mark. Once you observe that then the sentence is complete. 

If the length of a sentence in the input text file is less than n, then you may simply discard that sentence and not use it when computing n-gram probabilities.

For your input text, please use plain text as found at Project Gutenberg (http://www.gutenberg.org) You may find it interesting to develop your program using works from an author you are familiar with and enjoy reading. You may use whichever files you wish from Project Gutenberg, but make certain the total number of tokens in all your files is more than 1,000,000. You will notice some boilerplate text at the start and ending of all Project Gutenberg files.  You can keep or remove that as you wish, it is not a significant issue for this assignment.

Your program should output an informative message as a first line, stating what this program is and who is the author.  You should make it clear what ngram model is being used and how many sentences are generated. Please number your random sentences from 1 to m.

For example.... if I run the following ... 

Chatterbot 2 10 pg2554.txt pg2600.txt pg1399.txt

My output might look like this ...

This program generates 10 random sentences based on a 2-gram model. CS 4242 by Ted Pedersen.

[followed by 10 random sentences, numbered from 1 to 10 ]

Submit a single pdf file to Canvas by 11:59 pm on the deadline that contains your source code followed by the output of your program generating random text from a unigram, bigram, and trigram model. Your program must run on at least 1,000,000 tokens of Project Gutenberg text. Please use the same text for each run of your program.  

For example, I am using the following file names as examples, you may choose your own input files (but they must be from Project Gutenberg). Note that I am using the same input for each of my runs.

Chatterbot 1 10 pg2554.txt pg2600.txt pg1399.txt

Chatterbot 2 10 pg2554.txt pg2600.txt pg1399.txt

Chatterbot 3 10 pg2554.txt pg2600.txt pg1399.txt

Please do not submit screen shots or cut and paste from terminal windows. Instead export your source code to pdf, and print your output to pdf. Your source code should have line numbers. Both source code and output should be on a white background. 

You may use code from libraries, but do not use any functions or pre-existing code that are specific to generating random text or NLP tasks like text normalization or sentence boundary detection. Any regular expressions you use for text normalization, pre-processing, tokenization, and sentence boundary detection must be of your own creation.   

The specific things I will be looking for when grading the functionality of your program are :

1 point - n, m, and an arbitrary number input file names given as command line arguments and not hard coded in source code.

1 point - reasonable 1 gram random text generation with at least 1,000,000 tokens as input

1 point - reasonable 2 gram random text generation with at least 1,000,000 tokens as input

1 point - reasonable 3 gram random text generation with at least 1,000,000 tokens as input

1 point - random sentences end naturally through the random generation of a . ? or !. 

Note that reasonable ngram generation means that the random sentences have a distribution of words that is consistent  with what is found in your input text (aka your training data),  where "the" is the most frequent word, etc. and that sentence lengths are similar to those in the training data. Also note that coherence should improve as you move from unigrams, to bigrams, to trigrams.

You must work on this assignment individually, and please do not "get ideas" or model your solution from code you find online.  If you find yourself searching for NLP specific code, you should stop yourself and allow your own ideas to develop. If you are copying code specific to this assignment that you didn't write, please stop yourself.  Your solution should be original to you.  All of your NLP specific code should be original to you.  

Please review the programming assignment grading rubric and make sure to follow all the expectations described there. You can find the rubric on Google Drive, and there is also a supporting video explanation on Google Drive and Media Space.  

===================================

Note to class discussion list, Sept 25

Please remember that your programming assignments should be individual work where you have personally written the documentation and code.While libraries are allowed for some specific non-NLP uses, in general most of the functionality of your programs should be written by you.

That said, I have seen some PA 1 submissions that lean very heavily on Eliza-specific code from the web. I hope it is clear from the assignment specification, the programming assignment grading rubic, and my own (frequent) remarks that this isn't ok.

To help avoid these issues in PA 2, here are a few practical tips.

If you find yourself doing Google searches for Ngram text generation, sentence boundary detection, etc. please stop yourself. You are doing it wrong. Even if your intent is to "just get ideas," you are doing it wrong. Stop now.

If you are writing your program by copying or looking at other Ngram text generation or NLP code that you haven't written, again, you are doing it wrong. Stop now.

What should you do instead? Before writing any code for PA 2, your goal should be to understand random text generation well enough to work out a solution and manually carry it out on a tiny made up data set (think 10 or 20 tokens, 4 or 5 types, so clearly very artificial).

Once you can do that, then you should start thinking about code you can personally write and document to replicate what you just did by hand.

We are each at different levels of coding skill, so how you do that will vary from person to person. If you don't think you are a very good programmer, that's ok. There are lots of ways to solve all of our assignments, and many of them are not that complicated.

If you find yourself searching for code, you'll often find code you couldn't have written, don't understand, and that you can't document very well. If you find and use code like that, you are setting a number of traps for yourself. It may be clear to me that a student has submitted code they couldn't have written, or more often a student and a classmate both find and submit the same code, with minor modifications. That's often how I notice these situations in fact.

So, please remember that even a limited program that is genuinely and obviously your own will get a higher grade than code that is taken from someone else.

Finally, the number of submissions that do not appear to be original to the student submitting are relatively small. That said, I wanted to send this to everyone to make sure it's clear I am expecting that we'll follow the guidelines that I've been giving, and that your efforts in doing so are indeed appreciated! 
