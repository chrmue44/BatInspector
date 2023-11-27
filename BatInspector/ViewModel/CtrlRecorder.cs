/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-11-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/


namespace BatInspector
{

  public class Trigger
  {
    
    NumType _level = new NumType("Prh");
    NumType _freq = new NumType("Prf");
    NumType _length = new NumType("Prt");
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
    
    public Acquisition()
    {
    }

    public EnumType SampleRate { get { return _sampleRate; } } 
    public EnumType Gain { get { return _gain; } }
    public NumType PreTrigger {  get { return _preTrig; } }

    public void init()
    {
      _sampleRate.init();
      _gain.init();
      _preTrig.init();
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
    NumType _lat = new NumType("Pla");
    NumType _lon = new NumType("Plo");

    public NumType Latitude { get { return _lat; } } 
    public NumType Longitude { get { return _lon;} }

    public EnumType Language { get { return _language; } }
    public void init()
    {
      _language.init();
      _lat.init();
      _lon.init();
    }
  }

  public class RecStatus
  {
    NumType _battVoltage = new NumType("sv");
    NumType _audioBlocks = new NumType("sa");
    NumType _cpuAvg = new NumType("sg");
    NumType _cpuMax = new NumType("sx");
    NumType _mainLoop = new NumType("sl");

    public NumType BattVoltage { get { return _battVoltage; } }
    public NumType AudioBlocks { get { return _audioBlocks; } }
    public NumType CpuLoadAvg { get { return _cpuAvg; } }
    public NumType CpuLoadMax { get { return _cpuMax; } }
    public NumType MainLoop { get { return _mainLoop; } }
    public void init()
    {
      _battVoltage.init();
      _audioBlocks.init();
      _cpuMax.init();
      _cpuAvg.init();
      _mainLoop.init();
    }

  }

  public class CtrlRecorder
  {
    Trigger _trigger;
    Acquisition _acq;
    ControlRec _ctrl;
    GeneralRec _gen;
    BatSpy _device;
    RecStatus _status;
    bool _connected = false;
    public CtrlRecorder()
    {
      _device = new BatSpy();
      _trigger = new Trigger();
      _acq = new Acquisition();
      _ctrl = new ControlRec();
      _gen = new GeneralRec();
      _status = new RecStatus();
    }

    public bool connect()
    {
      _connected = BatSpy.connect();
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
