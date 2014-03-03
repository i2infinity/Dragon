using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DragonClassifier
{
    public enum ReviewType
    {
        ELECTRONICS,
        AUTOMOTIVE,
        CAMERA,
        CELL_PHONE,
        GAME_SOFTWARE,
        OFFICE_PRODUCTS,
        SOFTWARE,
        DRAGON_VERGE,
        DRAGON_AMAZON
    };

    public enum Mood
    {
        POSITIVE,
        NEGATIVE,
        NEUTRAL,
        UNKNOWN
    }

    public class AmazonEvidenceParser
    {
        private static int _positiveReviewCount;
        private static int _negativeReviewCount;
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string REVIEWS_AUTOMOTIVE = @"C:\Dragon\RawData\Reviews\automotive\";
        private const string REVIEWS_CAMERA_AND_PHOTO = @"C:\Dragon\RawData\Reviews\camera_&_photo\";
        private const string REVIEWS_MOBILE = @"C:\Dragon\RawData\Reviews\cell_phones_&_service\";
        private const string REVIEWS_GAMES = @"C:\Dragon\RawData\Reviews\computer_&_video_games\";
        private const string REVIEWS_ELECTRONICS = @"C:\Dragon\RawData\Reviews\electronics\";
        private const string REVIEWS_OFFICE_PRODUCTS = @"C:\Dragon\RawData\Reviews\office_products\";
        private const string REVIEWS_SOFTWARE = @"C:\Dragon\RawData\Reviews\software\";
        private const string REVIEWS_DRAGON_VERGE = @"C:\Dragon\RawData\Reviews\DragonVerge\";
        private const string REVIEWS_DRAGON_AMAZON = @"C:\Dragon\RawData\Reviews\DragonAmazon\";    //This is the review extracted using the Crawler

        public static IEnumerable<string> GetPositiveReviews()
        {
            var results = new List<string>();

            //results.AddRange(GetPositiveReviews(ReviewType.DRAGON_AMAZON));   //Reviews extracted using the crawler
            //results.AddRange(GetPositiveReviews(ReviewType.DRAGON_VERGE));      //Verge reviews
            results.AddRange(GetPositiveReviews(ReviewType.ELECTRONICS));
            results.AddRange(GetPositiveReviews(ReviewType.AUTOMOTIVE));
            results.AddRange(GetPositiveReviews(ReviewType.CAMERA));
            results.AddRange(GetPositiveReviews(ReviewType.CELL_PHONE));
            results.AddRange(GetPositiveReviews(ReviewType.GAME_SOFTWARE));
            results.AddRange(GetPositiveReviews(ReviewType.OFFICE_PRODUCTS));
            results.AddRange(GetPositiveReviews(ReviewType.SOFTWARE));

            return results;
        }

        public static IEnumerable<string> GetNegativeReviews()
        {
            var results = new List<string>();
            //results.AddRange(GetNegativeReviews(ReviewType.DRAGON_AMAZON));
            //results.AddRange(GetNegativeReviews(ReviewType.DRAGON_VERGE));
            results.AddRange(GetNegativeReviews(ReviewType.ELECTRONICS));
            results.AddRange(GetNegativeReviews(ReviewType.AUTOMOTIVE));
            results.AddRange(GetNegativeReviews(ReviewType.CAMERA));
            results.AddRange(GetNegativeReviews(ReviewType.CELL_PHONE));
            results.AddRange(GetNegativeReviews(ReviewType.GAME_SOFTWARE));
            results.AddRange(GetNegativeReviews(ReviewType.OFFICE_PRODUCTS));
            results.AddRange(GetNegativeReviews(ReviewType.SOFTWARE));

            return results;

        }

        private static IEnumerable<string> GetPositiveReviews(ReviewType type)
        {
            return ExtractReviews(GetReviewsFolder(type) + "positive.review", Mood.POSITIVE);
        }

        private static IEnumerable<string> GetNegativeReviews(ReviewType type)
        {
            return ExtractReviews(GetReviewsFolder(type) + "negative.review", Mood.NEGATIVE);
        }

        public static IEnumerable<string> ExtractReviews(string filePath, Mood mood)
        {
            string contents;
            if (!File.Exists(filePath)) throw new Exception(string.Format("Unable to locate {0}", filePath));
            using (var sr = new StreamReader(filePath, Encoding.UTF7))
            {
                contents = sr.ReadToEnd();
            }

            contents = EscapeXmlContents(contents); //Escape all special characters
            contents = SanitizeXmlString(contents); //Remove any hex characters

            var xDoc = new XmlDocument();
            xDoc.LoadXml(contents);

            var results = new List<string>();
            var localCount = 0;
            foreach (XmlNode review in xDoc.SelectNodes("//review"))
            {
                var productName = review.SelectSingleNode("product_name").InnerText.Replace("\n","");
                var productReview = review.SelectSingleNode("review_text").InnerText;
                localCount++;
                if (mood == Mood.POSITIVE)
                {
                    _positiveReviewCount++;
                    Logger.DebugFormat("Positive[{0}.{1}] Parsing review {2}", _positiveReviewCount, localCount, productName);
                }
                else
                {
                    _negativeReviewCount++;
                    Logger.DebugFormat("Negative[{0}.{1}] Parsing review {2}", _negativeReviewCount, localCount, productName);
                }

                results.Add(productReview);
            }

            return results;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/13962225/conditionally-escape-special-xml-characters
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static string EscapeXmlContents(string contents)
        {
            var result = Regex.Replace(contents, @"(?'Tag'\<{1}[^\>\<]*[\>]{1})|(?'Ampy'\&[A-Za-z0-9]+;)|(?'Special'[\<\>\""\'\&])", match =>
            {
                // If a special (escapable) character was found, replace it.
                if (match.Groups["Special"].Success)
                {
                    switch (match.Groups["Special"].Value)
                    {
                        case "<":
                            return "&lt;";
                        case ">":
                            return "&gt;";
                        case "\"":
                            return "&quot;";
                        case "\'":
                            return "&apos;";
                        case "&":
                            return "&amp;";
                        default:
                            return match.Groups["Special"].Value;
                    }
                }

                // Otherwise, just return what was found.
                return match.Value;
            });

            return result;
        }

        private static string GetReviewsFolder(ReviewType type)
        {
            var filePath = string.Empty;

            switch (type)
            {
                case ReviewType.AUTOMOTIVE:
                    filePath = REVIEWS_AUTOMOTIVE;
                    break;
                case ReviewType.CAMERA:
                    filePath = REVIEWS_CAMERA_AND_PHOTO;
                    break;
                case ReviewType.CELL_PHONE:
                    filePath = REVIEWS_MOBILE;
                    break;
                case ReviewType.ELECTRONICS:
                    filePath = REVIEWS_ELECTRONICS;
                    break;
                case ReviewType.GAME_SOFTWARE:
                    filePath = REVIEWS_GAMES;
                    break;
                case ReviewType.OFFICE_PRODUCTS:
                    filePath = REVIEWS_OFFICE_PRODUCTS;
                    break;
                case ReviewType.SOFTWARE:
                    filePath = REVIEWS_SOFTWARE;
                    break;
                case ReviewType.DRAGON_VERGE:
                    filePath = REVIEWS_DRAGON_VERGE;
                    break;
                case ReviewType.DRAGON_AMAZON:
                    filePath = REVIEWS_DRAGON_AMAZON;
                    break;
            }

            return filePath;
        }

        /// <summary>
        /// Remove illegal XML characters from a string.
        /// http://seattlesoftware.wordpress.com/2008/09/11/hexadecimal-value-0-is-an-invalid-character/
        /// </summary>
        public static string SanitizeXmlString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            var buffer = new StringBuilder(xml.Length);

            foreach (var c in xml.Where(c => IsLegalXmlChar(c)))
            {
                buffer.Append(c);
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// http://seattlesoftware.wordpress.com/2008/09/11/hexadecimal-value-0-is-an-invalid-character/
        /// </summary>
        private static bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }
    }
}
