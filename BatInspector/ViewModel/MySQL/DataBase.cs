using BatInspector.Properties;
using libParser;
using libScripter;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.CRUD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;


namespace BatInspector
{
  public class DataBase
  {
    MySqlConnection _connection;
    bool _isOpen;
    DbBats _dbBats;
    DbBirds _dbBirds;

    public DataBase()
    {
      _dbBats = new DbBats(this);
      _dbBirds = new DbBirds(this);
    }

    public bool IsConnected {  get { return _isOpen; } }
    /// <summary>
    /// connect to mysql data base
    /// </summary>
    /// <returns>0 ok</returns>
    /// 

    public DbBats DbBats { get { return _dbBats; } }
    public DbBirds DbBirds { get { return _dbBirds; } }
    public int connect(string server, string dataBase, string user, string password)
    {

      int retVal = -1;
      if (_isOpen)
        return retVal;
      string connectStr = $"server={server};uid={user};pwd={password};database={dataBase}";

      _connection = new MySqlConnection();
      _connection.ConnectionString = connectStr;
      try
      {
        _connection.Open();
        _isOpen = true;
        DebugLog.log("connected succesfully to MySQL database", enLogType.INFO);
        retVal = 0;
      }
      catch (Exception ex)
      {
        _isOpen = false;
        DebugLog.log($"error connecting to MySql data base: '{connectStr}; {ex.ToString()}", enLogType.ERROR);
        retVal = -2;
      }
      
      return retVal;
    }


    /// <summary>
    /// disconnect from data base
    /// </summary>
    public void disconnect()
    {
      if (!_isOpen)
        return;
      _connection.Close();
      _connection.Dispose();
      _isOpen = false;
      DebugLog.log("disconnected from MySQL database", enLogType.INFO);
    }

    public string getStatus()
    {
      string retVal = "";
      if (_isOpen)
      {
        retVal = MyResources.DatabaseConnecteed;
        string cmd = "SHOW TABLE STATUS;";
        List<sqlRow> res = execQuery(cmd);
        retVal += $":  {res[0].Fields[0].getString()}: {res[0].Fields[4].getInt32()} {MyResources.DataBaseRows}  ---  ";
        retVal += $"{res[1].Fields[0].getString()}: {res[1].Fields[4].getInt32()} {MyResources.DataBaseRows}  ---  ";
        retVal += $"{res[2].Fields[0].getString()}: {res[2].Fields[4].getInt32()} {MyResources.DataBaseRows}";
      }
      else
        retVal = MyResources.DataBaseDisconnected;
      return retVal;
    }

    /// <summary>
    /// execute query (with reply)
    /// </summary>
    /// <param name="cmd">SQL command string</param>
    /// <returns>a list of rows</returns>
    public List<sqlRow> execQuery(string cmd)
    {
      List<sqlRow> retVal = new List<sqlRow>();
      try
      {
        if (_isOpen)
        {
          using (MySqlCommand sqlCmd = new MySqlCommand())
          {
            sqlCmd.Connection = _connection;
            sqlCmd.CommandText = cmd;
            sqlCmd.CommandType = System.Data.CommandType.Text;
            using (var myReader = sqlCmd.ExecuteReader())
            {
              sqlRow row = new sqlRow();
              for (int i = 0; i < myReader.FieldCount; i++)
              {
                string name = myReader.GetName(i);
                string dtType = myReader.GetDataTypeName(i);
                bool ok = Enum.TryParse(dtType, out enSqlType t);
                if (!ok)
                {
                  DebugLog.log($"unsupported data type in query: {dtType}", enLogType.ERROR);
                  t = enSqlType.VARCHAR;
                }
                row.addField(t, name);
              }

              while (myReader.Read())
              {
                sqlRow newRow = new sqlRow(row);
                for (int fIdx = 0; fIdx < myReader.FieldCount; fIdx++)
                {
                  switch (newRow.Fields[fIdx].FieldType)
                  {
                    case enSqlType.FLOAT:
                      if(!myReader.IsDBNull(fIdx))
                        newRow.Fields[fIdx].setFloat(myReader.GetFloat(fIdx));
                      break;
                    case enSqlType.INT:
                      if (!myReader.IsDBNull(fIdx))
                        newRow.Fields[fIdx].setInt32(myReader.GetInt32(fIdx));
                      break;
                    case enSqlType.BIGINT:
                      if (!myReader.IsDBNull(fIdx))
                        newRow.Fields[fIdx].setInt32((int)myReader.GetInt64(fIdx));
                      break;
                    case enSqlType.DATE:
                    case enSqlType.DATETIME:
                      if(!myReader.IsDBNull(fIdx))
                        newRow.Fields[fIdx].setDate(myReader.GetDateTime(fIdx));
                      break;
                    case enSqlType.TIMESTAMP:
                      if (!myReader.IsDBNull(fIdx))
                        newRow.Fields[fIdx].setDate(myReader.GetDateTime(fIdx));
                      break;
                    default:
                    case enSqlType.VARCHAR:
                      if (!myReader.IsDBNull(fIdx))
                        newRow.Fields[fIdx].setString(myReader.GetString(fIdx));
                      break;
                  }
                }
                retVal.Add(newRow);
              }
            }
          }
        }
      }
      catch(Exception ex)
      {
        DebugLog.log($"MySQL Query failed: {ex.ToString()}", enLogType.ERROR);
      }
      return retVal;
    }

