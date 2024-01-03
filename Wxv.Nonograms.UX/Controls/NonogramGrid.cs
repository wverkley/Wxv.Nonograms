using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wxv.Nonograms.Core;

namespace Wxv.Nonograms.UX.Controls;

internal enum MouseSelectionMode
{
    Set,
    Unknown,
    Unset,
    Measure
}

public delegate void OnTurnChange(Turn newTurn);

internal class NonogramGrid : FrameworkElement
{
    #region Constants
    private const double CellSize = 20.0;
    private const double BorderGridLineSize = 2.0;
    private const double MinorGridLineSize = 1.0;
    private const double MajorGridLineSize = 2.0;
    private const double SetLineSize = 2.0;
    private const double LengthFontSize = 14.0;
    private const double LegendFontSize = 12.0;
    private const double SelectionBoxLineSize = 2.0;
    private const double SelectionLegendLineSize = 1.0;    
    private const double SelectionLegendMargin = 5;    
    #endregion

    #region Brushes
    private static SolidColorBrush BackgroundBrush { get; } = new(Color.FromRgb(255, 255, 255));
    private static SolidColorBrush CornerBrush { get; } = new(Color.FromRgb(240, 240, 240));
    private static SolidColorBrush HeaderBrush { get; } = new(Color.FromRgb(218, 218, 218));
    private static SolidColorBrush HeaderSelectedBrush { get; } = new(Color.FromRgb(238, 238, 238));
    private static SolidColorBrush BorderLineBrush { get; } = new(Color.FromRgb(0, 0, 0));
    private static SolidColorBrush LengthBrush { get; } = new(Color.FromRgb(208, 208, 208));
    private static SolidColorBrush LengthLineBrush { get; } = new(Color.FromRgb(143, 143, 143));
    private static SolidColorBrush CellLineBrush { get; } = new(Color.FromRgb(112, 112, 112));
    private static SolidColorBrush HeaderGridLineBrush { get; } = new(Color.FromRgb(143, 143, 143));
    private static SolidColorBrush SetLineBrush { get; } = new(Color.FromRgb(0, 0, 0));
    private static SolidColorBrush SetLineWrongBrush { get; } = new(Color.FromRgb(128, 0, 0));
    private static SolidColorBrush SetLineHintBrush { get; } = new(Color.FromRgb(0, 128, 0));
    private static SolidColorBrush LengthTextBrush { get; } = new(Color.FromRgb(0, 0, 0));
    private static SolidColorBrush LengthCompletedBrush { get; } = new(Color.FromArgb(128 + 32, 0, 0, 0));
    private static SolidColorBrush SelectionBoxBrush { get; } = new(Color.FromRgb(0, 128, 255));
    private static SolidColorBrush SelectionLegendBorderBrush { get; } = new(Color.FromRgb(0, 0, 0));
    private static SolidColorBrush SelectionLegendBackgroundBrush { get; } = new(Color.FromRgb(255, 255, 255));
    #endregion
    
    #region Pens
    private static Pen BorderLinePen { get; } = new(BorderLineBrush, BorderGridLineSize);
    private static Pen MinorLengthLinePen { get; } = new(LengthLineBrush, MinorGridLineSize);
    private static Pen MajorLengthLinePen { get; } = new(LengthLineBrush, MajorGridLineSize);
    private static Pen MinorCellLinePen { get; } = new(CellLineBrush, MinorGridLineSize);
    private static Pen MajorCellLinePen { get; } = new(CellLineBrush, MajorGridLineSize);
    private static Pen SetLinePen { get; } = new(SetLineBrush, SetLineSize);
    private static Pen SetLineWrongPen { get; } = new(SetLineWrongBrush, SetLineSize);
    private static Pen SetLineHintPen { get; } = new(SetLineHintBrush, SetLineSize);
    private static Pen LengthTextPen { get; } = new(LengthTextBrush, SetLineSize);
    private static Pen LengthCompletedPen { get; } = new(LengthCompletedBrush, SetLineSize);
    private static Pen SelectionBoxPen { get; } = new(SelectionBoxBrush, SelectionBoxLineSize);
    private static Pen SelectionBoxMeasurePen { get; } = new(SelectionBoxBrush, SelectionBoxLineSize)
    {
        DashStyle = DashStyles.DashDot
    };
    private static Pen SelectionLegendPen { get; } = new(SelectionLegendBorderBrush, SelectionLegendLineSize);
    #endregion
    
    #region Type Faces
    private Typeface Typeface { get; } = new (
        new FontFamily("Segoe UI"),
        FontStyles.Normal,
        FontWeights.Normal,
        FontStretches.Normal);
    #endregion

