using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Hci.WebsiteDolly.Core.Domain;
using Hci.WebsiteDolly.Core.Utility;

namespace Hci.WebsiteDolly.Core.Business
{
    public static class StylesheetProcessor
    {
        readonly static int TOTAL = 1000;

        public static ProcessorResult Process(Delegate updater, StringBuilder html, string rootUrl, string destFolder, string stylesheetFolder, string imageFolder)
        {
            ProcessorResult result = new ProcessorResult();

            int count = 0;

            try
            {
                Uri rootUri = new Uri(rootUrl);

                Regex stylesheet = EnvironmentManager.RegularExpressions.Stylesheet;
                Regex attribute = EnvironmentManager.RegularExpressions.Attribute;

                string folder = Path.Combine(destFolder, stylesheetFolder);

                if (Directory.Exists(folder))
                {
                }
                else
                {
                    Directory.CreateDirectory(folder);
                }

                MatchCollection matches = stylesheet.Matches(html.ToString());

                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match styleMatch = matches[i];
                    MatchCollection atts = attribute.Matches(styleMatch.Value.ToString());

                    bool isCss = false;

                    foreach (Match attMatch in atts)
                    {
                        if (attMatch.Groups[1].ToString().Equals("rel", StringComparison.InvariantCultureIgnoreCase) &&
                            attMatch.Groups[2].ToString().Equals("stylesheet", StringComparison.InvariantCultureIgnoreCase))
                        {
                            isCss = true;

                            break;
                        }
                    }

                    if (isCss)
                    {
                        foreach (Match attMatch in atts)
                        {
                            if (attMatch.Groups[1].ToString().Equals("href", StringComparison.InvariantCultureIgnoreCase))
                            {
                                string url = UriUtility.GetUrl(attMatch.Groups[2].ToString(), rootUri);

                                string originalFileName = string.Empty;
                                string fileName = FileUtility.GetFileName(url, folder, out originalFileName);
                                bool useOriginal = false;

                                if (FileUtility.WriteUrlToFile(
                                    url,
                                    Path.Combine(folder, fileName),
                                    Path.Combine(folder, originalFileName),
                                    out useOriginal))
                                {
                                    string relativeRootUrl = UriUtility.GetRootUrl(url);

                                    ProcessStylesheetImage(Path.Combine(folder, fileName), relativeRootUrl, destFolder, imageFolder);
                                    //ProcessImportStylesheet(folder, relativeRootUrl, destFolder);

                                    html.Remove(styleMatch.Index + attMatch.Groups[2].Index, attMatch.Groups[2].Length);
                                    html.Insert(
                                        styleMatch.Index + attMatch.Groups[2].Index,
                                        UriUtility.HttpPathCombine(stylesheetFolder, useOriginal ? originalFileName : fileName));
                                }
                                else
                                {
                                    //
                                    // url may be invalid
                                    //
                                }

                                // TODO: massage css

                                break;
                            }
                        }
                    }

                    updater.DynamicInvoke((TOTAL / matches.Count) * (matches.Count - i), TOTAL, 0, ImportType.StyleSheets);
                }

                updater.DynamicInvoke(TOTAL, TOTAL, matches.Count, ImportType.StyleSheets);

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

        static void ProcessStylesheetImage(string stylesheetPath, string rootUrl, string destFolder, string imageFolder)
        {
            if (string.IsNullOrEmpty(rootUrl))
            {
            }
            else
            {
                Uri rootUri = new Uri(rootUrl);

                Regex stylesheetImage = EnvironmentManager.RegularExpressions.StylesheetImage;

                string folder = Path.Combine(destFolder, imageFolder);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                StringBuilder css = new StringBuilder(File.ReadAllText(stylesheetPath));

                MatchCollection matches = stylesheetImage.Matches(css.ToString());

                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match urlMatch = matches[i];

                    string url = UriUtility.GetUrl(urlMatch.Groups[1].ToString(), rootUri);
                    string originalFileName = string.Empty;
                    string fileName = FileUtility.GetFileName(url, folder, out originalFileName);
                    bool useOriginal = false;

                    FileUtility.WriteUrlToFile(
                        url,
                        Path.Combine(folder, fileName), Path.Combine(folder, originalFileName),
                        out useOriginal);

                    css.Remove(urlMatch.Groups[1].Index, urlMatch.Groups[1].Length);
                    css.Insert(
                        urlMatch.Groups[1].Index,
                        "../" + Path.Combine(imageFolder, useOriginal ? originalFileName : fileName).Replace("\\", "/"));
                }

                File.WriteAllText(stylesheetPath, css.ToString());
            }
        }

        /*
        public static void ProcessInternalStyle(Delegate updater, StringBuilder html, string rootUrl, string destFolder)
        {
        }
        */

        /*
        static void ProcessImportStylesheet(string stylesheetPath, string rootUrl, string destFolder)
        {
        }
        */
    }
}
