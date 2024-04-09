/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;
using libScripter;
using System.IO;

namespace BatInspector
{
  public class StringType
  {
    public StringType(string cmd)
    {
      _cmd = cmd;
    }

    string _cmd;

    public string Value
    {
      get
      {
        string val;
        bool ok = BatSpy.getStringValue(_cmd, out val);
        return val;
      }
      set
      {
        BatSpy.setStringValue(value, _cmd);
      }
    }
    public void init()
    {
    }
  }

  public class NumType
  {
    public NumType(string cmd)
    {
      _cmd = cmd;
    }

    string _cmd;
    double _min;
    double _max;
    int _decimals;

    public double Value
    {
      get
      {
        double val;
        bool ok = BatSpy.getDoubleValue(_cmd, out val);
        return val;
      }
      set
      {
        if ((value >= _min) && (value <= _max))
          BatSpy.setDoubleValue(value, _cmd);
        else
        {
          string errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + " " + _min.ToString() + " ... " + _max.ToString();
          DebugLog.log(errMsg, enLogType.ERROR);
        }
      }
    }
    public int Decimals { get { return _decimals; } }
    public double Min { get { return _min; } }
    public double Max { get { return _max; } }
    
    public void init()
    {
      BatSpy.getDoubleRange(_cmd, out _min, out _max, out _decimals);
    }
  }

  public class EnumType
  {
    public EnumType(string cmd)
    {
      _cmd = cmd;
    }

    string _cmd;
    string[] _items;
    bool _initialized = false;

    public int Value
    {
      get
      {
        int val;
        bool ok = BatSpy.getEnumIndex(_cmd, out val);
        return val;
      }
      set
      {
        if ((value >= 0) && (value < _items.Length))
          BatSpy.setEnumIndex(value, _cmd);
        else
        {
          string errMsg = BatInspector.Properties.MyResources.CtrlRecorderErrMsgRange + "0 ... " + _items.Length.ToString();
          DebugLog.log(errMsg, enLogType.ERROR);
        }
      }
    }

    public string[] Items { get { return _items; } }

    public void init()
    {
      _initialized = BatSpy.getEnumValues(_cmd, out _items);
    }
  }

  /// <summary>
  /// class for singleton objext to communicate with a BatSpy device connected to the USB port
  /// </summary>
  public class BatSpy
  {
    public const char NEWLINE_CHAR =  '\x04';
    public const string NEWLINE_STR = "\x04";
    public const string DEV_IDENT = "BatSpy";
    static BatSpy _inst = new BatSpy();

    SerialPort _port = null;
    bool _answerReceived = false;
    bool _receiveError = false;
    bool _isConnected = false;

    static public bool IsConnected { get{ return _inst._isConnected; } }

    static public bool connect(out string version, out string serialNr)
    {
      string[] ports = SerialPort.GetPortNames();
      version = "?";
      serialNr = "?";
      if (!_inst._isConnected)
      {
        SerialPort p = null;
        foreach (string port in ports)
        {
          try
          {
            p= new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
            p.ReadBufferSize = 32768;
            p.Open();
            p.ReadTimeout = 300;
            p.NewLine = NEWLINE_STR;
            p.DataReceived += _inst._port_DataReceived;
            p.ErrorReceived += _inst._port_ErrorReceived;
            string s = p.ReadExisting();
            p.Write("v\n");
            s = p.ReadLine();
            if (s.IndexOf(DEV_IDENT) == 0)
            {
              _inst._port = p;
              _inst._isConnected = true;
              version = s;
              p.Write("z\n");
              s = p.ReadLine();
              serialNr = s;
              break;
            }
            else
              p.Close();
          }
          catch 
          {
            if ((p != null) && p.IsOpen)
              p.Close();
       
          }
        }
        if (_inst._port == null)
          DebugLog.log("failed to connect to BatSpy device", enLogType.ERROR);
      }
      return _inst._isConnected;
    }

    static public void disConncet()
    {
      if(_inst._isConnected)
      {
        _inst._port.Close();
        _inst._port = null;
        _inst._isConnected = false;
      }
    }