    #region State
    public Solution Solution { get; private set; } = default!;
    public Puzzle Puzzle { get; private set; } = default!;
    public Turn Turn { get; private set; } = default!;
    #endregion
    
    #region Render data
    private double PuzzleHeaderRenderWidth { get; set; } 
    private double PuzzleCellRenderWidth { get; set; } 
    private double PuzzleRenderWidth { get; set; } 
    private double PuzzleHeaderRenderHeight { get; set; } 
    private double PuzzleCellRenderHeight { get; set; } 
    private double PuzzleRenderHeight { get; set; } 
    #endregion

    #region Mouse & Keyboard State
    public Point MouseRenderPosition { get; private set; }
    public int MouseCellX { get; private set; }
    public int MouseCellY { get; private set; }
    public bool IsMouseInCell()
        => MouseCellX >= 0 && MouseCellX < Puzzle.Width
        && MouseCellY >= 0 && MouseCellY < Puzzle.Height;
    public int MouseHeaderX { get; private set; }
    public int MouseHeaderY { get; private set; }
    public int MouseHorizontalIndex { get; private set; }
    public bool IsMouseInHorizontalLengths()
        => MouseCellY >= 0 && MouseCellY < Puzzle.Height
        && MouseHorizontalIndex >= 0;
    public int MouseVerticalIndex { get; private set; }
    public bool IsMouseInVerticalLengths()
        => MouseCellX >= 0 && MouseCellX < Puzzle.Width
        && MouseVerticalIndex >= 0;

    public bool IsMouseCellSelection { get; private set; }
    public Point MouseCellSelectionStartRenderPosition { get; private set; }
    public int MouseCellSelectionStartCellX { get; private set; }
    public int MouseCellSelectionStartCellY { get; private set; }
    
    public int SelectionBoxMinX { get; set; }
    public int SelectionBoxMinY { get; set; }
    public int SelectionBoxMaxX { get; set; }
    public int SelectionBoxMaxY { get; set; }
    public int SelectionBoxWidth { get; set; }
    public int SelectionBoxHeight { get; set; }

    public int SelectionLegendX { get; set; }
    public int SelectionLegendY { get; set; }
    
    public MouseSelectionMode MouseSelectionMode { get; private set; } 
    private bool IsShiftPressed { get; set; }
    #endregion

    #region Events
    public event OnTurnChange? TurnChange;
    #endregion

    #region ctor
    static NonogramGrid()
    {
        BackgroundBrush.Freeze();
        CornerBrush.Freeze();
        HeaderBrush.Freeze();
        BorderLineBrush.Freeze();
        LengthBrush.Freeze();
        LengthLineBrush.Freeze();
        CellLineBrush.Freeze();
        HeaderGridLineBrush.Freeze();
        SetLineBrush.Freeze();
        SetLineWrongBrush.Freeze();
        SetLineHintBrush.Freeze();
        LengthTextBrush.Freeze();
        LengthCompletedBrush.Freeze();
        SelectionBoxBrush.Freeze();
        SelectionLegendBorderBrush.Freeze();
        SelectionLegendBackgroundBrush.Freeze();

        BorderLinePen.Freeze();
        MinorLengthLinePen.Freeze();
        MajorLengthLinePen.Freeze();
        MinorCellLinePen.Freeze();
        MajorCellLinePen.Freeze();
        SetLinePen.Freeze();
        SetLineWrongPen.Freeze();
        SetLineHintPen.Freeze();
        LengthTextPen.Freeze();
        LengthCompletedPen.Freeze();
        SelectionBoxPen.Freeze();
        SelectionBoxMeasurePen.Freeze();
        SelectionLegendPen.Freeze();
    }

