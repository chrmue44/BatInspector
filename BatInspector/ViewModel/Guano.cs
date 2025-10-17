using libParser;
using System;
using System.Collections.Generic;
using System.Linq;


/*
 * https://github.com/riggsd/guano-py
 */


namespace BatInspector
{
  enum enGuanoToken
  {
    NAME = 0,
    DIVIDER = 1,
    EOL = 3,
    EOF = 4,
  }

  class Guano
  {
    string _buf;
    string _name;
    int _pos;
    string[] _dictionary = new string[]
    {
      "GUANO|Version",
      "Make",
      "Model",
      "Firmware Version",
      "Timestamp",
      "Loc Position",
      "Loc Source",
      "Original Filename",
      "Samplerate",
      "User|GAIN",
      "User|Trigger TYPE",
      "User|Trigger EVENTLEN",
      "User|Trigger FREQ",
      "User|Trigger FILTTYPE",
      "User|AMP",
      "Temperature",
      "Humidity"
    };

    List<ModelParItem> _list = new List<ModelParItem>();

    char getChar()
    {
      char retVal = '\0';
      if (_pos < _buf.Length)
      {
        retVal = _buf[_pos];
        _pos++;
      }
      return retVal;
    }


    public void parse(string s)
    {
      ModelParItem par;
      _buf = s;
      _pos = 0;
      enGuanoToken tok;
      bool err = true;
      _list.Clear();
      do
      {
        err = true;
        par = new ModelParItem();
        tok = getToken();
        if (tok == enGuanoToken.NAME)
        {
          par.Name = "";
          do
          {
            if (par.Name != "")
              par.Name += " ";
            par.Name = _name;
            tok = getToken();
          } while (tok == enGuanoToken.NAME);
          if (tok == enGuanoToken.DIVIDER)
          {
            tok = getToken();
            if (tok == enGuanoToken.NAME)
            {
              par.Value = _name;
              tok = getToken();
              if (tok == enGuanoToken.EOL)
              {
                if (_dictionary.Contains(par.Name))
                  _list.Add(par);
                err = false;
              }
            }
          }
        }
      }
      while ((tok != enGuanoToken.EOF) && !err);
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
          break;
        case '\n':
          retVal = enGuanoToken.EOL;
          break;
        default:
          _name = "";
          while (Utils.isalnum(c) || (c == '|'))
          {
            _name += c;
            c = getChar();
          }
          retVal = enGuanoToken.NAME;
          break;
      }
      return retVal;
    }
  }
}
