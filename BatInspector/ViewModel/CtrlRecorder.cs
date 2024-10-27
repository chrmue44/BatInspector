/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-11-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/


using System;
using System.Windows.Media.Imaging;

namespace BatInspector
{

  public class Trigger
  {
    
    NumType _level = new NumType("Prh");
    NumType _freq = new NumType("Prf");
    NumType _length = new NumType("Prm");
    EnumType _filter = new EnumType("Pry");
    EnumType _type = new EnumType("Prr");

    public NumType Level { get { return _level; } }
    public NumType Frequency { get { return _freq; } }
    public NumType EventLength { get { return _length; } }
    public EnumType Type { get { return _type; } }
    public EnumType Filter { get { return _filter; }  }

    public void init()
    {
      _level.init();
      _freq.init();
      _length.init();
      _filter.init();
      _type.init();
    }
  }

  public class Acquisition
  {

    EnumType _sampleRate = new EnumType("Prs");
    EnumType _gain = new EnumType("Prg");
    NumType _preTrig = new NumType("Prp");
    NumType _recFilter = new NumType("Pru");
    EnumType _recFiltType = new EnumType("Prv");

    public Acquisition()
    {
    }

    public EnumType SampleRate { get { return _sampleRate; } } 
    public EnumType Gain { get { return _gain; } }
    public NumType PreTrigger {  get { return _preTrig; } }

    public NumType RecordingFilter { get { return _recFilter; } }
    public EnumType RecFiltType { get { return _recFiltType; } }

    public void init()
    {
      _sampleRate.init();
      _gain.init();
      _preTrig.init();
      _recFilter.init();
      _recFiltType.init();
    }
  }

  public class ControlRec
  {
    EnumType _recMode = new EnumType("Pao");
    NumType _startH = new NumType("Pah");
    NumType _startMin = new NumType("Pam");
    NumType _stopH = new NumType("Pai");
    NumType _stopMin = new NumType("Pan");
    NumType _recTime = new NumType("Prt");
    NumType _deadTime = new NumType("Prd");


    public EnumType Mode { get { return _recMode; } }
    public NumType StartH { get { return _startH; } }
    public NumType StartMin { get { return _startMin; } }
    public NumType StopH { get { return _stopH; } }
    public NumType StopMin { get { return _stopMin; } }

    public NumType RecTime { get { return _recTime; } }  
    public NumType DeadTime { get { return _deadTime; } }

    public ControlRec() { }

    public void init()
    {
      _recMode.init();
      _startH.init();
      _startMin.init();
      _stopH.init();
      _stopMin.init();
      _recTime.init();
      _deadTime.init();
    }
  }

  public class GeneralRec
  {
    EnumType _language = new EnumType("Pn");
    NumType _backLightTime = new NumType("Pb");
    EnumType _displayMode = new EnumType("Pd");
    EnumType _posMode = new EnumType("Pp");
    NumType _lat = new NumType("Pla");
    NumType _lon = new NumType("Plo");

    public NumType Latitude { get { return _lat; } } 
    public NumType Longitude { get { return _lon;} }

    public EnumType Language { get { return _language; } }

    public NumType BackLightTime { get { return _backLightTime; } }

    public EnumType DisplayMode {  get { return _displayMode; } }

    public EnumType PositionMode{ get { return _posMode; } }
    public void init()
    {
      _language.init();
      _backLightTime.init();
      _displayMode.init();
      _lat.init();
      _lon.init();
      _posMode.init();
    }
  }

  public class RecStatus
  {
    NumType _temp = new NumType("sb");
    NumType _humid = new NumType("sc");
    NumType _battVoltage = new NumType("sv");
    NumType _chargeLevel = new NumType("si");
    NumType _audioBlocks = new NumType("sa");
    NumType _cpuAvg = new NumType("sg");
    NumType _cpuMax = new NumType("sx");
    NumType _mainLoop = new NumType("sm");
    NumType _diskSpace = new NumType("sf");
    NumType _recCount = new NumType("sr");
    StringType _date = new StringType("Sd");
    StringType _time = new StringType("St");
    NumType _nrSatellites = new NumType("ss");
    EnumType _recStatus = new EnumType("se");
    StringType _location = new StringType("sl");
    NumType _height = new NumType("sh");
    EnumType _gps = new EnumType("sp");
    ColorTable _colorTable;

