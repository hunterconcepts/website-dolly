using System.IO;
using System.Windows.Forms;

namespace Hci.WebsiteDolly.Core.Utility
{
    public static class FormUtility
    {
        public static void BrowseForFolder(TextBox textBox)
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog())
            {
                d.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(textBox.Text) && Directory.Exists(textBox.Text))
                    d.SelectedPath = textBox.Text;

                if (d.ShowDialog() == DialogResult.OK)
                    textBox.Text = d.SelectedPath;
            }
        }

    }
}
