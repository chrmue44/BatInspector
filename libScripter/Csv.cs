/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using libParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libScripter
{
  public class Csv
  {
    List<List<string>> _cells;
    string _fileName;
    char _separator = ';';
    int _colCnt = 0;

    public int RowCnt {  get { return _cells.Count; } }
    public int ColCnt { get { return _colCnt; } }

    public Csv()
    {
      _cells = new List<List<string>>();
    }

    public int read(string file, char separator = ';')
    {
      int retVal = 0;
      try
      {
        _separator = separator;
        if (File.Exists(file))
        {
          _fileName = file;
          string[] lines = File.ReadAllLines(file);
          _cells.Clear();
          foreach (string line in lines)
          {
            string[] s = line.Split(separator);
            List<string> row = s.ToList();
            _cells.Add(row);
            if (_colCnt < s.Length)
              _colCnt = s.Length;
          }
          log("file " + file + " opened successfully", enLogType.DEBUG);
        }
        else
        {
          retVal = 1;
          log("could not open file " + file, enLogType.ERROR);
        }
      }
      catch(Exception ex)
      {
        log("could not open file " + file + ex.ToString(), enLogType.ERROR);
        retVal = 2;
      }
      return retVal;
    }

    /// <summary>
    /// find a value in a row
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="val">value to search</param>
    /// <returns>column number 1..n, if not found or error: 0</returns>
    public int findInRow(int row, string val)
    {
      int retVal = 0;
      if ((row > 0) && (row <= _cells.Count))
      {
        row--;
        for (int i = 0; i < _cells[row].Count; i++)
        {
          if(_cells[row][i] == val)
          {
            retVal = i + 1;
            break;
          }
        }
      }
      else
        DebugLog.log("Csv.findInRow(): rom number " + row.ToString() + " not valid", enLogType.ERROR);
      return retVal;
    }


    public int save(bool withBackup = true)
    {
      int retVal = 0;
      if (_fileName != null)
      {
        if (withBackup)
        {
          string bakName = _fileName + ".bak";
          if (File.Exists(bakName))
            File.Delete(bakName);
          File.Copy(_fileName, bakName);
        }

        string[] lines = new string[_cells.Count];
        string sep = _separator.ToString();
        for (int rowNr = 0; rowNr < lines.Length; rowNr++)
          lines[rowNr] = String.Join(sep, _cells[rowNr]);

        File.WriteAllLines(_fileName, lines);
      }
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
          log("getCell(): column " + col.ToString() + " does not exist", enLogType.ERROR);
      }
      else
        log("getCell():row " + row.ToString() + " does not exist", enLogType.ERROR);
      return retVal;
    }

    public double getCellAsDouble(int row, int col)
    {
      string str = getCell(row, col);
      double retVal = 0;
      if (str != "")
      {
        try
        {
          retVal = double.Parse(str, CultureInfo.InvariantCulture);
        }
        catch
        {
          log("value: '" + str + "' in row " + row.ToString() + ", col " + col.ToString() + " is not a well formed double", enLogType.ERROR);
        }
      }
      return retVal;
    }

    public int getCellAsInt(int row, int col)
    {
      string str = getCell(row, col);
      int retVal = 0;
      if (str != "")
      {
        try
        {
          retVal = int.Parse(str, CultureInfo.InvariantCulture);
        }
        catch
        {
          log("value: '" + str + "' in row " + row.ToString() + ", col " + col.ToString() + " is not a well formed integer", enLogType.ERROR);
        }
      }
      return retVal;
    }

    public void setCell(int row, int col, string value)
    {
      row--;
      col--;
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
          log("setCell():column number must not be lower than 1", enLogType.ERROR);
      }
      else
        log("setCell():row number must not be lower than 1", enLogType.ERROR);
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

    public int findInCol(string val1, int col1, string val2, int col2)
    {
      int retVal = 0;
      for (int row = 1; row <= _cells.Count; row++)
      {
        if ((col1 <= _cells[row - 1].Count) && (_cells[row - 1][col1 - 1] == val1) &&
            (col2 <= _cells[row - 1].Count) && (_cells[row - 1][col2 - 1] == val2))
        {
          retVal = row;
          break;
        }
      }
      return retVal;
    }

    public void insertCol(int col, string ins = "")
    {
      bool err = false;
      if (col > 0)
      {
        foreach (List<string> row in _cells)
        {
          if (col <= row.Count)
            row.Insert(col - 1, ins);
          else
          {
            while (row.Count < col-1)
              row.Add("");
            row.Add(ins);
          }
        }
      }
      else
        err = true;
      if(err)
        log("insertCol: invalid col nr:" + col.ToString(), enLogType.ERROR);
    }

    public void removeRow(int row)
    {
      if((row > 0) && (row <= _cells.Count))
      {
        List<string> r = _cells[row - 1];
        _cells.Remove(r);
      }
      else
        log("removeRow: invalid row nr:" + row.ToString(), enLogType.ERROR);
    }

    void log(string msg, enLogType type)
    {
      DebugLog.log("[Csv] " + msg, type);
    }
  }
}
