using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  class Csv
  {
    List<List<string>> _cells;
    string _fileName;
    char _separator = ';';

    public int RowCnt {  get { return _cells.Count; } }

    public Csv()
    {
      _cells = new List<List<string>>();
    }

    public int read(string file, char separator = ';')
    {
      int retVal = 0;
      _separator = separator;
      if (File.Exists(file))
      {
        _fileName = file;
        string[] lines = File.ReadAllLines(file);
        _cells.Clear();
        foreach(string line in lines)
        {
          string[] s = line.Split(separator);
          List<string> row = s.ToList();
          _cells.Add(row);
        }
      }
      else
        retVal = 1;
      return retVal;
    }

    public int save(bool withBackup = true)
    {
      int retVal = 0;
      if(withBackup)
        File.Copy(_fileName, _fileName + ".bak");
      
      string[] lines = new string[_cells.Count];
      string sep = _separator.ToString();
      for (int rowNr = 0;  rowNr < lines.Length; rowNr++)
        lines[rowNr] = String.Join(sep, _cells[rowNr]);

      File.WriteAllLines(_fileName, lines);
      return retVal;
    }

    public string getCell(int row, int col)
    {
      string retVal = "";
      row--;
      if((row >= 0) && (row < _cells.Count))
      {
        col--;
        if ((col >= 0) && (col < _cells[row].Count))
        {
          retVal = _cells[row][col];
        }
        else
          log("getCell(): column " + col.ToString() + " does not exist");
      }
      else
        log("getCell():row " + row.ToString() + " does not exist");
      return retVal;
    }

    public double getCellAsDouble(int row, int col)
    {
      string str = getCell(row, col);
      double retVal = 0;
      try
      {
        retVal = double.Parse(str, CultureInfo.InvariantCulture);
      }
      catch
      {
        log("value: " + str + " in row " + row.ToString() + ", col " + col.ToString() + " is not a well formed double");
      }
      return retVal;
    }

    public void setCell(int row, int col, string value)
    {
      row--;
      if (row >= 0)
      {
        if(row >= _cells.Count)
        {
          _cells.Capacity = row + 1;
          while (_cells.Count < (row + 1))
            _cells.Add(new List<string>());
        }
        if(col >= 0)
        {
          if(col >= _cells[row].Count)
          {
            _cells[row].Capacity = col + 1;
            while(_cells[row].Count < (col + 1))
              _cells[row].Add("");
          }
          _cells[row][col] = value;
        }
        else
          log("setCell():column number must not be lower than 1");
      }
      else
        log("setCell():row number must not be lower than 1");
    }

    public int findInCol(string val, int col)
    {
      int retVal = 0;
      for(int row = 1; row <= _cells.Count; row++)
      {
        if((col <= _cells[row-1].Count) && (_cells[row-1][col-1] == val))
        {
          retVal = row;
          break;
        }
      }
      return retVal;
    }

    public void removeRow(int row)
    {
      if((row > 0) && (row <= _cells.Count))
      {
        List<string> r = _cells[row - 1];
        _cells.Remove(r);
      }
    }

    void log(string msg)
    {

    }
  }
}