    static public bool save()
    {
      return _inst.writeCommand("A");
    }

    static public bool load()
    {
      return _inst.writeCommand("L");
    }

    static public string ExecuteCommand(string cmd)
    {
      return _inst.execCommand(cmd);
    }

    static public bool setSystemSettings(string passWd, string serial, double voltage)
    {
      bool retVal = false;
      if (_inst._isConnected)
      {
        string res = _inst.execCommand("Y" + passWd);
        if (res == "0")
        {
          res = _inst.execCommand("Z" + serial);
          if (res == "0")
          {
            res = _inst.execCommand("Pf" + voltage.ToString("#.###", CultureInfo.InvariantCulture));
            if (res == "0")
              retVal = true;
            _inst.execCommand("Y ");  //lock system
          }
        }
      }
      return retVal;
    }

    private void _port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
      _receiveError = true;
    }

    static public byte[] getLiveFft(int part)
    {
      byte[] buf = new byte[64 * 256];
      int iBuf = 0;
      int recLength = 0x4000;  //buf.Length

      _inst._port.Write("f" + part.ToString() +"\n");
      DebugLog.log("BatSpy CMD: f" + part.ToString(), enLogType.DEBUG);
      _inst._port.ReadTimeout = 10000;
      Stopwatch w = new Stopwatch();
      w.Start();

      while (!_inst._answerReceived && !_inst._receiveError && 
             (w.ElapsedMilliseconds < _inst._port.ReadTimeout))
        Thread.Yield();
      if (_inst._answerReceived && !_inst._receiveError)
      {
        while ((iBuf < recLength) && (w.ElapsedMilliseconds < _inst._port.ReadTimeout))
        {
          while ((_inst._port.BytesToRead == 0) && (w.ElapsedMilliseconds < _inst._port.ReadTimeout)) { }
          while ((_inst._port.BytesToRead > 0) && (iBuf < recLength))
          {
            buf[iBuf] = (byte)_inst._port.ReadByte();
            iBuf++;
          }
        }
      }
      if (iBuf < recLength)
        DebugLog.log("incomplete live FFT, part " + part.ToString(), enLogType.ERROR);
      return buf;
    }

    private bool writeCommand(string cmd)
    {
      string res = execCommand(cmd);
      return (res == "0");
    }

    private string execCommand(string cmd)
    {
      string retVal = "";
      try
      {
        string str = cmd + "\n";
        byte[] buf = new byte[4096];
        _answerReceived = false;
        _receiveError = false;
        _port.ReadExisting();
        _port.Write(str);
        DebugLog.log("BatSpy CMD: " + cmd, enLogType.DEBUG);
        _port.ReadTimeout = 1000;
        Stopwatch w = new Stopwatch();
        w.Start();
        while (!_answerReceived && !_receiveError && (w.ElapsedMilliseconds < _port.ReadTimeout))
          Thread.Yield();
        if (_answerReceived && !_receiveError)
        {
          int iBuf = 0;
          while ((_port.BytesToRead > 0) && (iBuf < buf.Length))
          {
            buf[iBuf] = (byte)_port.ReadByte();
            if (buf[iBuf] == NEWLINE_CHAR)
              break;
            iBuf++;
          }
          retVal = Encoding.Default.GetString(buf);
          int pos = retVal.IndexOf(NEWLINE_CHAR);
          if (pos >= 0)
            retVal = retVal.Substring(0, pos);
          DebugLog.log("BatSpy ANS: " + retVal, enLogType.DEBUG);
        }
        else
          DebugLog.log("BatSpy command " + cmd + " timed out", enLogType.ERROR);
      }
      catch(Exception ex)
      {
        DebugLog.log("connection lost: " + ex.ToString(), enLogType.ERROR);
      }
      return retVal;
    }

