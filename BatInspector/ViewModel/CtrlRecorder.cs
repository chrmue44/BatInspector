/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-11-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.CodeDom;
using System.Windows.Media;

namespace BatInspector
{
  public enum enTrigFilter
  {
    HIGHPASS = 0,
    BANDPASS = 1,
    LOWPASS = 2,
  }

  public enum enTrigType
  {
    LEVEL = 0,
    FREQ = 1,
    LEVEL_FREQ = 2,
  }

  public enum enGain
  {
    LOW = 0,
    HIGH = 1,
  }

  public enum enSampleRate
  {
    SR_19K = 0,
    SR_32K = 1,
    SR_38K = 2,
    SR_44K = 3,
    SR_48K = 4,
    SR_192K = 5,
    SR_384K = 6,
    SR_480K = 7,
  }

  public enum enRecMode
  {
    OFF = 0,
    ON = 1,
    TIME = 2,
    TWILIGHT = 3,
  }

  public class Trigger
  {
    const double TRIG_MIN = -24.0;
    const double TRIG_MAX = -1;
    const double FREQ_MIN = 5;
    const double FREQ_MAX = 70;
    const double LENGTH_MIN = 0.5;
    const double LENGTH_MAX = 5;

    double _level = -10;
    double _freq = 20;
    double _length = 1;
    enTrigFilter _filter = enTrigFilter.HIGHPASS;
    enTrigType _type = enTrigType.LEVEL_FREQ;

    public double Level { get { return _level; } }
    public double Frequency { get { return _freq; } }
    public double Length { get { return _length; } }

    public enTrigType Type { get { return _type; } set { _type = value; } }
    public enTrigFilter Filter { get { return _filter; } set { _filter = value; } }

