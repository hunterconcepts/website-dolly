using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Hci.WebsiteDolly.Core.Domain;
using System.IO;
using Hci.WebsiteDolly.Core.Utility;
using System.Windows.Forms;

namespace Hci.WebsiteDolly.Core.Business
{
    public class Processor
    {
        string _url;
        string _destFolder;

        string _username;
        string _password;

        string _imageFolder;
        string _stylesheetFolder;
        string _javascriptFolder;

        int _folderCount = 0;

        public delegate void ProgressUpdateHandler(int current, int total, int totalItems, ImportType type);
        public event ProgressUpdateHandler ProgessUpdate;

        public delegate void SkinStartImportHandler(string oldSkinId);
        public event SkinStartImportHandler SkinStartImport;

        public delegate void SkinFinishedImportHandler(string oldSkinId, string newSkinId, bool isSuccess, Exception ex);
        public event SkinFinishedImportHandler SkinFinishedImport;

        public delegate void SkinFailedImportHandler(string message, string oldSkinId);
        public event SkinFailedImportHandler SkinFailedImport;

        /*
        static readonly Regex _docTypeRegEx = new Regex(@"<!DOCTYPE.*>", RegexOptions.Compiled);
        static readonly Regex _headRegEx = new Regex(@"(?i)<head>(.*)(?i)</head>", RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex _bodyStartRegEx = new Regex(@"((?i)<body.*)\{/literal\}", RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex _bodyEndRegex = new Regex(@"\{literal\}(.*)(?i)</html>", RegexOptions.Compiled | RegexOptions.Singleline);
        */

        public Processor(string url, string destFolder, string username, string password, string imageFolder, string stylesheetFolder, string javascriptFolder)
        {
            _url = url;
            _destFolder = destFolder;
            _username = username;
            _password = password;
            _imageFolder = imageFolder;
            _stylesheetFolder = stylesheetFolder;
            _javascriptFolder = javascriptFolder;
        }

        public void Clone()
        {
            Thread th = new Thread(new ThreadStart(CloneThread));

            th.Start();            
        }

        void CloneThread()
        {
            bool isSuccess = false;
            Exception ex = null;

            try
            {
                string actualUrl;

                StringBuilder builder = new StringBuilder(UriUtility.RequestUrlContent(_url, _username, _password, out actualUrl));

                //
                // Process Images
                //
                ProcessorResult imageResult = ImageProcessor.Process(ProgessUpdate, builder, actualUrl, _destFolder, _imageFolder);
                if (imageResult.Status == ProcessorResultStatus.Exception)
                    throw imageResult.Exception;

                //
                // Process Javascript
                //
                ProcessorResult javascriptResult = JavascriptProcessor.Process(ProgessUpdate, builder, actualUrl, _destFolder, _javascriptFolder);
                if (javascriptResult.Status == ProcessorResultStatus.Exception)
                    throw javascriptResult.Exception;

                //Processor.ProcessInternalStyle(ProgessUpdate, html, actualUrl, _destFolder);
                
                //
                // Process Stylesheets
                //
                ProcessorResult stylesheetResult = StylesheetProcessor.Process(ProgessUpdate, builder, actualUrl, _destFolder, _stylesheetFolder, _imageFolder);
                if (stylesheetResult.Status == ProcessorResultStatus.Exception)
                    throw stylesheetResult.Exception;

                //
                // Process Hyperlinks
                //
                ProcessorResult hyperlinkResult = HyperlinkProcessor.Process(ProgessUpdate, builder, actualUrl);
                if (hyperlinkResult.Status == ProcessorResultStatus.Exception)
                    throw hyperlinkResult.Exception;

                //
                // Write New Html Content
                //
                File.WriteAllText(Path.Combine(_destFolder, "skin.html"), builder.ToString());

                isSuccess = true;

            }
            catch (ArgumentNullException argumentNullException)
            {
                ex = argumentNullException;
                MessageBox.Show("The url cannot be contacted. This is usually related to the url being invalid, the url requiring authentication, or the url timing out", "Url cannot be reached");
            }
            catch (Exception unhandledException)
            {
                ex = unhandledException;
                MessageBox.Show("An unhandled exception has occurred" + Environment.NewLine + Environment.NewLine + ex.StackTrace, ex.Message);
            }
            finally
            {
                SkinFinishedImport("0", "1", isSuccess, ex);
            }
        }

        void OnProgressUpdate(int current, int totalItems, ImportType type)
        {
            if (ProgessUpdate != null)
                ProgessUpdate(current, _folderCount, totalItems, type);
        }

        void OnSkinStartImport(string oldSkinId)
        {
            if (SkinStartImport != null)
                SkinStartImport(oldSkinId);
        }

        void OnSkinFinishedImport(string oldSkinId, string newSkinId, bool isSuccess, Exception ex)
        {
            if (SkinFinishedImport != null)
                SkinFinishedImport(oldSkinId, newSkinId, isSuccess, ex);
           
        }

        void OnSkinFailedImport(string message, string oldSkinId)
        {
            if (SkinFailedImport != null)
                SkinFailedImport(message, oldSkinId);
        }
    }
}
