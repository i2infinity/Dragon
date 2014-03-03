using System.Collections.Generic;

namespace Sentiment.Analysis.Interfaces
{
    public interface IClassifier
    {
        IDictionary<string, double> Classify(string contents, IDictionary<char, char> charTransformations,
                                             HashSet<string> wordsToIgnore);
    }
}
