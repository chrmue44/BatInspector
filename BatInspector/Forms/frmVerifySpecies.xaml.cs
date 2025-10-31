using BatInspector.Controls;
using BatInspector.Properties;
using libParser;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;

namespace BatInspector.Forms
{
  enum enVerifyState
  {
    SET_FSTART = 0,
    SET_FEND = 1,
    SET_FMK = 2,
    CLEAR = 3
  }

  /// <summary>
  /// Interaktionslogik für frmVerifySpecies.xaml
  /// </summary>
  public partial class frmVerifySpecies : Window
  {
    Sonogram _sonogram;
    double _tMin, _tMax;
    double _fMin, _fMax;
    enVerifyState _state = enVerifyState.SET_FSTART;
    double _tStart;
    double _lat, _lon;
    double _tEend;
    double _tMk;

    public frmVerifySpecies()
    {
      InitializeComponent();
    }

    private void updateImage()
    {
      _sonogram.createZoomViewFt(_tMin,_tMax, _fMin, _fMax);
      if (_sonogram.ImageFt != null)
        _img.Source = _sonogram.ImageFt;
    }

    public void setup(AnalysisCall analysisCall, Sonogram sono, double tMin, double tMax, double fMin, double fMax)
    {
      int lw = 260;

      _sonogram = sono;
      _tMin  = tMin;
      _tMax = tMax;
      _fMin = fMin;
      _fMax = fMax;
      _ctlFstart.setup(MyResources.frmVerify_Fstart, Controls.enDataType.DOUBLE, 1, lw, true);
      _ctlFend.setup(MyResources.frmVerfy_fEnd, Controls.enDataType.DOUBLE, 1, lw, true);
      _ctlFMk.setup(MyResources.frmVerify_fMk, Controls.enDataType.DOUBLE, 1, lw, true);
      _ctlFChar.setup(MyResources.frmVerify_fChar, Controls.enDataType.DOUBLE, 1, lw, true);
      _ctlCallInterval.setup(MyResources.ctlStatCallInterval, enDataType.DOUBLE, 1, lw, true);
      _ctlDuration.setup($"{MyResources.ctlStatDuration} [ms]", enDataType.DOUBLE, 1, lw, true);

      int qw = 90;
      int qlw = 230;
    
      string[] qItems = new string[] { MyResources.frmVerifySpecies_setup_Yes, MyResources.frmVerifySpecies_setup_No, MyResources.DonTCare };
      _ctlHasMyotisKink.setup(MyResources.frmVerifySpecies_CallHasMyotisKink, 0, qlw, qw);
      _ctlHasMyotisKink.setItems(qItems);
      _lblMyoK.Content = "Myotis";
      _ctlHasMyotisKink.SelectIndex = 2;
      _ctlHasStrongHarmonics.setup(MyResources.frmVerify_CallHarmonics, 1, qlw, qw);
      _ctlHasStrongHarmonics.setItems(qItems);
      _ctlHasStrongHarmonics.SelectIndex = 2;
      _lblHarmonic.Content = "Plecotus";
      _ctlHasCallTypeAB.setup(MyResources.frmVerify_CallAB, 2, qlw, qw);
      _ctlHasCallTypeAB.setItems(qItems);
      _ctlHasCallTypeAB.SelectIndex = 2;
      _lblCallAB.Content = "BBAR";
      _ctlHasKneeClearly.setup(MyResources.frmVerifyCallHasKnee,3 , qlw, qw);
      _ctlHasKneeClearly.setItems(qItems);
      _ctlHasKneeClearly.SelectIndex = 2;
      _lblKnee.Content = "Myotis";
      _ctlIsUniForm.setup(MyResources.frmVerify_CallsAreUniform, 4, qlw, qw);
      _ctlIsUniForm.setItems(qItems);
      _ctlIsUniForm.SelectIndex = 2;
      _lblUniform.Content = "ESER/Plecotus";
      _ctlHasUpwardHookAtEnd.setup(MyResources.frmVerifySpecies_HookAtEnd, 5, qlw, qw);
      _ctlHasUpwardHookAtEnd.setItems(qItems);
      _ctlHasUpwardHookAtEnd.SelectIndex = 2;
      _lblHook.Content = "ESER";
      _ctlHasNoCallChanges.setup(MyResources.frmVerify_CallChanges, 6, qlw, qw);
      _ctlHasNoCallChanges.setItems(qItems);
      _ctlHasNoCallChanges.SelectIndex = 2;
      _lblNoChange.Content = "ENIL";
      _ctlIsConvex.setup(MyResources.frmVerifySpecies_Convex, 7, qlw, qw);
      _ctlIsConvex.setItems(qItems);
      _ctlIsConvex.SelectIndex = 2;
      _lblConvex.Content = "MMYO";

      _lblClick.Content = MyResources.frmVerifySpecies_MsgClickFstart;
      _ctlCallType.setup(BatInspector.Properties.MyResources.frmVerifySpecies_CallCharcteristic, 0, lw, 70);
      string[] items = new string[] { "CF", "FM", "FM-QCF", "FM-QCF-FM", "QCF", "QCF-FM" };
      _ctlCallType.setItems(items);

      _cbVerbose.IsChecked = true;
      _cbNotDeterminable.IsChecked = false;
      _cbLocalNames.IsChecked = true;

      if (analysisCall != null)
      {
        _tStart = analysisCall.getDouble(Cols.START_TIME);
        _lat = analysisCall.getDouble(Cols.LAT);
        _lon = analysisCall.getDouble(Cols.LON);

        _ctlFstart.setValue(analysisCall.getDouble(Cols.F_MAX) / 1000);
        _ctlFend.setValue(analysisCall.getDouble(Cols.F_MIN) / 1000);
        _ctlFChar.setValue(analysisCall.getDouble(Cols.F_MAX_AMP) / 1000);
        if (_ctlFChar.getDoubleValue() < 0.01)
          _ctlFChar.setValue(_ctlFend.getDoubleValue());
        _ctlDuration.setValue(analysisCall.getDouble(Cols.DURATION));
        _ctlCallInterval.setValue(analysisCall.getDouble(Cols.CALL_INTERVALL));

        double fStart = _ctlFstart.getDoubleValue();
        double fEnd = _ctlFend.getDoubleValue();
        double dur = _ctlDuration.getDoubleValue();
        if ((fStart - fEnd) / dur  < 0.1)
          _ctlCallType.SelectIndex = 0;
        else if ((fStart - fEnd) / dur < 1.0)
          _ctlCallType.SelectIndex = 4;
        else
          _ctlCallType.SelectIndex = 1;
      }

      App.Model.ZoomView.Waterfall.generateFtDiagram(tMin, tMax, AppParams.Inst.WaterfallWidth);
      updateImage();
    }

