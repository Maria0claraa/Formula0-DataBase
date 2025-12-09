namespace ProjetoFBD
{
    partial class GPForm
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
            this.dgvGrandPrix = new System.Windows.Forms.DataGridView();
            this.pnlStaffActions = new System.Windows.Forms.Panel();
            this.pnlAdditionalActions = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGrandPrix)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvGrandPrix
            // 
            this.dgvGrandPrix.AllowUserToAddRows = false;
            this.dgvGrandPrix.AllowUserToDeleteRows = false;
            this.dgvGrandPrix.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvGrandPrix.Location = new System.Drawing.Point(0, 0);
            this.dgvGrandPrix.Name = "dgvGrandPrix";
            this.dgvGrandPrix.ReadOnly = true;
            this.dgvGrandPrix.RowHeadersWidth = 51;
            this.dgvGrandPrix.RowTemplate.Height = 29;
            this.dgvGrandPrix.Size = new System.Drawing.Size(300, 300);
            this.dgvGrandPrix.TabIndex = 0;
            // 
            // pnlStaffActions
            // 
            this.pnlStaffActions.Location = new System.Drawing.Point(0, 0);
            this.pnlStaffActions.Name = "pnlStaffActions";
            this.pnlStaffActions.Size = new System.Drawing.Size(200, 100);
            this.pnlStaffActions.TabIndex = 1;
            // 
            // pnlAdditionalActions
            // 
            this.pnlAdditionalActions.Location = new System.Drawing.Point(0, 0);
            this.pnlAdditionalActions.Name = "pnlAdditionalActions";
            this.pnlAdditionalActions.Size = new System.Drawing.Size(200, 100);
            this.pnlAdditionalActions.TabIndex = 2;
            // 
            // GPForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.pnlAdditionalActions);
            this.Controls.Add(this.pnlStaffActions);
            this.Controls.Add(this.dgvGrandPrix);
            this.Name = "GPForm";
            this.Text = "GPForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgvGrandPrix)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvGrandPrix;
        private System.Windows.Forms.Panel pnlStaffActions;
        private System.Windows.Forms.Panel pnlAdditionalActions;
    }
}