# Wxv.Nonograms

_Version 1.0.0_

Nonograms are picture logic puzzles which when solved reveal a hidden picture.  See also [Wikipedia](https://en.wikipedia.org/wiki/Nonogram).  This is a little windows application to play them.

![Screenshot](Images/Screenshot.png)

I built this because I was a little frustrated with the limitations of the otherwise excellent existing online web interfaces for this game.  It is heavily inspired by [www.nonograms.org](https://www.nonograms.org), but also has my own original contributions.  It's main extra features include:

- Lightweight UX, no web page or other extras to get in the way.
- Provides the dimension of the cells your drawing.
- Allows you to measure a dimension without drawing any cells.
- When you start drawing or measuring a row or column of cells, it won't draw outside that range (unless you press shift).  This prevents simple mistakes caused by clumsy hands. 
- Unlimited hints ( but i don't need that 😊 ).
- Undo and Redo.
- Remembers the puzzle you've worked on when you previously ran the application.

## Instructions

- `Left mouse button` click marks or un-marks cells as being set.  
- `Right mouse button` click marks or un-marks cells as being unset.
- `Middle mouse button` click measures the dimensions of a set of cells.
- Keep pressing the mouse button and move the mouse pointer to draw or measure a range of cells.
- Hold `Shift` while pressing the mouse button if you want to draw or measure a block (not a column or row) of cells.
- The `Del` key clears the drawing. 
- `Control+Z` undoes an operation.
- `Control+Y` redoes an operation.
- `Space` checks the picture for correctness or completeness.  Incorrect cells show as red.
- The `H` key provides a hint.  Hints show as green.  Press space to _complete_ the hint.  _Note:_ A nonogram puzzle must have a specific determinable possible solution in at least one row or column for hints to work. 
- `Control+V` will import a puzzle from the clipboard.  It can be one of the following formats:
  - A URL to a [www.nonograms.org](https://www.nonograms.org) black and white puzzle online e.g. https://www.nonograms.org/nonograms/i/1348 .
  - A URL to a [www.nonograms.org](https://www.nonograms.org) answer image online e.g. https://static.nonograms.org/files/nonograms/large/kotyonok3_12_1_1p.png .
  - A solution image.  This must be a simple two color image.  This allows you to make and test your own nonogram puzzles in an application like paintbrush and play or test them in this windows application.
  - _Note:_ Puzzles should be no smaller the 5 cells and no larger then 50 cells in width and height.

# Installation

- Prerequisites:
  - Windows 11 (10 should probably work too, as long as it supports .NET 7)
  - .NET 7 Desktop Runtime ( or later versions ), or any other .NET distribution that includes this e.g. the SDK.  https://dotnet.microsoft.com/en-us/download/dotnet/7.0
    - The is most likely already installed on your Windows computer. 
- Download the source and run the `Wxv.Nonograms.UX` project from the IDE or command line.  It needs .NET 7 SDK and/or an IDE that supports it.
- _or_ download the release [here](https://github.com/wverkley/Wxv.Nonograms/releases/tag/v1.0.0), unzip it somewhere, and run the `Wxv.Nonograms.UX.exe` application.  
- _Note:_ The application saves the puzzle you last worked on to the `~user\AppData\Local\Wxv.Nonograms.UX` folder.  Delete this folder manually if don't want it.  

## Notes

- This is just a personal hobby project.  No warranty provided.  There are probably lots of things to fix or improve.  Feel free to fork.

## Acknowledgements 

- Thanks to [www.nonograms.org](https://www.nonograms.org) which provide a really excellent web interface to play this game.