    private void _btnCheck_Click(object sender, RoutedEventArgs e)
    {
      CallData cd = new CallData();
      if (_ctlFstart.getDoubleValue() > 0)
        cd.FreqStart = _ctlFstart.getDoubleValue();
      if(_ctlFend.getDoubleValue() > 0)
       cd.FreqEnd = _ctlFend.getDoubleValue();
      if(_ctlFChar.getDoubleValue() > 0)
        cd.FreqChar = _ctlFChar.getDoubleValue();
      if(_ctlFMk.getDoubleValue() > 0)
        cd.FreqMk = _ctlFMk.getDoubleValue();
      if (_ctlCallInterval.getDoubleValue() > 0)
        cd.CallInterval = _ctlCallInterval.getDoubleValue();
      if(_ctlDuration.getDoubleValue() > 0)
        cd.Duration = _ctlDuration.getDoubleValue();

      cd.CallCharacteristic = (enCallChar)_ctlCallType.SelectIndex;
      cd.HasCallTypeAandB = (enYesNoProperty)_ctlHasCallTypeAB.SelectIndex;
      cd.HasKneeClearly = (enYesNoProperty)_ctlHasKneeClearly.SelectIndex;
      cd.HasMyotisKink = (enYesNoProperty)_ctlHasMyotisKink.SelectIndex;
      cd.HasNoCallChanges = (enYesNoProperty)_ctlHasNoCallChanges.SelectIndex;
      cd.HasUpwardHookAtEnd = (enYesNoProperty)_ctlHasUpwardHookAtEnd.SelectIndex;
      cd.HasStrongHarmonic = (enYesNoProperty)_ctlHasStrongHarmonics.SelectIndex;
      cd.IsConvex = (enYesNoProperty) _ctlIsConvex.SelectIndex;
      cd.IsUniformFormFreqInt = (enYesNoProperty)_ctlIsUniForm.SelectIndex;

      _tbResult.Document = BatInfo.checkBatSpecies(cd, App.Model.SpeciesInfos, _lat, _lon,
                                           _cbVerbose.IsChecked == true, 
                                           _cbNotDeterminable.IsChecked == true,
                                           _cbLocalNames.IsChecked == true,
                                           HandleRequestNavigate
                                           );
    }

    private void HandleRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        string navigateUri = e.Uri.AbsolutePath.Replace("%23","#");
        string filePath = navigateUri.Substring(0, navigateUri.IndexOf('#'));
        string pageNumber = navigateUri.Substring(navigateUri.IndexOf('#') + 1);
        string arguments = $"/A \"{pageNumber}\" \"{filePath}\"";
        
        try
        {
          Process.Start(AppParams.Inst.ExeAcrobat, arguments);
        }
        catch (System.ComponentModel.Win32Exception)
        {
          // If the Adobe Reader process fails, try another viewer or just open the file.
          DebugLog.log("Could not find AcroRd32.exe. Attempting to use default handler.", enLogType.WARNING);
          try
          {
            Process.Start(filePath);
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Error: {ex.Message}");
          }
        }
      e.Handled = true;
    }

    private void _img_MouseLeave(object sender, MouseEventArgs e)
    {
      _ftToolTip.IsOpen = false;
    }

