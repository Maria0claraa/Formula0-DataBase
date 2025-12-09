using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace ProjetoFBD
{
    public partial class GPForm : Form
    {
        public GPForm() : this("Staff") { }
        private string userRole;
        private SqlDataAdapter? dataAdapter;
        private DataTable? gpTable;

        public GPForm(string role)
        {
            InitializeComponent(); // DEVE ser chamado primeiro
            
            this.userRole = role;
            this.Text = "Grand Prix Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            SetupGPLayout();
            LoadGPData();
        }

        // -------------------------------------------------------------------------
        // UI SETUP - ATUALIZADO (sem criar novos controles)
        // -------------------------------------------------------------------------

        private void SetupGPLayout()
        {
            // --- 1. Configurar DataGridView existente ---
            if (dgvGrandPrix != null)
            {
                dgvGrandPrix.Location = new Point(10, 10);
                dgvGrandPrix.Size = new Size(1160, 480);
                dgvGrandPrix.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                dgvGrandPrix.AllowUserToAddRows = false;
                dgvGrandPrix.ReadOnly = true;
            }

            // --- 2. Configurar Painéis existentes ---
            if (pnlStaffActions != null)
            {
                pnlStaffActions.Location = new Point(10, 500);
                pnlStaffActions.Size = new Size(570, 50);
                pnlStaffActions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                
                // Criar e adicionar botões ao painel Staff
                Button btnSave = CreateActionButton("Save Changes", new Point(0, 5));
                Button btnAdd = CreateActionButton("Add New GP", new Point(140, 5));
                Button btnDelete = CreateActionButton("Delete GP", new Point(280, 5));
                Button btnRefresh = CreateActionButton("Refresh", new Point(420, 5));

                btnSave.Click += btnSave_Click;
                btnAdd.Click += btnAdd_Click;
                btnDelete.Click += btnDelete_Click;
                btnRefresh.Click += btnRefresh_Click;

                pnlStaffActions.Controls.Add(btnSave);
                pnlStaffActions.Controls.Add(btnAdd);
                pnlStaffActions.Controls.Add(btnDelete);
                pnlStaffActions.Controls.Add(btnRefresh);
            }

            if (pnlAdditionalActions != null)
            {
                pnlAdditionalActions.Location = new Point(600, 500);
                pnlAdditionalActions.Size = new Size(570, 50);
                pnlAdditionalActions.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                
                // Criar botão de gerenciar sessões
                Button btnManageSessions = CreateActionButton("Manage Sessions", new Point(0, 5));
                btnManageSessions.BackColor = Color.FromArgb(0, 102, 204);
                btnManageSessions.Click += btnManageSessions_Click;
                pnlAdditionalActions.Controls.Add(btnManageSessions);
            }

            // --- 3. Role-Based Access Control (RBAC) ---
            if (dgvGrandPrix != null)
            {
                dgvGrandPrix.ReadOnly = (this.userRole != "Staff");
            }
            
            if (pnlStaffActions != null)
            {
                pnlStaffActions.Visible = (this.userRole == "Staff");
            }
            
            if (pnlAdditionalActions != null)
            {
                pnlAdditionalActions.Visible = (this.userRole == "Staff");
            }
        }

        private Button CreateActionButton(string text, Point location)
        {
            return new Button 
            { 
                Text = text, 
                Location = location, 
                Size = new Size(130, 40), 
                BackColor = Color.FromArgb(204, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        // -------------------------------------------------------------------------
        // DATA ACCESS METHODS (CRUD) - SIMPLIFICADO (apenas 4 colunas)
        // -------------------------------------------------------------------------

        private void LoadGPData()
        {
            string connectionString = DbConfig.ConnectionString;
            
            // QUERY SIMPLIFICADA - apenas as 4 colunas que você quer
            string query = @"
                SELECT 
                    NomeGP,
                    DataCorrida,
                    ID_Circuito,
                    Ano_Temporada AS Season
                FROM Grande_Prémio 
                ORDER BY Ano_Temporada DESC, DataCorrida ASC";

            try
            {
                dataAdapter = new SqlDataAdapter(query, connectionString);
                gpTable = new DataTable();
                
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
                dataAdapter.Fill(gpTable);
                
                if (dgvGrandPrix != null)
                {
                    dgvGrandPrix.DataSource = gpTable;
                    ConfigureColumnHeaders();
                    
                    // Remover handlers existentes antes de adicionar novos
                    dgvGrandPrix.CellValidating -= DgvGrandPrix_CellValidating;
                    dgvGrandPrix.CellEndEdit -= DgvGrandPrix_CellEndEdit;
                    
                    dgvGrandPrix.CellValidating += DgvGrandPrix_CellValidating;
                    dgvGrandPrix.CellEndEdit += DgvGrandPrix_CellEndEdit;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading GP data: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureColumnHeaders()
        {
            if (dgvGrandPrix == null) return;
            
            // APENAS as 4 colunas que você quer
            var columnHeaders = new Dictionary<string, string>
            {
                { "NomeGP", "Grand Prix Name" },
                { "DataCorrida", "Race Date" },
                { "ID_Circuito", "Circuit ID" },
                { "Season", "Season" }
            };
            
            foreach (var mapping in columnHeaders)
            {
                if (dgvGrandPrix.Columns.Contains(mapping.Key) && dgvGrandPrix.Columns[mapping.Key] != null)
                {
                    DataGridViewColumn? column = dgvGrandPrix.Columns[mapping.Key];
                    if (column != null)
                    {
                        column.HeaderText = mapping.Value;
                        
                        // Apenas NomeGP é read-only (chave primária)
                        if (mapping.Key == "NomeGP")
                        {
                            column.ReadOnly = true;
                        }
                        
                        // Formatar data
                        if (mapping.Key == "DataCorrida")
                        {
                            column.DefaultCellStyle.Format = "dd/MM/yyyy";
                        }
                        
                        // Formatar números
                        if (mapping.Key == "ID_Circuito" || mapping.Key == "Season")
                        {
                            column.DefaultCellStyle.Format = "N0";
                            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        }
                    }
                }
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (dataAdapter != null && gpTable != null && userRole == "Staff")
            {
                string connectionString = DbConfig.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        if (dgvGrandPrix != null)
                        {
                            dgvGrandPrix.EndEdit();
                        }
                        
                        // Validações
                        foreach (DataRow row in gpTable.Rows)
                        {
                            if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                            {
                                // Validar DataCorrida
                                if (row["DataCorrida"] == DBNull.Value || string.IsNullOrWhiteSpace(row["DataCorrida"].ToString()))
                                {
                                    MessageBox.Show("Race Date is required!", "Validation Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                
                                // Validar chave estrangeira obrigatória
                                if (row["ID_Circuito"] == DBNull.Value || string.IsNullOrWhiteSpace(row["ID_Circuito"].ToString()))
                                {
                                    MessageBox.Show("Circuit ID is required!", "Validation Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                
                                // Alterado de "Ano_Temporada" para "Season"
                                if (row["Season"] == DBNull.Value || string.IsNullOrWhiteSpace(row["Season"].ToString()))
                                {
                                    MessageBox.Show("Season is required!", "Validation Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }

                        // Verificar erros
                        var errorRows = gpTable.GetErrors();
                        if (errorRows.Length > 0)
                        {
                            MessageBox.Show($"Please fix errors before saving:\n{string.Join("\n", errorRows.Select(r => r.RowError))}", 
                                "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
                        dataAdapter.InsertCommand = commandBuilder.GetInsertCommand();
                        dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                        dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand();

                        connection.Open();
                        int rowsAffected = dataAdapter.Update(gpTable);
                        
                        MessageBox.Show($"{rowsAffected} rows saved successfully!", "Success");
                        gpTable.AcceptChanges();
                    }
                    catch (SqlException sqlEx)
                    {
                        MessageBox.Show($"Database error: {sqlEx.Message}\nCheck if GP name is unique and foreign keys exist.", 
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        gpTable.RejectChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving data: {ex.Message}", 
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        gpTable.RejectChanges();
                    }
                }
            }
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            if (gpTable != null && userRole == "Staff")
            {
                using (var inputForm = new InputDialog("Add New Grand Prix", "Enter the Grand Prix name:"))
                {
                    if (inputForm.ShowDialog() == DialogResult.OK && 
                        !string.IsNullOrWhiteSpace(inputForm.InputValue))
                    {
                        string gpName = inputForm.InputValue.Trim();
                        
                        // Validação adicional: não permitir apenas números
                        if (gpName.All(char.IsDigit))
                        {
                            MessageBox.Show("Grand Prix name cannot contain only numbers!", 
                                "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        // Verificar se o GP já existe
                        bool gpExists = false;
                        foreach (DataRow row in gpTable.Rows)
                        {
                            if (row.RowState != DataRowState.Deleted && 
                                row["NomeGP"] != DBNull.Value && 
                                row["NomeGP"].ToString() == gpName)
                            {
                                gpExists = true;
                                break;
                            }
                        }
                        
                        if (gpExists)
                        {
                            MessageBox.Show($"Grand Prix '{gpName}' already exists!", 
                                "Duplicate GP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        try
                        {
                            DataRow newRow = gpTable.NewRow();
                            newRow["NomeGP"] = gpName;
                            newRow["DataCorrida"] = DBNull.Value;
                            newRow["ID_Circuito"] = DBNull.Value;
                            newRow["Season"] = DBNull.Value; // Alterado de "Ano_Temporada" para "Season"
                            
                            gpTable.Rows.InsertAt(newRow, 0);
                            
                            if (dgvGrandPrix != null && dgvGrandPrix.Columns.Contains("DataCorrida"))
                            {
                                dgvGrandPrix.CurrentCell = dgvGrandPrix.Rows[0].Cells["DataCorrida"];
                                dgvGrandPrix.BeginEdit(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error adding new GP: {ex.Message}", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (userRole == "Staff" && dgvGrandPrix != null && dgvGrandPrix.SelectedRows.Count > 0 && gpTable != null)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Are you sure you want to delete the selected Grand Prix? This will also delete all related sessions.", 
                    "Confirm Deletion", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        foreach (DataGridViewRow row in dgvGrandPrix.SelectedRows.Cast<DataGridViewRow>().OrderByDescending(r => r.Index))
                        {
                            DataRow? dataRow = (row.DataBoundItem as DataRowView)?.Row;
                            if (dataRow != null)
                            {
                                dataRow.Delete();
                            }
                        }
                        
                        btnSave_Click(sender, e);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during deletion: {ex.Message}", 
                            "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        gpTable.RejectChanges();
                    }
                }
            }
        }

        private void btnRefresh_Click(object? sender, EventArgs e)
        {
            if (gpTable != null && gpTable.GetChanges() != null)
            {
                DialogResult result = MessageBox.Show(
                    "You have unsaved changes. Do you want to discard them and refresh the data?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    gpTable.RejectChanges();
                    LoadGPData();
                }
            }
            else
            {
                LoadGPData();
            }
        }

        private void btnManageSessions_Click(object? sender, EventArgs e)
        {
            if (dgvGrandPrix == null || dgvGrandPrix.CurrentRow == null || dgvGrandPrix.CurrentRow.Cells["NomeGP"].Value == null)
            {
                MessageBox.Show("Please select a Grand Prix to manage its sessions.", 
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string? selectedGP = dgvGrandPrix.CurrentRow.Cells["NomeGP"].Value?.ToString();
            
            // Adicione verificação para nulo
            if (string.IsNullOrEmpty(selectedGP))
            {
                MessageBox.Show("Invalid Grand Prix selection.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Alternativa temporária:
            MessageBox.Show($"Would manage sessions for: {selectedGP}\n(SessionForm not implemented yet)", 
                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // -------------------------------------------------------------------------
        // VALIDATION METHODS - ATUALIZADO
        // -------------------------------------------------------------------------

        private void DgvGrandPrix_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvGrandPrix == null) return;
            
            string? columnName = dgvGrandPrix.Columns[e.ColumnIndex].Name;
            string value = e.FormattedValue?.ToString() ?? "";
            
            try
            {
                if (columnName == "NomeGP")
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = "Grand Prix name cannot be empty!";
                        e.Cancel = true;
                        return;
                    }
                    
                    // Validação: não permitir apenas números
                    if (value.All(char.IsDigit))
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = "Grand Prix name cannot contain only numbers!";
                        e.Cancel = true;
                        return;
                    }
                    
                    // Verificar duplicatas
                    for (int i = 0; i < dgvGrandPrix.Rows.Count; i++)
                    {
                        if (i != e.RowIndex && 
                            dgvGrandPrix.Rows[i].Cells["NomeGP"].Value?.ToString() == value &&
                            !dgvGrandPrix.Rows[i].IsNewRow)
                        {
                            dgvGrandPrix.Rows[e.RowIndex].ErrorText = "Grand Prix name must be unique!";
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                
                if (columnName == "DataCorrida")
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = "Race Date is required!";
                        e.Cancel = true;
                        return;
                    }
                    
                    if (!DateTime.TryParse(value, out DateTime dateValue))
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = "Please enter a valid date (dd/MM/yyyy)";
                        e.Cancel = true;
                        return;
                    }
                    
                    if (dateValue.Year < 1900 || dateValue.Year > DateTime.Now.Year + 1)
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"Year must be between 1900 and {DateTime.Now.Year + 1}";
                        e.Cancel = true;
                        return;
                    }
                }
                
                // Alterado: "Ano_Temporada" → "Season"
                if (columnName == "ID_Circuito" || columnName == "Season")
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"{columnName} is required!";
                        e.Cancel = true;
                        return;
                    }
                    
                    if (!int.TryParse(value, out int intValue))
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"Please enter a valid integer for {columnName}";
                        e.Cancel = true;
                        return;
                    }
                    
                    if (intValue <= 0)
                    {
                        dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"{columnName} must be positive";
                        e.Cancel = true;
                        return;
                    }
                    
                    // Validar se a temporada existe
                    if (columnName == "Season")
                    {
                        if (!CheckIfSeasonExists(intValue))
                        {
                            dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"Season {intValue} does not exist in the database";
                            e.Cancel = true;
                            return;
                        }
                    }
                    
                    // Validar se o circuito existe
                    if (columnName == "ID_Circuito")
                    {
                        if (!CheckIfCircuitExists(intValue))
                        {
                            dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"Circuit ID {intValue} does not exist in the database";
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                
                dgvGrandPrix.Rows[e.RowIndex].ErrorText = "";
            }
            catch (Exception ex)
            {
                dgvGrandPrix.Rows[e.RowIndex].ErrorText = $"Validation error: {ex.Message}";
                e.Cancel = true;
            }
        }

        private void DgvGrandPrix_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (dgvGrandPrix == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            dgvGrandPrix.Rows[e.RowIndex].ErrorText = "";
        }

        // -------------------------------------------------------------------------
        // HELPER METHODS
        // -------------------------------------------------------------------------

        private bool CheckIfSeasonExists(int year)
        {
            try
            {
                string connectionString = DbConfig.ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(1) FROM Temporada WHERE Ano = @Year";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Year", year);
                    
                    object? result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CheckIfCircuitExists(int circuitId)
        {
            try
            {
                string connectionString = DbConfig.ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(1) FROM Circuito WHERE ID_Circuito = @CircuitId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CircuitId", circuitId);
                    
                    object? result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}