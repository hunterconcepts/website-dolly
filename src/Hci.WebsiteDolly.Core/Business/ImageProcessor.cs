using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Hci.WebsiteDolly.Core.Domain;
using Hci.WebsiteDolly.Core.Utility;

namespace Hci.WebsiteDolly.Core.Business
{
    internal static class ImageProcessor
    {
        readonly static int TOTAL = 1000;

        internal static ProcessorResult Process(Delegate updater, StringBuilder html, string rootUrl, string destFolder, string imageFolderName)
        {
            ProcessorResult result = new ProcessorResult();

            int count = 0;

            try
            {
                Uri rootUri = new Uri(rootUrl);
                Regex image = EnvironmentManager.RegularExpressions.Image;
                Regex attribute = EnvironmentManager.RegularExpressions.Attribute;

                string folder = Path.Combine(destFolder, imageFolderName);

                if (Directory.Exists(folder))
                {
                }
                else
                {
                    Directory.CreateDirectory(folder);
                }

                MatchCollection matches = image.Matches(html.ToString());

                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match imageMatch = matches[i];

                    foreach (Match attMatch in attribute.Matches(imageMatch.Value.ToString()))
                    {
                        if (attMatch.Groups[1].ToString().Equals("src", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string url = UriUtility.GetUrl(attMatch.Groups[2].ToString(), rootUri);
                            string originalFileName = string.Empty;
                            string fileName = FileUtility.GetFileName(url, folder, out originalFileName);
                            bool useOriginal = false;

                            FileUtility.WriteUrlToFile(
                                url,
                                Path.Combine(folder, fileName),
                                Path.Combine(folder, originalFileName),
                                out useOriginal);

                            html.Remove(imageMatch.Index + attMatch.Groups[2].Index, attMatch.Groups[2].Length);
                            html.Insert(
                                imageMatch.Index + attMatch.Groups[2].Index,
                                UriUtility.HttpPathCombine(imageFolderName, useOriginal ? originalFileName : fileName));

                            count++;

                            break;
                        }
                    }

                    updater.DynamicInvoke((TOTAL / matches.Count) * (matches.Count - i), TOTAL, 0, ImportType.Images);
                }

                updater.DynamicInvoke(TOTAL, TOTAL, matches.Count, ImportType.Images);

                result.Status = ProcessorResultStatus.Success;
                result.Count = count;
            }
            catch (ArgumentNullException argumentNullException)
            {
                result.Status = ProcessorResultStatus.Exception;
                result.Exception = argumentNullException;
            }
            catch (Exception ex)
            {
                result.Status = ProcessorResultStatus.Exception;
                result.Exception = ex;
            }

            return result;
        }
    }
}
