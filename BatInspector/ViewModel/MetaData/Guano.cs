/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2025-11-01                                       
 *   Copyright (C) 2025: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using libParser;
using NAudio.Utils;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows.Markup;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;


/*
 * https://github.com/riggsd/guano-py
 * https://www.wildlifeacoustics.com/SCHEMA/GUANO.html
 * 
 */


namespace BatInspector
{
  enum enGuanoToken
  {
    NAME = 0,
    DIVIDER = 1,
    NUMBER = 2,
    SPECIAL = 3,
    EOL = 4,
    EOF = 5,
    JSON_STR = 6
  }

  public class GuanoItem
  {
    public string NameSpace { get; set; }
    public string FieldName { get; set; }
    public string Value { get; set; }
    public string Comment { get; set; }
  
    public GuanoItem()
    {
      NameSpace = "";
      FieldName = "";
      Value = "";
      Comment = "";
    }

    public GuanoItem(string nameSpace, string fieldName, string value)
    {
      NameSpace = nameSpace;
      FieldName = fieldName;
      Value = value;

      Comment = "";
    }

    public override string ToString()
    {
      if(string.IsNullOrEmpty(NameSpace))       
        return $"{FieldName}:{Value}\n";
      else
        return $"{NameSpace}|{FieldName}:{Value}\n";
    }
  }


  class GuanoDictItem
  {
    public string Name { get; set; }
    public AnyType.tType Type {get; set; }

    public GuanoDictItem(string name, AnyType.tType type)
    {
      Name = name;
      Type = type;
    }
  }


  public class Guano
  {
    byte[] _buf;
    string _name = "";
    int _pos = 0;
    byte[] _data;

    List<GuanoItem> _items = new List<GuanoItem>();

    public string ChunkId { get; private set; }
    public UInt32 ChunkSize { get; private set; }

    public List<GuanoItem> Fields { get { return _items; } }

