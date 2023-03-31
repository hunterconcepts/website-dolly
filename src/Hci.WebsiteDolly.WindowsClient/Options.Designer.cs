namespace Hci.WebsiteDolly.WindowsClient
{
    partial class Options
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("Destination");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("General", new System.Windows.Forms.TreeNode[] {
            treeNode21});
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("Files");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("Pre Processes", new System.Windows.Forms.TreeNode[] {
            treeNode23});
            System.Windows.Forms.TreeNode treeNode25 = new System.Windows.Forms.TreeNode("Actions");
            System.Windows.Forms.TreeNode treeNode26 = new System.Windows.Forms.TreeNode("Archiving");
            System.Windows.Forms.TreeNode treeNode27 = new System.Windows.Forms.TreeNode("Post Processes", new System.Windows.Forms.TreeNode[] {
            treeNode25,
            treeNode26});
            System.Windows.Forms.TreeNode treeNode28 = new System.Windows.Forms.TreeNode("Location");
            System.Windows.Forms.TreeNode treeNode29 = new System.Windows.Forms.TreeNode("Format");
            System.Windows.Forms.TreeNode treeNode30 = new System.Windows.Forms.TreeNode("Logging & Output", new System.Windows.Forms.TreeNode[] {
            treeNode28,
            treeNode29});
            this.optionsTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // optionsTreeView
            // 
            this.optionsTreeView.HideSelection = false;
            this.optionsTreeView.Location = new System.Drawing.Point(12, 12);
            this.optionsTreeView.Name = "optionsTreeView";
            treeNode21.Name = "Node5";
            treeNode21.Text = "Destination";
            treeNode22.Name = "Node0";
            treeNode22.Text = "General";
            treeNode23.Name = "Node10";
            treeNode23.Text = "Files";
            treeNode24.Name = "Node8";
            treeNode24.Text = "Pre Processes";
            treeNode25.Name = "Node7";
            treeNode25.Text = "Actions";
            treeNode26.Name = "Node6";
            treeNode26.Text = "Archiving";
            treeNode27.Name = "Node1";
            treeNode27.Text = "Post Processes";
            treeNode28.Name = "Node11";
            treeNode28.Text = "Location";
            treeNode29.Name = "Node12";
            treeNode29.Text = "Format";
            treeNode30.Name = "Node2";
            treeNode30.Text = "Logging & Output";
            this.optionsTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode22,
            treeNode24,
            treeNode27,
            treeNode30});
            this.optionsTreeView.Size = new System.Drawing.Size(172, 540);
            this.optionsTreeView.TabIndex = 0;
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 564);
            this.ControlBox = false;
            this.Controls.Add(this.optionsTreeView);
            this.Name = "Options";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.Options_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView optionsTreeView;
    }
}