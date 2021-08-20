using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für FrmZoom.xaml
  /// </summary>
  public partial class FrmZoom : Window
  {
    AnalysisFile _analysis;
    public FrmZoom(string name, AnalysisFile analysis)
    {
      InitializeComponent();
      this.Title = name;
      _analysis = analysis;
      ContentRendered += FrmZoom_ContentRendered;
    }

    private void FrmZoom_ContentRendered(object sender, EventArgs e)
    {
      initRulerY();
      initRulerX();
    }

    void initRulerY()
    {
      createLine(_rulerY, _rulerY.ActualWidth - 3, _img.Margin.Top,
                          _rulerY.ActualWidth - 3, _img.ActualHeight + _img.Margin.Top, Brushes.Black);
      for(int i = 0; i <= 10; i++)
      {
        double y = _img.Margin.Top +_img.ActualHeight * i / 10;
        createLine(_rulerY, _rulerY.ActualWidth - 3, y,
                            _rulerY.ActualWidth - 10, y, Brushes.Black);
        string str = (_analysis.SampleRate * (10 - i) / 10000).ToString();
        createText(_rulerY, _rulerY.ActualWidth - 35, y - 5, str, Colors.Black);
      }
    }

    void initRulerX()
    {
      createLine(_rulerX, 0, 3, _rulerX.ActualWidth, 3, Brushes.Black);
      for (int i = 0; i <= 10; i++)
      {
        createLine(_rulerX, _rulerX.ActualWidth * i / 10, 3, _rulerX.ActualWidth * i / 10, 10, Brushes.Black);
        string str = (_analysis.Duration * (10 - i) / 10000).ToString();
        createText(_rulerX, _rulerX.ActualWidth * i / 10 - 20, 15, str, Colors.Black);
      }
    }


    private void createLine(Canvas ca, double x1, double y1, double x2, double y2, Brush brush, int thickness = 1)
    {
      Line li = new Line();
      li.X1 = x1;
      li.X2 = x2;
      li.Y1 = y1;
      li.Y2 = y2;
      li.Stroke = brush;
      li.StrokeThickness = thickness;
      ca.Children.Add(li);
    }

    private void createText(Canvas can, double x, double y, string text, Color color)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = text;
      textBlock.Foreground = new SolidColorBrush(color);
      textBlock.TextAlignment = TextAlignment.Right;
      Canvas.SetLeft(textBlock, x);
      Canvas.SetTop(textBlock, y);
      can.Children.Add(textBlock);
    }
  }
}
