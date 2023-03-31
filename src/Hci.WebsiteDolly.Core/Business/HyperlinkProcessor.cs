using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hci.WebsiteDolly.Core.Domain;
using System.Text.RegularExpressions;
using Hci.WebsiteDolly.Core.Utility;

namespace Hci.WebsiteDolly.Core.Business
{
    internal static class HyperlinkProcessor
    {
        readonly static int TOTAL = 1000;

        internal static ProcessorResult Process(Delegate updater, StringBuilder html, string rootUrl)
        {
            ProcessorResult result = new ProcessorResult();

            int count = 0;

            try
            {
                Uri rootUri = new Uri(rootUrl);

                Regex _hyperlink = EnvironmentManager.RegularExpressions.Hyperlink;
                Regex _attribute = EnvironmentManager.RegularExpressions.Attribute;

                MatchCollection matches = _hyperlink.Matches(html.ToString());

                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match linkMatch = matches[i];

                    foreach (Match attMatch in _attribute.Matches(linkMatch.Value.ToString()))
                    {
                        if (string.Equals(attMatch.Groups[1].ToString(), "href", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string absoluteUrl = UriUtility.GetUrl(attMatch.Groups[2].ToString(), rootUri);

                            if (!string.IsNullOrEmpty(absoluteUrl))
                            {
                                html.Remove(linkMatch.Index + attMatch.Groups[2].Index, attMatch.Groups[2].Length);
                                html.Insert(linkMatch.Index + attMatch.Groups[2].Index, absoluteUrl);
                                count++;
                            }

                            break;
                        }
                    }

                    updater.DynamicInvoke((TOTAL / matches.Count) * (matches.Count - i), TOTAL, 0, ImportType.Html);
                }

                updater.DynamicInvoke(TOTAL, TOTAL, matches.Count, ImportType.Html);

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
