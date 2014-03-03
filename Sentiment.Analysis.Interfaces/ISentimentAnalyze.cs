using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentiment.Analysis.Interfaces
{
    public class Score
    {
        public double PositiveScore { get; set; }
        public double NegativeScore { get; set; }
        public string AnalyzedBy { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    interface ISentimentAnalyze
    {
        Score DoSentimentAnalysis(string contents);
        Score DoSentimentAnalysis(Uri url);
    }
}
