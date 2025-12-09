public class InputDialog : Form
{
    private TextBox txtInput = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;
    private Label lblPrompt = null!;
    
    public string? InputValue { get; private set; } // Tornar nullable
    
    public InputDialog(string title, string prompt)
    {
        InitializeComponents(title, prompt);
    }
    
    private void InitializeComponents(string title, string prompt)
    {
        this.Text = title;
        this.Size = new Size(400, 180);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        lblPrompt = new Label
        {
            Text = prompt,
            Location = new Point(20, 20),
            Size = new Size(350, 40),
            Font = new Font("Arial", 10)
        };
        
        txtInput = new TextBox
        {
            Location = new Point(20, 70),
            Size = new Size(350, 30),
            Font = new Font("Arial", 10)
        };
        
        btnOk = new Button
        {
            Text = "OK",
            Location = new Point(200, 110),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        
        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(290, 110),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };
        
        btnOk.Click += (s, e) => { InputValue = txtInput.Text; };
        
        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
        
        this.Controls.Add(lblPrompt);
        this.Controls.Add(txtInput);
        this.Controls.Add(btnOk);
        this.Controls.Add(btnCancel);
    }
}