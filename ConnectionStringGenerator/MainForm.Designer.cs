namespace ConnectionStringGenerator
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tbConnectionString = new System.Windows.Forms.RichTextBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.tbConnectionResult = new System.Windows.Forms.RichTextBox();
            this.connectionStringPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnStartService = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCopyToClipboard = new System.Windows.Forms.ToolStripButton();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbConnectionString
            // 
            this.tbConnectionString.Location = new System.Drawing.Point(295, 58);
            this.tbConnectionString.Name = "tbConnectionString";
            this.tbConnectionString.ReadOnly = true;
            this.tbConnectionString.Size = new System.Drawing.Size(310, 95);
            this.tbConnectionString.TabIndex = 2;
            this.tbConnectionString.Text = "";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(295, 376);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(310, 23);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "Test Connection String";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.TestConnectionString);
            // 
            // tbConnectionResult
            // 
            this.tbConnectionResult.Location = new System.Drawing.Point(295, 172);
            this.tbConnectionResult.Name = "tbConnectionResult";
            this.tbConnectionResult.ReadOnly = true;
            this.tbConnectionResult.Size = new System.Drawing.Size(310, 198);
            this.tbConnectionResult.TabIndex = 4;
            this.tbConnectionResult.Text = "";
            // 
            // connectionStringPropertyGrid
            // 
            this.connectionStringPropertyGrid.Location = new System.Drawing.Point(12, 28);
            this.connectionStringPropertyGrid.Name = "connectionStringPropertyGrid";
            this.connectionStringPropertyGrid.Size = new System.Drawing.Size(277, 371);
            this.connectionStringPropertyGrid.TabIndex = 5;
            this.connectionStringPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.ConnectionStringPropertyGridValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(292, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Connection String:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(292, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Messages:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStartService,
            this.toolStripSeparator1,
            this.btnCopyToClipboard});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(613, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnStartService
            // 
            this.btnStartService.Image = ((System.Drawing.Image)(resources.GetObject("btnStartService.Image")));
            this.btnStartService.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Size = new System.Drawing.Size(204, 22);
            this.btnStartService.Text = "Start Ingres Service on local machine";
            this.btnStartService.Click += new System.EventHandler(this.StartIngresService);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyToClipboard.Image")));
            this.btnCopyToClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(203, 22);
            this.btnCopyToClipboard.Text = "Copy Connection String To Clipboard";
            this.btnCopyToClipboard.Click += new System.EventHandler(this.CopyConnectionStringToClipboard);
            // 
            // timer
            // 
            this.timer.Interval = 10000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 412);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectionStringPropertyGrid);
            this.Controls.Add(this.tbConnectionResult);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.tbConnectionString);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Ingres .NET Connection String Generator";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox tbConnectionString;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.RichTextBox tbConnectionResult;
        private System.Windows.Forms.PropertyGrid connectionStringPropertyGrid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnStartService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnCopyToClipboard;
        private System.Windows.Forms.Timer timer;
    }
}

