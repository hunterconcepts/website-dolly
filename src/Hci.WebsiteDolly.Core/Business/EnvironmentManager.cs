using System.Text.RegularExpressions;

namespace Hci.WebsiteDolly.Core.Business
{
    public static class EnvironmentManager
    {
        public static class ApplicationSettings
        {
            public static int RequestTimeout
            {
                get { return 10 * 1000; }
            }

            public static int ChunkSize
            {
                get { return 8 * 1024; }
            }
        }

        public static class RegularExpressions
        {
            internal static string _hyperlink = @"(?<anchor><\s*(a|area)\s*(?:(?:\b\w+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)?\s*>)";
            internal static string _image = @"<img\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>";
            internal static string _attribute = @"(?<name>\b\w+\b)\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> \s//]+)\s*)";
            internal static string _javascript = @"<script\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>";
            internal static string _stylesheetImage = @"url\('?(?<url>(\w|/|.|_|-)*?)'?\)";
            internal static string _stylesheetImport = @"import."".*?""";
            internal static string _style = @"<style\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>(?<content>.*?)</style>";            
            internal static string _stylesheet = @"<link\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>";
            internal static string _meta = @"<meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>";

            public static Regex Hyperlink
            {
                get { return GetRegex(_hyperlink); }
            }

            public static Regex Attribute
            {
                get { return GetRegex(_attribute); }
            }

            public static Regex Image
            {
                get { return GetRegex(_image); }
            }

            public static Regex StylesheetImage
            {
                get { return GetRegex(_stylesheetImage); }
            }

            public static Regex StylesheetImport
            {
                get { return GetRegex(_stylesheetImport); }
            }

            public static Regex Javascript
            {
                get { return GetRegex(_javascript); }
            }

            public static Regex Style
            {
                get { return GetRegex(_style); }
            }

            public static Regex Stylesheet
            {
                get { return GetRegex(_stylesheet); }
            }

            public static Regex Meta
            {
                get { return GetRegex(_meta); }
            }

            static Regex GetRegex(string regex)
            { 
                return new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            }
        }
    }
}
