using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Hci.WebsiteDolly.Core.Business;
using Hci.WebsiteDolly.Core.Domain;
using Hci.WebsiteDolly.Core.Utility;
using Hci.WebsiteDolly.WindowsClient.Properties;

namespace Hci.WebsiteDolly.WindowsClient
{
    public partial class Dolly : Form
    {
        //--------------------------------------------------------------------------
        //
        //  Variables
        //
        //--------------------------------------------------------------------------

        readonly string _closedText = "Options >>";
        readonly string _openText = "Options <<";
        readonly int _maxHeight = 600;
        readonly int _minHeight = 325;
        readonly int _slideStep = 10;
        readonly int _progressBarMaximum = 1000;
        readonly int _progressBarChunks = 4;
        bool _destinationSet = false;
        DateTime _startTime = new DateTime();
        DateTime _endTime = new DateTime();

        //--------------------------------------------------------------------------
        //
        //  Constructors
        //
        //--------------------------------------------------------------------------

        public Dolly()
        {
            InitializeComponent();

            Thread thread = new Thread(new ThreadStart(ShowSplash));
            thread.Start();
            Thread.Sleep(3200);

            Reset();

            LoadSettings();
            
            OptionsVisible(false);

            CheckLicense();
        }

        //--------------------------------------------------------------------------
        //
        //  Properties
        //
        //--------------------------------------------------------------------------

        public string Source
        {
            get { return sourceTextBox.Text.Trim(); }
        }

        public string Destination
        {
            get { return destTextBox.Text.Trim(); }
        }

        public string Username
        {
            get { return usernameTextBox.Text.Trim(); }
        }

        public string Password
        {
            get { return passwordTextBox.Text; }
        }

        //--------------------------------------------------------------------------
        //
        //  Methods [Private]
        //
        //--------------------------------------------------------------------------

        static void ShowSplash()
        {
            Splash splash = new Splash();
            splash.ShowDialog();
        }

        void Reset()
        {
            importerStatusLabel.Text = Resources.WaitingText;
            sourceTextBox.Text = string.Empty;
            destTextBox.Text = Settings.Default.DestinationFolder;
        }

        void LoadSettings()
        {
            useFoldersResourcesCheckBox.Checked = Settings.Default.UserFolders;
            destinationTextBox.Text = Settings.Default.DestinationFolder;
            imageTextBox.Text = Settings.Default.ImageFolderName;
            stylesheetTextBox.Text = Settings.Default.StylesheetFolderName;
            scriptsTextBox.Text = Settings.Default.ScriptFolderName;
            videoTextBox.Text = Settings.Default.VideoFolderName;

            openDestinationCheckBox.Checked = Settings.Default.OpenDestination;
            openSourceCheckBox.Checked = Settings.Default.OpenSource;
            exploreDestinationCheckBox.Checked = Settings.Default.ExploreDestination;
            sourceShortcutCheckBox.Checked = Settings.Default.SourceShortcut;
            clearDestinationCheckBox.Checked = Settings.Default.ClearDestination;
            loggingEnabledCheckBox.Checked = Settings.Default.LoggingEnabled;

            logFileTextBox.Text = Settings.Default.LoggingPath;
            logFileTextBox.Enabled = loggingEnabledCheckBox.Checked;
            separateFilesCheckBox.Checked = Settings.Default.SeparateFiles;

            licenseTextBox.Text = Settings.Default.LicenseKey;
        }

        void OptionsVisible(bool show, bool slide = false)
        {
            optionsButton.Text = show ? _openText : _closedText;
            optionsTabControl.Visible = show;

            if (slide)
            {
                if (show)
                {
                    for (int i = _maxHeight - _minHeight; i <= _maxHeight; i += _slideStep)
                    {
                        this.Height = i;
                    }

                    this.Height = _maxHeight;
                }
                else
                {
                    for (int i = _maxHeight; i >= _maxHeight - _minHeight; i -= _slideStep)
                    {
                        this.Height = i;
                    }

                    this.Height = _maxHeight - _minHeight;
                }

                //optionsTabControl.Visible = show;
            }
            else
            {
                //optionsTabControl.Visible = show;
                Height = show ? _maxHeight : _maxHeight - _minHeight;
            }

            if (show)
            {
                usernameTextBox.Focus();
            }
        }

