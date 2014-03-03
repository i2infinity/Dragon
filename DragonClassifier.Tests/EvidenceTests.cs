using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DragonClassifier.Tests
{
    [TestClass]
    public class EvidenceTests
    {
        [TestMethod]
        public void AddEvidenceDataTest()
        {
            #region string data

            var inputs = new List<string>();

            const string input1 = @"Clam’'       With's smartphones 21st-century, as with any category of consumer electronics,we 
have no choice but to accept compromises. This has been the case throughout the 
history of cell phones and it continues to hold true even with best handsets on 
the market today. Apple’s () iPhone 5 features #%#5 a class-leading design with 
fast, smooth software, but it has a comparatively small @#$@34 display * and lacks some 
of the great new functionality we’ve seen intr 29347qi kahg kbreier 8woduced on other,,, platforms in 
recent years. The Samsung (005930) Galaxy S III354 is a SLEEK sleek handset with a 
stunning screen and a great feature set, but;it feels like9(^*&((^*^T*^*() ^%$&^% a cheap toy, 
as does its successor. Nokia’s (NOK) Lumia 920 packs plenty of punch in (^(*&%*z;pe,jpqweqw6e6qw7ec62qe
a sleek package, but it’s thick and heavy, and it is missing a boatload of 
top apps. It’s inevitable — some level of compromise is inherent in all smartphones.";

            var custom = AmazonEvidenceParser.ExtractReviews(ConfigurationManager.AppSettings["CustomReviewData1"], Mood.NEUTRAL).ToList();

            inputs.Add(input1);
            inputs.AddRange(custom);

            //inputs.Add(input);

            #endregion string data

            foreach (var input in inputs)
            {
                var evidence = new Evidence("DragonClassifier.Tests.EvidenceTests", ConfigurationManager.AppSettings["EvidenceRepository"]);
                evidence.AddEvidenceData(input, DragonHelper.DragonHelper.ExcludeList);

                var evidenceData = evidence.GetEvidence();
                var evidenceKeys = evidenceData.Keys.ToList();
                evidenceKeys.Sort();
                var parsedInput = Regex.Replace(input, "[^a-zA-Z]+", " ");
                foreach (var key in evidenceKeys)
                {
                    var expectedKey = key;
                    var regexMatch = @"\b" + expectedKey + @"\b";
                    var regex = new Regex(regexMatch, RegexOptions.IgnoreCase);
                    var expectedCount = regex.Matches(parsedInput).Count;
                    var actualCount = evidenceData[key];

                    parsedInput = regex.Replace(parsedInput, string.Empty);

                    Assert.AreEqual(expectedCount, actualCount);
                }

                parsedInput =
                    DragonHelper.DragonHelper.ExcludeList.Select(exclude => @"\b" + exclude + @"\b")
                                .Select(regexMatch => new Regex(regexMatch, RegexOptions.IgnoreCase))
                                .Aggregate(parsedInput, (current, regex) => regex.Replace(current, string.Empty));

                Assert.AreEqual(parsedInput.Trim(), string.Empty);

                Thread.Sleep(1000);

                evidence.PersistEvidence(false);

                var evidenceLoaded = new Evidence("DragonClassifier.Tests.EvidenceTests", ConfigurationManager.AppSettings["EvidenceRepository"], true);

                var evidenceLoadedData = evidenceLoaded.GetEvidence();
                var evidenceLoadedKeys = evidenceLoadedData.Keys.ToList();
                evidenceLoadedKeys.Sort();

                foreach (var key in evidenceLoadedKeys)
                {
                    Assert.AreEqual(evidenceData[key], evidenceLoadedData[key]);
                }
            }
        }
    }
}
