namespace WorldsServer
{
    partial class ServerForm
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
            this.GoButton = new System.Windows.Forms.Button();
            this.worldTypesList = new System.Windows.Forms.ListBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.worldListLabel = new System.Windows.Forms.Label();
            this.newGameButton = new System.Windows.Forms.RadioButton();
            this.loadGameButton = new System.Windows.Forms.RadioButton();
            this.orLabel = new System.Windows.Forms.Label();
            this.loadLabel = new System.Windows.Forms.Label();
            this.loadGameList = new System.Windows.Forms.ListBox();
            this.seedLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.newGamePanel = new System.Windows.Forms.Panel();
            this.loadGamePanel = new System.Windows.Forms.Panel();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.loadSinglePlayerButton = new System.Windows.Forms.Button();
            this.newGamePanel.SuspendLayout();
            this.loadGamePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // GoButton
            // 
            this.GoButton.Location = new System.Drawing.Point(199, 386);
            this.GoButton.Name = "GoButton";
            this.GoButton.Size = new System.Drawing.Size(75, 23);
            this.GoButton.TabIndex = 0;
            this.GoButton.Text = "Go!";
            this.GoButton.UseVisualStyleBackColor = true;
            this.GoButton.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // worldTypesList
            // 
            this.worldTypesList.FormattingEnabled = true;
            this.worldTypesList.Location = new System.Drawing.Point(102, 81);
            this.worldTypesList.Name = "worldTypesList";
            this.worldTypesList.Size = new System.Drawing.Size(168, 95);
            this.worldTypesList.TabIndex = 1;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(5, 11);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(67, 13);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "World name:";
            // 
            // nameBox
            // 
            this.nameBox.Location = new System.Drawing.Point(102, 8);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(168, 20);
            this.nameBox.TabIndex = 3;
            // 
            // worldListLabel
            // 
            this.worldListLabel.AutoSize = true;
            this.worldListLabel.Location = new System.Drawing.Point(5, 81);
            this.worldListLabel.Name = "worldListLabel";
            this.worldListLabel.Size = new System.Drawing.Size(91, 13);
            this.worldListLabel.TabIndex = 4;
            this.worldListLabel.Text = "Select world type:";
            // 
            // newGameButton
            // 
            this.newGameButton.AutoSize = true;
            this.newGameButton.Location = new System.Drawing.Point(12, 21);
            this.newGameButton.Name = "newGameButton";
            this.newGameButton.Size = new System.Drawing.Size(76, 17);
            this.newGameButton.TabIndex = 5;
            this.newGameButton.TabStop = true;
            this.newGameButton.Text = "New game";
            this.newGameButton.UseVisualStyleBackColor = true;
            this.newGameButton.CheckedChanged += new System.EventHandler(this.newGameButton_CheckedChanged);
            // 
            // loadGameButton
            // 
            this.loadGameButton.AutoSize = true;
            this.loadGameButton.Location = new System.Drawing.Point(139, 21);
            this.loadGameButton.Name = "loadGameButton";
            this.loadGameButton.Size = new System.Drawing.Size(78, 17);
            this.loadGameButton.TabIndex = 6;
            this.loadGameButton.TabStop = true;
            this.loadGameButton.Text = "Load game";
            this.loadGameButton.UseVisualStyleBackColor = true;
            this.loadGameButton.CheckedChanged += new System.EventHandler(this.loadGameButton_CheckedChanged);
            // 
            // orLabel
            // 
            this.orLabel.AutoSize = true;
            this.orLabel.Location = new System.Drawing.Point(103, 23);
            this.orLabel.Name = "orLabel";
            this.orLabel.Size = new System.Drawing.Size(16, 13);
            this.orLabel.TabIndex = 7;
            this.orLabel.Text = "or";
            // 
            // loadLabel
            // 
            this.loadLabel.AutoSize = true;
            this.loadLabel.Location = new System.Drawing.Point(5, 10);
            this.loadLabel.Name = "loadLabel";
            this.loadLabel.Size = new System.Drawing.Size(63, 13);
            this.loadLabel.TabIndex = 8;
            this.loadLabel.Text = "Load game:";
            // 
            // loadGameList
            // 
            this.loadGameList.FormattingEnabled = true;
            this.loadGameList.Location = new System.Drawing.Point(102, 10);
            this.loadGameList.Name = "loadGameList";
            this.loadGameList.Size = new System.Drawing.Size(168, 95);
            this.loadGameList.TabIndex = 9;
            // 
            // seedLabel
            // 
            this.seedLabel.AutoSize = true;
            this.seedLabel.Location = new System.Drawing.Point(5, 44);
            this.seedLabel.Name = "seedLabel";
            this.seedLabel.Size = new System.Drawing.Size(35, 13);
            this.seedLabel.TabIndex = 10;
            this.seedLabel.Text = "Seed:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(102, 44);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(168, 20);
            this.textBox1.TabIndex = 11;
            // 
            // newGamePanel
            // 
            this.newGamePanel.Controls.Add(this.nameLabel);
            this.newGamePanel.Controls.Add(this.seedLabel);
            this.newGamePanel.Controls.Add(this.textBox1);
            this.newGamePanel.Controls.Add(this.nameBox);
            this.newGamePanel.Controls.Add(this.worldTypesList);
            this.newGamePanel.Controls.Add(this.worldListLabel);
            this.newGamePanel.Location = new System.Drawing.Point(4, 79);
            this.newGamePanel.Name = "newGamePanel";
            this.newGamePanel.Size = new System.Drawing.Size(280, 183);
            this.newGamePanel.TabIndex = 12;
            this.newGamePanel.Visible = false;
            // 
            // loadGamePanel
            // 
            this.loadGamePanel.Controls.Add(this.loadGameList);
            this.loadGamePanel.Controls.Add(this.loadLabel);
            this.loadGamePanel.Location = new System.Drawing.Point(4, 258);
            this.loadGamePanel.Name = "loadGamePanel";
            this.loadGamePanel.Size = new System.Drawing.Size(280, 111);
            this.loadGamePanel.TabIndex = 13;
            this.loadGamePanel.Visible = false;
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(106, 53);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(55, 20);
            this.portTextBox.TabIndex = 12;
            this.portTextBox.Text = "4815";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(9, 56);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 12;
            this.portLabel.Text = "Port:";
            // 
            // loadSinglePlayerButton
            // 
            this.loadSinglePlayerButton.Location = new System.Drawing.Point(12, 386);
            this.loadSinglePlayerButton.Name = "loadSinglePlayerButton";
            this.loadSinglePlayerButton.Size = new System.Drawing.Size(169, 23);
            this.loadSinglePlayerButton.TabIndex = 14;
            this.loadSinglePlayerButton.Text = "Copy singleplayer data";
            this.loadSinglePlayerButton.UseVisualStyleBackColor = true;
            this.loadSinglePlayerButton.Click += new System.EventHandler(this.loadSinglePlayerButton_Click);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 421);
            this.Controls.Add(this.loadSinglePlayerButton);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.loadGamePanel);
            this.Controls.Add(this.newGamePanel);
            this.Controls.Add(this.orLabel);
            this.Controls.Add(this.loadGameButton);
            this.Controls.Add(this.newGameButton);
            this.Controls.Add(this.GoButton);
            this.Name = "ServerForm";
            this.Text = "Start Worlds server";
            this.newGamePanel.ResumeLayout(false);
            this.newGamePanel.PerformLayout();
            this.loadGamePanel.ResumeLayout(false);
            this.loadGamePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GoButton;
        private System.Windows.Forms.ListBox worldTypesList;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.Label worldListLabel;
        private System.Windows.Forms.RadioButton newGameButton;
        private System.Windows.Forms.RadioButton loadGameButton;
        private System.Windows.Forms.Label orLabel;
        private System.Windows.Forms.Label loadLabel;
        private System.Windows.Forms.ListBox loadGameList;
        private System.Windows.Forms.Label seedLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel newGamePanel;
        private System.Windows.Forms.Panel loadGamePanel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Button loadSinglePlayerButton;
    }
}

