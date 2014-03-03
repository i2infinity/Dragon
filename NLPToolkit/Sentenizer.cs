using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;

namespace NLPToolkit
{
    /// <summary>
    /// http://opennlp.apache.org/documentation/manual/opennlp.html#tools.sentdetect.detection.api
    /// </summary>
    public static class Sentenizer
    {
        private static readonly java.io.FileInputStream ModelIn;
        private static readonly SentenceModel Model;

        static Sentenizer()
        {
            var modelFile = ConfigurationManager.AppSettings["ModelSentenizer"] ?? string.Empty;
            ModelIn = new java.io.FileInputStream(modelFile);
            Model = new SentenceModel(ModelIn);
        }

        /// <summary>
        /// Split the input content to individual words
        /// </summary>
        /// <param name="contents">Content to split into words</param>
        /// <returns></returns>
        public static IEnumerable<string> ExtractSentences(string contents)
        {
            var sentenizer = new SentenceDetectorME(Model);
            var sentences = sentenizer.sentDetect(contents);

            return sentences;
        }
    }
}
