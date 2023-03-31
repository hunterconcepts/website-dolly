using System;
using System.IO;
using System.Net;
using System.Text;
using Hci.WebsiteDolly.Core.Business;

namespace Hci.WebsiteDolly.Core.Utility
{
    public static class UriUtility
    {
        public static string GetDomain(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            else
            {
                string formattedSource = url.Trim().ToLower().Replace("http://", string.Empty).Replace("https://", string.Empty).Replace("www.", string.Empty);

                int index = formattedSource.IndexOf('/');

                if (index >= 0)
                {
                    formattedSource = formattedSource.Substring(0, index);
                }

                return formattedSource;
            }
        }

        public static string GetRootUrl(string url)
        {
            int queryPos = url.IndexOf('?');

            if (queryPos == -1)
                queryPos = url.Length;

            int lastWhackPos = url.LastIndexOf('/', queryPos - 1, queryPos - 1) + 1;

            return url.Substring(0, lastWhackPos);
        }

        public static string GetUrl(string link, Uri rootUri)
        {
            string originalLink = link;

            if (link != null)
            {
                link = System.Web.HttpUtility.HtmlDecode(link);

                if (link.StartsWith("//"))
                    link = "http:" + link;

                try
                {
                    //
                    // strip off internal links, so we don't index same page over again
                    //
                    int hashPos = link.IndexOf('#');

                    if (hashPos != -1)
                    {
                        link = link.Substring(0, hashPos);
                    }

                    if (link.IndexOf("javascript:") == -1 && link.IndexOf("mailto:") == -1)
                    {
                        if ((link.Length > 8) && (link.StartsWith("http://")
                                                    || link.StartsWith("https://")
                                                    || link.StartsWith("file://")
                                                    || link.StartsWith("//")
                                                    || link.StartsWith(@"\\")))
                        {
                            // all assumed to be 'external' links, which we don't process at all
                            // in this version, although we still populate the linkExternal array
                            // for possible future use.
                            return link;
                        }
                        else if (link.StartsWith("?"))
                        {
                            // SPECIAL CASE:
                            // it's possible to have /?query which sends the querystring to the
                            // 'default' page in a directory
                            return rootUri + link;
                        }
                        else
                        {
                            return new Uri(rootUri, link).ToString();
                        }
                    }
                }
                catch
                {
                    // TODO: log
                    return null;
                }
            }

            return null;
        }

        public static string RequestUrlContent(string url, string username, string password, out string actualUrl)
        {
            int requestTimeout = EnvironmentManager.ApplicationSettings.RequestTimeout;

            try
            {
                //
                // Open the requested URL
                //
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                }
                else
                {
                    if (username.Contains("\\"))
                    {
                        req.Credentials = new NetworkCredential(
                            username.Split('\\')[1],
                            password,
                            username.Split('\\')[0]);
                    }
                    else
                    {
                        req.Credentials = new NetworkCredential(username, password);
                    }
                }

                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 3;
                req.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; WebCrawler)";
                req.KeepAlive = true;
                req.Timeout = requestTimeout;

                //
                // Get the stream from the returned web response
                //
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    string enc = resp.ContentEncoding;

                    if (string.IsNullOrEmpty(enc))
                    {
                        if (resp.CharacterSet.Contains("8859"))
                            enc = "ascii";
                        else
                            enc = "utf-8";
                    }

                    actualUrl = resp.ResponseUri.AbsoluteUri;

                    string html = string.Empty;

                    using (StreamReader rd = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(enc)))
                    {
                        html = rd.ReadToEnd();
                    }

                    return html.Replace("�", "'");
                }
            }
            catch
            {
                actualUrl = null;

                return null;
            }
        }

        public static string HttpPathCombine(string first, string last)
        {
            return Path.Combine(first, last).Replace("\\", "/");
        }
    }
}
