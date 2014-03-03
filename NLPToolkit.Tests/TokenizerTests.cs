using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DragonClassifier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace NLPToolkit.Tests
{
    [TestClass]
    public class TokenizerTests
    {
        #region Input parameters

        private const string CONTENT_A = @"The phone’s gaming capabilities are likewise disappointing: Although the Lumia 928 runs 
                            buttery smooth while browsing the Web or switching between apps, the phone has issues 
                            playing games with advanced 3D graphics. In my tests, games such as Asphalt Heat 7 and 
                            Modern Combat 4 took a considerable amount of time to start up, and weren’t as responsive 
                            as they were on other platforms. Even simple games like Angry Birds caused the Lumia 928 
                            to heat up, obviously taxing the antiquated 1.5GHz dual-core Snapdragon processor. Gaming fans, 
                            this isn’t the phone for you.The phone’s gaming capabilities are likewise disappointing: 
                            The US Department of Justice has accused Apple of conspiring with five of the country's six 
                            largest book publishers to fix ebook prices in the trial, which was held in the US District Court 
                            of Manhattan and kicked off this week. On the second day of the trial, Saul described the attitude 
                            toward content creators instilled in the company by co-founder Steve Jobs. He said Apple treats 
                            content suppliers the same regardless of size or influence and wanted to create a level 
                            playing field with ebook suppliers. For that reason, they they offered the same contract 
                            terms to all publishers, big or small. ""That's why I love Apple,"" Saul told the court.
                            Such an effusive statement was rare in Saul's testimony and the comment drew a laugh from 
                            some in the gallery. The problem for Apple is that the DOJ supplied the court with emails 
                            and testimony that seemed to offer other motivations for why the company required all its 
                            ebook suppliers to agree to a contract that guaranteed they would meet the lowest prices 
                            found online.The government, which filed antitrust charges against Apple in April 2012, 
                            alleges that Apple used the contracts as a means of thwarting price competition — 
                            specifically to prevent chief rival Amazon from discounting ebooks. Amazon dominated the 
                            ebook sector and offering lower prices was one of its competitive advantages. But Apple 
                            and the publishers were successful and Amazon was forced to raise prices, costing consumers 
                            hundreds of millions of dollars. Eddy Cue, Apple's senior vice president of internet software 
                            and services, emailed Jobs on Jan. 13, 2010 (see copy of email below) exactly two weeks before 
                            Apple unveiled the iPad and iBookstore. He gave his boss a status report about a meetings he 
                            had with the leaders of Hachette, Penguin, and Harper Collins. He notes that he received a 
                            similar response from Hachette and Penguin and wrote both were ""willing to do an agency 
                            model"" and ""go agency model for new releases with everyone else.Apple didn't offer much 
                            of a choice to publishers, according to David Shanks, the CEO of Penguin Books, one of 
                            the six largest book publishers. Penguin is one of the publishers accused of price 
                            fixing that eventually settled. On the stand Shanks initially appeared nervous and 
                            reluctant to say anything negative about Apple. When Mark Ryan, a DOJ lawyer, asked 
                            him whether the Apple agency agreement was a significant factor in deciding to move 
                            every other retailer to the agency model, Shanks stammered and said he didn't know if 
                            he could agree with that description. That's when Ryan showed the court a video recording 
                            of Shanks’ deposition.";

        #endregion Input parameters

        [TestMethod]
        public void TokensExtract()
        {
            var actualTokens = Tokenizer.TokenizeNow(CONTENT_A);
            Assert.IsNotNull(actualTokens);
        }

        [TestMethod]
        public void TokenMatch()
        {
            const string contents = "the quick brown fox.jumps. over the lazy dog";
            var actualTokens = Tokenizer.TokenizeNow(contents).ToList();
            var expectedTokens = new[] { "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], actualTokens[i]);
            }
        }

        [TestMethod]
        public void CompareStringReplaceMethods()
        {
            const int times = 1000;

            var watch = new Stopwatch();

            watch.Restart();
            for (var i = 0; i < times; i++)
            {
                var resultsA = StringReplaceMethodA(CONTENT_A);
            }
            watch.Stop();

            Console.WriteLine("Time taken for Method A : " + watch.ElapsedMilliseconds + "ms");

            watch.Restart();
            for (var i = 0; i < times; i++)
            {
                var resultsB = StringReplaceMethodB(CONTENT_A);
            }
            watch.Stop();

            Console.WriteLine("Time taken for Method B : " + watch.ElapsedMilliseconds + "ms");

            watch.Restart();
            for (var i = 0; i < times; i++)
            {
                var resultsB = StringReplaceMethodC(CONTENT_A);
            }
            watch.Stop();

            Console.WriteLine("Time taken for Method C : " + watch.ElapsedMilliseconds + "ms");


            Assert.IsNotNull("hello");
        }


        public string StringReplaceMethodA(string content)
        {
            var results = content;

            results = results.Replace('a', 'a');
            results = results.Replace('b', 'a');
            results = results.Replace('c', 'a');
            results = results.Replace('d', 'a');
            results = results.Replace('e', 'a');
            results = results.Replace('f', 'a');
            results = results.Replace('g', 'a');
            results = results.Replace('h', 'a');
            results = results.Replace('i', 'a');
            results = results.Replace('j', 'a');
            results = results.Replace('k', 'a');

            return results;
        }

        public string StringReplaceMethodB(string content)
        {
            var sb = new StringBuilder(content);

            var result = sb.Replace('a', 'a')
                           .Replace('b', 'a')
                           .Replace('c', 'a')
                           .Replace('d', 'a')
                           .Replace('e', 'a')
                           .Replace('f', 'a')
                           .Replace('g', 'a')
                           .Replace('h', 'a')
                           .Replace('i', 'a')
                           .Replace('j', 'a')
                           .Replace('k', 'a');

            return result.ToString();
        }

        public string StringReplaceMethodC(string content)
        {
            var pattern = new Regex("[abcdefghijk]");
            content = pattern.Replace(content, "a");

            return content;
        }

    }
}
