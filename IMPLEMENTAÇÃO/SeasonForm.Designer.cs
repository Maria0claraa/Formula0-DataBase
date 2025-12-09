namespace ProjetoFBD
{
    partial class SeasonForm
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
            // O InitializeComponent está vazio porque a lógica e a criação dos controlos 
            // (dgvSeasons e pnlStaffActions) são feitas programaticamente no SeasonForm.cs
            // dentro do método SetupSeasonsLayout().

            // Adicionamos a lógica para configurar o próprio formulário, se necessário:
            this.SuspendLayout();
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600); // Tamanho definido no SeasonForm.cs
            this.Name = "SeasonForm";
            this.Text = "Seasons Management";
            
            this.ResumeLayout(false);
        }

        #endregion
        
        // --- DECLARAÇÃO DE VARIÁVEIS ---
        // CRÍTICO: Estas variáveis DEVE ser declaradas para ligar ao código em SeasonForm.cs
        private System.Windows.Forms.DataGridView dgvSeasons;
        private System.Windows.Forms.Panel pnlStaffActions;
    }
}