    /// <summary>
    /// execute non query command (no reply)
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public int execNonQuery(string cmd)
    {
      int retVal = -1;
      if (!checkIfOpen("execNonQuery"))
        return retVal;
      try
      {
        using (MySqlCommand sqlCmd = new MySqlCommand())
        {
          sqlCmd.Connection = _connection;
          sqlCmd.CommandText = cmd;
          sqlCmd.ExecuteNonQuery();
          retVal = 0;
        }
      }
      catch (Exception ex)
      {
        DebugLog.log($"MySql command failed: {ex.ToString()}", enLogType.ERROR);
      }
      return retVal;
    }

    /// <summary>
    /// translates BatInspector filter expressions to MySQL WHERE expressions
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static string translateFilterExpressionToMySQL(string exp)
    {
      string retVal = exp.Replace("indexOf", "INSTR");
      int pos = 0;

      // replace indexOf() with INSTR()
      while ((pos >= 0) && (pos < retVal.Length))
      {
        pos = retVal.IndexOf("INSTR", pos);
        if (pos >= 0)
        {
          pos += 6;
          pos = Utils.findMatchingClosingBrace(retVal, pos);
          if (pos < 0)
            break;
          int pos1 = retVal.IndexOf(">=", pos);
          if (pos1 >= 0)
          {
            retVal = retVal.Remove(pos1, 2);
            retVal = retVal.Insert(pos1, ">");
            pos = pos1;
          }
          else
          {
            pos1 = retVal.IndexOf("<", pos);
            if (pos1 >= 0)
            {
              retVal = retVal.Remove(pos1, 1);
              retVal = retVal.Insert(pos1, "=");
              pos = pos1;
            }
          }
        }
      }
      // replace string delimiters
      retVal = retVal.Replace('\"', '\'');

      // replace operators
      retVal = retVal.Replace("!=", "<>");
      retVal = retVal.Replace("==", "=");
      return retVal;
    }


   

    public int deleteRow(string table, string condition)
    {
      if (!checkIfOpen("deleteRow"))
        return 1;
      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append($"DELETE FROM {table}\n");
      sqlCmd.Append($"WHERE {condition};");
      return execNonQuery(sqlCmd.ToString());
    }

    public void exportQuery(List<sqlRow> query, string fileName)
    {
      try
      {
        Csv csv = new Csv();
        csv.addRow();
        for (int i = 0; i < query[0].Fields.Count; i++)
          csv.setCell(1, i + 1, query[0].Fields[i].Name);

        for (int i = 0; i < query.Count; i++)
        {
          int row = i + 2;
          csv.addRow();
          for (int j = 0; j < query[i].Fields.Count; j++)
          {
            int col = j + 1;
            sqlField fi = query[i].Fields[j];
            switch (fi.FieldType)
            {
              case enSqlType.VARCHAR:
                csv.setCell(row, col, fi.getString());
                break;
              case enSqlType.INT:
                csv.setCell(row, col, fi.getInt32());
                break;
              case enSqlType.BIGINT:
                csv.setCell(row, col, fi.getInt32());
                break;
              case enSqlType.FLOAT:
                csv.setCell(row, col, (double)fi.getFloat());
                break;
              case enSqlType.DATE:
                csv.setCell(row, col, fi.getDateAsString());
                break;
              case enSqlType.DATETIME:
                csv.setCell(row, col, fi.getDateTimeAsString());
                break;
              case enSqlType.TIMESTAMP:
                csv.setCell(row, col, fi.getDateTimeAsString());
                break;
            }
          }
        }
        csv.saveAs(fileName);
      }
      catch (Exception ex)
      {
        DebugLog.log($"Error export query: {ex.ToString()}", enLogType.ERROR);
      }
    }

    public bool checkIfOpen(string cmd)
    {
      if (!_isOpen)
        DebugLog.log($"{cmd}: data base not open, connect first", enLogType.ERROR);
      return _isOpen;
    }
  }
}