    private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      _answerReceived = true;
    }

    static public bool getEnumValues(string cmd, out string[] items)
    {
      bool retVal = false;
      items = new string[0];
      if(_inst._port != null)
      {
        string str = cmd.ToLower() + "r";
        string s = _inst.execCommand(str);
        if (s.IndexOf('\n') >= 0)
        {
          int pos = s.LastIndexOf('\n');
          s = s.Substring(0, pos);
          items = s.Split('\n');
          for(int i = 0; i < items.Length; i++)
          {
            pos = items[i].IndexOf(':');
            items[i] = items[i].Substring(pos + 1);
            byte[] bytes = Encoding.Default.GetBytes(items[i]);
            items[i] = Encoding.UTF8.GetString(bytes);
          }
          retVal = true;
        }
      }
      if (!retVal)
        DebugLog.log("error reading enum items, cmd: " + cmd, enLogType.ERROR);
      return retVal;
    }

    static public bool getDoubleValue(string cmd, out double val)
    {
      string str = cmd.ToLower();
      string res = _inst.execCommand(str);
      bool ok = double.TryParse(res, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
      if (!ok)
      {
        DebugLog.log("error reading numeric value from BatSpy, cmd: " + cmd, enLogType.ERROR);
        val = 0;
      }
      return ok;
    }

    static public bool getStringValue(string cmd, out string val)
    {
      string str = cmd.ToLower();
      string res = _inst.execCommand(str);

      bool ok = (res != "?");
      val = res;
      return ok;
    }

    static public bool getEnumIndex(string cmd, out int val)
    {
      string str = cmd.ToLower();
      string res = _inst.execCommand(str);
      bool ok = int.TryParse(res, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
      if (!ok)
      {
        DebugLog.log("error reading enumeration index from BatSpy, cmd: " + cmd, enLogType.ERROR);
        val = 0;
      }
      return ok;
    }

    static public void setDoubleValue(double val, string cmd)
    {
      string str = cmd + val.ToString("0.####", CultureInfo.InvariantCulture);
      bool ok = _inst.writeCommand(str);
      if (!ok)
        DebugLog.log("error writing value, cmd: " + str, enLogType.ERROR);
    }

    static public void setStringValue(string val, string cmd)
    {
      string str = cmd + val;
      bool ok = _inst.writeCommand(str);
      if (!ok)
        DebugLog.log("error writing value, cmd: " + str, enLogType.ERROR);
    }

    static public void setEnumIndex(int val, string cmd)
    {
      string str = cmd + val.ToString();
      bool ok = _inst.writeCommand(str);
      if (!ok)
        DebugLog.log("error writing value, cmd: " + str, enLogType.ERROR);

    }

    static public void getDoubleRange(string cmd, out double min, out double max, out int decimals)
    {
      string str = cmd.ToLower() + "r";
      string res = _inst.execCommand(str);
      string[] tokens = res.Split(',');
      bool ok = true;
      if(tokens.Length == 4)
      {
        int pos = tokens[0].IndexOf(':');
        ok = double.TryParse(tokens[0].Substring(pos + 1), NumberStyles.Any, CultureInfo.InvariantCulture, out min);
        pos = tokens[1].IndexOf(':');
        ok &= double.TryParse(tokens[1].Substring(pos + 1), NumberStyles.Any, CultureInfo.InvariantCulture, out max);
        pos = tokens[3].IndexOf(':');
        ok &= int.TryParse(tokens[3].Substring(pos + 1), NumberStyles.Any, CultureInfo.InvariantCulture, out decimals);
      }
      else
      {
        min = 0;
        max = 0;
        decimals = 0;
      }
      if (!ok)
        DebugLog.log("error reading range, cmd: " + cmd + ", result: " + res, enLogType.ERROR);
    }


    static public void uploadFirmware(string file)
    {
      _inst._port.BaudRate = 134;
      Thread.Sleep(2000);
      ProcessRunner proc = new ProcessRunner();
      string exe = Path.Combine(AppParams.Inst.AppRootPath, "teensy_loader_cli.exe");
      string args = "--mcu=TEENSY40 -v " + file;
      // first attempt fails always
      proc.launchCommandLineApp(exe, null, AppParams.Inst.AppRootPath, true, args, false);
      Thread.Sleep(2000);
      // 2nd time it works... (don't know why)
      proc.launchCommandLineApp(exe, null, AppParams.Inst.AppRootPath, true, args, false);
    }
  }
}