    static GuanoDictItem[] _dictionary = new GuanoDictItem[]
    {
      new GuanoDictItem("GUANO|Version",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Filter HP",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Filter LP",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Firmware Version", AnyType.tType.RT_STR),
      new GuanoDictItem("Hardware Version",AnyType.tType.RT_STR),
      new GuanoDictItem("Humidity",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Length",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Loc Position",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("OAD|Loc Source",AnyType.tType.RT_STR),
      new GuanoDictItem("Loc Accuracy",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Loc Elevation",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Make",AnyType.tType.RT_STR),
      new GuanoDictItem("Model",AnyType.tType.RT_STR),
      new GuanoDictItem("Original Filename",AnyType.tType.RT_STR),
      new GuanoDictItem("Samplerate",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|GAIN",AnyType.tType.RT_STR),
      new GuanoDictItem("BatSpy|Trigger AMP",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|Trigger TYPE",AnyType.tType.RT_STR),
      new GuanoDictItem("BatSpy|Trigger EVENTLEN",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|Trigger FREQ",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|Trigger FILTTYPE",AnyType.tType.RT_STR),
      new GuanoDictItem("BatSpy|AMP",AnyType.tType.RT_STR),
      new GuanoDictItem("Serial",AnyType.tType.RT_STR),
      new GuanoDictItem("TE",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Temperature Ext",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Temperature Int",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Timestamp",AnyType.tType.RT_TIME),
    };

    public Guano()
    {
      ChunkId = "guan";
      ChunkSize = 8;
      _data = new byte[8];
    }

    public Guano(byte[] data)
    {
      ChunkId = System.Text.Encoding.ASCII.GetString(data, 0, 4);
      ChunkSize = BitConverter.ToUInt32(data, 4);
      #if DEBUG
      string guanoStr = System.Text.Encoding.ASCII.GetString(data, 8, data.Length - 8);
      #endif
      _data = WavFile.partArray(data, 8, data.Length - 8);
      parse(_data);
    }

    public byte[] GetBytes()
    {
      ChunkSize = (UInt32)_data.Length;
      List<byte> bytes = new List<byte>();
      bytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
      bytes.AddRange(BitConverter.GetBytes(ChunkSize));
      foreach (byte b in _data)
        bytes.Add(b);
      return bytes.ToArray();
    }

    public UInt32 Length()
    {
      return (UInt32)_data.Length + 8;
    }

    public GuanoItem getField(string fieldName, string nameSpace = "")
    {
      GuanoItem retVal = null;
      foreach(GuanoItem field in _items)
      {
        if ((field.NameSpace == nameSpace) && (field.FieldName == fieldName))
        {
          retVal = field;
          break;
        }
      }
      return retVal;
    }

    public BatRecord getMetaData()
    {
      BatRecord retVal = new BatRecord();
      retVal.Samplerate = getFieldAsString("Samplerate");
      retVal.Temparature = getFieldAsString("Temperature Ext");
      retVal.GPS.Position = getFieldAsString("Loc Position");
      retVal.DateTime = getFieldAsString("Timestamp");
      return retVal;
    }


    private double getFieldAsDouble(string fieldName, string nameSpace = "")
    {
      double retVal = 0;
      bool ok = false;
      GuanoItem it = getField(fieldName, nameSpace);
      if(it != null)
        ok = double.TryParse(it.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out retVal);
      if (!ok)
        DebugLog.log($"could not read Guano field {fieldName} as double", enLogType.ERROR);
      return retVal;
    }

    private string getFieldAsString(string fieldName, string nameSpace = "")
    {
      string retVal = "";
      GuanoItem it = getField(fieldName, nameSpace);
      if (it != null)
        retVal = it.Value;
      else
        DebugLog.log($"could not read Guano field {fieldName}", enLogType.ERROR);
      return retVal;
    }

    private void writeDataToChunk()
    {
      List<byte> data = new List<byte>();
      foreach(GuanoItem it in _items)
      {
        string str = it.ToString();
        foreach(char c in str)
        {
          data.Add((byte)c);
        }
      }
      _data = data.ToArray();
    }

    private void parse(byte[] s)
    {
      GuanoItem par;
      _buf = s;
      _pos = 0;
      enGuanoToken tok;
      bool err = true;
      string str = System.Text.Encoding.UTF8.GetString(s,0, s.Length);
      _items.Clear();
      do
      {
        par = new GuanoItem();
        tok = getToken();
        if (tok == enGuanoToken.NAME)
        {
          par.FieldName = "";
          do
          {
            if (par.FieldName != "")
              par.FieldName += " ";
            par.FieldName += _name;
            tok = getToken();
          } while (tok == enGuanoToken.NAME);
          GuanoDictItem dictEntry = getDictEntry(par.FieldName);

          bool pushBack = false;
          if (dictEntry != null )
          {
            par.Comment = "";
            if (tok == enGuanoToken.DIVIDER)
            {
              tok = getToken();
              switch(dictEntry.Type)
              {

                case AnyType.tType.RT_FLOAT:
                  if (par.FieldName == "Loc Position")
                  {
                    par.Value = _name;
                    tok = getToken();
                    par.Value += " " + _name;
                    pushBack = true;
                  }
                  else if (tok == enGuanoToken.NUMBER)
                  {
                    par.Value = _name;
                    pushBack = true;
                  }
                  break;

                case AnyType.tType.RT_TIME:
                  if (tok == enGuanoToken.NUMBER)
                  {
                    par.Value = _name;
                    tok = getToken();
                    while (tok != enGuanoToken.EOL)
                    {
                      par.Value += _name;
                      tok = getToken();
                    }
                    pushBack = true;
                  }
                  break;
                case AnyType.tType.RT_STR:
                default:
                  if (tok == enGuanoToken.NAME)
                  {
                    par.Value = _name;
                    tok = getToken();
                    while (tok != enGuanoToken.EOL)
                    {
                      if (tok == enGuanoToken.NAME)
                        par.Value += " ";
                      par.Value +=_name;
                      tok = getToken();
                    }
                    pushBack = true;
                  }
                  break;
              }
            }
          }
          else
          {
            tok = getToken();
            par.Comment = "unknown FieldName";
            while ((tok != enGuanoToken.EOL) && (tok != enGuanoToken.EOF))
            {
              par.Value += _name;
              tok = getToken();
            }
            pushBack = true;
          }

          if (pushBack)
          {
            string[] toks = par.FieldName.Split('|');
            if (toks.Length > 1)
            {
              par.NameSpace = toks[0];
              par.FieldName = toks[1];
              for(int i = 2; i < toks.Length; i++)
                par.FieldName += $"|{toks[i]}";

            }
            else
              par.NameSpace = "";
            _items.Add(par);
          }
          while ((tok != enGuanoToken.EOL) && (tok != enGuanoToken.EOF))
            tok = getToken();
        }
      }
      while (tok != enGuanoToken.EOF);
    }

    public void copyMetaData(WavFile wav)
    {
      _data = new byte[wav.Guano._data.Length];
      Array.Copy(wav.Guano._data, _data, wav.Guano._data.Length);
      ChunkId = wav.Guano.ChunkId;
      ChunkSize = wav.Guano.ChunkSize;
    }

    public void createMetaData(BatRecord rec, int timeExpFactor = 1)
    {
      GuanoItem item = getItem("GUANO", "Version");
      item.Value = "1.0";
      item = getItem("", "Timestamp");
      item.Value = rec.DateTime;
      item = getItem("", "Firmware Version");
      item.Value = rec.Firmware;
      item = getItem("", "Serial");
      item.Value = rec.SN;
      item = getItem("", "Length");
      item.Value = rec.Duration;
      item = getItem("", "TE");
      item.Value = $"{timeExpFactor}";
      item = getItem("", "Loc Position");
      item.Value = rec.GPS.Position;
      item = getItem("", "Samplerate");
      item.Value = rec.Samplerate;
      if (!string.IsNullOrEmpty(rec.Temparature))
      {
        item = getItem("", "Temperature Ext");
        item.Value = rec.Temparature;
      }
      if (!string.IsNullOrEmpty(rec.Humidity))
      {
        item = getItem("", "Humidity");
        item.Value = rec.Humidity;
      }
      if (!string.IsNullOrEmpty(rec.Trigger.TriggerType))
      {
        item = getItem("BatSpy", "Trigger TYPE");
        item.Value = rec.Trigger.TriggerType;
      }
      if (!string.IsNullOrEmpty(rec.Trigger.Frequency))
      {
        item = getItem("BatSpy", "Trigger FREQ");
        item.Value = rec.Trigger.Frequency;
      }
      if (!string.IsNullOrEmpty(rec.Trigger.EventLength))
      {
        item = getItem("BatSpy", "Trigger EVENTLEN");
        item.Value = rec.Trigger.EventLength;
      }
      if (!string.IsNullOrEmpty(rec.Trigger.Level))
      {
        item = getItem("BatSpy", "Trigger AMP");
        item.Value = rec.Trigger.Level;
      }
      writeDataToChunk();
    }


    private GuanoItem getItem(string nameSpace, string fieldName)
    {
      foreach (GuanoItem item in _items)
      {
        if((item.NameSpace == nameSpace) && (item.FieldName == fieldName))
          return item;
      }
      GuanoItem retVal = new GuanoItem(nameSpace, fieldName, "");
      _items.Add(retVal);
      return retVal;
    }

    char getChar()
    {
      char retVal = '\0';
      if (_pos < _buf.Length)
      {
        if (((_buf[_pos] > 0) && (_buf[_pos] < 4)) || ((_buf[_pos] > 4) && (_buf[_pos] < 9)))
          retVal = '_';
        else
          retVal = (char)_buf[_pos];
        _pos++;
      }
      return retVal;
    }

    void putBack()
    {
      if (_pos > 0)
        _pos--;
    }

    GuanoDictItem getDictEntry(string key)
    {
      foreach (GuanoDictItem d in _dictionary)
      {
        if (d.Name == key)
          return d;
      }
      return null;
    }
    enGuanoToken getToken()
    {
      enGuanoToken retVal = enGuanoToken.EOF;
      char c;
      do
      {
        c = getChar();
      } while (Utils.isWhiteSpace(c));
      switch (c)
      {
        case '\0':
          retVal = enGuanoToken.EOF;
          break;
        case ':':
          retVal = enGuanoToken.DIVIDER;
          _name = ":";
          break;
        case '[':
          {
            int braceCnt = 1;

            _name = "" + c;
            while ((c != ']') && (braceCnt != 0))
            {
              c = getChar();
              if (c == '[')
                braceCnt++;
              if(c == ']')
                braceCnt--;
              _name += c;
            }
            retVal = enGuanoToken.JSON_STR;
          }
          break;        
        case '\n':
          retVal = enGuanoToken.EOL;
          break;
        default:
          _name = "";
          if (Utils.isdigit(c) || (c == '-'))
          {
            _name += c;
            c = getChar();
            while (Utils.isdigit(c) || (c == '.'))
            {
              _name += c;
              c = getChar();
            }
            if(c != 0)
              putBack();
            retVal = enGuanoToken.NUMBER;
          }
          else if("-+(),.{}[]".IndexOf(c) >= 0)
          {
            _name += c;
            retVal = enGuanoToken.SPECIAL;
          }
          else
          {
            while (Utils.isalpha(c) || (c == '|') || (c == '.') || (c == '/') || (c == '_'))
            {
              _name += c;
              c = getChar();
            }
            if(c != 0)
              putBack();
            retVal = enGuanoToken.NAME;
          }
          break;
      }
      return retVal;
    }
  }
}
