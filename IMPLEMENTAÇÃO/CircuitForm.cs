using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration; 
using System.Linq;

namespace ProjetoFBD
{
    public partial class CircuitForm : Form
    {
        private string userRole;
        private SqlDataAdapter? dataAdapter; // Tornar anulável
        private DataTable? circuitoTable;
        // Fields must be declared in the Designer file (CircuitForm.Designer.cs)
        // We'll assume they are declared there, so we remove the '?' to simplify usage.
        // Data management fields

        // Parameterless constructor required by the Designer
        public CircuitForm() : this("Guest") { } 

        public CircuitForm(string role)
        {
            // CRITICAL: InitializeComponent must be available from the Designer file
            InitializeComponent(); 
            this.userRole = role;
            
            // UI Text in English
            this.Text = "Circuits Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            SetupCircuitsLayout();
            LoadCircuitoData();
        }

        private void SetupCircuitsLayout()
        {
            // --- 1. DataGridView for Listing ---
            dgvCircuitos = new DataGridView
            {
                Name = "dgvCircuits",
                Location = new Point(10, 10),
                Size = new Size(1160, 550),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                // Default to ReadOnly. Staff access handled below.
                ReadOnly = true 
            };
            this.Controls.Add(dgvCircuitos);

            // --- 2. Staff Actions Panel ---
            pnlStaffActions = new Panel
            {
                Location = new Point(10, 580),
                Size = new Size(700, 50),  // ← Aumentar para 700 (4 botões x 130 + espaços)
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(pnlStaffActions);
            

            // UI Text in English
            Button btnSave = CreateActionButton("Save Changes", new Point(0, 5));
            Button btnAdd = CreateActionButton("Add New", new Point(140, 5));
            Button btnDelete = CreateActionButton("Delete Selected", new Point(280, 5));
            Button btnRefresh = CreateActionButton("Refresh", new Point(420, 5)); // <-- NOVO BOTÃO

            btnSave.Click += btnSave_Click;
            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            btnRefresh.Click += btnRefresh_Click; // <-- NOVO EVENTO
            this.pnlStaffActions.Controls.Add(btnDelete);
            // You will need to implement btnDelete_Click separately
            
            pnlStaffActions.Controls.Add(btnSave);
            pnlStaffActions.Controls.Add(btnAdd);
            pnlStaffActions.Controls.Add(btnDelete);
            pnlStaffActions.Controls.Add(btnRefresh);
            
            // --- 3. Role-Based Access Control (RBAC) ---
            if (this.userRole == "Staff")
            {
                dgvCircuitos.ReadOnly = false; // Allow inline editing for Staff
                pnlStaffActions.Visible = true; // Show action buttons
            }
            else
            {
                // Guest access: Read-only and hide action buttons
                dgvCircuitos.ReadOnly = true; 
                pnlStaffActions.Visible = false;
            }
        }
        
        // Helper method for action buttons
        private Button CreateActionButton(string text, Point location)
        {
            Button btn = new Button 
            { 
                Text = text, 
                Location = location, 
                Size = new Size(130, 40), 
                BackColor = Color.FromArgb(204, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            return btn;
        }

        // -------------------------------------------------------------------------
        // DATA ACCESS METHODS
        // -------------------------------------------------------------------------

// Ficheiro: CircuitForm.cs

private void LoadCircuitoData()
{
    string connectionString = DbConfig.ConnectionString;
    string query = "SELECT ID_Circuito, Nome, Cidade, Pais, Comprimento_km, NumCurvas FROM Circuito";

    try
    {
        dataAdapter = new SqlDataAdapter(query, connectionString);
        circuitoTable = new DataTable();
        
        dataAdapter.Fill(circuitoTable!);
        dgvCircuitos.DataSource = circuitoTable;
        
        // --- CRÍTICO: Configurar os comandos de salvamento AQUI e APENAS AQUI ---
        
        SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
        
        // Atribui os comandos gerados (o erro NullRef vinha desta reatribuição no Save)
        dataAdapter.InsertCommand = commandBuilder.GetInsertCommand(true);
        dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand(true);
        dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand(true);

        // Garante que o ID gerado pelo SQL volta para o DataGridView
        dataAdapter.InsertCommand.UpdatedRowSource = UpdateRowSource.Both; 
        
                    
                    // ID_Circuito (Primary Key) should be read-only even for Staff
                    if (dgvCircuitos.Columns.Contains("ID_Circuito"))
                    {
                        var column = dgvCircuitos.Columns["ID_Circuito"];
                        if (column != null)
                        {
                            column.ReadOnly = true;
                        }
                    }

                    // Translate column headers to English for the UI
                    var nomeColumn = dgvCircuitos.Columns["Nome"];
                    if (nomeColumn != null) nomeColumn.HeaderText = "Name";
                    var cidadeColumn = dgvCircuitos.Columns["Cidade"];
                    if (cidadeColumn != null) cidadeColumn.HeaderText = "City";
                    var paisColumn = dgvCircuitos.Columns["Pais"];
                    if (paisColumn != null) paisColumn.HeaderText = "Country";
                    var comprimentoColumn = dgvCircuitos.Columns["Comprimento_km"];
                    if (comprimentoColumn != null) comprimentoColumn.HeaderText = "Length (km)";
                    var numCurvasColumn = dgvCircuitos.Columns["NumCurvas"];
                    if (numCurvasColumn != null) numCurvasColumn.HeaderText = "Num Corners";

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Circuit data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        
private void btnSave_Click(object? sender, EventArgs e)
{
    if (dataAdapter != null && circuitoTable != null && userRole == "Staff")
    {
        string connectionString = DbConfig.ConnectionString;

        // Utilizamos 'using' para garantir que a conexão fecha automaticamente
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                dgvCircuitos.EndEdit(); 
                
                // CRÍTICO: Ligar os comandos gerados à NOVA conexão ativa
                dataAdapter.InsertCommand!.Connection = connection;
                dataAdapter.UpdateCommand!.Connection = connection;
                dataAdapter.DeleteCommand!.Connection = connection;

                connection.Open();
                
                // Salvar as alterações
                int rowsAffected = dataAdapter.Update(circuitoTable); 
                
                MessageBox.Show($"{rowsAffected} rows saved successfully!", "Success");
                circuitoTable.AcceptChanges();
            }
            catch (Exception ex)
            {
                // Este catch irá capturar erros de violação de chave primária, NULL, ou dados inválidos.
                MessageBox.Show($"Error saving data: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                circuitoTable.RejectChanges(); 
            }
        } 
    }
}
        // Ficheiro: CircuitForm.cs

private void btnAdd_Click(object? sender, EventArgs e)
{
    
    if (circuitoTable != null && userRole == "Staff")
    {
        // 1. Adiciona uma nova linha vazia no topo do DataTable
        DataRow newRow = circuitoTable.NewRow();
        circuitoTable.Rows.InsertAt(newRow, 0);
        
        // --- CRÍTICO: Forçar o foco e a edição na nova linha ---
        
        // 2. Seleciona a primeira linha (onde a nova linha foi inserida)
        dgvCircuitos.CurrentCell = dgvCircuitos.Rows[0].Cells["Nome"]; // Assumimos que 'Nome' é a primeira célula editável
        
        // 3. Força o início do modo de edição
        dgvCircuitos.BeginEdit(true);
    }
}

// Ficheiro: CircuitForm.cs

private void btnDelete_Click(object? sender, EventArgs e)
{
    // Verifica se o utilizador tem permissão e se há alguma linha selecionada
    if (userRole == "Staff" && dgvCircuitos.SelectedRows.Count > 0 && circuitoTable != null)
    {
        // Confirmação de segurança antes de eliminar
        DialogResult dialogResult = MessageBox.Show(
            "Are you sure you want to delete the selected row(s)? This action cannot be undone.", 
            "Confirm Deletion", 
            MessageBoxButtons.YesNo, 
            MessageBoxIcon.Warning);

        if (dialogResult == DialogResult.Yes)
        {
            try
            {
                // Percorre as linhas selecionadas (em ordem inversa para evitar problemas de índice)
                foreach (DataGridViewRow row in dgvCircuitos.SelectedRows.Cast<DataGridViewRow>().OrderByDescending(r => r.Index))
                {
                    // Obtém a DataRow correspondente no DataTable
                    DataRow? dataRow = (row.DataBoundItem as DataRowView)?.Row;
                    
                    if (dataRow != null)
                    {
                        // Marca a linha para eliminação
                        dataRow.Delete();
                    }
                }
                
                // Salva as alterações na BD imediatamente após a eliminação
                btnSave_Click(sender, e); 
                
                // Recarrega os dados (opcional, mas garante que a visualização é atualizada)
                // LoadCircuitoData(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during deletion: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                circuitoTable.RejectChanges(); // Reverte a eliminação se houver erro
            }
        }
    }
}

// No CircuitForm.cs (Adicione junto dos outros métodos de evento, ex: btnSave_Click)

private void btnRefresh_Click(object? sender, EventArgs e)
{
    // Confirma as alterações pendentes para evitar perda de dados
    if (circuitoTable != null && circuitoTable.GetChanges() != null)
    {
        DialogResult result = MessageBox.Show(
            "You have unsaved changes. Do you want to discard them and refresh the data?",
            "Unsaved Changes",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            circuitoTable.RejectChanges();
            LoadCircuitoData(); // Recarrega os dados do banco de dados
        }
        // Se o utilizador clicar em 'No', não faz nada.
    }
    else
    {
        LoadCircuitoData(); // Não há alterações pendentes, carrega diretamente.
    }
}
// Ficheiro: CircuitForm.cs

        // You may need to add the btnDelete_Click method here (implementing logic to delete the selected row)
    }
}