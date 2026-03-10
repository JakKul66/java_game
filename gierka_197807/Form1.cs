using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace gierka_197807
{
    public partial class Form1 : Form
    {
        //interfejs
        private Panel pnlMenu;
        private Panel pnlDifficulty;
        private Panel pnlInstructions;
        private Panel pnlGame;
        private Panel pnlTest;

        // telefon klawiatura
        private Panel pnlPhonePad;
        private Label lblPhoneDisplay;

        //elementy gry
        private Label lblInstruction;
        private Label lblClickHistory;
        private Label lblScore;
        private Label lblGlobalTimer;
        private Label lblWaveTimer;
        private Label lblWave;
        private Button btnBackToMenu;

        //kubly
        private Button btnBinPaper;
        private Button btnBinPlastic;
        private Button btnBinGlass;

        //instruckja
        private Label lblInstTitle;
        private Label lblInstBody;


        private GameLogic logic;
        private Timer gameTimer;

        private int globalTimeLeft;
        private int waveTimeLeft;
        private int waveDuration;

        //test
        private List<DotColor> testExpectedSequence;
        private int testCurrentIndex;
        private int testCorrectAnswers;
        private List<ContextItem> _contextTestQuestions;
        private int _contextQuestionIndex;
        private int _contextTestScore;


        private bool _pendingContextMode;
        private DifficultyLevel _pendingDifficulty;

        private List<string> _historyLog = new List<string>();

        // do kogo dzwonimy
        private ContextItem _pendingPhoneTask;

        public Form1()
        {
            this.Text = "Neuraflex - Projekt 197807";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.BackColor = Color.AliceBlue;

            logic = new GameLogic();
            InitializeCustomUI();
            ShowMenu();
        }

        // interfejs
        private void InitializeCustomUI()
        {
            //menu
            pnlMenu = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            PictureBox pbLogo = new PictureBox();
            try { pbLogo.Image = Properties.Resources.mozg; } catch { }
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.Size = new Size(120, 120);
            pbLogo.Location = new Point(240, 70);
            pnlMenu.Controls.Add(pbLogo);

            Label title = new Label
            {
                Text = "NEURAFLEX",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                AutoSize = true,
                Top = 100,
                Left = 380
            };

            //tryb gry
            Button btnBasic = CreateButton("Tryb Podstawowy", 250, (s, e) => ShowDifficultySelection(false));
            Button btnContext = CreateButton("Tryb Kontekstowy (ADL)", 340, (s, e) => ShowDifficultySelection(true));
            Button btnExit = CreateButton("Wyjscie", 430, (s, e) => Application.Exit());

            btnExit.BackColor = Color.IndianRed;
            btnExit.MouseEnter += (s, e) => btnExit.BackColor = Color.Red;
            btnExit.MouseLeave += (s, e) => btnExit.BackColor = Color.IndianRed;

            pnlMenu.Controls.Add(title);
            pnlMenu.Controls.Add(btnBasic);
            pnlMenu.Controls.Add(btnContext);
            pnlMenu.Controls.Add(btnExit);
            this.Controls.Add(pnlMenu);

            //poz trudnosci
            pnlDifficulty = new Panel { Dock = DockStyle.Fill, Visible = false, BackColor = Color.Transparent };
            Label lblDiffTitle = new Label { Text = "WYBIERZ POZIOM TRUDNOSCI", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Top = 100, Left = 300, ForeColor = Color.DarkSlateBlue };

            Button btnEasy = CreateButton("LATWY (Bez limitu zestawu)", 200, (s, e) => ShowInstructions(DifficultyLevel.Easy));
            Button btnNormal = CreateButton("NORMALNY", 290, (s, e) => ShowInstructions(DifficultyLevel.Normal));
            Button btnHard = CreateButton("TRUDNY", 380, (s, e) => ShowInstructions(DifficultyLevel.Hard));

            Button btnBackDiff = new Button { Text = "Wroc", Size = new Size(100, 40), Location = new Point(20, 20), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBackDiff.Click += (s, e) => ShowMenu();

            pnlDifficulty.Controls.Add(lblDiffTitle); pnlDifficulty.Controls.Add(btnEasy); pnlDifficulty.Controls.Add(btnNormal); pnlDifficulty.Controls.Add(btnHard); pnlDifficulty.Controls.Add(btnBackDiff);
            this.Controls.Add(pnlDifficulty);

            //instrukcje
            pnlInstructions = new Panel { Dock = DockStyle.Fill, Visible = false, BackColor = Color.Transparent };
            lblInstTitle = new Label { Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(300, 50), ForeColor = Color.DarkBlue };
            lblInstBody = new Label { Font = new Font("Segoe UI", 16), AutoSize = false, Size = new Size(800, 400), Location = new Point(100, 120), TextAlign = ContentAlignment.TopCenter };

            Button btnStartGame = new Button { Text = "ROZPOCZNIJ", Font = new Font("Segoe UI", 18, FontStyle.Bold), Size = new Size(300, 80), Location = new Point(350, 550), BackColor = Color.LimeGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnStartGame.FlatAppearance.BorderSize = 0;
            btnStartGame.Click += (s, e) => StartGame();

            Button btnBackInst = new Button { Text = "Wroc", Size = new Size(100, 40), Location = new Point(20, 20), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBackInst.Click += (s, e) => ShowMenu();

            pnlInstructions.Controls.Add(lblInstTitle); pnlInstructions.Controls.Add(lblInstBody); pnlInstructions.Controls.Add(btnStartGame); pnlInstructions.Controls.Add(btnBackInst);
            this.Controls.Add(pnlInstructions);

            //gra
            pnlGame = new Panel { Dock = DockStyle.Fill, Visible = false };

            lblInstruction = new Label
            {
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(760, 70),
                Location = new Point(20, 10),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblClickHistory = new Label
            {
                Font = new Font("Arial", 12, FontStyle.Italic),
                AutoSize = false,
                Size = new Size(760, 30),
                Location = new Point(20, 85),
                ForeColor = Color.DimGray
            };

            lblScore = new Label { Font = new Font("Arial", 14, FontStyle.Bold), Text = "Wynik: 0", Location = new Point(800, 20), AutoSize = true };
            lblGlobalTimer = new Label { Font = new Font("Arial", 14, FontStyle.Bold), Text = "Gra: 65s", Location = new Point(800, 50), AutoSize = true, ForeColor = Color.Black };
            lblWaveTimer = new Label { Font = new Font("Arial", 14, FontStyle.Bold), Text = "Zestaw: --", Location = new Point(800, 80), AutoSize = true, ForeColor = Color.Red };
            lblWave = new Label { Font = new Font("Arial", 12), Text = "Fala: 1", Location = new Point(800, 110), AutoSize = true };
            btnBackToMenu = new Button { Text = "Menu", Size = new Size(80, 40), Location = new Point(900, 20), BackColor = Color.LightGray };
            btnBackToMenu.Click += (s, e) => { gameTimer.Stop(); ShowMenu(); };

            InitializeTrashBins();

            pnlGame.Controls.Add(lblInstruction); pnlGame.Controls.Add(lblClickHistory); pnlGame.Controls.Add(lblScore); pnlGame.Controls.Add(lblGlobalTimer); pnlGame.Controls.Add(lblWaveTimer); pnlGame.Controls.Add(lblWave); pnlGame.Controls.Add(btnBackToMenu);
            pnlGame.Controls.Add(btnBinPaper); pnlGame.Controls.Add(btnBinPlastic); pnlGame.Controls.Add(btnBinGlass);
            this.Controls.Add(pnlGame);

            InitializePhonePad();
            this.Controls.Add(pnlPhonePad);

            //test koncowy
            pnlTest = new Panel { Dock = DockStyle.Fill, Visible = false, BackColor = Color.AliceBlue };
            this.Controls.Add(pnlTest);

            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;
        }

        // kalwiatura
        private void InitializePhonePad()
        {
            pnlPhonePad = new Panel
            {
                Size = new Size(300, 450),
                BackColor = Color.Black,
                Visible = false,
                BorderStyle = BorderStyle.Fixed3D
            };
            pnlPhonePad.Location = new Point((this.ClientSize.Width - pnlPhonePad.Width) / 2, (this.ClientSize.Height - pnlPhonePad.Height) / 2);

            Label lblTitle = new Label { Text = "TELEFON", ForeColor = Color.White, Font = new Font("Arial", 12), AutoSize = true, Location = new Point(110, 10) };
            pnlPhonePad.Controls.Add(lblTitle);

            lblPhoneDisplay = new Label
            {
                Text = "",
                Font = new Font("Consolas", 24, FontStyle.Bold),
                BackColor = Color.DarkSeaGreen,
                ForeColor = Color.Black,
                Size = new Size(260, 50),
                Location = new Point(20, 40),
                TextAlign = ContentAlignment.MiddleRight,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlPhonePad.Controls.Add(lblPhoneDisplay);

            int startX = 40;
            int startY = 110;
            int btnSize = 60;
            int gap = 15;

            // telefon przyciski
            for (int i = 1; i <= 9; i++)
            {
                int row = (i - 1) / 3;
                int col = (i - 1) % 3;
                Button btnNum = CreatePhoneButton(i.ToString(), startX + col * (btnSize + gap), startY + row * (btnSize + gap));
                pnlPhonePad.Controls.Add(btnNum);
            }


            Button btnZero = CreatePhoneButton("0", startX + 1 * (btnSize + gap), startY + 3 * (btnSize + gap));
            pnlPhonePad.Controls.Add(btnZero);


            Button btnClear = new Button { Text = "C", Font = new Font("Arial", 14, FontStyle.Bold), Size = new Size(60, 60), Location = new Point(startX, startY + 3 * (btnSize + gap)), BackColor = Color.OrangeRed, ForeColor = Color.White };
            btnClear.Click += (s, e) => { lblPhoneDisplay.Text = ""; };
            pnlPhonePad.Controls.Add(btnClear);


            Button btnCall = new Button { Text = "📞", Font = new Font("Arial", 20), Size = new Size(60, 60), Location = new Point(startX + 2 * (btnSize + gap), startY + 3 * (btnSize + gap)), BackColor = Color.LimeGreen, ForeColor = Color.White };
            btnCall.Click += BtnCall_Click;
            pnlPhonePad.Controls.Add(btnCall);
        }

        private Button CreatePhoneButton(string num, int x, int y)
        {
            Button btn = new Button
            {
                Text = num,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Size = new Size(60, 60),
                Location = new Point(x, y),
                BackColor = Color.Gray,
                ForeColor = Color.White
            };
            btn.Click += (s, e) => { if (lblPhoneDisplay.Text.Length < 6) lblPhoneDisplay.Text += num; };
            return btn;
        }


        private void BtnCall_Click(object sender, EventArgs e)
        {
            string entered = lblPhoneDisplay.Text;
            bool isCorrect = false;
            string successMsg = "";

            switch (_pendingPhoneTask)
            {
                case ContextItem.PhoneFamily:
                    if (entered == GameLogic.GRANDSON_NUMBER) { isCorrect = true; successMsg = "Wnuczek odebral!"; }
                    break;
                case ContextItem.Phone112:
                    if (entered == GameLogic.NUMBER_112) { isCorrect = true; successMsg = "Polaczono z 112 (SOS)."; }
                    break;
                case ContextItem.Phone999:
                    if (entered == GameLogic.NUMBER_999) { isCorrect = true; successMsg = "Polaczono z Pogotowiem."; }
                    break;
                case ContextItem.Phone998:
                    if (entered == GameLogic.NUMBER_998) { isCorrect = true; successMsg = "Polaczono ze Straza."; }
                    break;
                case ContextItem.Phone997:
                    if (entered == GameLogic.NUMBER_997) { isCorrect = true; successMsg = "Polaczono z Policja."; }
                    break;
            }

            if (isCorrect)
            {
                pnlPhonePad.Visible = false;

                logic.AddScore(150);
                lblScore.Text = "Wynik: " + logic.Score;


                if (_pendingPhoneTask == ContextItem.PhoneFamily)
                {
                    var previousItem = logic.GetItemBeforeLast();
                    if (previousItem != ContextItem.None)
                    {
                        ShowImmediateMemoryTest(previousItem);
                    }
                    else
                    {
                        MessageBox.Show(successMsg);
                        ContinueGame();
                    }
                }
                else
                {
                    MessageBox.Show(successMsg, "Polaczenie udane");
                    ContinueGame();
                }
            }
            else
            {
                MessageBox.Show("Niepoprawny numer!", "Blad polaczenia");
                lblPhoneDisplay.Text = "";
            }
        }

        private void ContinueGame()
        {
            pnlTest.Visible = false;
            pnlGame.Visible = true;
            if (logic.IsWaveFinished()) { logic.NextWave(); LoadWave(); }
            gameTimer.Start();
        }

        private void ShowPhonePad()
        {
            gameTimer.Stop();
            lblPhoneDisplay.Text = "";
            pnlPhonePad.BringToFront();
            pnlPhonePad.Visible = true;
        }

        // przeciaganie smieci
        private void InitializeTrashBins()
        {
            int binY = 600;
            int binSize = 120;
            int gap = 50;
            int startX = 300;
            btnBinPaper = CreateBinButton("PAPIER\n(Niebieski)", Color.RoyalBlue, startX, binY, binSize);
            btnBinPlastic = CreateBinButton("PLASTIK\n(Zolty)", Color.Gold, startX + binSize + gap, binY, binSize);
            btnBinGlass = CreateBinButton("SZKLO\n(Zielony)", Color.ForestGreen, startX + 2 * (binSize + gap), binY, binSize);
        }

        private Button CreateBinButton(string text, Color color, int x, int y, int size)
        {
            Button btn = new Button { Text = "🗑️\n" + text, BackColor = color, ForeColor = (color == Color.Gold) ? Color.Black : Color.White, Font = new Font("Arial", 10, FontStyle.Bold), Size = new Size(size, size), Location = new Point(x, y), AllowDrop = true, Visible = false };
            btn.DragEnter += Bin_DragEnter;
            btn.DragDrop += Bin_DragDrop;
            return btn;
        }

        private Button CreateButton(string text, int top, EventHandler action)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btn.Size = new Size(350, 70);
            btn.Location = new Point((this.ClientSize.Width - btn.Width) / 2, top);

            btn.BackColor = Color.RoyalBlue;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            btn.Click += action;
            btn.MouseEnter += (s, e) => { if (btn.Text != "Wyjscie") btn.BackColor = Color.CornflowerBlue; };
            btn.MouseLeave += (s, e) => { if (btn.Text != "Wyjscie") btn.BackColor = Color.RoyalBlue; };

            return btn;
        }


        private void ShowMenu()
        {
            pnlMenu.Visible = true; pnlDifficulty.Visible = false; pnlInstructions.Visible = false; pnlGame.Visible = false; pnlTest.Visible = false; pnlPhonePad.Visible = false;
        }

        private void ShowDifficultySelection(bool isContextMode)
        {
            _pendingContextMode = isContextMode;
            pnlMenu.Visible = false; pnlDifficulty.Visible = true;
        }

        private void ShowInstructions(DifficultyLevel difficulty)
        {
            _pendingDifficulty = difficulty;
            pnlDifficulty.Visible = false; pnlInstructions.Visible = true;
            string modeName = _pendingContextMode ? "Tryb Kontekstowy (ADL)" : "Tryb Podstawowy";
            int contextTime = 20;
            if (difficulty == DifficultyLevel.Hard) contextTime = 12;
            string waveInfo = "";
            if (difficulty == DifficultyLevel.Easy) waveInfo = "Czas nielimitowany.";
            else if (difficulty == DifficultyLevel.Normal) waveInfo = _pendingContextMode ? $"Masz {contextTime} sekund na zestaw." : "Masz 12 sekund na zestaw.";
            else waveInfo = _pendingContextMode ? "Masz 12 sekund na zestaw." : "Masz 7 sekund na zestaw.";

            string extra = "";
            if (_pendingContextMode) extra = $"\n\nZASADY ADL:\n- Smieci PRZECIAGNIJ do kublow.\n- Aby zadzwonic, kliknij ikone i wybierz numer:\n  WNUCZEK={GameLogic.GRANDSON_NUMBER}, SOS=112, KARETKA=999, STRAZ=998, POLICJA=997";
            else if (difficulty != DifficultyLevel.Easy) extra = "\n\nUWAGA: Klikaj cyfry w LOSOWEJ kolejnosci!";

            lblInstTitle.Text = $"Instrukcja: {modeName}";
            lblInstBody.Text = $"Poziom: {difficulty.ToString().ToUpper()}\n{waveInfo}{extra}";
        }

        //rozpoczecie gry
        private void StartGame()
        {
            pnlInstructions.Visible = false; pnlGame.Visible = true;
            logic.ResetGame(_pendingContextMode, _pendingDifficulty);

            if (_pendingDifficulty == DifficultyLevel.Hard) globalTimeLeft = 45;
            else globalTimeLeft = 65;

            lblGlobalTimer.Text = "Gra: " + globalTimeLeft + "s";
            int baseTime = _pendingContextMode ? 20 : 12;
            if (_pendingDifficulty == DifficultyLevel.Easy) waveDuration = 999;
            else if (_pendingDifficulty == DifficultyLevel.Normal) waveDuration = baseTime;
            else waveDuration = _pendingContextMode ? 12 : 7;

            _historyLog.Clear();
            lblClickHistory.Text = "Historia: ";

            LoadWave();
            gameTimer.Start();
        }

        // zestawy
        private void LoadWave()
        {
            waveTimeLeft = waveDuration;
            UpdateWaveTimerDisplay();
            bool showBins = logic.IsContextMode;
            btnBinPaper.Visible = showBins; btnBinPlastic.Visible = showBins; btnBinGlass.Visible = showBins;

            var toRemove = new List<Control>();
            foreach (Control c in pnlGame.Controls) if (c is CircularButton) toRemove.Add(c);
            foreach (Control c in toRemove) pnlGame.Controls.Remove(c);


            var dots = logic.GenerateLevelDots(pnlGame.Width, pnlGame.Height - 150);
            UpdateInstruction();
            lblWave.Text = "Zestaw: " + logic.CurrentLevel;
            lblScore.Text = "Wynik: " + logic.Score;


            foreach (var dot in dots) { dot.MouseDown += Dot_MouseDown; dot.Click += Dot_Click; pnlGame.Controls.Add(dot); }


            btnBinPaper.BringToFront(); btnBinPlastic.BringToFront(); btnBinGlass.BringToFront();
            lblGlobalTimer.BringToFront(); lblWaveTimer.BringToFront(); lblWave.BringToFront();
            btnBackToMenu.BringToFront(); lblScore.BringToFront(); lblInstruction.BringToFront(); lblClickHistory.BringToFront();
        }

        private void UpdateInstruction()
        {
            if (logic.IsContextMode)
            {
                List<string> taskDescriptions = new List<string>();
                foreach (var item in logic.TargetSequence) taskDescriptions.Add(logic.GetContextInstruction(item));
                string fullText = string.Join(" -> ", taskDescriptions);
                lblInstruction.Text = "ZADANIE: " + fullText;
            }
            else
            {
                string colorName = GetPolishColorName(logic.TargetColor);
                if (logic.CurrentDifficulty == DifficultyLevel.Easy) lblInstruction.Text = $"ZADANIE: Klikaj 1, 2, 3... w kolorze {colorName}";
                else { string seq = string.Join(" -> ", logic.BasicTargetSequence); lblInstruction.Text = $"KOLEJNOSC: {seq} ({colorName})"; }
            }
        }

        // przeciaganie smieci
        private void Dot_MouseDown(object sender, MouseEventArgs e)
        {
            var btn = sender as CircularButton;
            if (btn == null) return;
            bool isTrash = btn.ItemType == ContextItem.TrashPaper || btn.ItemType == ContextItem.TrashPlastic || btn.ItemType == ContextItem.TrashGlass || btn.ItemType == ContextItem.TrashMixed;
            if (isTrash && logic.IsContextMode) btn.DoDragDrop(btn, DragDropEffects.Move);
        }

        //logika dla kubla
        private void Bin_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CircularButton))) e.Effect = DragDropEffects.Move;
            else e.Effect = DragDropEffects.None;
        }


        private void Bin_DragDrop(object sender, DragEventArgs e)
        {
            var bin = sender as Button;
            var trashBtn = e.Data.GetData(typeof(CircularButton)) as CircularButton;
            if (bin != null && trashBtn != null)
            {
                bool isCorrectBin = false;
                if (bin == btnBinPaper && trashBtn.ItemType == ContextItem.TrashPaper) isCorrectBin = true;
                else if (bin == btnBinPlastic && trashBtn.ItemType == ContextItem.TrashPlastic) isCorrectBin = true;
                else if (bin == btnBinGlass && trashBtn.ItemType == ContextItem.TrashGlass) isCorrectBin = true;

                if (isCorrectBin)
                {
                    bool sequenceCorrect = logic.ValidateClick(trashBtn);
                    if (sequenceCorrect) ProcessValidAction(trashBtn);
                    else ProcessInvalidAction();
                }
                else ProcessInvalidAction();
            }
        }


        private async void Dot_Click(object sender, EventArgs e)
        {
            var btn = sender as CircularButton;
            if (btn == null) return;


            bool isTrash = btn.ItemType == ContextItem.TrashPaper || btn.ItemType == ContextItem.TrashPlastic || btn.ItemType == ContextItem.TrashGlass;
            if (logic.IsContextMode && isTrash) return;


            bool isPhone = btn.ItemType == ContextItem.PhoneFamily ||
                           btn.ItemType == ContextItem.Phone112 ||
                           btn.ItemType == ContextItem.Phone999 ||
                           btn.ItemType == ContextItem.Phone998 ||
                           btn.ItemType == ContextItem.Phone997;

            if (logic.IsContextMode && isPhone)
            {
                bool sequenceCorrect = logic.ValidateClick(btn);
                if (sequenceCorrect)
                {
                    lblScore.Text = "Wynik: " + logic.Score;
                    string logText = GetItemName(btn);
                    AddToHistory(logText);
                    _pendingPhoneTask = btn.ItemType;
                    btn.Visible = false;
                    ShowPhonePad();
                }
                else ProcessInvalidAction();
                return;
            }


            bool correct = logic.ValidateClick(btn);
            if (correct) ProcessValidAction(btn);
            else ProcessInvalidAction();
        }

        private string GetItemName(CircularButton btn)
        {
            if (!string.IsNullOrWhiteSpace(btn.Text) && btn.Text != "?") return btn.Text.Replace("\n", " ");

            switch (btn.ItemType)
            {
                case ContextItem.TrashPaper: return "Gazeta";
                case ContextItem.TrashPlastic: return "Butelka";
                case ContextItem.TrashGlass: return "Sloik";
                case ContextItem.TrashMixed: return "Smieci";
                case ContextItem.Phone112: return "SOS (112)";
                case ContextItem.Phone999: return "Karetka";
                case ContextItem.Phone998: return "Straz";
                case ContextItem.Phone997: return "Policja";
                case ContextItem.PhoneFamily: return "Wnuczek";
                case ContextItem.PillMorning: return "Leki rano";
                case ContextItem.PillEvening: return "Leki wieczor";
                case ContextItem.PillPain: return "Leki p/bolowe";
                default: return "Element";
            }
        }

        private void AddToHistory(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry)) return;
            _historyLog.Add(entry);
            if (_historyLog.Count > 5) { _historyLog.RemoveAt(0); }
            string displayHistory = string.Join(", ", _historyLog);
            lblClickHistory.Text = "Historia: " + displayHistory;
        }


        private void ProcessValidAction(CircularButton btn)
        {
            string entry = logic.IsContextMode ? GetItemName(btn) : btn.DotNumber.ToString();
            AddToHistory(entry);
            btn.Visible = false;
            lblScore.Text = "Wynik: " + logic.Score;
            if (logic.IsWaveFinished()) { logic.NextWave(); LoadWave(); }
        }


        private async void ProcessInvalidAction()
        {
            this.BackColor = Color.MistyRose;
            await Task.Delay(200);
            this.BackColor = Color.AliceBlue;
            lblScore.Text = "Wynik: " + logic.Score;
        }

        // wnuczek
        private void ShowImmediateMemoryTest(ContextItem correctAnswer)
        {
            pnlGame.Visible = false;
            pnlTest.Visible = true;
            pnlTest.BringToFront();
            pnlTest.Controls.Clear();

            // 1. Tytuł na samej górze
            Label lblTitle = new Label
            {
                Text = "WNUCZEK ODEBRAL!",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Centrowanie poziome, Y = 40
            lblTitle.Location = new Point((pnlTest.Width - lblTitle.PreferredWidth) / 2, 40);
            pnlTest.Controls.Add(lblTitle);

            // 2. Pytanie pod tytułem
            Label lblQuestion = new Label
            {
                Text = "\"Czesc Dziadku/Babciu! Co przed chwila robiles?\"",
                Font = new Font("Segoe UI", 16, FontStyle.Italic),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Centrowanie poziome, Y = 100 (odstęp od tytułu)
            lblQuestion.Location = new Point((pnlTest.Width - lblQuestion.PreferredWidth) / 2, 100);
            pnlTest.Controls.Add(lblQuestion);

            // Przygotowanie odpowiedzi
            var distractors = logic.GetRandomDistractors(3, new List<ContextItem> { correctAnswer, ContextItem.PhoneFamily });
            var answers = new List<ContextItem> { correctAnswer };
            answers.AddRange(distractors);
            var rand = new Random();
            answers = answers.OrderBy(x => rand.Next()).ToList();

            // 3. Przyciski zaczynają się znacznie niżej (Y = 200), żeby nie nachodziły na tekst
            int startY = 200;
            int btnGap = 85;

            foreach (var item in answers)
            {
                string btnText = logic.GetContextInstruction(item)
                    .Replace("Zadzwon na ", "")
                    .Replace("Wyrzuc ", "")
                    .Replace("Wez ", "");

                Button btn = new Button
                {
                    Text = btnText,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Size = new Size(400, 70),
                    BackColor = Color.RoyalBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                // Centrowanie przycisku
                btn.Location = new Point((pnlTest.Width - btn.Width) / 2, startY);

                btn.MouseEnter += (s, e) => btn.BackColor = Color.CornflowerBlue;
                btn.MouseLeave += (s, e) => btn.BackColor = Color.RoyalBlue;

                btn.Click += (s, e) => ResumeGameAfterTest(item == correctAnswer);
                pnlTest.Controls.Add(btn);

                startY += btnGap;
            }
        }

        private void ResumeGameAfterTest(bool wasCorrect)
        {
            if (wasCorrect) MessageBox.Show("Wnuczek: \"Aha, rozumiem! To swietnie!\"", "Brawo");
            else MessageBox.Show("Wnuczek: \"Naprawde? Wydawalo mi sie, ze cos innego...\"", "Pomylka");
            ContinueGame();
        }

        //timer
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            globalTimeLeft--;
            lblGlobalTimer.Text = "Gra: " + globalTimeLeft + "s";


            if (globalTimeLeft <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show($"Koniec gry! Czas minal.\nTwoj wynik: {logic.Score}.");
                ShowTestPhase();
                return;
            }


            if (logic.CurrentDifficulty != DifficultyLevel.Easy)
            {
                waveTimeLeft--;
                UpdateWaveTimerDisplay();
                if (waveTimeLeft <= 0)
                {
                    lblWaveTimer.Text = "Zestaw: 0s!";
                    logic.ApplyTimePenalty();
                    lblScore.Text = "Wynik: " + logic.Score;
                    this.BackColor = Color.Salmon;
                    Task.Delay(200).ContinueWith(t => this.Invoke((Action)(() => this.BackColor = Color.AliceBlue)));
                    Application.DoEvents();
                    logic.NextWave();
                    LoadWave();
                }
            }
        }

        private void UpdateWaveTimerDisplay()
        {
            if (logic.CurrentDifficulty == DifficultyLevel.Easy) lblWaveTimer.Text = "Zestaw: ∞";
            else { lblWaveTimer.Text = "Zestaw: " + waveTimeLeft + "s"; if (waveTimeLeft <= 3) lblWaveTimer.ForeColor = Color.DarkRed; else lblWaveTimer.ForeColor = Color.Red; }
        }

        //test
        private void ShowTestPhase()
        {
            pnlGame.Visible = false;
            pnlTest.Visible = true;
            pnlTest.BringToFront();
            pnlTest.Controls.Clear();

            // WARUNEK: Tryb podstawowy (Kolory)
            if (!logic.IsContextMode)
            {
                testExpectedSequence = logic.GetLastColors(3);
                testCurrentIndex = 0;
                testCorrectAnswers = 0;

                Label lblQuestion = new Label
                {
                    Font = new Font("Arial", 18, FontStyle.Bold),
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                if (testExpectedSequence.Count == 0)
                {
                    lblQuestion.Text = "Brak ukonczonych fal.";
                    lblQuestion.Location = new Point((pnlTest.Width - lblQuestion.PreferredWidth) / 2, 50);
                    pnlTest.Controls.Add(lblQuestion);
                    CreateBackToMenuButton(pnlTest, 300);
                    return;
                }

                // Ustawienie tekstu pytania
                lblQuestion.Text = $"Odtworz kolejnosc {testExpectedSequence.Count} ostatnich kolorow:";
                lblQuestion.Location = new Point((pnlTest.Width - lblQuestion.PreferredWidth) / 2, 40);
                pnlTest.Controls.Add(lblQuestion);

                // Ustawienie tekstu postępu
                Label lblTestProgress = new Label
                {
                    Text = $"Postep: 0/{testExpectedSequence.Count}",
                    Font = new Font("Arial", 14),
                    AutoSize = true
                };
                lblTestProgress.Location = new Point((pnlTest.Width - lblTestProgress.PreferredWidth) / 2, 90);
                pnlTest.Controls.Add(lblTestProgress);

                // Przesunięcie przycisków w dół (Y = 160)
                int y = 160;
                int gap = 70; // mniejszy odstęp żeby się zmieściły 4
                int btnWidth = 350;

                // Funkcja pomocnicza do centrowania przycisku koloru
                Action<string, Color, DotColor, int> makeBtn = (txt, col, type, posY) => {
                    Button btn = new Button
                    {
                        Text = txt,
                        BackColor = col,
                        ForeColor = Color.White,
                        Font = new Font("Arial", 16, FontStyle.Bold),
                        Size = new Size(btnWidth, 60),
                        // Centrowanie
                        Location = new Point((pnlTest.Width - btnWidth) / 2, posY)
                    };
                    btn.Click += (s, e) => CheckMultiColorAnswer(type);
                    pnlTest.Controls.Add(btn);
                };

                makeBtn("CZERWONY", Color.Crimson, DotColor.Red, y);
                makeBtn("ZIELONY", Color.ForestGreen, DotColor.Green, y + gap);
                makeBtn("NIEBIESKI", Color.RoyalBlue, DotColor.Blue, y + gap * 2);
                makeBtn("ZOLTY", Color.Goldenrod, DotColor.Yellow, y + gap * 3);
            }
            // WARUNEK: Tryb Kontekstowy (ADL)
            else
            {
                _contextTestQuestions = logic.GetLastContextItems(3);
                _contextQuestionIndex = 0;
                _contextTestScore = 0;
                if (_contextTestQuestions.Count == 0)
                {
                    Label lblErr = new Label
                    {
                        Text = "Brak historii do testu.",
                        Font = new Font("Arial", 16),
                        AutoSize = true
                    };
                    lblErr.Location = new Point((pnlTest.Width - lblErr.PreferredWidth) / 2, 100);
                    pnlTest.Controls.Add(lblErr);
                    CreateBackToMenuButton(pnlTest, 300);
                    return;
                }
                ShowNextContextQuestion();
            }
        }

        private void ShowNextContextQuestion()
        {
            pnlTest.Controls.Clear();

            // Koniec testu
            if (_contextQuestionIndex >= _contextTestQuestions.Count)
            {
                Label lblEnd = new Label
                {
                    Text = $"KONIEC TESTU\nWynik testu: {_contextTestScore} / {_contextTestQuestions.Count}\nWynik gry: {logic.Score}",
                    Font = new Font("Arial", 20, FontStyle.Bold),
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                lblEnd.Location = new Point((pnlTest.Width - lblEnd.PreferredWidth) / 2, 100);
                pnlTest.Controls.Add(lblEnd);
                CreateBackToMenuButton(pnlTest, 300);
                return;
            }

            var currentCorrectItem = _contextTestQuestions[_contextQuestionIndex];

            // Etykieta 1: Numer pytania
            Label lblInfo = new Label
            {
                Text = $"Co robiles? (Krok {_contextQuestionIndex + 1} z {_contextTestQuestions.Count})",
                Font = new Font("Arial", 14, FontStyle.Underline),
                AutoSize = true
            };
            lblInfo.Location = new Point((pnlTest.Width - lblInfo.PreferredWidth) / 2, 30);
            pnlTest.Controls.Add(lblInfo);

            // Etykieta 2: Treść pytania
            Label lblQ = new Label
            {
                Text = "Wybierz czynnosc, ktora wykonywales w tej kolejnosci:",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblQ.Location = new Point((pnlTest.Width - lblQ.PreferredWidth) / 2, 80);
            pnlTest.Controls.Add(lblQ);

            // Generowanie odpowiedzi
            var distractors = logic.GetRandomDistractors(3, new List<ContextItem> { currentCorrectItem });
            var answers = new List<ContextItem> { currentCorrectItem };
            answers.AddRange(distractors);
            var rand = new Random();
            answers = answers.OrderBy(x => rand.Next()).ToList();

            // Start przycisków Y = 180 (bezpieczny odstęp od tekstu na Y=80)
            int startY = 180;

            foreach (var item in answers)
            {
                string btnText = logic.GetContextInstruction(item);
                Button btn = new Button
                {
                    Text = btnText,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(400, 60),
                    BackColor = Color.LightSlateGray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                // Centrowanie przycisku
                btn.Location = new Point((pnlTest.Width - btn.Width) / 2, startY);

                btn.Click += (s, e) => CheckContextQuestion(item, currentCorrectItem);
                pnlTest.Controls.Add(btn);
                startY += 70;
            }
        }

        private void CheckContextQuestion(ContextItem selected, ContextItem expected)
        {
            if (selected == expected) { _contextTestScore++; MessageBox.Show("Dobrze!", "Wynik"); }
            else { MessageBox.Show($"Zle! Poprawna odpowiedz to: {logic.GetContextInstruction(expected)}", "Blad"); }
            _contextQuestionIndex++; ShowNextContextQuestion();
        }

        private void CreateBackToMenuButton(Panel pnl, int y)
        {
            Button btnMenuTest = new Button { Text = "Wroc do Menu", Size = new Size(200, 50), Location = new Point(400, y), Font = new Font("Arial", 14) };
            btnMenuTest.Location = new Point((pnl.Width - btnMenuTest.Width) / 2, y);
            btnMenuTest.Click += (s, e) => ShowMenu();
            pnl.Controls.Add(btnMenuTest);
        }

        private void CreateColorTestButton(string text, Color bg, DotColor colorType, int x, int y)
        {
            Button btn = new Button { Text = text, BackColor = bg, ForeColor = Color.White, Font = new Font("Arial", 16, FontStyle.Bold), Size = new Size(300, 60), Location = new Point(x, y) };
            btn.Click += (s, e) => CheckMultiColorAnswer(colorType);
            pnlTest.Controls.Add(btn);
        }

        private void CheckMultiColorAnswer(DotColor selectedColor)
        {
            DotColor expected = testExpectedSequence[testCurrentIndex];
            if (selectedColor == expected) { testCorrectAnswers++; MessageBox.Show("Dobrze! Poprawna odpowiedz.", "Sukces"); }
            else { string polishName = GetPolishColorName(expected); MessageBox.Show($"Blad! Poprawny kolor to: {polishName}.", "Pomylka"); }
            testCurrentIndex++;
            if (testCurrentIndex >= testExpectedSequence.Count)
            {
                string msg = $"Koniec testu!\nPoprawne odpowiedzi: {testCorrectAnswers} z {testExpectedSequence.Count}.\n" + $"Calkowity wynik gry: {logic.Score}";
                MessageBox.Show(msg, "Podsumowanie"); ShowMenu();
            }
        }

        private string GetPolishColorName(DotColor color)
        {
            switch (color) { case DotColor.Red: return "CZERWONY"; case DotColor.Green: return "ZIELONY"; case DotColor.Blue: return "NIEBIESKI"; case DotColor.Yellow: return "ZOLTY"; default: return "Nieznany"; }
        }
    }
}