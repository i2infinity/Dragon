using System;
using System.Collections.Generic;
using System.Linq;
using NLPToolkit;
using Sentiment.Analysis.Interfaces;

namespace DragonClassifier
{
    public class Classifier // : IClassifier
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEvidence _classA;
        private readonly IEvidence _classB;
        private const int WORDS_IN_CHUNK = 50;

        public Classifier(IEvidence first, IEvidence second)
        {
            _classA = first;
            _classB = second;
        }

        /// <summary>
        /// Classifies the sentiment of the input text corpus
        /// </summary>
        /// <param name="contents">Text contents that needs to be classified. Works best for 1000+ words</param>
        /// <param name="wordsToIgnore"></param>
        /// <returns></returns>
        public IDictionary<string, double> Classify(string contents, HashSet<string> wordsToIgnore)
        {
            if (_classA == null || _classB == null)
                throw new Exception("One of the evidences were not properly defined");
            if (string.IsNullOrWhiteSpace(_classA.Name()))
                throw new Exception("Evidence name not defined on the first one");
            if (string.IsNullOrWhiteSpace(_classB.Name()))
                throw new Exception("Evidence name not defined on the second one");

            var words = Tokenizer.TokenizeNow(contents).ToList();
            var chunkSize = Math.Ceiling(words.Count()/(double) WORDS_IN_CHUNK);
            var index = 0;

            #region Classify in chunks

            var scores = new List<Dictionary<string, decimal>>();
            for (var i = 0; i < chunkSize; i++)
            {
                var score = new Dictionary<string, decimal>
                    {
                        {_classA.Name(), (decimal) 0.0},
                        {_classB.Name(), (decimal) 0.0}
                    };
                scores.Add(score);
            }

            foreach (var wordsChunk in words.Chunk(WORDS_IN_CHUNK))
            {
                foreach (
                    var word in
                        wordsChunk.Where(word => !string.IsNullOrWhiteSpace(word) && !wordsToIgnore.Contains(word)))
                {
                    //First Class
                    var classAEvidence = _classA.GetEvidence();
                    double wordCountInClassA;
                    wordCountInClassA = classAEvidence.TryGetValue(word, out wordCountInClassA)
                                            ? wordCountInClassA
                                            : 1;    //ToDo - Make this 1 or 0.01

                    var scoreClassA = (decimal) Math.Log(wordCountInClassA/_classA.TotalWords());
                    scores[index][_classA.Name()] += scoreClassA;

                    //Second Class
                    var classBEvidence = _classB.GetEvidence();
                    double wordCountInClassB;
                    wordCountInClassB = classBEvidence.TryGetValue(word, out wordCountInClassB)
                                            ? wordCountInClassB
                                            : 1;    //ToDo - Make this 1 or 0.01
                    var scoreClassB = (decimal) Math.Log(wordCountInClassB/_classB.TotalWords());
                    scores[index][_classB.Name()] += scoreClassB;

                    //Logger.DebugFormat(",[TAG_A],{0}, {1}, {2}, {3}, {4},,{5}, {6}, {7}, {8}", word, _classA.Name(),
                                       //wordCountInClassA, _classA.TotalWords(), scoreClassA, _classB.Name(),
                                       //wordCountInClassB, _classB.TotalWords(), scoreClassB);
                }

                var totalWordsAllCategories = _classA.TotalWords() + _classB.TotalWords();
                scores[index][_classA.Name()] += (decimal)Math.Log(_classA.TotalWords() / totalWordsAllCategories);
                scores[index][_classB.Name()] += (decimal)Math.Log(_classB.TotalWords() / totalWordsAllCategories);

                var scoreA = Math.Exp((double) scores[index][_classA.Name()]);
                var scoreB = Math.Exp((double) scores[index][_classB.Name()]);
                var totalScore = scoreA + scoreB;

                try
                {
                    scores[index][_classA.Name()] = (decimal)(100 * scoreA / totalScore);
                    scores[index][_classB.Name()] = (decimal)(100 * scoreB / totalScore);   
                }
                catch (OverflowException overflow)
                {
                    Logger.Error(
                        string.Format("Overflow exception for scoreA: {0} scoreB: {1} TotalScore: {2}", scoreA, scoreB,
                                      totalScore), overflow);
                    throw;
                }

                //Logger.DebugFormat("Chunk_{0} score for {1} : {2}", index, _classA.Name(), scores[index][_classA.Name()]);
                //Logger.DebugFormat("Chunk_{0} score for {1} : {2}", index, _classB.Name(), scores[index][_classB.Name()]);

                index++;
            }

            //Coumpute the average
            var results = new Dictionary<string, double>
                                {
                                    {_classA.Name(), 0.0},
                                    {_classB.Name(), 0.0}
                                };

            foreach (var score in scores)
            {
                results[_classA.Name()] += (double) score[_classA.Name()];
                results[_classB.Name()] += (double) score[_classB.Name()];
            }

            results[_classA.Name()] = results[_classA.Name()] / scores.Count;
            results[_classB.Name()] = results[_classB.Name()] / scores.Count;

            //Logger.DebugFormat("Total score for {0} : {1}, {2} : {3} ", _classA.Name(), results[_classA.Name()],
            //                   _classB.Name(), results[_classB.Name()]);

            return results;

            #endregion Classify in chunks
        }
    }
}
