    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Reflection;
    using System.IO;
    using System.Linq; // Necessário para usar FirstOrDefault

    namespace ProjetoFBD
    {
        // A palavra-chave 'partial' é crucial para ligar ao HomePage.Designer.cs
        public partial class HomePage : Form
        {
            private string userRole;
            
            // As declarações dos Painéis foram movidas para o Designer.cs
            // private Panel pnlGrandPrix;
            // private Panel pnlSeasons;
            // private Panel pnlGrid;
            
            // -------------------------------------------------------------------------
            // CONSTRUTOR
            // -------------------------------------------------------------------------
            
            public HomePage(string role) 
            {
                // CRÍTICO: InitializeComponent() deve ser a primeira chamada
                InitializeComponent(); 
                
                // 1. Configurações Iniciais e Background
                SetupBackgroundImage();
                this.userRole = role; 
                
                this.Text = "Home Page - " + role;
                
                // 2. Criação do Layout
                SetupLayout(); 
            }

            // -------------------------------------------------------------------------
            // BACKGROUND IMAGE (Recurso Embutido com Fallback)
            // -------------------------------------------------------------------------
            
            private void SetupBackgroundImage()
            {
                var assembly = Assembly.GetExecutingAssembly();
                
                // Tenta encontrar o recurso que termine em "background.png"
                string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("background.png", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(resourceName))
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            this.BackgroundImage = Image.FromStream(stream);
                            this.BackgroundImageLayout = ImageLayout.Stretch;
                            return;
                        }
                    }
                }

                // Fallback: tenta carregar a partir da pasta de execução
                string fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.png");
                if (File.Exists(fallbackPath))
                {
                    this.BackgroundImage = Image.FromFile(fallbackPath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                    return;
                }

                // Se nada funcionar, define uma cor de fundo sólida
                this.BackColor = Color.LightGray; 
            }

            // -------------------------------------------------------------------------
            // LAYOUT E MENU (Com Estilo F1)
            // -------------------------------------------------------------------------

            private void SetupLayout()
            {
                // Limpar quaisquer controlos criados anteriormente
                this.Controls.Clear();
                
                // --- 1. Criar e Ancorar o Painel de Menu Principal no Topo ---
                Panel pnlMainMenu = new Panel
                {
                    Size = new Size(this.ClientSize.Width, 60),
                    Location = new Point(0, 0),
                    BackColor = Color.FromArgb(150, 0, 0, 0), 
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Name = "pnlMainMenu"
                };
                this.Controls.Add(pnlMainMenu);
                
                // --- 2. Criação dos Botões do Cabeçalho (Estilo F1) ---
                
                Font menuFont = new Font("Arial", 16, FontStyle.Bold);
                Color headerColor = Color.FromArgb(102, 0, 0); // Vermelho Escuro/Bordô
                Color textColor = Color.White;
                
                Button btnGrandPrix = CreateMenuHeaderButton("GRAND PRIX", menuFont, headerColor, textColor, new Point(50, 10));
                Button btnSeasons = CreateMenuHeaderButton("SEASONS", menuFont, headerColor, textColor, new Point(250, 10));
                Button btnGrid = CreateMenuHeaderButton("GRID", menuFont, headerColor, textColor, new Point(450, 10));

                pnlMainMenu.Controls.Add(btnGrandPrix);
                pnlMainMenu.Controls.Add(btnSeasons);
                pnlMainMenu.Controls.Add(btnGrid);
                
                // --- 3. Criação e Configuração dos Painéis Dropdown ---
                
                // Os painéis globais são inicializados no Designer, mas reconfigurados e reposicionados aqui
                this.pnlGrandPrix = CreateDropdownPanel(new Point(50, 60)); 
                this.pnlSeasons = CreateDropdownPanel(new Point(250, 60)); 
                this.pnlGrid = CreateDropdownPanel(new Point(450, 60)); 
                
                // Adicionar Itens e Eventos Toggle... (Resto da lógica de layout)
                AddDropdownItem(pnlGrandPrix, "GP", 0);
                AddDropdownItem(pnlGrandPrix, "Circuits", 1);

                AddDropdownItem(pnlSeasons, "Team Standings", 0);
                AddDropdownItem(pnlSeasons, "Driver Standings", 1);
                AddDropdownItem(pnlSeasons, "2025 Season", 2);
                AddDropdownItem(pnlSeasons, "All Seasons", 3);
                
                AddDropdownItem(pnlGrid, "Drivers", 0);
                AddDropdownItem(pnlGrid, "Teams", 1);

                // Adicionar os painéis ao formulário (só se não os adicionou no Designer)
                // Se o Designer já os adicionou, remova as próximas 3 linhas
                this.Controls.Add(pnlGrandPrix);
                this.Controls.Add(pnlSeasons);
                this.Controls.Add(pnlGrid);

                pnlGrandPrix.Visible = false;
                pnlSeasons.Visible = false;
                pnlGrid.Visible = false;
                
                // Lógica de Clique (Toggle)
                btnGrandPrix.Click += (s, e) => ToggleDropdown(pnlGrandPrix);
                btnSeasons.Click += (s, e) => ToggleDropdown(pnlSeasons);
                btnGrid.Click += (s, e) => ToggleDropdown(pnlGrid);
                
                // Mensagem de Boas-Vindas e Logout
                DisplayWelcomeMessage();
                
                Button btnLogout = CreateMenuHeaderButton("Logout / Sair", new Font("Arial", 12), Color.Gray, Color.White, new Point(this.ClientSize.Width - 160, this.ClientSize.Height - 50));
                btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                btnLogout.Click += new EventHandler(this.btnLogout_Click);
                this.Controls.Add(btnLogout);
            }

            // -------------------------------------------------------------------------
            // MÉTODOS AUXILIARES E EVENTOS
            // -------------------------------------------------------------------------
            
            private void DisplayWelcomeMessage()
            {
                Label lblWelcome = new Label
                {
                    Text = $"Bem-vindo, {userRole}!",
                    Font = new Font("Arial", 24, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(50, 100), 
                    BackColor = Color.Transparent, 
                    ForeColor = Color.White 
                };
                this.Controls.Add(lblWelcome);
            }

            private void btnLogout_Click(object? sender, EventArgs e)
            {
                this.Close();
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
            }

            private Button CreateMenuHeaderButton(string text, Font font, Color backColor, Color foreColor, Point location)
            {
                Button btn = new Button
                {
                    Text = text,
                    Font = font,
                    BackColor = backColor,
                    ForeColor = foreColor,
                    Size = new Size(180, 40),
                    Location = location,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0, MouseDownBackColor = Color.FromArgb(130, 0, 0), MouseOverBackColor = Color.FromArgb(115, 0, 0) }, 
                    TextAlign = ContentAlignment.MiddleCenter
                };
                return btn;
            }

            private Panel CreateDropdownPanel(Point location)
            {
                Panel panel = new Panel
                {
                    Size = new Size(180, 200),
                    Location = location,
                    BackColor = Color.White, 
                    BorderStyle = BorderStyle.None, 
                    AutoScroll = true
                };
                
                panel.Paint += (sender, e) => {
                    ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, Color.Silver, ButtonBorderStyle.Solid);
                };
                
                panel.BringToFront(); 
                return panel;
            }

            private void AddDropdownItem(Panel parentPanel, string text, int index)
            {
                Button btnItem = new Button
                {
                    Text = text,
                    Size = new Size(parentPanel.Width, 30),
                    Location = new Point(0, index * 30),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { 
                        BorderSize = 0, 
                        MouseDownBackColor = Color.LightGray, 
                        MouseOverBackColor = Color.Gainsboro 
                    }, 
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0), 
                    BackColor = Color.White,
                    ForeColor = Color.Black 
                };
                
                btnItem.Paint += (sender, e) => {
                    e.Graphics.DrawLine(Pens.LightGray, 0, btnItem.Height - 1, btnItem.Width, btnItem.Height - 1);
                };
                
                btnItem.MouseEnter += (s, e) => btnItem.BackColor = Color.Gainsboro;
                btnItem.MouseLeave += (s, e) => btnItem.BackColor = Color.White;

                parentPanel.Controls.Add(btnItem);
        if (parentPanel == pnlGrandPrix && text == "GP") // Assumi que "Races" é o seu GP no código
    {
        btnItem.Click += (s, e) => OpenGPForm();
    }
    else if (parentPanel == pnlGrandPrix && text == "Circuits")
    {
        btnItem.Click += (s, e) => OpenCircuitForm(); 
    }
    else if (parentPanel == pnlSeasons && text == "All Seasons") // <-- NOVO CÓDIGO AQUI
    {
        btnItem.Click += (s, e) => OpenSeasonForm();
    }

            }
            

            private void ToggleDropdown(Panel targetPanel)
            {
                pnlGrandPrix.Visible = false;
                pnlSeasons.Visible = false;
                pnlGrid.Visible = false;
                
                targetPanel.Visible = !targetPanel.Visible;
            }


            // Ficheiro: HomePage.cs

    private void OpenGPForm()
    {
        // 1. Esconder o painel dropdown após a seleção
        pnlGrandPrix.Visible = false;
        
        // 2. Abre o formulário da Lista de Grandes Prémios
        // Passamos o papel do utilizador para que o GrandePremioForm saiba quem pode editar
        GPForm gpForm = new GPForm(this.userRole);
        
        // Usa ShowDialog() para que o utilizador feche a lista antes de interagir novamente com a HomePage/Menu
        gpForm.ShowDialog(); 
    }

    private void OpenCircuitForm()
    {
        // 1. Esconder o painel dropdown após a seleção
        pnlGrandPrix.Visible = false;
        
        // 2. Abre o formulário da Lista de Circuitos
        CircuitForm circuitForm = new CircuitForm(this.userRole);
        
        // Usa ShowDialog() para que o utilizador feche a lista antes de interagir novamente com a HomePage/Menu
        circuitForm.ShowDialog();
    }
    private void OpenSeasonForm()
    {
        //1. Esconder o painel dropdown após a seleção
        pnlSeasons.Visible = false; // Tem que esconder o painel correto
        
        // 2. Abre o formulário da Lista de Temporadas
        // Assumimos que a classe SeasonForm está disponível no namespace ProjetoFBD
        SeasonForm seasonForm = new SeasonForm(this.userRole);
        
        // Usa ShowDialog() para que o utilizador feche a lista antes de interagir novamente com a HomePage/Menu
        seasonForm.ShowDialog();
    }

    // Nota: Você pode precisar de ajustar os nomes dos seus métodos (OpenGPForm, OpenCircuitForm) 
    // para corresponder exatamente aos nomes que está a usar no seu projeto.
        }
    }