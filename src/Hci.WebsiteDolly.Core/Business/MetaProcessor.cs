using System;
using System.Text.RegularExpressions;
using Hci.WebsiteDolly.Core.Domain;

namespace Hci.WebsiteDolly.Core.Business
{
    internal static class MetaProcessor
    {
        internal static ProcessorResult Process(string html, out string metaKeywords, out string metaDescription)
        {
            ProcessorResult result = new ProcessorResult();
            metaKeywords = string.Empty;
            metaDescription = string.Empty;

            try
            {
                int count = 0;
                string keywords = null;
                string description = null;

                Regex meta = EnvironmentManager.RegularExpressions.Meta;
                Regex attribute = EnvironmentManager.RegularExpressions.Attribute;

                foreach (Match metaMatch in meta.Matches(html))
                {
                    string metaKey = null;
                    string metaValue = null;

                    foreach (Match attMatch in attribute.Matches(metaMatch.Value.ToString()))
                    {
                        switch (attMatch.Groups[1].ToString().ToLower())
                        {
                            case "http-equiv":
                                metaKey = attMatch.Groups[2].ToString();
                                count++;
                                break;
                            case "name":
                                //
                                // If it's already set, HTTP-EQUIV takes precedence
                                //
                                if (metaKey == null)
                                {
                                    metaKey = attMatch.Groups[2].ToString();
                                    count++;
                                }
                                break;
                            case "content":
                                metaValue = attMatch.Groups[2].ToString();
                                count++;
                                break;
                        }
                    }

                    if (metaKey != null)
                    {
                        switch (metaKey.ToLower())
                        {
                            case "description":
                                description = metaValue;
                                break;
                            case "keywords":
                            case "keyword":
                                keywords = metaValue;
                                break;
                        }
                    }
                }

                metaKeywords = keywords;
                metaDescription = description;

                result.Count = count;
                result.Status = ProcessorResultStatus.Success;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.Status = ProcessorResultStatus.Exception;
            }

            return result;
        }
    }
}
