using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace BatInspector
{
  public class DocHelper
  {
    FlowDocument _doc = new FlowDocument();
    public FontFamily Font { get; set; } = new FontFamily("Arial");
    public int FontSizeText { get; set; } = 11;
    public int FontSizeH1 { get; set; } = 16;
    public int FontSizeH2 { get; set; } = 14;
    public int FontSizeH3 { get; set; } = 12;

    public void init()
    {
      _doc.Blocks.Clear();
      _doc.FontFamily = Font;
      _doc.FontSize = FontSizeText;
    }

    public FlowDocument Doc { get { return _doc; } }

    public Paragraph addParagraph()
    {
      Paragraph p = new Paragraph();
      _doc.Blocks.Add(p);
      return p;
    }

    public void addHeader(int order, string text)
    {
      Paragraph p = addParagraph();
      Run t = new Run(text);
      p.Inlines.Add(t);
      _doc.Blocks.Add(p);
      t.FontFamily = Font;
      switch (order)
      {
        case 1:
          t.FontSize = FontSizeH1;
          t.FontWeight = FontWeights.Bold;
          break;
        case 2:
          t.FontSize = FontSizeH2;
          t.FontWeight = FontWeights.Bold;
          break;
        default:
        case 3:
          t.FontSize = FontSizeH3;
          t.FontWeight = FontWeights.Bold;
          break;
      }
    }

    public void addText(string text, bool bold = false, bool underline = false, SolidColorBrush fgColor = null, SolidColorBrush bgColor = null)
    {
      Paragraph p = getLastParagraph();
      Run t = new Run(text);
      t.FontFamily = Font;
      t.FontSize = FontSizeText;
      if (fgColor != null ) 
        t.Foreground = fgColor;
      if (bgColor != null)
        t.Background = bgColor;
      if (bold)
        t.FontWeight = FontWeights.Bold;
      if(underline)
        t.TextDecorations = TextDecorations.Underline;
      p.Inlines.Add(t);
    }

    public void addHyperlink(string text, string url, RequestNavigateEventHandler reqHnadler)
    {
      Run t = new Run(text);
      t.FontFamily = Font;
      t.FontSize = FontSizeText;
      Hyperlink hyperlink = new Hyperlink(t);
      hyperlink.IsEnabled = true;
      if(reqHnadler != null) 
        hyperlink.RequestNavigate += reqHnadler;
      hyperlink.NavigateUri = new Uri(url);
      hyperlink.Foreground = Brushes.Blue;
      hyperlink.FontWeight = FontWeights.Bold;
      Paragraph p = getLastParagraph();
      p.Inlines.Add(hyperlink);
    }


    public void addTable(DataTable dtbl, int width = 1, int border = 0, bool ommitHeader = false)
    {
      Table table1 = new Table();
      table1.BorderThickness = new Thickness(border);
      table1.BorderBrush = Brushes.Black;
      table1.Background = Brushes.Black;
      _doc.Blocks.Add(table1);

      // Set some global formatting properties for the table.
      table1.CellSpacing = width;
      table1.Background = Brushes.White;

      //add columns
      int numberOfColumns = dtbl.Columns.Count;
      for (int x = 0; x < numberOfColumns; x++)
      {
        TableColumn c = new TableColumn();
        table1.Columns.Add(c);
      }

      // Create and add an empty TableRowGroup to hold the table's Rows.
      table1.RowGroups.Add(new TableRowGroup());

      // Add the header row.
      if (!ommitHeader)
      {
        table1.RowGroups[0].Rows.Add(new TableRow());
        TableRow currentRow = table1.RowGroups[0].Rows[0];
        currentRow.Background = Brushes.LightSteelBlue;

        // Global formatting for the header row.
        currentRow.FontSize = FontSizeH1;
        currentRow.FontWeight = FontWeights.Bold;
        currentRow.FontFamily = Font;

        for (int i = 0; i < dtbl.Columns.Count; i++)
        {
          TableCell cell = new TableCell(new Paragraph(new Run(dtbl.Columns[i].ColumnName)));
          cell.BorderBrush = Brushes.Black;
          cell.BorderThickness = new Thickness(border);
          currentRow.Cells.Add(cell);
        }
      }

      for (int r = 0; r < dtbl.Rows.Count; r++)
      {
        table1.RowGroups[0].Rows.Add(new TableRow());
        currentRow = table1.RowGroups[0].Rows[r + 1];
        currentRow.FontWeight = FontWeights.Normal;
        currentRow.Background = Brushes.Black;
        for (int c = 0; c < dtbl.Columns.Count; c++)
        {
          Run t = new Run(dtbl.Rows[r].ItemArray[c].ToString());
          TableCell cell = new TableCell(new Paragraph(t));
          cell.BorderBrush = Brushes.Gray;
          cell.BorderThickness = new Thickness(border);
          cell.Background = Brushes.White;
          currentRow.Cells.Add(cell);
        }
      }
    }


    public void addImage(string imagePath, int width)
    {
      try
      {
        // Check if the provided image path is valid
        if (string.IsNullOrEmpty(imagePath))
        {
          throw new ArgumentException("Image path cannot be null or empty");
        }
        // Create an Image element and set its source to the provided image path
        Image image = new Image();
        image.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
        image.Stretch = Stretch.Uniform;
        image.Width = width;
        image.HorizontalAlignment = HorizontalAlignment.Left;

        // Create a BlockUIContainer to host the Image element
        BlockUIContainer blockUIContainer = new BlockUIContainer(image);
        // Add the BlockUIContainer to the FlowDocument
        _doc.Blocks.Add(blockUIContainer);
      }
      catch (Exception ex)
      {
        // Log the error
        Console.WriteLine($"Error: {ex.Message}");
      }
    }

    public void saveAs(string docPath)
    {
      TextRange content = new TextRange(_doc.ContentStart, _doc.ContentEnd);

      if (content.CanSave(DataFormats.Rtf))
      {
        using (FileStream stream = new FileStream(docPath, FileMode.Create))
        {
          content.Save(stream, DataFormats.Rtf);
        }
      }
    }


    private Paragraph getLastParagraph()
    {
      Paragraph p = _doc.Blocks.LastBlock as Paragraph;
      if (p == null)
        p = addParagraph();      
      
      return p;
    }
  }
}
