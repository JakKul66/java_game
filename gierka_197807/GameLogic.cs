using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace gierka_197807
{
    public class GameLogic
    {
        public int Score { get; private set; }
        public int CurrentLevel { get; private set; }
        public bool IsContextMode { get; set; }
        public DifficultyLevel CurrentDifficulty { get; set; }

        //numery
        public const string GRANDSON_NUMBER = "505";
        public const string NUMBER_112 = "112";
        public const string NUMBER_999 = "999";
        public const string NUMBER_998 = "998";
        public const string NUMBER_997 = "997";

        // historia do testow
        public List<DotColor> TargetColorHistory { get; private set; }
        public DotColor TargetColor { get; set; }
        public List<int> BasicTargetSequence { get; private set; }

        public List<ContextItem> TargetSequence { get; private set; }
        public List<ContextItem> ContextHistory { get; private set; }

        private int _currentSequenceIndex;
        private int _totalTargetsInWave;
        private int _clickedTargetsInWave;

        private const int DOT_SIZE = 90;
        private const int MARGIN = 10;

        public GameLogic()
        {
            TargetSequence = new List<ContextItem>();
            BasicTargetSequence = new List<int>();
            TargetColorHistory = new List<DotColor>();
            ContextHistory = new List<ContextItem>();
            ResetGame(false, DifficultyLevel.Normal);
        }

        public void ResetGame(bool contextMode, DifficultyLevel difficulty)
        {
            Score = 0;
            CurrentLevel = 1;
            IsContextMode = contextMode;
            CurrentDifficulty = difficulty;
            TargetColorHistory.Clear();
            ContextHistory.Clear();
            ResetWaveCounters();
        }

        private void ResetWaveCounters()
        {
            _currentSequenceIndex = 0;
            _clickedTargetsInWave = 0;
            _totalTargetsInWave = 0;
        }

        public void NextWave()
        {
            CurrentLevel++;
            ResetWaveCounters();
        }

       
        public List<DotColor> GetLastColors(int count)
        {
            int take = Math.Min(count, TargetColorHistory.Count);
            return TargetColorHistory.Skip(TargetColorHistory.Count - take).ToList();
        }

        public List<CircularButton> GenerateLevelDots(int width, int height)
        {
            if (IsContextMode) return GenerateContextLevel(width, height);
            else return GenerateBasicLevel(width, height);
        }

        // gen trybu podstawowego
        private List<CircularButton> GenerateBasicLevel(int width, int height)
        {
            var random = new Random();
            var dots = new List<CircularButton>();
            BasicTargetSequence.Clear();
            TargetColor = (DotColor)random.Next(0, 4);
            TargetColorHistory.Add(TargetColor);

            // skalowanie trudnosci 
            int baseTargets = 3 + (CurrentLevel / 2);
            int baseDistractors = 3 + CurrentLevel;
            int targetsCount = baseTargets;
            int distractorsCount = baseDistractors;

            switch (CurrentDifficulty)
            {
                case DifficultyLevel.Easy:
                    targetsCount = Math.Max(2, baseTargets - 1);
                    distractorsCount = Math.Max(1, baseDistractors - 2);
                    break;
                case DifficultyLevel.Normal:
                    targetsCount = Math.Min(baseTargets, 6);
                    break;
                case DifficultyLevel.Hard:
                    targetsCount = Math.Min(baseTargets + 1, 8);
                    distractorsCount = baseDistractors + 3;
                    break;
            }
            _totalTargetsInWave = targetsCount;
            List<int> numbers = Enumerable.Range(1, targetsCount).ToList();

            
            if (CurrentDifficulty == DifficultyLevel.Easy) BasicTargetSequence = numbers.ToList();
            else BasicTargetSequence = numbers.OrderBy(x => random.Next()).ToList();

            // Tworzenie elementow
            for (int i = 1; i <= targetsCount; i++)
            {
                var btn = CreateDot(width, height, random, dots);
                btn.LogicColor = TargetColor;
                btn.BackColor = GetColorFromEnum(TargetColor);
                btn.DotNumber = i;
                btn.Text = i.ToString();
                dots.Add(btn);
            }
            // dystraktory
            for (int i = 0; i < distractorsCount; i++)
            {
                var btn = CreateDot(width, height, random, dots);
                DotColor badColor;
                do { badColor = (DotColor)random.Next(0, 4); } while (badColor == TargetColor);
                btn.LogicColor = badColor;
                btn.BackColor = GetColorFromEnum(badColor);
                btn.DotNumber = random.Next(1, 10);
                btn.Text = btn.DotNumber.ToString();
                dots.Add(btn);
            }
            return dots.OrderBy(x => random.Next()).ToList();
        }

        // generowanie adl
        private List<CircularButton> GenerateContextLevel(int width, int height)
        {
            var random = new Random();
            var dots = new List<CircularButton>();
            TargetSequence.Clear();

            int calculatedLength = 3 + (CurrentLevel / 3);
            int sequenceLength = Math.Min(4, calculatedLength);

            if (CurrentDifficulty == DifficultyLevel.Easy)
                sequenceLength = Math.Max(2, sequenceLength - 1);

            _totalTargetsInWave = sequenceLength;

            var possibleTasks = new List<ContextItem>
            {
                ContextItem.TrashPaper, ContextItem.TrashPlastic, ContextItem.TrashGlass,
                ContextItem.Phone112, ContextItem.Phone999, ContextItem.Phone998, ContextItem.Phone997,
                ContextItem.PhoneFamily,
                ContextItem.PillMorning
            };

            // generowanie sekwencji zadan
            for (int i = 0; i < sequenceLength; i++)
            {
                ContextItem newItem;
                do { newItem = possibleTasks[random.Next(possibleTasks.Count)]; }
                while (i == 0 && newItem == ContextItem.PhoneFamily);

                TargetSequence.Add(newItem);
                var btn = CreateContextButton(newItem, width, height, random, dots);
                dots.Add(btn);
            }

            // dodawanie smieci zmieszanych i innych elementow jako tlo
            int distractorCount = 3 + CurrentLevel;
            if (sequenceLength >= 4) distractorCount = Math.Min(distractorCount, 3);

            for (int i = 0; i < distractorCount; i++)
            {
                var randomType = possibleTasks[random.Next(possibleTasks.Count)];

                if (randomType == ContextItem.PhoneFamily) randomType = ContextItem.TrashMixed;
                if (random.NextDouble() > 0.6) randomType = ContextItem.TrashMixed;

                var btn = CreateContextButton(randomType, width, height, random, dots);
                dots.Add(btn);
            }
            return dots.OrderBy(x => random.Next()).ToList();
        }

        private CircularButton CreateDot(int w, int h, Random r, List<CircularButton> existingDots)
        {
            var btn = new CircularButton();
            btn.Location = GetSafeLocation(w, h, r, existingDots);
            btn.ItemType = ContextItem.None;
            return btn;
        }

        // konfiguracja wygladu przyciskow w trybie kontekstowym
        private CircularButton CreateContextButton(ContextItem type, int w, int h, Random rnd, List<CircularButton> existingDots)
        {
            var btn = new CircularButton();
            btn.Location = GetSafeLocation(w, h, rnd, existingDots);
            btn.ItemType = type;

            btn.BackColor = Color.WhiteSmoke;
            btn.Text = "";

            switch (type)
            {
                case ContextItem.TrashPaper:
                    btn.CustomImage = Properties.Resources.papier;
                    btn.BackColor = Color.DeepSkyBlue;
                    break;
                case ContextItem.TrashPlastic:
                    btn.CustomImage = Properties.Resources.plastik;
                    btn.BackColor = Color.Yellow;
                    break;
                case ContextItem.TrashGlass:
                    btn.CustomImage = Properties.Resources.szklo1;
                    btn.BackColor = Color.LimeGreen;
                    break;
                case ContextItem.TrashMixed:
                    btn.CustomImage = Properties.Resources.smieci;
                    btn.BackColor = Color.Gray;
                    break;
                case ContextItem.Phone999:
                    btn.CustomImage = Properties.Resources.karetka1;
                    btn.BackColor = Color.HotPink;
                    break;
                case ContextItem.Phone998:
                    btn.CustomImage = Properties.Resources.straz;
                    btn.BackColor = Color.Orange;
                    break;
                case ContextItem.Phone997:
                    btn.CustomImage = Properties.Resources.policja;
                    btn.BackColor = Color.Blue;
                    break;
                case ContextItem.Phone112:
                    btn.Text = "112\nSOS";
                    btn.BackColor = Color.Red;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Arial", 14, FontStyle.Bold);
                    break;
                case ContextItem.PhoneFamily:
                    btn.Text = "WNUCZEK";
                    btn.BackColor = Color.LimeGreen;
                    btn.ForeColor = Color.White;
                    break;
                case ContextItem.PillMorning:
                    btn.CustomImage = Properties.Resources.tabletki1;
                    btn.BackColor = Color.LightPink;
                    break;
                case ContextItem.PillEvening:
                    btn.CustomImage = Properties.Resources.tabletki1;
                    btn.BackColor = Color.Plum;
                    break;
                case ContextItem.PillPain:
                    btn.CustomImage = Properties.Resources.tabletki1;
                    btn.BackColor = Color.White;
                    break;
                default:
                    btn.Text = "?";
                    break;
            }
            return btn;
        }

        public string GetContextInstruction(ContextItem item)
        {
            switch (item)
            {
                case ContextItem.TrashPaper: return "Wyrzuc Gazete (Papier)";
                case ContextItem.TrashPlastic: return "Wyrzuc Butelke (Plastik)";
                case ContextItem.TrashGlass: return "Wyrzuc Sloik (Szklo)";
                case ContextItem.TrashMixed: return "Wyrzuc Smieci (Zmieszane)";

                case ContextItem.Phone112: return "Zadzwon na 112 (Wypadek)";
                case ContextItem.Phone999: return "Zadzwon na Pogotowie (999)";
                case ContextItem.Phone998: return "Zadzwon na Straz (998)";
                case ContextItem.Phone997: return "Zadzwon na Policje (997)";
                case ContextItem.PhoneFamily: return $"Zadzwon do Wnuczka ({GRANDSON_NUMBER})";
                case ContextItem.PillMorning: return "Wez lek poranny";
                case ContextItem.PillEvening: return "Wez lek na sen";
                default: return "Kliknij element";
            }
        }

        public List<ContextItem> GetRandomDistractors(int count, List<ContextItem> excludes)
        {
            var random = new Random();
            var allItems = Enum.GetValues(typeof(ContextItem)).Cast<ContextItem>().ToList();

            allItems.Remove(ContextItem.None);
            foreach (var ex in excludes) allItems.Remove(ex);

            allItems = allItems.Where(x => GetContextInstruction(x) != "Kliknij element").ToList();

            return allItems.OrderBy(x => random.Next()).Take(count).ToList();
        }

        public ContextItem GetItemBeforeLast()
        {
            int prevIndex = _currentSequenceIndex - 2;
            if (prevIndex >= 0 && prevIndex < TargetSequence.Count) return TargetSequence[prevIndex];
            return ContextItem.None;
        }

        public List<ContextItem> GetLastContextItems(int count)
        {
            int take = Math.Min(count, ContextHistory.Count);
            return ContextHistory.Skip(ContextHistory.Count - take).ToList();
        }

        // kolizje
        private Point GetSafeLocation(int w, int h, Random r, List<CircularButton> existingDots)
        {
            int maxAttempts = 200;
            int safeSize = DOT_SIZE + MARGIN;
            for (int i = 0; i < maxAttempts; i++)
            {
                int x = r.Next(50, w - 100);
                int y = r.Next(100, h - 100);
                Rectangle proposedRect = new Rectangle(x, y, safeSize, safeSize);
                bool overlaps = false;
                foreach (var dot in existingDots)
                {
                    Rectangle existingRect = new Rectangle(dot.Location.X, dot.Location.Y, safeSize, safeSize);
                    if (proposedRect.IntersectsWith(existingRect)) { overlaps = true; break; }
                }
                if (!overlaps) return new Point(x, y);
            }
            return new Point(r.Next(50, w - 100), r.Next(100, h - 100));
        }

        //walidacja ruchu gracza
        public bool ValidateClick(CircularButton btn)
        {
            bool isCorrect = false;
            // walidacja dla trybu Kontekstowego
            if (IsContextMode)
            {
                if (_currentSequenceIndex < TargetSequence.Count && btn.ItemType == TargetSequence[_currentSequenceIndex])
                {
                    Score += 150;
                    ContextHistory.Add(btn.ItemType);
                    _currentSequenceIndex++;
                    _clickedTargetsInWave++;
                    isCorrect = true;
                }
                else
                {
                    Score -= 50;
                    isCorrect = false;
                }
            }
            // walidacja dla trybu Podstawowego
            else
            {
                if (btn.LogicColor == TargetColor &&
                    _currentSequenceIndex < BasicTargetSequence.Count &&
                    btn.DotNumber == BasicTargetSequence[_currentSequenceIndex])
                {
                    Score += 100;
                    _currentSequenceIndex++;
                    _clickedTargetsInWave++;
                    isCorrect = true;
                }
                else
                {
                    Score -= 50;
                    isCorrect = false;
                }
            }
            return isCorrect;
        }

        public bool IsWaveFinished()
        {
            return _clickedTargetsInWave >= _totalTargetsInWave;
        }

        private Color GetColorFromEnum(DotColor dc)
        {
            switch (dc)
            {
                case DotColor.Red: return Color.Crimson;
                case DotColor.Green: return Color.ForestGreen;
                case DotColor.Blue: return Color.RoyalBlue;
                case DotColor.Yellow: return Color.Goldenrod;
                default: return Color.Gray;
            }
        }

        public void ApplyTimePenalty()
        {
            Score -= 200;
        }

        public void AddScore(int points)
        {
            Score += points;
        }
    }
}