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
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// lblPathStatic
			// 
			this.lblPathStatic.AutoSize = true;
			this.lblPathStatic.Location = new System.Drawing.Point(41, 57);
			this.lblPathStatic.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblPathStatic.Name = "lblPathStatic";
			this.lblPathStatic.Size = new System.Drawing.Size(37, 17);
			this.lblPathStatic.TabIndex = 0;
			this.lblPathStatic.Text = "Path";
			// 
			// lblPath
			// 
			this.lblPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblPath.Location = new System.Drawing.Point(96, 57);
			this.lblPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblPath.Name = "lblPath";
			this.lblPath.Size = new System.Drawing.Size(761, 16);
			this.lblPath.TabIndex = 0;
			this.lblPath.Text = "<path>";
			// 
			// lblGitBranchStatic
			// 
			this.lblGitBranchStatic.AutoSize = true;
			this.lblGitBranchStatic.Location = new System.Drawing.Point(41, 118);
			this.lblGitBranchStatic.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGitBranchStatic.Name = "lblGitBranchStatic";
			this.lblGitBranchStatic.Size = new System.Drawing.Size(53, 17);
			this.lblGitBranchStatic.TabIndex = 0;
			this.lblGitBranchStatic.Text = "Branch";
			// 
			// lblGitBranch
			// 
			this.lblGitBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblGitBranch.Location = new System.Drawing.Point(96, 118);
			this.lblGitBranch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGitBranch.Name = "lblGitBranch";
			this.lblGitBranch.Size = new System.Drawing.Size(761, 16);
			this.lblGitBranch.TabIndex = 0;
			this.lblGitBranch.Text = "<Branch>";
			// 
			// lblRepoStatic
			// 
			this.lblRepoStatic.AutoSize = true;
			this.lblRepoStatic.Location = new System.Drawing.Point(41, 87);
			this.lblRepoStatic.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRepoStatic.Name = "lblRepoStatic";
			this.lblRepoStatic.Size = new System.Drawing.Size(42, 17);
			this.lblRepoStatic.TabIndex = 0;
			this.lblRepoStatic.Text = "Repo";
			// 
			// lblRepository
			// 
			this.lblRepository.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRepository.Location = new System.Drawing.Point(96, 87);
			this.lblRepository.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblRepository.Name = "lblRepository";
			this.lblRepository.Size = new System.Drawing.Size(761, 16);
			this.lblRepository.TabIndex = 0;
			this.lblRepository.Text = "<repo>";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(41, 30);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Found";
			// 
			// lblFound
			// 
			this.lblFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblFound.Location = new System.Drawing.Point(96, 30);
			this.lblFound.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblFound.Name = "lblFound";
			this.lblFound.Size = new System.Drawing.Size(761, 16);
			this.lblFound.TabIndex = 0;
			this.lblFound.Text = "<found>";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(41, 190);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(75, 17);
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
			this.dataGridView1.Location = new System.Drawing.Point(45, 226);
			this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.Size = new System.Drawing.Size(812, 444);
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
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(873, 685);
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
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
	}
}

