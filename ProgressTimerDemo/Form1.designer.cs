namespace NetEti.DemoApplications
{
  partial class Form1
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
      this.theProgressBar = new System.Windows.Forms.ProgressBar();
      this.lblPartInfo = new System.Windows.Forms.Label();
      this.btnStartAll = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // theProgressBar
      // 
      this.theProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.theProgressBar.Location = new System.Drawing.Point(26, 48);
      this.theProgressBar.Name = "theProgressBar";
      this.theProgressBar.Size = new System.Drawing.Size(690, 23);
      this.theProgressBar.TabIndex = 0;
      // 
      // lblPartInfo
      // 
      this.lblPartInfo.AutoSize = true;
      this.lblPartInfo.Location = new System.Drawing.Point(23, 32);
      this.lblPartInfo.Name = "lblPartInfo";
      this.lblPartInfo.Size = new System.Drawing.Size(53, 13);
      this.lblPartInfo.TabIndex = 1;
      this.lblPartInfo.Text = "Fortschritt";
      // 
      // btnStartAll
      // 
      this.btnStartAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStartAll.Location = new System.Drawing.Point(641, 108);
      this.btnStartAll.Name = "btnStartAll";
      this.btnStartAll.Size = new System.Drawing.Size(75, 20);
      this.btnStartAll.TabIndex = 2;
      this.btnStartAll.Text = "Start";
      this.btnStartAll.UseVisualStyleBackColor = true;
      this.btnStartAll.Click += new System.EventHandler(this.btnStartAll_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(748, 150);
      this.Controls.Add(this.btnStartAll);
      this.Controls.Add(this.lblPartInfo);
      this.Controls.Add(this.theProgressBar);
      this.MaximumSize = new System.Drawing.Size(2000, 188);
      this.MinimumSize = new System.Drawing.Size(228, 188);
      this.Name = "Form1";
      this.Text = "ProgressTimer Demo";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar theProgressBar;
    private System.Windows.Forms.Label lblPartInfo;
    private System.Windows.Forms.Button btnStartAll;
  }
}