    private void _img_MouseMove(object sender, MouseEventArgs e)
    {
      try
      {
        Point p = e.GetPosition(_img);
        double f = _fMin + (_img.ActualHeight - p.Y) / _img.ActualHeight * (_fMax - _fMin);
        double t = _tMin + p.X / _img.ActualWidth * (_tMax - _tMin);

        if (!_ftToolTip.IsOpen)
          _ftToolTip.IsOpen = true;

        _tbf.Text = f.ToString("#.#", CultureInfo.InvariantCulture) + "[kHz]/" +
        t.ToString("#.###" + "[s]", CultureInfo.InvariantCulture);
        _ftToolTip.HorizontalOffset = p.X + 20;
        _ftToolTip.VerticalOffset = p.Y + 20;
        DebugLog.log("ZoomBtn: image Ft mouse move", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("frmVerifySpecies: image Ft mouse move failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void setCrossPosition(System.Windows.Shapes.Line lx, System.Windows.Shapes.Line ly, Point p)
    {
      int w = 14;
      lx.X1 = p.X;
      lx.Y1 = p.Y;
      lx.X2 = p.X + w;
      lx.Y2 = p.Y;
      ly.X1 = p.X + w / 2;
      ly.Y1 = p.Y - w / 2;
      ly.X2 = p.X + w / 2;
      ly.Y2 = p.Y + w / 2;
    }

    private void setCross(Point p)
    {
      int w = 14;
      switch (_state)
      {
        case enVerifyState.SET_FSTART:
          setCrossPosition(_cross1X, _cross1Y, p);
          _cross1X.Visibility = Visibility.Visible;
          _cross1Y.Visibility = Visibility.Visible;
          break;
        case enVerifyState.SET_FEND:
          setCrossPosition(_cross2X, _cross2Y, p);
          _cross2X.Visibility = Visibility.Visible;
          _cross2Y.Visibility = Visibility.Visible;
          break;
        case enVerifyState.SET_FMK:
          setCrossPosition(_cross3X, _cross3Y, p);
          _cross3X.Visibility = Visibility.Visible;
          _cross3Y.Visibility = Visibility.Visible;
          break;
        case enVerifyState.CLEAR:
          _cross1X.Visibility = Visibility.Hidden;
          _cross1Y.Visibility = Visibility.Hidden;
          _cross2X.Visibility = Visibility.Hidden;
          _cross2Y.Visibility = Visibility.Hidden;
          _cross3X.Visibility = Visibility.Hidden;
          _cross3Y.Visibility = Visibility.Hidden;
          _ctlFMk.setValue(0.0);
          break;
      }
    }

    double getXFromt(double t)
    {
      return (t - _tMin) / (_tMax - _tMin) * _img.ActualWidth;
    }

    double getYFromF(double f)
    {
      return _img.ActualHeight - (f - _fMin) / (_fMax - _fMin) * _img.ActualHeight;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      Point p = new Point(getXFromt(_tStart), getYFromF(_ctlFstart.getDoubleValue()));
      setCrossPosition(_cross1X, _cross1Y, p);
      p = new Point(getXFromt(_tEend), getYFromF(_ctlFend.getDoubleValue()));
      setCrossPosition(_cross2X, _cross2Y, p);
      p = new Point(getXFromt(_tMk), getYFromF(_ctlFMk.getDoubleValue()));
      setCrossPosition(_cross3X, _cross3Y, p);
    }

    private void _tbResult_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      Hyperlink hyperlink = (Hyperlink)sender;
      Process.Start(hyperlink.NavigateUri.ToString());
    }

    private void _img_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        Point p = e.GetPosition(_img);
        double f = _fMin + (_img.ActualHeight - p.Y) / _img.ActualHeight * (_fMax - _fMin);
        double t = _tMin + p.X / _img.ActualWidth * (_tMax - _tMin);
        setCross(p);
        switch (_state)
        {
          case enVerifyState.SET_FSTART:
            _tStart = t;
            _ctlFstart.setValue(f);
            _state = enVerifyState.SET_FEND;
            _lblClick.Content = MyResources.frmVerifySpecies_MsgClickFend;
            break;

          case enVerifyState.SET_FEND:
            _ctlFend.setValue(f);
            _tEend = t;
            double duration = (t - _tStart) * 1000.0;
            _ctlDuration.setValue(duration);
            _state = enVerifyState.SET_FMK;
            _lblClick.Content = MyResources.frmVerifySpecies_MsgClickFmk;
            break;

          case enVerifyState.SET_FMK:
            _ctlFMk.setValue(f);
            _tMk = t;
            _state = enVerifyState.CLEAR;
            _lblClick.Content = MyResources.frmVerifySpecies_MsgClickClear;
            break;

          case enVerifyState.CLEAR:
            _state = enVerifyState.SET_FSTART;
            _lblClick.Content = MyResources.frmVerifySpecies_MsgClickFstart;
            break;
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("frmVerifySpecies: image Ft mouse move failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnIncRange_Click(object sender, RoutedEventArgs e)
    {
      App.Model.ZoomView.Waterfall.Range += 3.0;
      updateImage();
    }

    private void _btnDecRange_Click(object sender, RoutedEventArgs e)
    {
      if (App.Model.ZoomView.Waterfall.Range > 3)
        App.Model.ZoomView.Waterfall.Range -= 3.0;
      updateImage();

    }
  }
}
 