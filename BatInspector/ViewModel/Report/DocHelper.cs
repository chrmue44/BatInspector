using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ZstdSharp.Unsafe;

namespace BatInspector
{

  public enum enDocType
  { 
    RTF,
    HTML
  }
  public abstract class DocHelper
  {
    public enDocType DocType { get;}
    public int FontSizeText { get; set; } = 11;
    public int FontSizeH1 { get; set; } = 16;
    public int FontSizeH2 { get; set; } = 14;
    public int FontSizeH3 { get; set; } = 12;

    public DocHelper(enDocType docType)
    {
      DocType = docType;
    }

    public abstract void begin();
    public abstract void end();
    public abstract void addHeader(int order, string text);

    public abstract void addImage(string image, string altText, int sizeH);
    public abstract void addHyperlink(string text, string url, RequestNavigateEventHandler reqHnadler);
    public abstract void addText(string text, bool bold = false, bool underline = false, SolidColorBrush fgColor = null, SolidColorBrush bgColor = null);
    public abstract void addTable(DataTable dtbl, System.Collections.Generic.List<double> colWidth, int width = 1, int border = 0, bool ommitHeader = false);
    public abstract void saveAs(string docPath);

    public static DocHelper create(enDocType type)
    {
      switch(type)
      {
        case enDocType.RTF:
          return new DocHelperRtf() as DocHelper;
        default:
        case enDocType.HTML:
          return new DocHelperHtml() as DocHelper;
      }
    }
  }

  public class DocHelperRtf : DocHelper
  {
    FlowDocument _doc = new FlowDocument();

    public FontFamily Font { get; set; } = new FontFamily("Arial");

    public DocHelperRtf() : base(enDocType.RTF) 
    {
    }

    public FlowDocument Doc { get{ return _doc; }  }  

    public override void begin()
    {
      _doc.Blocks.Clear();
      _doc.FontFamily = Font;
      _doc.FontSize = FontSizeText;
    }

    public override void end()
    {
      
    }
    public override void addImage(string image, string altText, int sizeH)
    {

    }

    //  public FlowDocument Doc { get { return _doc; } }

    public Paragraph addParagraph()
    {
      Paragraph p = new Paragraph();
      _doc.Blocks.Add(p);
      return p;
    }

    public override void addHeader(int order, string text)
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

    public override void addText(string text, bool bold = false, bool underline = false, SolidColorBrush fgColor = null, SolidColorBrush bgColor = null)
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

    public override void addHyperlink(string text, string url, RequestNavigateEventHandler reqHnadler)
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


    public override void addTable(DataTable dtbl, System.Collections.Generic.List<double> colWidth, int width = 1, int border = 0, bool ommitHeader = false)
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
        if(colWidth.Count < x)
          c.Width = new GridLength(colWidth[x]);
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
        TableRow currentRow = table1.RowGroups[0].Rows[r];
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

    public override void saveAs(string docPath)
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


  public class DocHelperHtml : DocHelper
  {
    StringBuilder _doc;
    public DocHelperHtml() : base (enDocType.HTML)
    {
      _doc = new StringBuilder();
    }

    public override void begin()
    {
      _doc.Append("<!DOCTYPE html>\n<html>\n  <head>\n<meta charset=\"utf-8\">\n");
      _doc.Append("</head>\n");
      _doc.Append("<body>\n");
    }

    public override void end()
    {
      _doc.Append("</body>\n</html>\n");
    }

    public override void addHeader(int order, string text)
    {
      _doc.Append($"<h{order}>{text}</h{order}>\n");
    }

    public override void addHyperlink(string text, string url, RequestNavigateEventHandler reqHnadler)
    {
      _doc.Append($"<a href=\"{url}\">{text}</a>\n");
    }

    public override void addTable(DataTable dtbl, List<double> colWidth, int width = 1, int border = 0, bool ommitHeader = false)
    {
      _doc.Append("<table style=\"width:100%\">\n<tr>\n");
      for (int i= 0; i < dtbl.Columns.Count; i++)
        _doc.Append($"<th>{dtbl.Columns[i].ColumnName}</th>\n");
      _doc.Append("</tr>\n");

      for (int r = 0; r < dtbl.Rows.Count; r++)
      {
        _doc.Append("<tr>\n");
        for (int c = 0; c < dtbl.Columns.Count; c++)
          _doc.Append($"<td>{dtbl.Rows[r].ItemArray[c].ToString()}</td>\n");
        _doc.Append("</tr>\n");
      }
      _doc.Append("</table>\n");
    }

    public override void addText(string text, bool bold = false, bool underline = false, SolidColorBrush fgColor = null, SolidColorBrush bgColor = null)
    {
      _doc.Append($"<p>{text}</p>\n");
    }
    public override void addImage(string image, string altText, int sizeH)
    {
      _doc.Append($"<p><img src=\"{image}\" height=\"{sizeH}\" alt=\"{altText}\"></p>\n");
    }

    public override void saveAs(string docPath)
    {
       File.WriteAllText(docPath, _doc.ToString()); 
    }
  }
}