    public RecStatus(ColorTable colTable)
    {
      _colorTable = colTable;
    }

    public NumType Temperature { get { return _temp; } }
    public NumType Humidity { get { return _humid; } }
    public NumType BattVoltage { get { return _battVoltage; } }
    public NumType ChargeLevel { get { return _chargeLevel; } }
    public NumType AudioBlocks { get { return _audioBlocks; } }
    public NumType CpuLoadAvg { get { return _cpuAvg; } }
    public NumType CpuLoadMax { get { return _cpuMax; } }
    public NumType MainLoop { get { return _mainLoop; } }
    public NumType DiskSpace { get { return _diskSpace;} }
    public NumType RecCount { get { return _recCount; } }
    public StringType Date { get { return _date; } }
    public StringType Time { get { return _time; } }  
    public NumType NrSatellites { get { return _nrSatellites; } }
    public EnumType RecordingStatus { get { return _recStatus; } }
    public StringType Location { get { return _location; } }
    public NumType Height { get { return _height; } }

    public EnumType Gps { get{ return _gps; } }
    public BitmapImage getLiveFft()
    {
      
      
      int height = 128;
      int width = 256;
      byte[] fft = new byte[height * width];
      byte[] buf = BatSpy.getLiveFft(0);
      Array.Copy(buf, fft, buf.Length);
      buf = BatSpy.getLiveFft(1);
      Array.Copy(buf, 0, fft, buf.Length, buf.Length);
      BitmapFast bmp = new BitmapFast(width, height);
      for(int w = 0; w < width; w++)
      {
        for(int h = 0; h< height; h++) 
        {
          double val = fft[w * height+ h];
          System.Drawing.Color col = _colorTable.getColor(val, 0, 255);
          bmp.setPixel(w, height - 1 - h, col);
        }
      }
      BitmapImage img = PrjView.Convert(bmp.Bmp);
      return img;
    }

    public void init()
    {
      //     _battVoltage.init();
      //     _audioBlocks.init();
      //     _cpuMax.init();
      //     _cpuAvg.init();
      //     _mainLoop.init();
      //     _diskSpace.init();
      //      _date.init();
      //      _time.init();
      //      _nrSatellites.init();
      //      _temp.init();
      //      _humid.init();
      _recStatus.init();
      _gps.init();
    }

    public void setTime()
    {
      DateTime t = DateTime.Now;
      string tStr = t.ToString("HH:mm:ss");
      _time.Value = tStr;
      string dStr = t.ToString("dd.MM.yyyy");
      _date.Value = dStr;
    }
  }

  public class CtrlRecorder
  {
    Trigger _trigger;
    Acquisition _acq;
    ControlRec _ctrl;
    GeneralRec _gen;
    RecStatus _status;
    bool _connected = false;
    public CtrlRecorder(ColorTable colTable)
    {
      _trigger = new Trigger();
      _acq = new Acquisition();
      _ctrl = new ControlRec();
      _gen = new GeneralRec();
      _status = new RecStatus(colTable);
    }

    public bool connect(out string version, out string serialNr)
    {
      _connected = BatSpy.connect(out version, out serialNr);
      if (_connected)
        init();
      return _connected;
    }

    public void disConnect()
    {
      BatSpy.disConncet();
      _connected = false;
    }

    public bool savePars()
    {
      return BatSpy.save();
    }

    public bool loadPars()
    {
      return BatSpy.load();
    }

    public bool IsConnected { get { return _connected; } }
    public Trigger Trigger { get { return _trigger; } }
    public Acquisition Acquisition { get { return _acq;} }
    public ControlRec Control { get { return _ctrl; } }
    public GeneralRec General { get { return _gen; } }
    public RecStatus Status { get { return _status; } }
    public void init()
    {
      _trigger.init();
      _acq.init();  
      _ctrl.init();
      _gen.init();
      _status.init(); 
    }
  }
}
