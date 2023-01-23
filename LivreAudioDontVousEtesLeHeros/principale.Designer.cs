
namespace LivreAudioDontVousEtesLeHeros
{
    partial class principale
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmr = new System.Windows.Forms.Timer(this.components);
            this.txtBx = new System.Windows.Forms.TextBox();
            this.timeOut = new System.Windows.Forms.Timer(this.components);
            this.btPasser = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtApk = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAzureAPK = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAzureReg = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tmr
            // 
            this.tmr.Interval = 1000;
            // 
            // txtBx
            // 
            this.txtBx.Location = new System.Drawing.Point(57, 49);
            this.txtBx.Multiline = true;
            this.txtBx.Name = "txtBx";
            this.txtBx.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBx.Size = new System.Drawing.Size(1674, 870);
            this.txtBx.TabIndex = 0;
            // 
            // timeOut
            // 
            this.timeOut.Interval = 2000;
            this.timeOut.Tick += new System.EventHandler(this.timeOut_Tick);
            // 
            // btPasser
            // 
            this.btPasser.Location = new System.Drawing.Point(1750, 49);
            this.btPasser.Name = "btPasser";
            this.btPasser.Size = new System.Drawing.Size(78, 59);
            this.btPasser.TabIndex = 1;
            this.btPasser.Text = "Passer";
            this.btPasser.UseVisualStyleBackColor = true;
            this.btPasser.Click += new System.EventHandler(this.btPasser_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(60, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "OpenAI APK :";
            // 
            // txtApk
            // 
            this.txtApk.Location = new System.Drawing.Point(160, 11);
            this.txtApk.Name = "txtApk";
            this.txtApk.PasswordChar = '*';
            this.txtApk.Size = new System.Drawing.Size(311, 22);
            this.txtApk.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(922, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 17;
            this.label1.Text = "Azure APK :";
            // 
            // txtAzureAPK
            // 
            this.txtAzureAPK.Location = new System.Drawing.Point(1012, 14);
            this.txtAzureAPK.Name = "txtAzureAPK";
            this.txtAzureAPK.PasswordChar = '*';
            this.txtAzureAPK.Size = new System.Drawing.Size(311, 22);
            this.txtAzureAPK.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(497, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 17);
            this.label2.TabIndex = 19;
            this.label2.Text = "Azure région :";
            // 
            // txtAzureReg
            // 
            this.txtAzureReg.Location = new System.Drawing.Point(600, 12);
            this.txtAzureReg.Name = "txtAzureReg";
            this.txtAzureReg.PasswordChar = '*';
            this.txtAzureReg.Size = new System.Drawing.Size(311, 22);
            this.txtAzureReg.TabIndex = 18;
            // 
            // principale
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1840, 976);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtAzureReg);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtAzureAPK);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtApk);
            this.Controls.Add(this.btPasser);
            this.Controls.Add(this.txtBx);
            this.DoubleBuffered = true;
            this.Name = "principale";
            this.Text = "Livre dont vous êtes le héros";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.principale_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.principale_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer tmr;
        private System.Windows.Forms.TextBox txtBx;
        private System.Windows.Forms.Timer timeOut;
        private System.Windows.Forms.Button btPasser;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtApk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAzureAPK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAzureReg;
    }
}

