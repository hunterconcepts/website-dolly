using System.IO;
using System.Windows.Forms;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Hci.WebsiteDolly.Core.Business
{
    public static class Validation
    {
        public static bool ValidateFolder(TextBox textBox, ErrorProvider errorProvider, out bool created)
        {
            bool isValid = true;
            created = false;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                errorProvider.SetError(textBox, "Please select a valid folder.");
                isValid = false;
            }
            else
            {
                if (Directory.Exists(textBox.Text))
                {
                }
                else
                {
                    Directory.CreateDirectory(textBox.Text);
                    created = true;
                }

                errorProvider.SetError(textBox, null);
            }

            return isValid;
        }

        public static void Ping(TextBox textBox, ErrorProvider errorProvider)
        {
            string url = textBox.Text;

            Ping pingSender = new Ping();

            AutoResetEvent waiter = new AutoResetEvent(false);

            // When the PingCompleted event is raised,
            // the PingCompletedCallback method is called.
            pingSender.PingCompleted += new PingCompletedEventHandler
            (PingCompletedCallback);

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            // Wait 12 seconds for a reply.
            int timeout = 12000;

            // Set options for transmission:
            // The data can go through 64 gateways or routers
            // before it is destroyed, and the data packet
            // cannot be fragmented.
            PingOptions options = new PingOptions(64, true);

            Console.WriteLine("Time to live: {0}", options.Ttl);
            Console.WriteLine("Don't fragment: {0}", options.DontFragment);

            // Send the ping asynchronously.
            // Use the waiter as the user token.
            // When the callback completes, it can wake up this thread.
            pingSender.SendAsync(url, timeout, buffer, options, waiter);

            // Prevent this example application from ending.
            // A real application should do something useful
            // when possible.
            waiter.WaitOne();
            Console.WriteLine("Ping example completed.");
        }

        private static void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            // If the operation was canceled, display a message to the user.
            if (e.Cancelled)
            {
                Console.WriteLine("Ping canceled.");

                // Let the main thread resume.
                // UserToken is the AutoResetEvent object that the main thread
                // is waiting for.
                ((AutoResetEvent)e.UserState).Set();
            }

            // If an error occurred, display the exception to the user.
            if (e.Error != null)
            {
                Console.WriteLine("Ping failed:");
                Console.WriteLine(e.Error.ToString());

                // Let the main thread resume.
                ((AutoResetEvent)e.UserState).Set();
            }

            PingReply reply = e.Reply;

            DisplayReply(reply);

            // Let the main thread resume.
            ((AutoResetEvent)e.UserState).Set();
        }

        public static void DisplayReply(PingReply reply)
        {
            if (reply == null)
                return;

            Console.WriteLine("ping status: {0}", reply.Status);
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("Address: {0}", reply.Address.ToString
                ());
                Console.WriteLine("RoundTrip time: {0}",
                reply.RoundtripTime);
                Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                Console.WriteLine("Don't fragment: {0}",
                reply.Options.DontFragment);
                Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
            }
        }

        public static bool ValidateUrl(TextBox textBox, ErrorProvider errorProvider, out string validUrl)
        {
            string url = textBox.Text.ToLower().Trim();
            validUrl = string.Empty;
           
            if (string.IsNullOrEmpty(url))
            {                
                errorProvider.SetError(textBox, "Source is empty.");
                return false;
            }
            else
            {
                Uri uri;

                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    uri = new Uri(url);
                    validUrl = uri.AbsoluteUri;
                    return true;
                }
                else if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    textBox.Text = "http://" + textBox.Text;
                    return ValidateUrl(textBox, errorProvider, out validUrl);
                }
            }

            errorProvider.SetError(textBox, string.Format("'{0}' is invalid", url));

            return false;
        }

        public static bool IsFolderNameValid(TextBox textBox)
        {
            string value = textBox.Text.Trim();

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            else
            {
                string invalidCharacters = "\"*/:<>?\\|";

                foreach (char c in invalidCharacters.ToCharArray())
                {
                    if (textBox.Text.IndexOf(c) >= 0)
                        return false;
                }
            }

            return true;
        }
    }
}