        void UpdateProgressBar(ImportType type, int current, int total, int totalMatchedItems)
        {
            string statusLabelText = string.Empty;
            int stepStart = 0;
            int stepEnd = 250;

            switch (type)
            {
                case ImportType.Images:
                    statusLabelText = "Processing images...";
                    stepStart = 0;
                    stepEnd = _progressBarMaximum / _progressBarChunks;
                    break;
                case ImportType.JavaScript:
                    statusLabelText = "Processing scripts...";
                    stepStart = 251;
                    stepEnd = (_progressBarMaximum / _progressBarChunks) * 2;
                    break;
                case ImportType.StyleSheets:
                    stepStart = 501;
                    stepEnd = (_progressBarMaximum / _progressBarChunks) * 3;
                    statusLabelText = "Processing stylesheets...";
                    break;
                default:
                    stepStart = 751;
                    stepEnd = (_progressBarMaximum / _progressBarChunks) * 4;
                    statusLabelText = "Processing html...";
                    break;
            }

            importerStatusLabel.Text = statusLabelText + " ";

            int currentValue = stepStart + ((current / total) * 250);

            if (currentValue > stepEnd) currentValue = stepEnd;

            importerStatusProgressBar.Maximum = _progressBarMaximum;
            importerStatusProgressBar.Value = currentValue;

            //if (current == total)
            //{
            //    if (totalMatchedItems > 0)
            //    {
            //        //WriteMessage("Successfully imported " + totalMatchedItems + " " + GetDescription(type) + ".", Color.Green);
            //    }

            //    if (Settings.Default.LoggingEnabled)
            //    {
            //        if (!Directory.Exists(Settings.Default.LoggingPath))
            //        {
            //            Directory.CreateDirectory(Settings.Default.LoggingPath);
            //        }

            //        string domain = Utility.CrawlHelper.GetDomain(sourceTextBox.Text);
            //        string dateTime = DateTime.Now.ToString("yyyyMMdd-hhmmss");
            //        string path = Path.Combine(Settings.Default.LoggingPath, domain + "_" + dateTime + ".txt");

            //        using (FileStream stream = File.Create(path))
            //        {
            //            using (StreamWriter writer = new StreamWriter(stream))
            //            {
            //                //
            //                // TODO: More detail... 
            //                //
            //                writer.WriteLine(Source);
            //                writer.WriteLine(Destination);
            //                writer.WriteLine("Success");
            //            }
            //        }
            //    }

            //    importerStatusLabel.Text = "Successfully imported... ";
            //    importerStatusProgressBar.Maximum = _progressBarMaximum;
            //    importerStatusProgressBar.Value = _progressBarMaximum;
            //    Cursor = Cursors.Default;
            //    importButton.Enabled = true;
            //}
        }

        bool IsValidForDelete()
        {
            string destination = Destination.ToLower();

            if (destination == Settings.Default.DestinationFolder.ToLower())
            {
                //
                // Do not clear root destination folder
                //
                return false;
            }
            else if (destination.Length == 3)
            {
                //
                // Do not clear drive
                //
                return false;
            }
            else if (destination.Contains("program files"))
            {
                //
                // TODO: check if one level deeper or not allow program files to be used
                //

                //
                // Do not clear program files
                //
                return false;
            }
            else if (destination.Contains("programdata"))
            {
                //
                // Do not clear program data
                //
                return false;
            }
            else if (destination.Contains("windows"))
            {
                //
                // Do not clear windows
                //
                return false;
            }

            return true;
        }

        void SetDestinationFolder()
        {
            string domain = UriUtility.GetDomain(sourceTextBox.Text);

            if (string.IsNullOrEmpty(domain))
            {
            }
            else
            {
                string destination = Settings.Default.DestinationFolder;

                destTextBox.Text = Path.Combine(destination, domain);
            }

            _destinationSet = true;
        }

        void CheckLicense()
        {
            string title = "Website Dolly";
            string ver = string.Empty;
            string licensed = string.Empty;

            if (Debugger.IsAttached)
            {
                ver = " [DEBUG MODE]";
            }
            else
            {
                
            }

            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ver = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }
                else
                {
                    Assembly assembly = Assembly.GetAssembly(this.GetType());
                    AssemblyName assemblyName = assembly.GetName();
                    Version version = assemblyName.Version;

                    ver = version.ToString();
                }

