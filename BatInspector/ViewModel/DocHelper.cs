using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BatInspector
{
  public class DocHelper
  {
    FlowDocument _doc = new FlowDocument();

    public void init()
    {
      _doc.Blocks.Clear();
    }

    public FlowDocument Doc { get { return _doc; } }

    public void addText(string text, bool bold = false, bool underline = false, SolidColorBrush fgColor = null, SolidColorBrush bgColor = null)
    {
      Paragraph p = getLastParagraph();
      Run run = new Run(text);
      if (fgColor != null ) 
        run.Foreground = fgColor;
      if (bgColor != null)
        run.Background = bgColor;
      if (bold)
        run.FontWeight = FontWeights.Bold;
      if(underline)
        run.TextDecorations = TextDecorations.Underline;
      p.Inlines.Add(run);
    }

    public void addHyperlink(string text, string url, RequestNavigateEventHandler reqHnadler)
    {
      Run run = new Run(text);
      Hyperlink hyperlink = new Hyperlink(run);
      hyperlink.IsEnabled = true;
      if(reqHnadler != null) 
        hyperlink.RequestNavigate += reqHnadler;
      hyperlink.NavigateUri = new Uri(url);
      hyperlink.Foreground = Brushes.Blue;
      hyperlink.FontWeight = FontWeights.Bold;
      Paragraph p = getLastParagraph();
      p.Inlines.Add(hyperlink);
    }

    private Paragraph getLastParagraph()
    {
      Paragraph p = _doc.Blocks.LastBlock as Paragraph;
      if (p == null)
      {
        p = new Paragraph();
        _doc.Blocks.Add(p);
      }
      return p;
    }
  }
}
