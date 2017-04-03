namespace RepoZ.Win
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
			this.lblPathStatic = new System.Windows.Forms.Label();
			this.lblPath = new System.Windows.Forms.Label();
			this.lblGitBranchStatic = new System.Windows.Forms.Label();
			this.lblGitBranch = new System.Windows.Forms.Label();
			this.lblRepoStatic = new System.Windows.Forms.Label();
			this.lblRepository = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.lblFound = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.colRepo = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colBranch = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.buttonCrawlerTest = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// lblPathStatic
			// 
			this.lblPathStatic.AutoSize = true;
			this.lblPathStatic.Location = new System.Drawing.Point(31, 46);
			this.lblPathStatic.Name = "lblPathStatic";
			this.lblPathStatic.Size = new System.Drawing.Size(29, 13);
			this.lblPathStatic.TabIndex = 0;
			this.lblPathStatic.Text = "Path";
			// 
			// lblPath
			// 
			this.lblPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblPath.Location = new System.Drawing.Point(72, 46);
			this.lblPath.Name = "lblPath";
			this.lblPath.Size = new System.Drawing.Size(559, 13);
			this.lblPath.TabIndex = 0;
			this.lblPath.Text = "<path>";
			// 
			// lblGitBranchStatic
			// 
			this.lblGitBranchStatic.AutoSize = true;
			this.lblGitBranchStatic.Location = new System.Drawing.Point(31, 96);
			this.lblGitBranchStatic.Name = "lblGitBranchStatic";
			this.lblGitBranchStatic.Size = new System.Drawing.Size(41, 13);
			this.lblGitBranchStatic.TabIndex = 0;
			this.lblGitBranchStatic.Text = "Branch";
			// 
			// lblGitBranch
			// 
			this.lblGitBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblGitBranch.Location = new System.Drawing.Point(72, 96);
			this.lblGitBranch.Name = "lblGitBranch";
			this.lblGitBranch.Size = new System.Drawing.Size(559, 13);
			this.lblGitBranch.TabIndex = 0;
			this.lblGitBranch.Text = "<Branch>";
			// 
			// lblRepoStatic
			// 
			this.lblRepoStatic.AutoSize = true;
			this.lblRepoStatic.Location = new System.Drawing.Point(31, 71);
			this.lblRepoStatic.Name = "lblRepoStatic";
			this.lblRepoStatic.Size = new System.Drawing.Size(33, 13);
			this.lblRepoStatic.TabIndex = 0;
			this.lblRepoStatic.Text = "Repo";
			// 
			// lblRepository
			// 
			this.lblRepository.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRepository.Location = new System.Drawing.Point(72, 71);
			this.lblRepository.Name = "lblRepository";
			this.lblRepository.Size = new System.Drawing.Size(559, 13);
			this.lblRepository.TabIndex = 0;
			this.lblRepository.Text = "<repo>";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(31, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(37, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Found";
			// 
			// lblFound
			// 
			this.lblFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblFound.Location = new System.Drawing.Point(72, 24);
			this.lblFound.Name = "lblFound";
			this.lblFound.Size = new System.Drawing.Size(559, 13);
			this.lblFound.TabIndex = 0;
			this.lblFound.Text = "<found>";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(31, 154);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(57, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Alternative";
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRepo,
            this.colBranch,
            this.Path});
			this.dataGridView1.Location = new System.Drawing.Point(34, 184);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.Size = new System.Drawing.Size(597, 105);
			this.dataGridView1.TabIndex = 1;
			// 
			// colRepo
			// 
			this.colRepo.DataPropertyName = "Name";
			this.colRepo.HeaderText = "Repo";
			this.colRepo.Name = "colRepo";
			this.colRepo.ReadOnly = true;
			// 
			// colBranch
			// 
			this.colBranch.DataPropertyName = "CurrentBranch";
			this.colBranch.HeaderText = "Branch";
			this.colBranch.Name = "colBranch";
			this.colBranch.ReadOnly = true;
			this.colBranch.Width = 200;
			// 
			// Path
			// 
			this.Path.DataPropertyName = "Path";
			this.Path.HeaderText = "Path";
			this.Path.Name = "Path";
			this.Path.ReadOnly = true;
			this.Path.Width = 250;
			// 
			// buttonCrawlerTest
			// 
			this.buttonCrawlerTest.Location = new System.Drawing.Point(484, 41);
			this.buttonCrawlerTest.Name = "buttonCrawlerTest";
			this.buttonCrawlerTest.Size = new System.Drawing.Size(109, 23);
			this.buttonCrawlerTest.TabIndex = 2;
			this.buttonCrawlerTest.Text = "Test Crawlers";
			this.buttonCrawlerTest.UseVisualStyleBackColor = true;
			this.buttonCrawlerTest.Click += new System.EventHandler(this.buttonCrawlerTest_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(643, 301);
			this.Controls.Add(this.buttonCrawlerTest);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.lblRepository);
			this.Controls.Add(this.lblGitBranch);
			this.Controls.Add(this.lblRepoStatic);
			this.Controls.Add(this.lblFound);
			this.Controls.Add(this.lblPath);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblGitBranchStatic);
			this.Controls.Add(this.lblPathStatic);
			this.Name = "Form1";
			this.Text = "RepoZ";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblPathStatic;
		private System.Windows.Forms.Label lblPath;
		private System.Windows.Forms.Label lblGitBranchStatic;
		private System.Windows.Forms.Label lblGitBranch;
		private System.Windows.Forms.Label lblRepoStatic;
		private System.Windows.Forms.Label lblRepository;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblFound;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRepo;
		private System.Windows.Forms.DataGridViewTextBoxColumn colBranch;
		private System.Windows.Forms.DataGridViewTextBoxColumn Path;
		private System.Windows.Forms.Button buttonCrawlerTest;
	}
}