    public NonogramGrid()
    {
        var solution = Solution.Parse(@"#Solution
[ @@@ ]
[@   @]
[ @@@ ]
[  @  ]
[  @  ]
[ @@  ]
[@@@  ]");
        SetSolutionTurn(solution, solution.ToTurn());
        Focusable = true;
    }
    #endregion

    #region Rendering
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        GetTransforms(out var translateTransform, out var scaleTransform);

        drawingContext.PushTransform(translateTransform);
        drawingContext.PushTransform(scaleTransform);

        // Background
        drawingContext.DrawRectangle(
            BackgroundBrush,
            null,
            new Rect(
                new Point(0, 0),
                new Size(PuzzleRenderWidth, PuzzleRenderHeight)));

        // Puzzle
        if (!Turn.IsFinished)
            RenderPuzzle(drawingContext);
        else
            RenderFinishedPuzzle(drawingContext);

        // Corner Header
        {
            drawingContext.DrawRectangle(
                CornerBrush,
                null,
                new Rect(
                    new Point(MajorGridLineSize, MajorGridLineSize),
                    new Size(PuzzleHeaderRenderWidth, PuzzleHeaderRenderHeight)));
        }

        // Horizontal Length Header
        {
            drawingContext.DrawRectangle(
                HeaderBrush,
                null,
                new Rect(
                    new Point(MajorGridLineSize, MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize),
                    new Size(PuzzleHeaderRenderWidth, PuzzleCellRenderHeight)));
        }

        // Vertical Length Header
        {
            drawingContext.DrawRectangle(
                HeaderBrush,
                null,
                new Rect(
                    new Point(MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize, MajorGridLineSize),
                    new Size(PuzzleCellRenderWidth, PuzzleHeaderRenderHeight)));
        }

        // horizontal lengths
        RenderHorizonalLengths(drawingContext);

        // vertical lengths
        RenderVerticalLengths(drawingContext);

        // horizontal legend
        RenderHorizonalLegend(drawingContext);

        // vertical legend
        RenderVerticalLegend(drawingContext);

        // puzzle grid
        RenderCellGrid(
            drawingContext,
            new Point(
                MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize,
                MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize),
            Puzzle.Width,
            Puzzle.Height,
            MinorCellLinePen,
            MajorCellLinePen,
            true,
            true);

        // vertical lengths header
        RenderCellGrid(
            drawingContext,
            new Point(
                MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize,
                MajorGridLineSize),
            Puzzle.Width,
            Puzzle.HeaderHeight,
            MinorLengthLinePen,
            MajorLengthLinePen,
            false,
            true);

        // horizontal lengths header
        RenderCellGrid(
            drawingContext,
            new Point(
                MajorGridLineSize,
                MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize),
            Puzzle.HeaderWidth,
            Puzzle.Height,
            MinorLengthLinePen,
            MajorLengthLinePen,
            true,
            false);
        
        // Horizontal Major Grid Lines
        foreach (var y in new[]
        {
            MajorGridLineSize / 2.0,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize / 2.0,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize + PuzzleCellRenderHeight + MajorGridLineSize / 2.0,
        })
            drawingContext.DrawLine(
                BorderLinePen, 
                new Point(0.0, y), 
                new Point(PuzzleRenderWidth - CellSize, y));


        // Vertical Major Grid Lines
        foreach (var x in new[]
        {
            MajorGridLineSize / 2.0,
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize / 2.0,
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize + PuzzleCellRenderWidth + MajorGridLineSize / 2.0,
        })
            drawingContext.DrawLine(
                BorderLinePen, 
                new Point(x, 0.0), 
                new Point(x, PuzzleRenderHeight - CellSize));
        
        // Selection Box
        RenderSelectionBox(drawingContext);

        // Selection Legend
        RenderSelectionLegend(drawingContext);
    }

    private void RenderHorizonalLengths(DrawingContext drawingContext)
    {
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize); 
        
        for (var cellY = 0; cellY < Puzzle.Height; cellY++)
        {
            var y = start.Y + cellY * (CellSize + MinorGridLineSize);
            var lengthsCount = Puzzle.HorizontalLengths.ElementAt(cellY).Count;
            for (var lengthIndex = 0; lengthIndex < lengthsCount; lengthIndex++)
            {
                var x = start.X - (lengthIndex + 1) * (CellSize + MinorGridLineSize);
                var length = Puzzle.HorizontalLengths.ElementAt(cellY).ElementAt(lengthsCount - lengthIndex - 1);
                
                drawingContext.DrawRectangle(
                    MouseCellY == cellY ? HeaderSelectedBrush : LengthBrush, 
                    null,
                    new Rect(
                        new Point(x, y),
                        new Point(x + CellSize + MinorGridLineSize, y + CellSize + MinorGridLineSize)));

                // Create the initial formatted text string.
                var formattedText = new FormattedText(
                    length.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface,
                    LengthFontSize,
                    LengthTextBrush,
                    1.0)
                {
                    TextAlignment = TextAlignment.Center
                };

                drawingContext.DrawText(
                    formattedText, 
                    new Point(x + (MinorGridLineSize + CellSize) / 2.0, y));
            }
        }
        