    public bool setLevel(double level, out string errMsg)
    {
      errMsg = string.Empty;
      if ((level >= TRIG_MIN) && (level <= TRIG_MAX))
      {
        _level = level;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + TRIG_MIN.ToString() + " ... " + TRIG_MAX.ToString();
      return false;
    }

    public bool setFrequency(double freq, out string errMsg)
    {
      errMsg = string.Empty;
      if ((freq >= FREQ_MIN) && (freq <= FREQ_MAX))
      {
        _freq = freq;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + FREQ_MIN.ToString() + " ... " + FREQ_MAX.ToString(); ;
      return false;
    }

    public bool setLength(double length, out string errMsg)
    {
      errMsg = string.Empty;
      if ((length>= LENGTH_MIN) && (length <= LENGTH_MAX))
      {
        _length = length;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + LENGTH_MIN.ToString() + " ... " + LENGTH_MAX.ToString(); ;
      return false;
    }

  }

  public class Acquisition
  {
    const double PRE_TRIG_MIN = 0.0;
    const double PRE_TRIG_MAX = 70.0;

    enSampleRate _sampleRate = enSampleRate.SR_384K;
    enGain _gain = enGain.HIGH;
    double _preTrig = 70;
    

    public enSampleRate SampleRate { get { return _sampleRate; } set { _sampleRate = value; } } 
    public enGain Gain { get { return _gain; } set { _gain = value; } }
    public double PreTrigger {  get { return _preTrig; } }
    public Acquisition() { }
    public bool setPreTrigger(double length, out string errMsg)
    {
      errMsg = string.Empty;
      if ((length >= PRE_TRIG_MIN) && (length <= PRE_TRIG_MAX))
      {
        _preTrig = length;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + PRE_TRIG_MIN.ToString() + " ... " + PRE_TRIG_MAX.ToString(); ;
      return false;
    }
  }

  public class ControlRec
  {
    const int H_MIN = 0;
    const int H_MAX = 23;
    const int MIN_MIN = 0;
    const int MIN_MAX = 59;
    const double RECTIME_MIN = 1;
    const double RECTIME_MAX = 120;
    const double DEADTIME_MIN = 0;
    const double DEADTIME_MAX = 5;

    enRecMode _recMode = enRecMode.OFF;
    public enRecMode Mode { get { return _recMode; } set { _recMode = value; } }

    int _startH = 19;
    int _startMin = 0;
    int _stopH = 6;
    int _stopMin = 0;
    double _recTime = 3.0;
    double _deadTime = 1.0;

    public int StartH { get { return _startH; } }
    public int StartMin { get { return _startMin; } }
    public int StopH { get { return _stopH; } }
    public int StopMin { get { return _stopMin; } }

    public double RecTime { get { return _recTime; } }  
    public double DeadTime { get { return _deadTime; } }

    public ControlRec() { }

    public bool setStartH(int h, out string errMsg)
    {
      errMsg = string.Empty;
      if ((h >= H_MIN) && (h <= H_MAX))
      {
        _startH = h;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + H_MIN.ToString() + " ... " + H_MAX.ToString(); ;
      return false;
    }

    public bool setStopH(int h, out string errMsg)
    {
      errMsg = string.Empty;
      if ((h >= H_MIN) && (h <= H_MAX))
      {
        _stopH = h;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + H_MIN.ToString() + " ... " + H_MAX.ToString(); ;
      return false;
    }

    public bool setStartMin(int m, out string errMsg)
    {
      errMsg = string.Empty;
      if ((m >= MIN_MIN) && (m <= MIN_MAX))
      {
        _startMin = m;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + MIN_MIN.ToString() + " ... " + MIN_MAX.ToString(); ;
      return false;
    }

    public bool setStopMin(int m, out string errMsg)
    {
      errMsg = string.Empty;
      if ((m >= MIN_MIN) && (m <= MIN_MAX))
      {
        _stopMin = m;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + MIN_MIN.ToString() + " ... " + MIN_MAX.ToString(); ;
      return false;
    }

    public bool setRecTime(int t, out string errMsg)
    {
      errMsg = string.Empty;
      if ((t >= RECTIME_MIN) && (t <= RECTIME_MAX))
      {
        _recTime = t;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + RECTIME_MIN.ToString() + " ... " + RECTIME_MAX.ToString(); ;
      return false;
    }

    public bool setDeadTime(int t, out string errMsg)
    {
      errMsg = string.Empty;
      if ((t >= DEADTIME_MIN) && (t <= DEADTIME_MAX))
      {
        _deadTime = t;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + RECTIME_MIN.ToString() + " ... " + RECTIME_MAX.ToString(); ;
      return false;
    }


  }

  public class GeneralRec
  {
    const double LAT_MIN = -90;
    const double LAT_MAX = 90;
    const double LON_MIN = -180;
    const double LON_MAX = 180;
    double _lat = 49;
    double _lon = 8;

    public double Latitude { get { return _lat; } } 
    public double Longitude { get { return _lon;} }

    public bool setLatitude(double lat, out string errMsg)
    {
      errMsg = string.Empty;
      if ((lat >= LAT_MIN) && (lat <= LAT_MAX))
      {
        _lat = lat;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + LAT_MIN.ToString() + " ... " + LAT_MAX.ToString(); ;
      return false;
    }

    public bool setLongitude(double lon, out string errMsg)
    {
      errMsg = string.Empty;
      if ((lon >= LAT_MIN) && (lon <= LAT_MAX))
      {
        _lon = lon;
        return true;
      }
      else
        errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + LON_MIN.ToString() + " ... " + LON_MAX.ToString(); ;
      return false;
    }

  }

  public class CtrlRecorder
  {
    Trigger _trigger;
    Acquisition _acq;
    ControlRec _ctrl;
    GeneralRec _gen;

    public CtrlRecorder()
    {
      _trigger = new Trigger();
      _acq = new Acquisition();
      _ctrl = new ControlRec();
      _gen = new GeneralRec();
    }

    public Trigger Trigger { get { return _trigger; } }
    public Acquisition Acquisition { get { return _acq;} }
    public ControlRec Control { get { return _ctrl; } }
    public GeneralRec General { get { return _gen; } }
  }
}
