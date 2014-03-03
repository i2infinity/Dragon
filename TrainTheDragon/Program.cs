using System;
using System.Net;
using HtmlAgilityPack;
using NReadability;

namespace DragonClassifier
{
    class Program
    {
        static void Main()
        {
            const string url = "http://www.engadget.com/2014/02/19/nokia-lumia-icon-review/";


            //These evidences are used as training data for the Dragon Classigfier
            var positiveReviews = new Evidence("Positive", "Repository\\Positive.Evidence.csv");
            var negativeReviews = new Evidence("Negative", "Repository\\Negative.Evidence.csv");

            var testData = GetWebpageContents(url);
            var classifier = new Classifier(positiveReviews, negativeReviews);
            var scores = classifier.Classify(testData, DragonHelper.DragonHelper.ExcludeList);
            Console.WriteLine("Positive Score for " + url + " - " + scores["Positive"]);
        }

        private static String GetWebpageContents(String url)
        {
            var nreadabilityTranscoder = new NReadabilityTranscoder();
            using (var wc = new WebClient())
            {
                var rawHtml = wc.DownloadString(url);
                var transcodingInput = new TranscodingInput(rawHtml);
                var extractedHtml = nreadabilityTranscoder.Transcode(transcodingInput).ExtractedContent;
                var pageHtml = new HtmlDocument();
                pageHtml.LoadHtml(extractedHtml);
                return pageHtml.DocumentNode.SelectSingleNode("//body").InnerText;
            }
        }
    }
}
