﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
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
    string _fileName = "";
    string _separator = ";";
    int _colCnt = 0;
    bool _withHdr = false;
    Dictionary<string, int> _cols;
    bool _changed = false;
    static int _idGen = 0;
    int _id = 0;

    public int RowCnt {  get { return _cells.Count; } }
    public int ColCnt { get { return _colCnt; } }

    public string FileName {  get { return _fileName; } }

    public bool Changed { get { return _changed; } }

    public Dictionary<string, int> Cols { get { return _cols; } }

    public Csv(bool withHdr = true)
    {
      _id = _idGen++;
      _cells = new List<List<string>>();
      _withHdr = withHdr;
      _cols = new Dictionary<string, int>();
    }

    public Csv(string header, char separator = ';')
    {
      _id = _idGen++;
      _cells = new List<List<string>>();
      string[] cols = header.Split(separator);
      _colCnt = cols.Length;
      _cells.Add(cols.ToList());

      _cols = new Dictionary<string, int>();
      _withHdr = true;
      initColNames(header, false);
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
        file = file.Replace("\"", "");
        if (File.Exists(file))
        {
          _fileName = file;
          string[] lines = File.ReadAllLines(file);
          _cells.Clear();
          foreach (string line in lines)
          {
            if (line.Length > 0)
            {
              string[] s = line.Split(separator[0]);
              List<string> row = s.ToList();
              _cells.Add(row);
              if (_colCnt < s.Length)
                _colCnt = s.Length;
            }
          }
          if(_withHdr)
            initColNames(lines[0]);
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
      _changed = true;
    }

    public void addRow(List<string> row)
    {
      _cells.Add(row);
      _changed = true;
    }

    public List<string> getRow(int row)
    {
      List<string> retVal = null;
      if((row > 0) && (row <= _cells.Count))
      {
        row--;
        retVal = _cells[row];
      }
      return retVal;
    }

    public string getRowAsString(int row, string delimiter=";")
    {
      string retVal = "";
      List<string> cells = getRow(row);
      if(cells != null)
        retVal = string.Join(delimiter, cells);
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
      if ((_fileName != null) && _changed)
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
        try
        {
          File.WriteAllLines(_fileName, lines);
        }
        catch
        {
          log("could not save file " + _fileName, enLogType.ERROR);
        }

        _changed = false;
        log(_fileName + " saved", enLogType.INFO);
      }
      return retVal;
    }

    public string getCell(int row, string colName, bool disableErrorMsg = false)
    {
      int col = getColNr(colName, disableErrorMsg);
      return getCell(row, col, disableErrorMsg);
    }

    /// <summary>
    /// get call value as string
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="col">col nr 1..n</param>
    /// <param name="disableErrorMsg">true disables error message</param>
    /// <returns>int value </returns>    
    public string getCell(int row, int col, bool disableErrorMsg = false)
    {
      string retVal = "";
      row--;
      if ((row >= 0) && (row < _cells.Count))
      {
        col--;
        if ((col >= 0) && (col < _colCnt))
        {
          while (_cells[row].Count < _colCnt)
            _cells[row].Add("");
          retVal = _cells[row][col];
        }
        else
        {
          if (!disableErrorMsg)
            log("getCell(): column " + col.ToString() + " does not exist", enLogType.ERROR);
        }
      }
      else
      {
        if (!disableErrorMsg)
          log("getCell():row " + row.ToString() + " does not exist", enLogType.ERROR);
      }
      return retVal;
    }

    public double getCellAsDouble(int row, string colName, bool disableErrorMsg = false)
    {
      int col = getColNr(colName, disableErrorMsg);
      return getCellAsDouble(row, col, disableErrorMsg);
    }

    public DateTime getCellAsDate(int row, string colName)
    {
      int col = getColNr(colName);
      return getCellAsDate(row, col);
    }

    public DateTime getCellAsDate(int row, int col)
    {
      string str = getCell(row, col);
      DateTime retVal;
      bool ok = DateTime.TryParse(str, out retVal);
      if (!ok)
        log($"parsing of date string failed: {str}", enLogType.ERROR);
      return retVal;
    }

    /// <summary>
    /// get call value as double value if possible (with decimal POINT, independent from localization of PC)
    /// </summary>
    /// <param name="row">row nr 1..n</param>
    /// <param name="col">col nr 1..n</param>
    /// <returns>int value (or 0 if value isn't a double)</returns>
    public double getCellAsDouble(int row, int col, bool disableErrorMsg = false)
    {
      string str = getCell(row, col, disableErrorMsg);
      double retVal = 0;
      if (str != "")
      {
        bool ok = double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out retVal);
        if(!ok)  
          log("value: '" + str + "' in row " + row.ToString() + ", col " + col.ToString() + " is not a well formed double", enLogType.ERROR);
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
        bool ok = int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out retVal);
        if (!ok)
          log("value: '" + str + "' in row " + row.ToString() + ", col " + col.ToString() + " is not a well formed integer", enLogType.ERROR);
      }
      return retVal;
    }

    public void setCell(int row, string colName, int value)
    {
      int col = getColNr(colName);
      string valStr = value.ToString(CultureInfo.InvariantCulture);
      setCell(row, col, valStr);
    }

    public void setCell(int row, int col, int value)
    {
      string valStr = value.ToString(CultureInfo.InvariantCulture);
      setCell(row, col, valStr);
    }

    public void setCell(int row, string colName, double value)
    {
      int col = getColNr(colName);
      string valStr = value.ToString(CultureInfo.InvariantCulture);
      setCell(row, col, valStr);
    }

    public void setCell(int row, int col, double value)
    {
      string valStr = value.ToString(CultureInfo.InvariantCulture);
      setCell(row, col, valStr);
    }

    public void setCell(int row, string colName, double value, int decimals)
    {
      int col = getColNr(colName);
      string fmt = "0.";
      for (int i = 0; i < decimals; i++)
        fmt += "0";
      string valStr = value.ToString(fmt,CultureInfo.InvariantCulture);
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
      _changed = true;
    }


    public int findInCol(string val, string colstr, bool subStr = false)
    {
      int col = getColNr(colstr);
      return findInCol(val, col, subStr);
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
      return retVal;
    }

    public int findInCol(string val1, string col1str, string val2, string col2str)
    {
      int col1 = getColNr(col1str);
      int col2 = getColNr(col2str);
      return findInCol(val1, col1, val2, col2);
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


    public void insertRow(int row)
    {
      if ((row >= 1) && (row <= RowCnt))
      {
        List<string> insRow = new List<string>();
        for (int i = 0; i < ColCnt; i++)
          insRow.Add("");
          _cells.Insert(row - 1, insRow);
        _changed = true;
      }
      else
        log($"could not insert row in row {row}, file has only {RowCnt} rows", enLogType.ERROR);
    }
    
    /// <summary>
    /// insert a column at a given position
    /// </summary>
    /// <param name="col">column r (1..n)</param>
    /// <param name="ins">value to insert</param>
    public void insertCol(int col, string ins = "", string colHdr = "")
    {
      bool err = false;
      if (col > 0)
      {
        if(_cells.Count == 0)
          _cells.Add(new List<string>());
        foreach (List<string> row in _cells)
        {
          if (col <= row.Count)
            row.Insert(col - 1, ins);
          else
          {
            while (row.Count < col - 1)
            {
              row.Add("");
            }
            row.Add(ins);
          }
          if(_colCnt < row.Count)
            _colCnt = row.Count;
        }
        if((colHdr != "") && _withHdr)
        {
          setCell(1, col, colHdr);
          initColNames(_cells[0].ToArray());
        }
      }
      else
        err = true;
      if(err)
        log("insertCol: invalid col nr:" + col.ToString(), enLogType.ERROR);
       _changed = true;
     }

    public void removeRow(int row)
    {
      if((row > 0) && (row <= _cells.Count))
      {
        List<string> r = _cells[row - 1];
        _cells.Remove(r);
        _changed = true;
      }
      else
        log("removeRow: invalid row nr:" + row.ToString(), enLogType.ERROR);
    }


    public void removeCol(string colName)
    {
      int col = getColNr(colName);
      removeCol(col);
    }

    public void removeCol(int col)
    {
      if ((col > 0) && (col <= ColCnt) && (_colCnt > 0))
      {
        foreach(List<string> r in _cells)
        {
          if (col < r.Count)
            r.RemoveAt(col - 1);
        }
        _colCnt--;
        if (_withHdr)
          initColsFromExistingCols();
      }
      else
        log("remove col: invalic col nr: " + col.ToString(), enLogType.ERROR);
    }

    public int getColNr(string colName, bool disableErrorMsg = false)
    {
      int retVal = -1;
      bool ok;
      if (_withHdr)
        ok = _cols.TryGetValue(colName, out retVal);
      else
        ok = int.TryParse(colName, out retVal);
      if((retVal < 1) && !disableErrorMsg)
        log("col name '" + colName + "' not found!", enLogType.ERROR);
      return retVal;
    }

    void initColsFromExistingCols()
    {
      _cols = new Dictionary<string, int>();
      for (int c = 1; c <= ColCnt; c++)
        _cols.Add(getCell(1, c), c);
    }

    public void initColNames(string[] cols, bool createCols = false)
    {
      _cols.Clear();
      for (int i = 0; i < cols.Length; i++)
      {
        string colName = cols[i];
        if (createCols)
          insertCol(i + 1, cols[i], colName);
        else
        {
          try
          {
            if (colName.Length == 0)
              colName = "_colHeader" + i.ToString();
            _cols.Add(colName, i + 1);
          }
          catch
          {
            DebugLog.log("initColNames: col name already present: '" + colName + "'", enLogType.ERROR);
          }
        }
      }
    }

    public void initColNames(string header, bool createCols = false)
    {
      _withHdr = true;
      string[] cols = header.Split(_separator[0]);
      initColNames(cols, createCols);
    }

    void log(string msg, enLogType type)
    {
      DebugLog.log("[Csv] " + msg, type);
    }
  }
}
