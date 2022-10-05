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

namespace libScripter
{
  public class Csv
  {
    List<List<string>> _cells;
    string _fileName;
    string _separator = ";";
    int _colCnt = 0;
    bool _withHdr = false;
    Dictionary<string, int> _cols;

    public int RowCnt {  get { return _cells.Count; } }
    public int ColCnt { get { return _colCnt; } }

    public string FileName {  get { return _fileName; } }

    public Csv(bool withHdr = false)
    {
      _cells = new List<List<string>>();
      _cols = new Dictionary<string, int>();
      _withHdr = withHdr;
    }

    public void clear()
    {
      _cells.Clear();
      _cols.Clear();
    }

    /// <summary>
    /// read a file and close it.
    /// The complete file content is copied to memory. When the method exits, the file will be closed
    /// </summary>
    /// <param name="file">fiel name</param>
    /// <param name="separator">separator for cells in lines</param>
    /// <returns>0: ok, 1:error</returns>
    public int read(string file, string separator = ";", bool withHeader = false)
    {
      int retVal = 0;
      try
      {
        _withHdr = withHeader;
        _separator = separator;
        if (File.Exists(file))
        {
          _fileName = file;
          string[] lines = File.ReadAllLines(file);
          _cells.Clear();
          foreach (string line in lines)
          {
            string[] s = line.Split(separator[0]);
            List<string> row = s.ToList();
            _cells.Add(row);
            if (_colCnt < s.Length)
              _colCnt = s.Length;
          }
          if(_withHdr)
            initColNames(lines[0]);
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

    public int write()
    {
      int retVal = 0;
      List<string> lines = new List<string>();
      foreach(List<string> row in _cells)
      {
        string[] rowArr = row.ToArray();
        string line = string.Join(_separator, rowArr);
        lines.Add(line);
      }
      File.WriteAllLines(_fileName, lines);
      return retVal;
    }

    public void addRow()
    {
      List<string> row = new List<string>();
      row.Capacity = _colCnt;
      _cells.Add(row);

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

    public int saveAs(string name, bool withBackup = true)
    {
      _fileName = name;
      return save(withBackup);
    }

    /// <summary>
    /// save file
    /// </summary>
    /// <param name="withBackup">true: create a bockup before overwriting existing file</param>
    /// <returns></returns>
    public int save(bool withBackup = true)
    {
      int retVal = 0;
      if (_fileName != null)
      {
          if (withBackup && File.Exists(_fileName))
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

    public string getCell(int row, string colName)
    {
      int col = getColNr(colName);
      return getCell(row, col);
    }

    /// <summary>
    /// get call value as string
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="col">col nr 1..n</param>
    /// <returns>int value </returns>    
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

    public double getCellAsDouble(int row, string colName)
    {
      int col = getColNr(colName);
      return getCellAsDouble(row, col);
    }

    /// <summary>
    /// get call value as double value if possible (with decimal POINT, independent from localization of PC)
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="col">col nr 1..n</param>
    /// <returns>int value (or 0 if value isn't a double)</returns>
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


    public int getCellAsInt(int row, string colName)
    {
      int col = getColNr(colName);
      return getCellAsInt(row, col);
    }

    /// <summary>
    /// get call value as integer value if possible
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="col">col nr 1..n</param>
    /// <returns>int value (or 0 if value isn't an int)</returns>
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

    public void setCell(int row, string colName, int value)
    {
      int col = getColNr(colName);
      string valStr = value.ToString(CultureInfo.InvariantCulture);
      setCell(row, col, valStr);
    }

    public void setCell(int row, string colName, double value)
    {
      int col = getColNr(colName);
      string valStr = value.ToString(CultureInfo.InvariantCulture);
      setCell(row, col, valStr);
    }

    public void setCell(int row, string colName, string value)
    {
      int col = getColNr(colName);
      setCell(row, col, value);
    }

    /// <summary>
    /// set value of cell
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="col">col nr 1..n</param>
    /// <param name="value"></param>
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

    /// <summary>
    /// find a value in a column
    /// </summary>
    /// <param name="val">value to find</param>
    /// <param name="col">column nr (1..n)</param>
    /// <param name="subStr">true: search for val as substring in cell</param>
    /// <returns>row number containing the value, or 0 if not found</returns>
    public int findInCol(string val, int col, bool subStr = false)
    {
      int retVal = 0;
      if (col > 0)
      {
        if (subStr)
        {
          for (int row = 1; row <= _cells.Count; row++)
          {
            if ((col <= _cells[row - 1].Count) && (_cells[row - 1][col - 1].IndexOf(val) >= 0))
            {
              retVal = row;
              break;
            }
          }

        }
        else
        {
          for (int row = 1; row <= _cells.Count; row++)
          {
            if ((col <= _cells[row - 1].Count) && (_cells[row - 1][col - 1] == val))
            {
              retVal = row;
              break;
            }
          }
        }
      }
      else
        retVal = 1;
      return retVal;
    }

    /// <summary>
    /// find two values in two columns
    /// </summary>
    /// <param name="val">value to find</param>
    /// <param name="col">column nr (1..n)</param>
    /// <returns>row number containing the values, or 0 if not found</returns>
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

    /// <summary>
    /// insert a column at a given position
    /// </summary>
    /// <param name="col">column r (1..n)</param>
    /// <param name="ins">value to insert</param>
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
            while (row.Count < col - 1)
            {
              row.Add("");
              _colCnt++;
            }
            row.Add(ins);
          }
          _colCnt++;
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

    int getColNr(string colName)
    {
      int retVal = -1;
      try
      {
        if (_withHdr)
          retVal = _cols[colName];
        else
          int.TryParse(colName, out retVal);
      }
      catch { }
      return retVal;
    }

    public void initColNames(string header, bool createCols = false)
    {
      _withHdr = true;
      string[] cols = header.Split(_separator[0]);
      for (int i = 0; i < cols.Length; i++)
      {
        _cols.Add(cols[i], i + 1);
        if(createCols)
          insertCol(i + 1, cols[i]);
      }
    }

    void log(string msg, enLogType type)
    {
      DebugLog.log("[Csv] " + msg, type);
    }
  }
}
