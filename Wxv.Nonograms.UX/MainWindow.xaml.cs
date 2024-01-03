using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wxv.Nonograms.Core;

namespace Wxv.Nonograms.UX
{
    public partial class MainWindow
    {
        private const string DefaultSolutionText = @"
#Solution
[ @@@ ]
[@   @]
[ @@@ ]
[  @  ]
[  @  ]
[ @@  ]
[@@@  ]";
        
        private Solution Solution { get; set; }
        private List<Turn> TurnHistory { get; } = new();
        private int TurnHistoryIndex { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();

            Solution solution;
            Turn turn;
            try
            {
                solution = Solution.Parse(Settings.Default.Solution);
                turn = Turn.Parse(Settings.Default.Turn);
            }
            catch
            {
                solution = Solution.Parse(DefaultSolutionText);
                turn = solution.ToTurn();
            }

            Solution = solution;
            TurnHistory.Clear();
            TurnHistory.Add(turn);
            TurnHistoryIndex = 0;
            nonogramGrid.SetSolutionTurn(solution, turn);        
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            Settings.Default.Solution = Solution.ToString();
            Settings.Default.Turn = TurnHistory[TurnHistoryIndex].ToString();
            Settings.Default.Save();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
            => Close();

        private void CheckMenuItem_OnClick(object sender, RoutedEventArgs e)
            => TurnApplyCheck();

        private void HintMenuItem_OnClick(object sender, RoutedEventArgs e)
            => TurnApplyHint();

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                TurnApplyCheck();
            else if (e.Key == Key.H)
                TurnApplyHint();
            else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                TurnPaste();
            else if (e.Key == Key.Delete)
                TurnClear();
            else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                TurnApplyUndo();
            else if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                TurnApplyRedo();
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
            => TurnPaste();

        private void ClearMenuItem_Click(object sender, RoutedEventArgs e)
            => TurnClear();

        private void UndoMenuItem_OnClick(object sender, RoutedEventArgs e)
            => TurnApplyUndo();

        private void RedoMenuItem_OnClick(object sender, RoutedEventArgs e)
            => TurnApplyRedo();

        private void NonogramGrid_OnTurnChange(Turn newturn) 
            => TurnApplyChange(newturn);

        private void RefreshUX()
        {
            pasteMenuItem.IsEnabled = Clipboard.ContainsImage() || Clipboard.ContainsText(); 
            undoMenuItem.IsEnabled = TurnHistoryIndex > 0;
            redoMenuItem.IsEnabled = TurnHistoryIndex < TurnHistory.Count - 1;
            checkMenuItem.IsEnabled = !TurnHistory[TurnHistoryIndex].IsFinished;
            hintMenuItem.IsEnabled = !TurnHistory[TurnHistoryIndex].IsFinished;
        }

        private void TurnApplyCheck()
        {
            nonogramGrid.ApplyCheck();
        }

        private void TurnApplyHint()
        {
            nonogramGrid.ApplyHint();
        }

        private void TurnPaste()
        {
            object? data = null;

            var text = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(text))
                data = text;

            var image = Clipboard.GetImage();
            if (data == null && image != null)
            {
                var bitmapFrame = BitmapFrame.Create(image);
                var pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(bitmapFrame);
                using var memoryStream = new MemoryStream();
                pngBitmapEncoder.Save(memoryStream);
                data = memoryStream.ToArray();
            }

            if (data == null)
                return;

            var importDialog = new ImportDialog()
            {
                Data = data
            };
            var dialogResult = importDialog.ShowDialog(); 
            if (!(dialogResult ?? false) 
              || !importDialog.NonogrameImporterResult.HasValue 
              || !importDialog.NonogrameImporterResult.Value.IsSuccess)
                return;
            
            Solution = importDialog.NonogrameImporterResult.Value.Solution!;
            var turn = Solution.ToTurn();
            TurnHistory.Clear();
            TurnHistory.Add(turn);
            TurnHistoryIndex = 0;
            nonogramGrid.SetSolutionTurn(Solution, turn);
            RefreshUX();
        }

        private void TurnClear()
        {
            var turn = Solution.ToTurn();
            TurnHistory.Clear();
            TurnHistory.Add(turn);
            TurnHistoryIndex = 0;
            nonogramGrid.SetSolutionTurn(Solution, turn);
            RefreshUX();
        }

        private void TurnApplyUndo()
        {
            if (TurnHistoryIndex <= 0)
                return;

            TurnHistoryIndex--;
            nonogramGrid.SetSolutionTurn(Solution, TurnHistory[TurnHistoryIndex]);
            RefreshUX();
        }

        private void TurnApplyRedo()
        {
            if (TurnHistoryIndex >= TurnHistory.Count - 1)
                return;

            TurnHistoryIndex++;
            nonogramGrid.SetSolutionTurn(Solution, TurnHistory[TurnHistoryIndex]);
            RefreshUX();
        }

        private void TurnApplyChange(Turn newTurn)
        {
            TurnHistory.RemoveRange(TurnHistoryIndex + 1, TurnHistory.Count - TurnHistoryIndex - 1);
            TurnHistory.Add(newTurn);
            TurnHistoryIndex = TurnHistory.Count - 1; 
            RefreshUX();
        }

    }
}