                ver = "[Version: " + ver + "]";
            }
            catch { }

            string nfgKey = "nTdXGTUg+GgSLZuLYKbyz979PxlurTZ88fPqc0IgsBXUskYAYq6bgkDGkFeXuBIdulsO3iSGTjIivi+7IAQ+bA98l7CYWEIBKUHwtPboa5g1m8n0PoCzc2Vo2gBW6EQbxxNtUlf53FzdJUrvkozAsYRrmWZR2bAhU/1jRi+W7Ls=";

            cloneButton.Enabled = nfgKey == Settings.Default.LicenseKey;
            if (cloneButton.Enabled)
            {
                licensed = "Licensed to Network for Good";
                title = "Website Dolly Pro";
            }
            else
            {
                licensed = "Unlicensed";
            }

            Text = string.Format("{0} {1} {2}", title, ver, licensed);

        }

        //--------------------------------------------------------------------------
        //
        //  Methods [Private Static]
        //
        //--------------------------------------------------------------------------

        static void WriteExceptions(Exception ex, StreamWriter writer)
        {
            writer.WriteLine(string.Empty);
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                WriteExceptions(ex.InnerException, writer);
            }
        }

        static void TraverseDirectory(string path)
        {
            DirectoryInfo cacheDirectory = new DirectoryInfo(path);

            OutputText(path);

            FileInfo[] files = cacheDirectory.GetFiles();

            if (files.Length == 0)
            {
                OutputText("Directory has no files");
            }
            else
            {
                OutputText("Directory has " + files.Length + " files");

                if (files.Length > 300)
                {
                    int i = 0;
                }

                //GetUrlCacheEntryInfo(
                /*
                foreach (FileInfo file in files)
                {
                    OutputText(file.FullName);
                }
                */
            }

            DirectoryInfo[] directories = cacheDirectory.GetDirectories();

            if (directories.Length == 0)
            {
                OutputText("Directory has no directories");
            }
            else
            {
                foreach (DirectoryInfo directory in directories)
                {
                    TraverseDirectory(directory.FullName);
                }
            }
        }

        static void OutputText(string text)
        {
            /*
            outputTextBox.Text = "[" + DateTime.Now.ToString("mmddyyyy-hhMMss") + "] "
                + text + Environment.NewLine
                + outputTextBox.Text;
            */
        }
        
        //--------------------------------------------------------------------------
        //
        //  Methods [Form Events]
        //
        //--------------------------------------------------------------------------

        void Dolly_Load(object sender, EventArgs e)
        {
            CheckLicense();
        }

        //--------------------------------------------------------------------------
        //
        //  Methods [TextBox Events]
        //
        //--------------------------------------------------------------------------

        void sourceTextBox_TextChanged(object sender, EventArgs e)
        {
            _destinationSet = false;
        }

        //--------------------------------------------------------------------------
        //
        //  Methods [CheckBox Events]
        //
        //--------------------------------------------------------------------------

        void loggingEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            logFileTextBox.Enabled = loggingEnabledCheckBox.Checked;

            if (loggingEnabledCheckBox.Checked)
                logFileTextBox.Focus();
        }

        //--------------------------------------------------------------------------
        //
        //  Methods [Button Events]
        //
        //--------------------------------------------------------------------------
        
        void applySettingsButton_Click(object sender, EventArgs e)
        {
            //
            // Settings
            //
            Settings.Default.DestinationFolder = destinationTextBox.Text;
            Settings.Default.ImageFolderName = imageTextBox.Text;
            Settings.Default.StylesheetFolderName = stylesheetTextBox.Text;
            Settings.Default.ScriptFolderName = scriptsTextBox.Text;
            Settings.Default.VideoFolderName = videoTextBox.Text;
            Settings.Default.UserFolders = useFoldersResourcesCheckBox.Checked;

            Settings.Default.Save();
        }

        void applyActionsButton_Click(object sender, EventArgs e)
        {
            //
            // Actions
            //
            Settings.Default.OpenDestination = openDestinationCheckBox.Checked;
            Settings.Default.OpenSource = openSourceCheckBox.Checked;
            Settings.Default.ExploreDestination = exploreDestinationCheckBox.Checked;
            Settings.Default.ClearDestination = clearDestinationCheckBox.Checked;
            Settings.Default.SourceShortcut = sourceShortcutCheckBox.Checked;
            Settings.Default.CompareFiles = compareFilesCheckBox.Checked;

            Settings.Default.Save();
        }

        void applyLicenseButton_Click(object sender, EventArgs e)
        {
            Settings.Default.LicenseKey = licenseTextBox.Text;
            Settings.Default.Save();
            CheckLicense();
        }

        void destBrowseButton_Click(object sender, EventArgs e)
        {

            FormUtility.BrowseForFolder(destTextBox);
        }

        void CloneButtonClick(object sender, EventArgs e)
        {
            if (!_destinationSet)
            {
                SetDestinationFolder();
            }

            bool created = false;
            string validUrl = string.Empty;

            if (Validation.ValidateUrl(sourceTextBox, errorProvider, out validUrl) 
                && Validation.ValidateFolder(destTextBox, errorProvider, out created) 
                && ValidateChildren())
            {
                //
                // Clear Destination
                //
                if (clearDestinationCheckBox.Checked)
                {
                    if (IsValidForDelete())
                    {
                        if (Directory.Exists(Destination) && !created)
                        {
                            //
                            // Delete Directory
                            //
                            FileUtility.DeleteDirectory(Destination, true);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Your destination folder is invalid", "Invalid Destination");
                    }
                }

                //
                // Reset form controls
                //
                this.Cursor = Cursors.WaitCursor;
                cloneButton.Enabled = false;
                importerStatusProgressBar.Value = 0;
                importerStatusLabel.Text = string.Empty;

                Processor clone = new Processor(Source, Destination, Username, Password,
                    useFoldersResourcesCheckBox.Checked ? imageTextBox.Text : string.Empty,
                    useFoldersResourcesCheckBox.Checked ? stylesheetTextBox.Text : string.Empty,
                    useFoldersResourcesCheckBox.Checked ? scriptsTextBox.Text : string.Empty);

                clone.ProgessUpdate += new Processor.ProgressUpdateHandler(ProcessorProgressUpdate);
                clone.SkinStartImport += new Processor.SkinStartImportHandler(ProcessorCloneStart);
                clone.SkinFinishedImport += new Processor.SkinFinishedImportHandler(ProcessorCloneFinish);
                clone.SkinFailedImport += new Processor.SkinFailedImportHandler(ProcessorCloneFail);
                clone.Clone();
            }
        }

        void optionsButton_Click(object sender, EventArgs e)
        {
            OptionsVisible(optionsButton.Text != _openText, false);
        }

        void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        void loggingApplyButton_Click(object sender, EventArgs e)
        {
            Settings.Default.LoggingPath = logFileTextBox.Text;
            Settings.Default.LoggingEnabled = loggingEnabledCheckBox.Checked;
            Settings.Default.SeparateFiles = separateFilesCheckBox.Checked;

            Settings.Default.Save();
        }

        void normalizeDestinationButton_Click(object sender, EventArgs e)
        {
            FileUtility.NormalizeDirectory(destTextBox.Text);
        }

        void logPathBrowseButton_Click(object sender, EventArgs e)
        {
            FormUtility.BrowseForFolder(logFileTextBox);
        }

        void destinationFolderBrowseButton_Click(object sender, EventArgs e)
        {
            FormUtility.BrowseForFolder(destinationTextBox);
        }

        void importCacheButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Source))
            {
            }
            else
            {
                TraverseDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));
            }
        }

        //--------------------------------------------------------------------------
        //
        //  Methods [Importer Events]
        //
        //--------------------------------------------------------------------------

        void ProcessorProgressUpdate(int current, int total, int totalMatchedItems, ImportType type)
        {
            if (InvokeRequired)
            {
                Invoke(new Processor.ProgressUpdateHandler(ProcessorProgressUpdate), current, total, totalMatchedItems, type);
            }
            else
            {
                switch (type)
                {
                    case ImportType.Video:
                    case ImportType.Miscellaneous:
                    case ImportType.Anchors:
                    case ImportType.Html:
                        UpdateProgressBar(ImportType.Html, current, total, totalMatchedItems);
                        break;
                    case ImportType.Images:
                        UpdateProgressBar(ImportType.Images, current, total, totalMatchedItems);
                        break;
                    case ImportType.JavaScript:
                        UpdateProgressBar(ImportType.JavaScript, current, total, totalMatchedItems);
                        break;
                    case ImportType.StyleSheets:
                        UpdateProgressBar(ImportType.StyleSheets, current, total, totalMatchedItems);
                        break;
                }
            }
        }

        void ProcessorCloneStart(string oldSkinId)
        {
            if (InvokeRequired)
            {
                Invoke(new Processor.SkinStartImportHandler(ProcessorCloneStart), oldSkinId);
            }
            else
            {
                _startTime = DateTime.Now;
            }
        }

        void ProcessorCloneFinish(string oldSkinId, string newSkinId, bool isSuccess, Exception ex)
        {
            _endTime = DateTime.Now;

            if (InvokeRequired)
            {
                Invoke(new Processor.SkinFinishedImportHandler(ProcessorCloneFinish), oldSkinId, newSkinId, isSuccess, ex);
            }
            else
            {
                if (isSuccess)
                {
                    //
                    // Open Source and New pages in default browser
                    //
                    if (openDestinationCheckBox.Checked)
                    {
                        Process.Start(Path.Combine(Destination, "skin.html"));
                    }

                    if (openSourceCheckBox.Checked)
                    {
                        Process.Start(Source);
                    }

                    //
                    // Open Directory
                    //
                    if (exploreDestinationCheckBox.Checked)
                    {
                        Process.Start(Destination);
                    }

                    //
                    // Create Shortcut to Source
                    //
                    if (sourceShortcutCheckBox.Checked)
                    {
                        string domain = UriUtility.GetDomain(Source);

                        TextWriter writer = new StreamWriter(Path.Combine(Destination, domain) + ".url");

                        writer.WriteLine("[InternetShortcut]");
                        writer.WriteLine("URL=" + Source);
                        writer.Close();
                    }

                    //
                    // Write message
                    //
                    //WriteMessage(string.Empty);
                    //WriteMessage("Successfully imported '" + Source + "'!");

                    importerStatusLabel.Text = Resources.CloneSuccessMessage;

                    importerStatusProgressBar.Maximum = _progressBarMaximum;
                    importerStatusProgressBar.Value = _progressBarMaximum;
                }
                else
                {
                    importerStatusLabel.Text = Resources.CloneFailMessage;

                    importerStatusProgressBar.Maximum = _progressBarMaximum;
                    importerStatusProgressBar.Value = 0;
                }

                if (loggingEnabledCheckBox.Checked)
                {
                    if (!Directory.Exists(logFileTextBox.Text))
                    {
                        Directory.CreateDirectory(logFileTextBox.Text);
                    }

                    string domain = UriUtility.GetDomain(sourceTextBox.Text);
                    string dateTime = DateTime.Now.ToString("yyyyMMdd-hhmmss");

                    string path = string.Empty;

                    if (separateFilesCheckBox.Checked)
                    {
                        path = Path.Combine(logFileTextBox.Text, domain + "_" + dateTime + ".txt");
                    }
                    else
                    {
                        path = Path.Combine(logFileTextBox.Text, "Log.txt");
                    }

                    using (FileStream stream = (separateFilesCheckBox.Checked ? File.Create(path) : File.Open(path, FileMode.Append)))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            //
                            // TODO: More detail... 
                            //
                            if (!Settings.Default.SeparateFiles)
                            {
                                writer.WriteLine(string.Empty);
                                writer.WriteLine("--------------------------------------");
                                writer.WriteLine(string.Empty);
                            }
                            writer.WriteLine(Source);
                            writer.WriteLine(Destination);
                            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                                writer.WriteLine("Required Authentication");
                            writer.WriteLine(isSuccess ? "Success" : "Failed");
                            writer.WriteLine(string.Empty);
                            writer.WriteLine("Started  : " + _startTime.ToString("U"));
                            writer.WriteLine("Finished : " + _startTime.ToString("U"));

                            if (isSuccess)
                            {
                            }
                            else
                            {
                                WriteExceptions(ex, writer);
                            }
                        }
                    }
                }

                Cursor = Cursors.Default;
                cloneButton.Enabled = true;
            }
        }

        void ProcessorCloneFail(string message, string oldSkinId)
        {
            if (InvokeRequired)
            {
                Invoke(new Processor.SkinFailedImportHandler(ProcessorCloneFail), message, oldSkinId);
            }
            else
            {
                //WriteMessage(message, Color.Red);
            }
        }
    }
}
