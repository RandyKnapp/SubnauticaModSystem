namespace ModInjector_MoreQuickSlots
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.modStatusLabel = new System.Windows.Forms.Label();
			this.targetStatusLabel = new System.Windows.Forms.Label();
			this.injectedStatusLabel = new System.Windows.Forms.Label();
			this.uninstallButton = new System.Windows.Forms.Button();
			this.injectedStatusBad = new System.Windows.Forms.Label();
			this.targetStatusBad = new System.Windows.Forms.Label();
			this.modStatusBad = new System.Windows.Forms.Label();
			this.injectedStatusGood = new System.Windows.Forms.Label();
			this.targetStatusGood = new System.Windows.Forms.Label();
			this.modStatusGood = new System.Windows.Forms.Label();
			this.injectButton = new System.Windows.Forms.Button();
			this.directoryButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(109, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Subnautica Directory:";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(15, 26);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(406, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.ShowNewFolderButton = false;
			// 
			// modStatusLabel
			// 
			this.modStatusLabel.AutoSize = true;
			this.modStatusLabel.Location = new System.Drawing.Point(29, 55);
			this.modStatusLabel.Name = "modStatusLabel";
			this.modStatusLabel.Size = new System.Drawing.Size(40, 13);
			this.modStatusLabel.TabIndex = 4;
			this.modStatusLabel.Text = "Status:";
			// 
			// targetStatusLabel
			// 
			this.targetStatusLabel.AutoSize = true;
			this.targetStatusLabel.Location = new System.Drawing.Point(29, 75);
			this.targetStatusLabel.Name = "targetStatusLabel";
			this.targetStatusLabel.Size = new System.Drawing.Size(40, 13);
			this.targetStatusLabel.TabIndex = 11;
			this.targetStatusLabel.Text = "Status:";
			// 
			// injectedStatusLabel
			// 
			this.injectedStatusLabel.AutoSize = true;
			this.injectedStatusLabel.Location = new System.Drawing.Point(29, 95);
			this.injectedStatusLabel.Name = "injectedStatusLabel";
			this.injectedStatusLabel.Size = new System.Drawing.Size(40, 13);
			this.injectedStatusLabel.TabIndex = 12;
			this.injectedStatusLabel.Text = "Status:";
			// 
			// uninstallButton
			// 
			this.uninstallButton.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.uninstall;
			this.uninstallButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.uninstallButton.Location = new System.Drawing.Point(12, 114);
			this.uninstallButton.Name = "uninstallButton";
			this.uninstallButton.Size = new System.Drawing.Size(75, 23);
			this.uninstallButton.TabIndex = 13;
			this.uninstallButton.Text = "    Uninstall";
			this.uninstallButton.UseVisualStyleBackColor = true;
			this.uninstallButton.Click += new System.EventHandler(this.uninstallButton_Click);
			// 
			// injectedStatusBad
			// 
			this.injectedStatusBad.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.cross;
			this.injectedStatusBad.Location = new System.Drawing.Point(12, 94);
			this.injectedStatusBad.Name = "injectedStatusBad";
			this.injectedStatusBad.Size = new System.Drawing.Size(16, 16);
			this.injectedStatusBad.TabIndex = 10;
			// 
			// targetStatusBad
			// 
			this.targetStatusBad.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.cross;
			this.targetStatusBad.Location = new System.Drawing.Point(12, 74);
			this.targetStatusBad.Name = "targetStatusBad";
			this.targetStatusBad.Size = new System.Drawing.Size(16, 16);
			this.targetStatusBad.TabIndex = 9;
			// 
			// modStatusBad
			// 
			this.modStatusBad.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.cross;
			this.modStatusBad.Location = new System.Drawing.Point(12, 54);
			this.modStatusBad.Name = "modStatusBad";
			this.modStatusBad.Size = new System.Drawing.Size(16, 16);
			this.modStatusBad.TabIndex = 8;
			// 
			// injectedStatusGood
			// 
			this.injectedStatusGood.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.tick;
			this.injectedStatusGood.Location = new System.Drawing.Point(12, 94);
			this.injectedStatusGood.Name = "injectedStatusGood";
			this.injectedStatusGood.Size = new System.Drawing.Size(16, 16);
			this.injectedStatusGood.TabIndex = 7;
			// 
			// targetStatusGood
			// 
			this.targetStatusGood.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.tick;
			this.targetStatusGood.Location = new System.Drawing.Point(12, 74);
			this.targetStatusGood.Name = "targetStatusGood";
			this.targetStatusGood.Size = new System.Drawing.Size(16, 16);
			this.targetStatusGood.TabIndex = 6;
			// 
			// modStatusGood
			// 
			this.modStatusGood.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.tick;
			this.modStatusGood.Location = new System.Drawing.Point(12, 54);
			this.modStatusGood.Name = "modStatusGood";
			this.modStatusGood.Size = new System.Drawing.Size(16, 16);
			this.modStatusGood.TabIndex = 5;
			// 
			// injectButton
			// 
			this.injectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.injectButton.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.inject64;
			this.injectButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.injectButton.Location = new System.Drawing.Point(257, 57);
			this.injectButton.Name = "injectButton";
			this.injectButton.Size = new System.Drawing.Size(200, 78);
			this.injectButton.TabIndex = 3;
			this.injectButton.Text = "      Inject";
			this.injectButton.UseVisualStyleBackColor = true;
			this.injectButton.Click += new System.EventHandler(this.button2_Click);
			// 
			// directoryButton
			// 
			this.directoryButton.Image = global::ModInjector_MoreQuickSlots.Properties.Resources.folder;
			this.directoryButton.Location = new System.Drawing.Point(427, 21);
			this.directoryButton.Name = "directoryButton";
			this.directoryButton.Size = new System.Drawing.Size(30, 30);
			this.directoryButton.TabIndex = 2;
			this.directoryButton.UseVisualStyleBackColor = true;
			this.directoryButton.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(470, 149);
			this.Controls.Add(this.uninstallButton);
			this.Controls.Add(this.injectedStatusLabel);
			this.Controls.Add(this.targetStatusLabel);
			this.Controls.Add(this.injectedStatusBad);
			this.Controls.Add(this.targetStatusBad);
			this.Controls.Add(this.modStatusBad);
			this.Controls.Add(this.injectedStatusGood);
			this.Controls.Add(this.targetStatusGood);
			this.Controls.Add(this.modStatusGood);
			this.Controls.Add(this.modStatusLabel);
			this.Controls.Add(this.injectButton);
			this.Controls.Add(this.directoryButton);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "ModInjector for MoreQuickSlots";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button directoryButton;
		private System.Windows.Forms.Button injectButton;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Label modStatusLabel;
		private System.Windows.Forms.Label modStatusGood;
		private System.Windows.Forms.Label targetStatusGood;
		private System.Windows.Forms.Label injectedStatusGood;
		private System.Windows.Forms.Label modStatusBad;
		private System.Windows.Forms.Label targetStatusBad;
		private System.Windows.Forms.Label injectedStatusBad;
		private System.Windows.Forms.Label targetStatusLabel;
		private System.Windows.Forms.Label injectedStatusLabel;
		private System.Windows.Forms.Button uninstallButton;
	}
}

