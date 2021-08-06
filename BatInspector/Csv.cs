using System;
using System.Collections.Generic;
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

    public Csv()
    {
      _cells = new List<List<string>>();
    }

    public int open(string file, char separator = ';')
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

    public int save()
    {
      int retVal = 0;
      string[] lines = new string[_cells.Count];
      int rowNr = 0;
      string sep = _separator.ToString();
      foreach(List<string> row in _cells)
        lines[rowNr] = String.Join(sep, row);

      File.WriteAllLines(_fileName, lines);
      return retVal;
    }

    string getCell(int row, int col)
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

    void log(string msg)
    {

    }
  }
}
