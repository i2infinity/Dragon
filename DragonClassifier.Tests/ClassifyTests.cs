using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "DragonClassifierTests.config", Watch = true)]
namespace DragonClassifier.Tests
{
    [TestClass]
    [DeploymentItem("DragonClassifierTests.config")]
    public class ClassifyTests
    {
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger("ClassifyTests");

        //[TestMethod]
        public void GenerateAmazonEvidence()
        {
            //Empty brazes to call the desructor of Evidence class
            {
                const string evidenceRepo = @"C:\Dragon\Evidence\";
                var positiveReviews = new Evidence("Positive", evidenceRepo, loadEvidence: false, saveEvidence: true);
                var negativeReviews = new Evidence("Negative", evidenceRepo, loadEvidence: false, saveEvidence: true);

                positiveReviews.AddEvidenceData(AmazonEvidenceParser.GetPositiveReviews(), DragonHelper.DragonHelper.ExcludeList);
                negativeReviews.AddEvidenceData(AmazonEvidenceParser.GetNegativeReviews(), DragonHelper.DragonHelper.ExcludeList);
            }
        }

        [TestMethod]
        public void ClassifyHappyPath()
        {
            Logger.Debug("Hello");

            const string evidenceRepo = @"C:\Dragon\Evidence\";
            var positiveReviews = new Evidence("Positive", evidenceRepo, loadEvidence: true);
            var negativeReviews = new Evidence("Negative", evidenceRepo, loadEvidence: true);
            string testData;
            using (var sr = new StreamReader(@"C:\Users\Amrish\Desktop\Review.txt", Encoding.UTF7))
            {
                testData = sr.ReadToEnd();
            }

            var classifier = new Classifier(positiveReviews, negativeReviews);
            var scores = classifier.Classify(testData, DragonHelper.DragonHelper.TokenCharMappings, DragonHelper.DragonHelper.ExcludeList);
            var positive = scores["Positive"];
            Assert.IsNotNull(positive);
        }

        //[TestMethod]
        public void ExtractContentsAndClassify()
        {
            const string evidenceRepo = @"C:\Dragon\Evidence\";
            var positiveReviews = new Evidence("Positive", evidenceRepo, loadEvidence: true);
            var negativeReviews = new Evidence("Negative", evidenceRepo, loadEvidence: true);
            var classifier = new Classifier(positiveReviews, negativeReviews);
            const string reviewUrl = "http://www.theverge.com/2012/9/30/3433110/amazon-kindle-paperwhite-review";
            var extractedContent = Helper.GetUrlContentsViaReadability(reviewUrl);
            var scores = classifier.Classify(extractedContent, DragonHelper.DragonHelper.TokenCharMappings, DragonHelper.DragonHelper.ExcludeList);
            var positiveFromUrl = scores["Positive"];
            Assert.IsNotNull(positiveFromUrl);

            string testData;
            using (var sr = new StreamReader(@"C:\Users\Amrish\Desktop\Review.txt", Encoding.UTF7))
            {
                testData = sr.ReadToEnd();
            }
            scores = classifier.Classify(testData, DragonHelper.DragonHelper.TokenCharMappings, DragonHelper.DragonHelper.ExcludeList);
            var positiveFromContents = scores["Positive"];
            Assert.IsNotNull(positiveFromContents);
        }

        [TestMethod]
        public void CompareReviews()
        {
            const string evidenceRepo = @"C:\Dragon\Evidence\";
            var positiveReviews = new Evidence("Positive", evidenceRepo, loadEvidence: true);
            var negativeReviews = new Evidence("Negative", evidenceRepo, loadEvidence: true);
            var classifier = new Classifier(positiveReviews, negativeReviews);
            var reviewUrls = new List<string>();

            using (var file = new StreamReader(@"C:\Dragon\RawData\ReviewUrls.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line.Trim())) continue;
                    reviewUrls.Add(line.Trim());
                }
            }

            var boilerPipeContents = Helper.GetUrlContentsViaBoilerPipe(reviewUrls);
            var readabilityContents = Helper.GetUrlContentsViaReadability(reviewUrls);
            var uClassifyResults = Helper.GetUClassifyReviewScores(reviewUrls);
            var dragonResultsViaReadability = new Dictionary<string, double>();
            var dragonResultsViaBoilerPipe = new Dictionary<string, double>();

            foreach (var content in boilerPipeContents)
            {
                var scores = classifier.Classify(content.Value, DragonHelper.DragonHelper.TokenCharMappings, DragonHelper.DragonHelper.ExcludeList);
                var positive = scores["Positive"];
                dragonResultsViaBoilerPipe.Add(content.Key, positive);
            }

            foreach (var content in readabilityContents)
            {
                var scores = classifier.Classify(content.Value, DragonHelper.DragonHelper.TokenCharMappings, DragonHelper.DragonHelper.ExcludeList);
                var positive = scores["Positive"];
                dragonResultsViaReadability.Add(content.Key, positive);
            }

            Console.Out.WriteLine("Url,Dragon_Readability,Dragon_BoilerPipe,uClassify");
            foreach (var url in reviewUrls)
            {
                var output = string.Format("{0}, {1}, {2}, {3}", url, dragonResultsViaReadability[url], dragonResultsViaBoilerPipe[url], uClassifyResults[url]);
                Console.Out.WriteLine(output);
            }

            //foreach (var dragonResult in dragonResultsViaReadability)
            //{
            //    var output = string.Format("{0}, {1}, {2}", dragonResult.Key, dragonResult.Value, uClassifyResults[dragonResult.Key]);
            //    Console.Out.WriteLine(output);
            //}
        }
        
    }
}
