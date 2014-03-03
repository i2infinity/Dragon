using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using HtmlAgilityPack;
using NBoilerpipe.Extractors;

namespace DragonClassifier.Tests
{
    public class Helper
    {
        public static string GetUrlContentsViaReadability(string revieUrl)
        {
            const string readabilityKey = "e3db0ba08b629761c5011e26851ce42ae4c90873";
            var searchUrl = "https://readability.com/api/content/v1/parser?" +
                "&token=" + readabilityKey +
                "&url=" + revieUrl;

            using (var wc = new WebClient())
            {
                var jsonStringResult = wc.DownloadString(searchUrl);
                var value = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(jsonStringResult);
                var result = StripTagsCharArray(value["content"]);
                var newResult = StripIndentAndWhiteSpaceChars(result);
                return newResult;
            }
        }

        public static string GetUrlContentsViaNBoilerPipe(string revieUrl)
        {
            var url = new Uri(revieUrl);
            var web = new HtmlWeb();
            var doc = web.Load(url.AbsoluteUri);
            var html = doc.DocumentNode.OuterHtml;
            var extractedContents = ArticleExtractor.INSTANCE.GetText(html);
            return extractedContents;
        }

        public static IDictionary<string, string> GetUrlContentsViaBoilerPipe(IEnumerable<string> reviewUrls)
        {
            var results = new Dictionary<string, string>();
            foreach (var url in reviewUrls)
            {
                var contents = GetUrlContentsViaNBoilerPipe(url);
                results.Add(url, contents);
            }

            return results;         
        }

        public static IDictionary<string, string> GetUrlContentsViaReadability(IEnumerable<string> reviewUrls)
        {
            var results = new Dictionary<string, string>();
            foreach (var url in reviewUrls)
            {
                var contents = GetUrlContentsViaReadability(url);
                results.Add(url, contents);
            }

            return results;
        }

        private static string StripIndentAndWhiteSpaceChars(string inputString)
        {
            var builderOutput = new StringBuilder(inputString.Length);
            var builderInput = new StringBuilder(inputString);
            for (var i = 0; i < builderInput.Length; i++)
            {
                var c = builderInput[i];

                if (c == ' ' && i < (inputString.Length - 1)
                    && inputString[i + 1] != ' '
                    && inputString[i + 1] != '\t'
                    && inputString[i + 1] != '\n'
                    && inputString[i + 1] != '\r')
                {
                    builderOutput.Append(c);
                    continue;
                }

                if (c == ' ')
                {
                    continue;
                }

                if (c == '\t' || c == '\n' || c == '\r')
                {
                    builderInput[i] = ' ';
                    i--;
                    continue;
                }

                builderOutput.Append(c);
            }

            return builderOutput.ToString();

        }

        public static string StripTagsCharArray(string source)
        {
            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;

            foreach (var letter in source)
            {
                if (letter == '<')
                {
                    inside = true;
                    continue;
                }
                if (letter == '>')
                {
                    inside = false;
                    continue;
                }
                if (inside) continue;
                array[arrayIndex] = letter;
                arrayIndex++;
            }
            return new string(array, 0, arrayIndex);
        }

        public static IDictionary<string, double> GetUClassifyReviewScores(IEnumerable<string> reviewUrls)
        {
            var results = new Dictionary<string, double>();

            foreach (var url in reviewUrls)
            {
                var encodedUrl = @"http://uclassify.com/browse/uClassify/Sentiment/ClassifyUrl/"
                    + "?readkey=" + HttpUtility.UrlEncode("77CmRIn84LVj2uf3ozdO5o6Jts")
                    + "&url=" + HttpUtility.UrlEncode(url)
                    + "&version=1.01";

                string content;
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    content = wc.DownloadString(encodedUrl);
                }

                var rdr = new StringReader(content);
                var xmlDoc = XDocument.Load(rdr);
                XNamespace p = xmlDoc.Root.Attribute("xmlns").Value;

                var classes = xmlDoc.Root.Descendants(p + "classification").Elements(p + "class");
                double result = 0.0;
                foreach (var value in from c in classes let classAttr = c.Attribute("className") where classAttr == null || classAttr.Value == "positive" select c.Attribute("p") into positiveAttr where positiveAttr != null select positiveAttr.Value)
                {
                    double positiveMood;

                    if (!double.TryParse(value, out positiveMood))
                    {
                        throw new Exception("uClassify error");
                    }

                    result = positiveMood * 100;
                }

                results[url] = result;
            }



            return results;
        }
    }
}