        for (var cellY = 0; cellY < Puzzle.Height; cellY++)
        {
            var y = start.Y + cellY * (CellSize + MinorGridLineSize);
            var lengthsCount = Puzzle.HorizontalLengths.ElementAt(cellY).Count;
            for (var lengthIndex = 0; lengthIndex < lengthsCount; lengthIndex++)
            {
                var x = start.X - (lengthIndex + 1) * (CellSize + MinorGridLineSize);
                if (!Turn.HorizontalLengthsCompleted.ElementAt(cellY).ElementAt(lengthsCount - lengthIndex - 1))
                    continue;

                drawingContext.DrawLine(
                    LengthCompletedPen, 
                    new Point(x, y),
                    new Point(x + CellSize + MinorGridLineSize, y + CellSize + MinorGridLineSize));
                drawingContext.DrawLine(
                    LengthCompletedPen, 
                    new Point(x + CellSize + MinorGridLineSize, y),
                    new Point(x, y + CellSize + MinorGridLineSize));
            }
        }
    }

    private void RenderVerticalLengths(DrawingContext drawingContext)
    {
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize,
            MajorGridLineSize + PuzzleHeaderRenderHeight); 
        
        for (var cellX = 0; cellX < Puzzle.Width; cellX++)
        {
            var x = start.X + cellX * (CellSize + MinorGridLineSize);
            var lengthsCount = Puzzle.VerticalLengths.ElementAt(cellX).Count;
            for (var lengthIndex = 0; lengthIndex < lengthsCount; lengthIndex++)
            {
                var y = start.Y - (lengthIndex + 1) * (CellSize + MinorGridLineSize);
                var length = Puzzle.VerticalLengths.ElementAt(cellX).ElementAt(lengthsCount - lengthIndex - 1);
                
                drawingContext.DrawRectangle(
                    MouseCellX == cellX ? HeaderSelectedBrush : LengthBrush, 
                    null,
                    new Rect(
                        new Point(x, y),
                        new Point(x + CellSize + MinorGridLineSize, y + CellSize + MinorGridLineSize)));

                // Create the initial formatted text string.
                var formattedText = new FormattedText(
                    length.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface,
                    LengthFontSize,
                    Brushes.Black,
                    1.0)
                {
                    TextAlignment = TextAlignment.Center
                };

                drawingContext.DrawText(
                    formattedText, 
                    new Point(x + (MinorGridLineSize + CellSize) / 2.0, y));

            }
        }
        
        for (var cellX = 0; cellX < Puzzle.Width; cellX++)
        {
            var x = start.X + cellX * (CellSize + MinorGridLineSize);
            var lengthsCount = Puzzle.VerticalLengths.ElementAt(cellX).Count;
            for (var lengthIndex = 0; lengthIndex < lengthsCount; lengthIndex++)
            {
                var y = start.Y - (lengthIndex + 1) * (CellSize + MinorGridLineSize);
                if (!Turn.VerticalLengthsCompleted.ElementAt(cellX).ElementAt(lengthsCount - lengthIndex - 1))
                    continue;

                drawingContext.DrawLine(
                    LengthCompletedPen, 
                    new Point(x, y),
                    new Point(x + CellSize + MinorGridLineSize, y + CellSize + MinorGridLineSize));
                drawingContext.DrawLine(
                    LengthCompletedPen, 
                    new Point(x + CellSize + MinorGridLineSize, y),
                    new Point(x, y + CellSize + MinorGridLineSize));
            }
        }
    }
    
    private void RenderCellGrid(
        DrawingContext drawingContext, 
        Point start, 
        int cellWidth, 
        int cellHeight, 
        Pen minorPen,
        Pen majorPen,
        bool majorHorizontal,
        bool majorVerical)
    {
        var width = cellWidth * (CellSize + MinorGridLineSize) + MinorGridLineSize;  
        var height = cellHeight * (CellSize + MinorGridLineSize) + MinorGridLineSize;  
        
        for (var cellX = 0; cellX <= cellWidth; cellX++)
        {
            var x = start.X + cellX * (CellSize + MinorGridLineSize) + MinorGridLineSize / 2; 
            
            drawingContext.DrawLine(
                cellX % 5 == 0 || cellX == cellWidth && majorVerical ? majorPen : minorPen, 
                new Point(x, start.Y),
                new Point(x, start.Y + height));
        }
        
        for (var cellY = 0; cellY <= cellHeight; cellY++)
        {
            var y = start.Y + cellY * (CellSize + MinorGridLineSize) + MinorGridLineSize / 2; 
            
            drawingContext.DrawLine(
                cellY % 5 == 0 || cellY == cellHeight && majorHorizontal ? majorPen : minorPen, 
                new Point(start.X, y),
                new Point(start.X + width, y));
        }
    }

    private void RenderPuzzle(DrawingContext drawingContext)
    {
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize + MinorGridLineSize / 2.0,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize + MinorGridLineSize / 2.0);
        
        for (var cellX = 0; cellX < Puzzle.Width; cellX++)
        for (var cellY = 0; cellY < Puzzle.Height; cellY++)
        {
            var rect = new Rect(
                new Point(
                    start.X + (CellSize + MinorGridLineSize) * cellX,
                    start.Y + (CellSize + MinorGridLineSize) * cellY),
                new Size(
                    CellSize + MinorGridLineSize,
                    CellSize + MinorGridLineSize));

            var renderCell = Turn.Cells[cellX, cellY];
            var renderWrong = false;
            var renderHint = false;
            if (IsMouseCellSelection
                && cellX >= SelectionBoxMinX 
                && cellX < SelectionBoxMaxX
                && cellY >= SelectionBoxMinY 
                && cellY < SelectionBoxMaxY)
            {
                if (MouseSelectionMode == MouseSelectionMode.Set)
                    renderCell = Cell.Set;
                else if (MouseSelectionMode == MouseSelectionMode.Unset)
                    renderCell = Cell.Unset;
                else if (MouseSelectionMode == MouseSelectionMode.Unknown)
                    renderCell = Cell.Unknown;
            }
            else if (Turn.WrongCells[cellX, cellY])
                renderWrong = true;
            else if (Turn.HintCells[cellX, cellY])
            {
                renderHint = true;
                renderCell = Solution.Cells[cellX, cellY];
            }

            switch (renderCell)
            {
                case Cell.Set: 
                    drawingContext.DrawRectangle(
                        renderWrong ? SetLineWrongBrush 
                        : renderHint ? SetLineHintBrush 
                        : SetLineBrush, 
                        null, 
                        rect);
                    break;
                case Cell.Unset:
                    drawingContext.DrawLine(
                        renderWrong ? SetLineWrongPen
                        : renderHint ? SetLineHintPen
                        : SetLinePen, 
                        new Point(rect.X, rect.Y), 
                        new Point(rect.X + CellSize + MinorGridLineSize, rect.Y + CellSize + MinorGridLineSize));
                    drawingContext.DrawLine(
                        renderWrong ? SetLineWrongPen
                        : renderHint ? SetLineHintPen
                        : SetLinePen, 
                        new Point(rect.X + CellSize + MinorGridLineSize, rect.Y), 
                        new Point(rect.X, rect.Y + CellSize + MinorGridLineSize));
                    break;
                case Cell.Unknown: 
                    break;
            }
        }
    }

    private void RenderFinishedPuzzle(DrawingContext drawingContext)
    {
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize + MinorGridLineSize / 2.0,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize + MinorGridLineSize / 2.0);
        
        for (var cellX = 0; cellX < Puzzle.Width; cellX++)
        for (var cellY = 0; cellY < Puzzle.Height; cellY++)
        {
            var rect = new Rect(
                new Point(
                    start.X + (CellSize + MinorGridLineSize) * cellX,
                    start.Y + (CellSize + MinorGridLineSize) * cellY),
                new Size(
                    CellSize + MinorGridLineSize,
                    CellSize + MinorGridLineSize));

            var renderCell = Solution.Cells[cellX, cellY];

            switch (renderCell)
            {
                case Cell.Set: 
                    drawingContext.DrawRectangle(
                        SetLineBrush, 
                        null, 
                        rect);
                    break;
                case Cell.Unset:
                case Cell.Unknown: 
                    break;
            }
        }
    }

    private void RenderHorizonalLegend(DrawingContext drawingContext)
    {
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize + PuzzleCellRenderHeight + MajorGridLineSize);

        for (var cellX = 1; cellX < Puzzle.Width; cellX++)
        {
            if (cellX % 5 != 4 && cellX != Puzzle.Width - 1)
                continue;
            
            var x = start.X + (cellX + 1) * (CellSize + MinorGridLineSize) + MinorGridLineSize / 2; 
            
            var formattedText = new FormattedText(
                (cellX + 1).ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface,
                LegendFontSize,
                Brushes.Black,
                1.0)
            {
                TextAlignment = TextAlignment.Right
            };

            drawingContext.DrawText(
                formattedText, 
                new Point(x, start.Y));
        }
        
    }

    private void RenderVerticalLegend(DrawingContext drawingContext)
    {
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize + PuzzleCellRenderWidth + MajorGridLineSize + MajorGridLineSize,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize);

        for (var cellY = 1; cellY < Puzzle.Height; cellY++)
        {
            if (cellY % 5 != 4 && cellY != Puzzle.Height - 1)
                continue;
            
            var y = start.Y + cellY * (CellSize + MinorGridLineSize) + MinorGridLineSize / 2 + CellSize * 0.33; 
            
            var formattedText = new FormattedText(
                (cellY + 1).ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface,
                LegendFontSize,
                Brushes.Black,
                1.0)
            {
                TextAlignment = TextAlignment.Left
            };

            drawingContext.DrawText(
                formattedText, 
                new Point(start.X, y));
        }
    }

    private void RenderSelectionBox(DrawingContext drawingContext)
    {
        if (!IsMouseCellSelection)
            return;

        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize);
        var size = CellSize + MinorGridLineSize;
        
        drawingContext.DrawRectangle(
            null, 
            MouseSelectionMode != MouseSelectionMode.Measure ? SelectionBoxPen : SelectionBoxMeasurePen,
            new Rect(
                new Point(start.X + SelectionBoxMinX * size, start.Y + SelectionBoxMinY * size),
                new Point(start.X + SelectionBoxMaxX * size, start.Y + SelectionBoxMaxY * size)));
    }

    private void RenderSelectionLegend(DrawingContext drawingContext)
    {
        if (!IsMouseCellSelection)
            return;

        string legend;
        if (SelectionBoxWidth == 1)
            legend = SelectionBoxHeight.ToString();
        else if (SelectionBoxHeight == 1)
            legend = SelectionBoxWidth.ToString();
        else
            legend = $"{SelectionBoxWidth},{SelectionBoxHeight}";
        
        var size = CellSize + MinorGridLineSize;
        var start = new Point(
            MajorGridLineSize + PuzzleHeaderRenderWidth + MajorGridLineSize + SelectionLegendX * size - size * 0.75,
            MajorGridLineSize + PuzzleHeaderRenderHeight + MajorGridLineSize + SelectionLegendY * size - size * 0.75);
        
        var formattedText = new FormattedText(
            legend,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface,
            LegendFontSize,
            SelectionLegendBorderBrush,
            1.0)
        {
            TextAlignment = TextAlignment.Left
        };

        drawingContext.DrawRectangle(
            SelectionLegendBackgroundBrush, 
            SelectionLegendPen,
            new Rect(
                start,
                new Size(formattedText.Width + SelectionLegendMargin * 2, formattedText.Height * 1.2)));
        drawingContext.DrawText(
            formattedText, 
            new Point(start.X + SelectionLegendMargin,  start.Y + formattedText.Height * 0.1));
    }
    #endregion

    #region Events Handlers
    protected override void OnMouseMove(MouseEventArgs e)
    {
        BuildMousePositions(e);
        base.OnMouseMove(e);
        InvalidateVisual();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            && IsMouseInHorizontalLengths()
            && !Turn.IsFinished)
        {
            ApplyTurnChange(Turn.ApplyHorizontalLengthToggle(MouseCellY, MouseHorizontalIndex));
        }
        else if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            && IsMouseInVerticalLengths()
            && !Turn.IsFinished)
        {
            ApplyTurnChange(Turn.ApplyVerticalLengthToggle(MouseCellX, MouseVerticalIndex));
        }
        else if ((e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) 
            && IsMouseInCell()
            && (!Turn.IsFinished || e.MiddleButton == MouseButtonState.Pressed))
        {
            Keyboard.Focus(this);

            MouseCellSelectionStartRenderPosition = MouseRenderPosition;
            MouseCellSelectionStartCellX = MouseCellX;
            MouseCellSelectionStartCellY = MouseCellY;
            IsMouseCellSelection = true;
            if (e.MiddleButton == MouseButtonState.Pressed)
                MouseSelectionMode = MouseSelectionMode.Measure;
            else if (e.RightButton == MouseButtonState.Pressed)
                MouseSelectionMode 
                    = Turn.Cells[MouseCellX, MouseCellY] == Cell.Unset
                    ? MouseSelectionMode.Unknown
                    : MouseSelectionMode.Unset;
            else 
                MouseSelectionMode 
                    = Turn.Cells[MouseCellX, MouseCellY] == Cell.Set
                    ? MouseSelectionMode.Unknown
                    : MouseSelectionMode.Set;

            BuildMousePositions(e);
            CaptureMouse();
            InvalidateVisual();
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        BuildMousePositions(e);
        ReleaseMouseCapture();

        if (IsMouseCellSelection)
        {
            if (MouseSelectionMode != MouseSelectionMode.Measure)
            {
                var newTurn = Turn.ApplyChanges(
                    SelectionBoxMinX,
                    SelectionBoxMinY,
                    SelectionBoxWidth,
                    SelectionBoxHeight,
                    MouseSelectionMode == MouseSelectionMode.Set ? Cell.Set
                    : MouseSelectionMode == MouseSelectionMode.Unset ? Cell.Unset
                    : Cell.Unknown);
                
                ApplyTurnChange(newTurn);
            }
        }

        IsMouseCellSelection = false;
        InvalidateVisual();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        
        var isShiftPressed = e.Key == Key.LeftShift || e.Key == Key.RightShift;
        if (isShiftPressed != IsShiftPressed)
        {
            IsShiftPressed = isShiftPressed;
            BuildMousePositions();
            InvalidateVisual();
        }

        if (e.Key == Key.Escape && IsMouseCellSelection)
        {
            IsMouseCellSelection = false;
            BuildMousePositions();
            ReleaseMouseCapture();
            InvalidateVisual();
        }
    }

    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
        base.OnPreviewKeyUp(e);

        var isShiftPressed = e.Key == Key.LeftShift || e.Key == Key.RightShift;
        if (isShiftPressed && IsShiftPressed)
        {
            IsShiftPressed = false;
            BuildMousePositions();
            InvalidateVisual();
        }
    }
    #endregion
    
    public void SetSolutionTurn(Solution solution, Turn turn)
    {
        if (solution.Width != turn.Width || solution.Height != turn.Height)
            throw new ArgumentException();

        Solution = solution;
        Puzzle = solution.ToPuzzle();
        Turn = turn;

        PuzzleHeaderRenderWidth = 
            + Puzzle.HeaderWidth * (CellSize + MinorGridLineSize) 
            + MinorGridLineSize;
        PuzzleCellRenderWidth = 
            + Puzzle.Width * (CellSize + MinorGridLineSize)
            + MinorGridLineSize;
        PuzzleRenderWidth = 
            + MajorGridLineSize 
            + PuzzleHeaderRenderWidth
            + MajorGridLineSize
            + PuzzleCellRenderWidth
            + MajorGridLineSize
            + CellSize;
            
        PuzzleHeaderRenderHeight = 
            + Puzzle.HeaderHeight * (CellSize + MinorGridLineSize) 
            + MinorGridLineSize;
        PuzzleCellRenderHeight = 
            + Puzzle.Height * (CellSize + MinorGridLineSize)
            + MinorGridLineSize;
        PuzzleRenderHeight = 
            + MajorGridLineSize 
            + PuzzleHeaderRenderHeight
            + MajorGridLineSize
            + PuzzleCellRenderHeight
            + MajorGridLineSize
            + CellSize;
        
        InvalidateVisual();
    }
    
    public void GetTransforms(out TranslateTransform translateTransform, out ScaleTransform scaleTransform)
    {
        var actualWidth = ActualWidth;
        var actualHeight = ActualHeight;
        var actualRatio = actualWidth / actualHeight;
        var puzzleRenderWidth = PuzzleRenderWidth;
        var puzzleRenderHeight = PuzzleRenderHeight;
        var gridRatio = puzzleRenderWidth / puzzleRenderHeight;

        double transformX, transformY;
        double scaleXY;
        if (actualRatio > gridRatio)
        {
            scaleXY = actualHeight / puzzleRenderHeight;
            transformX = actualWidth / 2.0 - puzzleRenderWidth * scaleXY / 2.0;
            transformY = 0.0;
        }
        else 
        {
            scaleXY = actualWidth / puzzleRenderWidth;
            transformY = actualHeight / 2.0 - puzzleRenderHeight * scaleXY / 2.0;
            transformX = 0.0;
        }

        translateTransform = new TranslateTransform(transformX, transformY);
        scaleTransform = new ScaleTransform(scaleXY, scaleXY);
    }

    private void BuildMousePositions(MouseEventArgs? e = null)
    {
        GetTransforms(out var translateTransform, out var scaleTransform);

        if (e != null)
        {
            var position = e.GetPosition(this);
            MouseRenderPosition = new Point(
                (position.X - translateTransform.X) / scaleTransform.ScaleX,
                (position.Y - translateTransform.Y) / scaleTransform.ScaleY);

            MouseCellX = (int)Math.Floor(
                (MouseRenderPosition.X
                 - MajorGridLineSize
                 - PuzzleHeaderRenderWidth
                 - MajorGridLineSize
                 - MinorGridLineSize / 2.0)
                / (CellSize + MinorGridLineSize));
            MouseCellY = (int)Math.Floor(
                (MouseRenderPosition.Y
                 - MajorGridLineSize
                 - PuzzleHeaderRenderHeight
                 - MajorGridLineSize
                 - MinorGridLineSize / 2.0)
                / (CellSize + MinorGridLineSize));
            MouseHeaderX = (int)Math.Floor(
                (MouseRenderPosition.X
                 - MajorGridLineSize
                 - MinorGridLineSize / 2.0)
                / (CellSize + MinorGridLineSize));
            MouseHeaderY = (int)Math.Floor(
                (MouseRenderPosition.Y
                 - MajorGridLineSize
                 - MinorGridLineSize / 2.0)
                / (CellSize + MinorGridLineSize));

            MouseHorizontalIndex = -1;
            if (MouseCellY >= 0 && MouseCellY < Puzzle.Height)
            {
                var hli = Puzzle.HeaderWidth - MouseHeaderX - 1;
                var lengths = Puzzle.HorizontalLengths.ElementAt(MouseCellY);
                if (hli >= 0 && hli < lengths.Count)
                    MouseHorizontalIndex = lengths.Count - hli - 1;
            }

            MouseVerticalIndex = -1;
            if (MouseCellX >= 0 && MouseCellX < Puzzle.Width)
            {
                var vli = Puzzle.HeaderHeight - MouseHeaderY - 1;
                var lengths = Puzzle.VerticalLengths.ElementAt(MouseCellX);
                if (vli >= 0 && vli < lengths.Count)
                    MouseVerticalIndex = lengths.Count - vli - 1;
            }
        }

        var renderWidth = Math.Abs(MouseCellSelectionStartRenderPosition.X - MouseRenderPosition.X);
        var renderHeight = Math.Abs(MouseCellSelectionStartRenderPosition.Y - MouseRenderPosition.Y);

        if (IsMouseCellSelection)
        {
            SelectionBoxMinX = Math.Max(0, Math.Min(MouseCellX, MouseCellSelectionStartCellX));
            SelectionBoxMaxX = Math.Min(Puzzle.Width, Math.Max(MouseCellX, MouseCellSelectionStartCellX) + 1);
            SelectionBoxMinY = Math.Max(0, Math.Min(MouseCellY, MouseCellSelectionStartCellY));
            SelectionBoxMaxY = Math.Min(Puzzle.Height, Math.Max(MouseCellY, MouseCellSelectionStartCellY) + 1);

            SelectionLegendX = Math.Min(Puzzle.Width - 1, Math.Max(0, MouseCellX));
            SelectionLegendY = Math.Min(Puzzle.Height - 1, Math.Max(0, MouseCellY));

            if (!IsShiftPressed)
            {
                if (renderWidth > renderHeight)
                {
                    SelectionBoxMinY = MouseCellSelectionStartCellY;
                    SelectionBoxMaxY = MouseCellSelectionStartCellY + 1;
                }
                else
                {
                    SelectionBoxMinX = MouseCellSelectionStartCellX;
                    SelectionBoxMaxX = MouseCellSelectionStartCellX + 1;
                }

                if (renderWidth > renderHeight)
                    SelectionLegendY = MouseCellSelectionStartCellY;
                else
                    SelectionLegendX = MouseCellSelectionStartCellX;
            }

            SelectionBoxWidth = SelectionBoxMaxX - SelectionBoxMinX;
            SelectionBoxHeight = SelectionBoxMaxY - SelectionBoxMinY;
        }
        else
        {
            SelectionBoxMinX = -1;
            SelectionBoxMaxX = -1;
            SelectionBoxMinY = -1;
            SelectionBoxMaxY = -1;
            SelectionBoxWidth = 0;
            SelectionBoxHeight = 0;
            SelectionLegendX = -1;
            SelectionLegendY = -1;
        }
    }

    public void ApplyCheck()
    {
        var newTurn = Turn.ApplyCheck(Solution);
        ApplyTurnChange(newTurn);
    }

    public void ApplyHint()
    {
        var newTurn = Turn.ApplyHint(Solution);
        ApplyTurnChange(newTurn);
    }

    private void ApplyTurnChange(Turn? newTurn)
    {
        if (newTurn == null || Turn.Equals(newTurn))
            return;

        Turn = newTurn;
        InvalidateVisual();
        TurnChange?.Invoke(Turn);
    }
    
}
