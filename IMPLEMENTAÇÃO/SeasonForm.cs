using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration; 
using System.Linq;

namespace ProjetoFBD
{
    // O seu formulário de temporada
    public partial class SeasonForm : Form
    {
        public SeasonForm() : this("Staff") { }
        private string userRole;
        private SqlDataAdapter? dataAdapter;
        private DataTable? seasonTable;

        public SeasonForm(string role)
        {
            // CRITICAL: InitializeComponent must be available from the Designer file
            InitializeComponent(); 
            this.userRole = role;
            
            // UI Text in English
            this.Text = "Seasons Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            SetupSeasonsLayout();
            LoadSeasonData();
        }

        // -------------------------------------------------------------------------
        // UI SETUP
        // -------------------------------------------------------------------------

        private void SetupSeasonsLayout()
        {
            // --- 1. DataGridView for Listing ---
            dgvSeasons = new DataGridView
            {
                Name = "dgvSeasons",
                Location = new Point(10, 10),
                Size = new Size(960, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                ReadOnly = true 
            };
            this.Controls.Add(dgvSeasons);

            // --- 2. Staff Actions Panel ---
            pnlStaffActions = new Panel
            {
                Location = new Point(10, 500),
                Size = new Size(570, 50),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(pnlStaffActions);
            

            // --- 3. Criar Botões ---
            Button btnSave = CreateActionButton("Save Changes", new Point(0, 5));
            Button btnAdd = CreateActionButton("Add New", new Point(140, 5));
            Button btnDelete = CreateActionButton("Delete Selected", new Point(280, 5));
            Button btnRefresh = CreateActionButton("Refresh", new Point(420, 5)); 

            // --- Ligar Eventos ---
            btnSave.Click += btnSave_Click;
            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            btnRefresh.Click += btnRefresh_Click; 
            
            pnlStaffActions.Controls.Add(btnSave);
            pnlStaffActions.Controls.Add(btnAdd);
            pnlStaffActions.Controls.Add(btnDelete);
            pnlStaffActions.Controls.Add(btnRefresh);

            // --- 4. Role-Based Access Control (RBAC) ---
            if (this.userRole == "Staff")
            {
                dgvSeasons.ReadOnly = false; // Allow inline editing for Staff
                pnlStaffActions.Visible = true; // Show action buttons
            }
            else
            {
                // Guest access: Read-only and hide action buttons
                dgvSeasons.ReadOnly = true; 
                pnlStaffActions.Visible = false;
            }
        }
        
        // Helper method for action buttons (reutilizado do CircuitForm)
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
        // DATA ACCESS METHODS (CRUD)
        // -------------------------------------------------------------------------

private void LoadSeasonData()
{
    string connectionString = DbConfig.ConnectionString;
    
    string query = "SELECT Ano, NumCorridas, PontosPiloto, PontosEquipa, PosiçãoPiloto, PosiçãoEquipa FROM Temporada ORDER BY Ano DESC";

    try
    {
        dataAdapter = new SqlDataAdapter(query, connectionString);
        seasonTable = new DataTable();
        
        // Configurar comandos
        SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
        
        // Fill data
        dataAdapter.Fill(seasonTable);
        dgvSeasons.DataSource = seasonTable;
        
        // --- Configurações de Coluna COM VERIFICAÇÃO DE NULOS ---
        // Verifique se as colunas existem antes de acessá-las
        if (seasonTable != null && seasonTable.Columns.Contains("PosiçãoPiloto"))
        {
            seasonTable.Columns["PosiçãoPiloto"].AllowDBNull = true;
        }
        
        if (seasonTable != null && seasonTable.Columns.Contains("PosiçãoEquipa"))
        {
            seasonTable.Columns["PosiçãoEquipa"].AllowDBNull = true;
        }
        
        // Configurar cabeçalhos em inglês com verificação de nulos
        if (dgvSeasons != null)
        {
            if (dgvSeasons.Columns.Contains("Ano"))
                dgvSeasons.Columns["Ano"]!.HeaderText = "Year";
            if (dgvSeasons.Columns.Contains("NumCorridas"))
                dgvSeasons.Columns["NumCorridas"]!.HeaderText = "Races Count";
            if (dgvSeasons.Columns.Contains("PontosPiloto"))
                dgvSeasons.Columns["PontosPiloto"]!.HeaderText = "Driver Points";
            if (dgvSeasons.Columns.Contains("PontosEquipa"))
                dgvSeasons.Columns["PontosEquipa"]!.HeaderText = "Team Points";
            if (dgvSeasons.Columns.Contains("PosiçãoPiloto"))
                dgvSeasons.Columns["PosiçãoPiloto"]!.HeaderText = "Driver Position";
            if (dgvSeasons.Columns.Contains("PosiçãoEquipa"))
                dgvSeasons.Columns["PosiçãoEquipa"]!.HeaderText = "Team Position";
            
            // Tornar a coluna Ano somente leitura
            if (dgvSeasons.Columns.Contains("Ano"))
            {
                dgvSeasons.Columns["Ano"]!.ReadOnly = true;
            }
            
            // Configurar formatação para colunas numéricas
            foreach (DataGridViewColumn column in dgvSeasons.Columns)
            {
                if (column.Name == "NumCorridas" || column.Name == "PontosPiloto" || 
                    column.Name == "PontosEquipa" || column.Name == "PosiçãoPiloto" || 
                    column.Name == "PosiçãoEquipa")
                {
                    column.DefaultCellStyle.Format = "N0";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
            
            // Adicionar validação de célula - CORRIGIDO OS NOMES
            dgvSeasons.CellValidating += DgvSeasons_CellValidating;
            dgvSeasons.CellEndEdit += DgvSeasons_CellEndEdit;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading Season data: {ex.Message}", "Database Error", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void btnSave_Click(object? sender, EventArgs e)
{
    if (dataAdapter != null && seasonTable != null && userRole == "Staff")
    {
        string connectionString = DbConfig.ConnectionString;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                dgvSeasons.EndEdit();
                
                // Antes de salvar, converter strings vazias para DBNull.Value
                foreach (DataRow row in seasonTable.Rows)
                {
                    if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                    {
                        // Para colunas numéricas
                        string[] numericColumns = { "PosiçãoPiloto", "PosiçãoEquipa", "PontosPiloto", "PontosEquipa", "NumCorridas" };
                        foreach (string col in numericColumns)
                        {
                            if (seasonTable.Columns.Contains(col) && 
                                row[col] != DBNull.Value && 
                                string.IsNullOrWhiteSpace(row[col].ToString()))
                            {
                                row[col] = DBNull.Value;
                            }
                        }
                    }
                }
                
                // Verificar se há erros
                var errorRows = seasonTable.GetErrors();
                if (errorRows.Length > 0)
                {
                    MessageBox.Show($"Please fix errors before saving:\n{string.Join("\n", errorRows.Select(r => r.RowError))}", 
                        "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Atualizar comandos
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
                dataAdapter.InsertCommand = commandBuilder.GetInsertCommand();
                dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand();

                connection.Open();
                int rowsAffected = dataAdapter.Update(seasonTable);
                
                MessageBox.Show($"{rowsAffected} rows saved successfully!", "Success");
                seasonTable.AcceptChanges();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}\nCheck if year is unique and all required fields are filled.", 
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                seasonTable.RejectChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", 
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                seasonTable.RejectChanges();
            }
        }
    }
}
        
        private void btnAdd_Click(object? sender, EventArgs e)
{
    if (seasonTable != null && userRole == "Staff")
    {
        // Solicitar o ano ao usuário
        using (var inputForm = new InputDialog("Add New Season", "Enter the year:"))
        {
            if (inputForm.ShowDialog() == DialogResult.OK && 
                !string.IsNullOrWhiteSpace(inputForm.InputValue))
            {
                string year = inputForm.InputValue.Trim();
                
                // Validar ano
                if (!int.TryParse(year, out int yearInt) || 
                    yearInt < 1900 || yearInt > DateTime.Now.Year + 1)
                {
                    MessageBox.Show($"Please enter a valid year between 1900 and {DateTime.Now.Year + 1}", 
                        "Invalid Year", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Verificar se o ano já existe
                bool yearExists = false;
                foreach (DataRow row in seasonTable.Rows)
                {
                    if (row.RowState != DataRowState.Deleted && 
                        row["Ano"] != DBNull.Value && 
                        row["Ano"].ToString() == year)
                    {
                        yearExists = true;
                        break;
                    }
                }
                
                if (yearExists)
                {
                    MessageBox.Show($"Season for year {year} already exists!", 
                        "Duplicate Year", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Adicionar nova linha com valores padrão
                try
                {
                    DataRow newRow = seasonTable.NewRow();
                    newRow["Ano"] = yearInt; // Usar int diretamente
                    newRow["NumCorridas"] = DBNull.Value; // Ou 0
                    newRow["PontosPiloto"] = DBNull.Value;
                    newRow["PontosEquipa"] = DBNull.Value;
                    newRow["PosiçãoPiloto"] = DBNull.Value; // Usar DBNull.Value em vez de string vazia
                    newRow["PosiçãoEquipa"] = DBNull.Value;
                    
                    seasonTable.Rows.InsertAt(newRow, 0);
                    
                    // Mover foco para a primeira célula editável
                    if (dgvSeasons.Columns.Contains("NumCorridas"))
                    {
                        dgvSeasons.CurrentCell = dgvSeasons.Rows[0].Cells["NumCorridas"];
                        dgvSeasons.BeginEdit(true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding new season: {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (userRole == "Staff" && dgvSeasons.SelectedRows.Count > 0 && seasonTable != null)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Are you sure you want to delete the selected row(s)? This action cannot be undone.", 
                    "Confirm Deletion", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        foreach (DataGridViewRow row in dgvSeasons.SelectedRows.Cast<DataGridViewRow>().OrderByDescending(r => r.Index))
                        {
                            DataRow? dataRow = (row.DataBoundItem as DataRowView)?.Row;
                            if (dataRow != null)
                            {
                                dataRow.Delete();
                            }
                        }
                        
                        // Tenta salvar as alterações imediatamente
                        btnSave_Click(sender, e); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during deletion: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        seasonTable.RejectChanges(); 
                    }
                }
            }
        }

        private void btnRefresh_Click(object? sender, EventArgs e)
        {
            // Confirma as alterações pendentes para evitar perda de dados
            if (seasonTable != null && seasonTable.GetChanges() != null)
            {
                DialogResult result = MessageBox.Show(
                    "You have unsaved changes. Do you want to discard them and refresh the data?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    seasonTable.RejectChanges();
                    LoadSeasonData(); // Recarrega os dados do banco de dados
                }
            }
            else
            {
                LoadSeasonData(); // Não há alterações pendentes, carrega diretamente.
            }
        }
        private void DgvSeasons_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
{
    if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvSeasons == null) return;
    
    string? columnName = dgvSeasons.Columns[e.ColumnIndex].Name;
    string value = e.FormattedValue?.ToString() ?? "";
    
    try
    {
        // Validar colunas numéricas
        if (columnName == "Ano" || columnName == "NumCorridas" || 
            columnName == "PontosPiloto" || columnName == "PontosEquipa" ||
            columnName == "PosiçãoPiloto" || columnName == "PosiçãoEquipa")
        {
            // Se estiver vazio, permitir (será convertido para DBNull.Value)
            if (string.IsNullOrWhiteSpace(value))
            {
                dgvSeasons.Rows[e.RowIndex].ErrorText = "";
                return;
            }
            
            // Tentar converter para inteiro
            if (!int.TryParse(value, out int intValue))
            {
                dgvSeasons.Rows[e.RowIndex].ErrorText = $"Please enter a valid integer for {columnName}";
                e.Cancel = true;
                return;
            }
            
            // Validações específicas por coluna
            if (columnName == "Ano" && (intValue < 1900 || intValue > DateTime.Now.Year + 1))
            {
                dgvSeasons.Rows[e.RowIndex].ErrorText = $"Year must be between 1900 and {DateTime.Now.Year + 1}";
                e.Cancel = true;
                return;
            }
            
            if ((columnName == "NumCorridas" || columnName == "PontosPiloto" || 
                 columnName == "PontosEquipa" || columnName == "PosiçãoPiloto" || 
                 columnName == "PosiçãoEquipa") && intValue < 0)
            {
                dgvSeasons.Rows[e.RowIndex].ErrorText = "Value cannot be negative";
                e.Cancel = true;
                return;
            }
            
            dgvSeasons.Rows[e.RowIndex].ErrorText = "";
        }
    }
    catch (Exception ex)
    {
        if (dgvSeasons != null)
        {
            dgvSeasons.Rows[e.RowIndex].ErrorText = $"Validation error: {ex.Message}";
        }
        e.Cancel = true;
    }
}

private void DgvSeasons_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
{
    if (dgvSeasons == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
    
    // Limpar mensagem de erro
    dgvSeasons.Rows[e.RowIndex].ErrorText = "";
    
    // Converter string vazia para DBNull.Value para colunas numéricas
    string? columnName = dgvSeasons.Columns[e.ColumnIndex].Name;
    if (columnName == "PosiçãoPiloto" || columnName == "PosiçãoEquipa" ||
        columnName == "PontosPiloto" || columnName == "PontosEquipa" ||
        columnName == "NumCorridas")
    {
        var cell = dgvSeasons.Rows[e.RowIndex].Cells[e.ColumnIndex];
        if (cell.Value != null && cell.Value.ToString() == "")
        {
            cell.Value = DBNull.Value;
        }
    }
}
        
        // Método de tradução de cabeçalhos (ajuste conforme necessário)
        // private void TranslateColumnHeaders() { /* ... */ }

        // Mantenha isto no final da classe
    }
}