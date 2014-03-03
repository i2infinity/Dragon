using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DragonHelper
{
    public static class DragonHelper
    {
        //http://stackoverflow.com/questions/150750/hashset-vs-list-performance
        public static readonly HashSet<string> ExcludeList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<char, char> TokenCharMappings;

        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static DragonHelper()
        {
            //Tokens to exclude
            var excludedTokens = ConfigurationManager.AppSettings["ExcludedTokensList"];

            if (!string.IsNullOrWhiteSpace(excludedTokens))
            {
                excludedTokens = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, excludedTokens);
                if (!File.Exists(excludedTokens))
                    throw new FileNotFoundException("Unable to find Exluded Files List at " + excludedTokens);

                using (var file = new StreamReader(excludedTokens))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line.Trim())) continue;

                        if (line.StartsWith("--")) continue;

                        if (!ExcludeList.Contains(line.Trim()))
                            ExcludeList.Add(line);
                    }
                }
            }

            #region Token character mappings

            var alphabets = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            var numerics = "0123456789".ToCharArray();

            TokenCharMappings = alphabets.ToDictionary(alpha => alpha);
            foreach (var number in numerics)
            {
                TokenCharMappings.Add(number, ' ');
            }

            #endregion Token character mappings
        }

        public static void SaveReviewFile(Dictionary<string, string> contents, string fileName)
        {
            var xDoc = new XmlDocument();
            var reviews = xDoc.CreateElement("reviews");
            foreach (var content in contents)
            {
                var review = xDoc.CreateElement("review");
                var productName = xDoc.CreateElement("product_name");
                var reviewText = xDoc.CreateElement("review_text");

                productName.InnerText = content.Key;
                reviewText.InnerText = content.Value.Trim();

                review.AppendChild(productName);
                review.AppendChild(reviewText);

                reviews.AppendChild(review);
            }

            xDoc.AppendChild(reviews);

            BackupFile(fileName);

            xDoc.Save(fileName);

            Logger.DebugFormat("Saved contents to {0}",fileName);
        }

        public static void BackupFile(string fullFileName)
        {
            try
            {
                if (!File.Exists(fullFileName))
                {
                    return;
                }

                var ext = Path.GetExtension(fullFileName);
                var newFileName = fullFileName + DateTime.Now.ToString(".yyyy.MM.dd.HH.mm.ss.fff") + ext;
                File.Copy(fullFileName, newFileName);
            }
            catch (Exception ex)
            {
                Logger.Error("BackupFile - ", ex);
                throw;
            }
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

        private static string StripIndentAndWhiteSpaceChars(string inputString)
        {
            var builderOutput = new StringBuilder(inputString.Length);
            var builderInput = new StringBuilder(inputString);
            try
            {
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

                    if(c == '\t' || c == '\n' || c == '\r')
                    {
                        builderInput[i] = ' ';
                        i--;
                        continue;
                    }

                    builderOutput.Append(c);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("StripIndentAndWhiteSpaceChars - ", ex);
                throw;
            }

            return builderOutput.ToString();

        }

        public static Dictionary<string, int> GetWordFrequency(IEnumerable<string> words, bool writeToFile)
        {
            var wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var token in words)
            {
                if (!wordFrequency.ContainsKey(token))
                    wordFrequency[token] = 0;

                wordFrequency[token]++;
            }

            #region Write to CSV File

            StreamWriter writer;
            var oldOut = Console.Out;
            FileStream ostrm;
            try
            {
                ostrm = new FileStream(@".\WordFrequencyReport.csv", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return wordFrequency;
            }

            Console.SetOut(writer);
            foreach (var word in wordFrequency)
            {
                Console.WriteLine("{0},{1}", word.Key, word.Value);
            }
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();

            #endregion Write to CSV File

            return wordFrequency;
        }

        public static string StringCleanserRetainAlphabets(string input)
        {
            var sb = new StringBuilder();

            //Retain only characters
            foreach (var c in input)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
                else// if (c == '.' || c == ' ' || c == ',' || c == ';' || c == '-' || c == '\'' || c == '’')
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }
    }
}

