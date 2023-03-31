using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Hci.WebsiteDolly.WindowsClient
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();
        }

        private void Splash_Shown(object sender, EventArgs e)
        {
            Thread.Sleep(750);

            while (Opacity != 0)
            {
                Opacity -= 0.03;
                Thread.Sleep(40);//This is for the speed of the opacity... and will let the form redraw
            }

            Close();
        }
    }
}
