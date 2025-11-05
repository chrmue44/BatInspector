using libParser;
using System.Collections.Generic;


/*
 * https://github.com/riggsd/guano-py
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
  }

  public class GuanoItem
  {
    public string NameSpace { get; set; }
    public string FieldName { get; set; }
    public string Value { get; set; }
    public string Comment { get; set; }
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

  class Guano
  {
    byte[] _buf;
    string _name = "";
    int _pos = 0;
    List<GuanoItem> _items = new List<GuanoItem>();
    public List<GuanoItem> Fields { get { return _items; } }

    GuanoDictItem[] _dictionary = new GuanoDictItem[]
    {
      new GuanoDictItem("GUANO|Version",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Filter HP",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Filter LP",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Firmware Version", AnyType.tType.RT_STR),
      new GuanoDictItem("Hardware Version",AnyType.tType.RT_STR),
      new GuanoDictItem("Humidity",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Length",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Loc Position",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Loc Source",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Loc Accuracy",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Loc Elevation",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Make",AnyType.tType.RT_STR),
      new GuanoDictItem("Model",AnyType.tType.RT_STR),
      new GuanoDictItem("Original Filename",AnyType.tType.RT_STR),
      new GuanoDictItem("Samplerate",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|GAIN",AnyType.tType.RT_STR),
      new GuanoDictItem("BatSpy|Trigger TYPE",AnyType.tType.RT_STR),
      new GuanoDictItem("BatSpy|Trigger EVENTLEN",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|Trigger FREQ",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("BatSpy|Trigger FILTTYPE",AnyType.tType.RT_STR),
      new GuanoDictItem("BatSpy|AMP",AnyType.tType.RT_STR),
      new GuanoDictItem("TE",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Temperature Ext",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Temperature Int",AnyType.tType.RT_FLOAT),
      new GuanoDictItem("Timestamp",AnyType.tType.RT_TIME),
    };


    public void parse(byte[] s)
    {
      GuanoItem par;
      _buf = s;
      _pos = 0;
      enGuanoToken tok;
      bool err = true;
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
            par.FieldName = _name;
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
            while (tok != enGuanoToken.EOL)
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
            }
            else
              par.NameSpace = "General";
            _items.Add(par);
          }
          while ((tok != enGuanoToken.EOL) && (tok != enGuanoToken.EOF))
            tok = getToken();
        }
      }
      while (tok != enGuanoToken.EOF);
    }


    char getChar()
    {
      char retVal = '\0';
      if (_pos < _buf.Length)
      {
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
        case '\n':
          retVal = enGuanoToken.EOL;
          break;
        default:
          _name = "";
          if (Utils.isdigit(c))
          {
            while (Utils.isdigit(c) || (c == '.'))
            {
              _name += c;
              c = getChar();
            }
            putBack();
            retVal = enGuanoToken.NUMBER;
          }
          else if((c == '-') || (c=='+') || (c=='(') || (c==')'))
          {
            _name += c;
            retVal = enGuanoToken.SPECIAL;
          }
          else
          {
            while (Utils.isalpha(c) || (c == '|') || (c == '.'))
            {
              _name += c;
              c = getChar();
            }
            putBack();
            retVal = enGuanoToken.NAME;
          }
          break;
      }
      return retVal;
    }
  }
}
