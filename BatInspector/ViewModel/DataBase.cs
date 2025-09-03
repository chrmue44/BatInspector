using libParser;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.CRUD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Transactions;
using System.Windows;
using ZstdSharp.Unsafe;

namespace BatInspector
{
  public enum enSqlType
  {
    VARCHAR = 0,
    INT = 1,
    FLOAT = 2,
    DATE = 3,
  }
  
  public class sqlField
  {
    public string Name { get; set;}
    public enSqlType FieldType { get; set;}


    public sqlField()
    {

    }

    public sqlField(enSqlType type, string name)
    {
      Name = name;
      FieldType = type;
    }

    public sqlField(sqlField f)
    {
      _valFloat = f._valFloat;
      _valInt32 = f._valInt32;
      _valString = f._valString;
      Name = f.Name;
      FieldType = f.FieldType;
    }

    float _valFloat;
    int _valInt32;
    string _valString;
    DateTime _date;

    object getValue()
    {
      switch (FieldType)
      {
        case enSqlType.FLOAT:
          return _valFloat;
        case enSqlType.INT:
          return _valInt32;
        case enSqlType.DATE:
          return _date;
        default:
        case enSqlType.VARCHAR:
          return _valString;
      }
      return null;
    }

    public override string ToString()
    {
      switch (FieldType)
      {
        case enSqlType.FLOAT:
          return _valFloat.ToString(CultureInfo.InvariantCulture);
        case enSqlType.INT:
          return _valInt32.ToString(CultureInfo.InvariantCulture);
        case enSqlType.DATE:
          return _date.ToString(CultureInfo.InvariantCulture);
        default:
        case enSqlType.VARCHAR:
          return _valString;
      }
    }
    public void setInt32(int val)
    {
    _valInt32 = val;
    }

    public void setFloat(float val)
    {
      _valFloat = val;
    }

    public void setString(string val)
    {
      _valString = val;
    }
    public void setDate(DateTime val)
    {
      _date = val;
    }

    public string getDateAsString()
    {
      string mo = _date.Month.ToString("00");
      string day = _date.Day.ToString("00");
      return $"{_date.Year}-{mo}-{day}";
    }
    public int getInt32()
    {
      return _valInt32;
    }

    public string getString()
    {
      return _valString;
    }

    public float getFloat()
    {
      return _valFloat;
    }

    void setValue(enSqlType type, object value)
    {
      switch(type)
      {
        case enSqlType.FLOAT:
          _valFloat = (float)value;
          break;
        case enSqlType.INT:
          _valInt32 = (int)value;
          break;
        case enSqlType.DATE:
          _date = (DateTime)value;
          break;
        default:
        case enSqlType.VARCHAR:
          _valString = (string)value;
          break;
      }
    }
  }

  public class sqlRow
  {
    public List<sqlField> Fields { get; } 
    
    public sqlRow()
    {
      Fields = new List<sqlField>();
    }

    public sqlRow(sqlRow row)
    {
      Fields = new List<sqlField>();
      foreach(sqlField f in row.Fields)
      {
        sqlField fn = new sqlField(f);
        Fields.Add(fn);
      }
    }

    public sqlRow getCopy(sqlRow row)
    {
      return new sqlRow(row);
    }

    public void addField(enSqlType type, string name)
    {
      sqlField field = new sqlField(type, name);
      Fields.Add(field);
    }
  }


  public class SqlQueryBuilder
  {
    public string Command { get { return _str.ToString() +";"; } }

    StringBuilder _str;
    bool _select = false;
    bool _from = false;
    bool _where = false;
    bool _order = false;
    public SqlQueryBuilder()
    {
      _str = new StringBuilder();
    }

    public void init()
    {
      _str.Clear();
      _select = false;
      _from = false;
      _where = false;
      _order = false;
    }

    public void addSelectStatement(string expr)
    {
      addStatement("SELECT", ref _select, expr);      
    }
    public void addFromStatement(string expr)
    {
      addStatement("FROM", ref _from, expr);
    }
    public void addWhereStatement(string expr)
    {
      addStatement("WHERE", ref _where, expr);
    }
    public void addOrderStatement(string expr)
    {
      addStatement("ORDER BY", ref _order, expr);
    }
    public void addLimitStatement(int rows)
    {
      if(rows > 0)
        addStatement("LIMIT", ref _order, rows.ToString());
    }

    private void addStatement(string key, ref bool on, string expr)
    {
      if (on)
      {
        DebugLog.log($"query already contains {key}", enLogType.ERROR);
        return;
      }
      _str.Append(key);
      _str.Append(" ");
      _str.Append(expr);
      _str.Append("\n");

      on = true;

    }
  }

  public class DataBase
  {
    MySqlConnection _connection;
    bool _isOpen;

    public DataBase()
    {
    }

    /// <summary>
    /// connect to mysql data base
    /// </summary>
    /// <param name="connectStr"></param>
    /// <returns></returns>
    public int connect(string connectStr)
    {
      int retVal = -1;
      _connection = new MySqlConnection();
      _connection.ConnectionString = connectStr;
      if (_isOpen)
        return -1;
      try
      {
        _connection.Open();
        _isOpen = true;
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
    }
    
    /// <summary>
    /// execute query (with reply)
    /// </summary>
    /// <param name="cmd">SQL command string</param>
    /// <returns>a list of rows</returns>
    public List<sqlRow> execQuery(string cmd)
    {
      List<sqlRow> retVal = new List<sqlRow>();
      int count = 0;
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
                      newRow.Fields[fIdx].setFloat(myReader.GetFloat(fIdx));
                      break;
                    case enSqlType.INT:
                      newRow.Fields[fIdx].setInt32(myReader.GetInt32(fIdx));
                      break;
                    case enSqlType.DATE:
                      newRow.Fields[fIdx].setDate(myReader.GetDateTime(fIdx));
                      break;
                    default:
                    case enSqlType.VARCHAR:
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
    /// add complete project with project infomramtion, files and calls to tables
    /// 'prjects', 'tables' and 'calls'
    /// </summary>
    /// <param name="prj"></param>
    /// <param name="altWavPath">alternative wav path if differing from project</param>
    /// <returns></returns>
    public int addProjectToDb(Project prj, string altWavPath = "")
    {
      int retVal = -1;
      if (!prj.Ok)
      {
        DebugLog.log("addProjectToDb: project is not ok, aborted", enLogType.ERROR);
        return 1;
      }
      if((prj.Analysis == null) || (prj.Analysis.Files.Count == 0))
      {
        DebugLog.log("addProjectToDb: project does not contain analysis data, aborted", enLogType.ERROR);
        return 2;
      }
      retVal = addPrjToTableProjects(prj, altWavPath);
      if (retVal != 0)
        return retVal;

      string devId = prj.DeviceId;
      string prjId = prj.PrjId;
      for (int i = 0; i < prj.Analysis.Files.Count; i++)
      {
        retVal = addFile(devId, prjId, prj.Analysis.Files[i]);
        if (retVal != 0)
          break;
        string fileId = createFileId(devId, prj.Analysis.Files[i].Name); 
        for(int c = 0; c < prj.Analysis.Files[i].Calls.Count; c++)
        {
          retVal =addCall(fileId, prj.Analysis.Files[i].Calls[c]);
          if (retVal != 0)
            break;
        }
        if (retVal != 0)
          break;
      }
      return retVal;
    }

    /// <summary>
    /// add project information to table 'projects'
    /// </summary>
    /// <param name="prj"></param>
    /// <param name="person"></param>
    /// <param name="altWavPath"></param>
    /// <returns></returns>
    public int addPrjToTableProjects(Project prj, string altWavPath = "")
    {
      
      if (!checkIfOpen("addPrj"))
        return 1;
      if ((prj.Analysis == null) || (prj.Analysis.Files == null) || (prj.Analysis.Files.Count == 0))
      {
        DebugLog.log($"project {prj.Name} doesn't contain analysis data", enLogType.ERROR);
        return 2;
      }
      
      //check if record already exists
      string xmlName = Path.Combine(prj.PrjDir,
                                    prj.WavSubDir,
                                    prj.Analysis.Files[0].Name.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO)
                                    );
      BatRecord r = ElekonInfoFile.read(xmlName);
      string date = prj.Analysis.Files[0].getString(Cols.REC_TIME);
      bool ok = DateTime.TryParse(date, out DateTime t);
      date = $"{t.Year}-{t.Month.ToString("00")}-{t.Day.ToString("00")}";
      string recDev = r.SN;
      string id = prj.PrjId; // recDev + "_" + date;
      string sqlc = $"SELECT id FROM projects WHERE id ='{id}';";
      List<sqlRow> res = execQuery(sqlc);
      if(res.Count > 0)
      {
        DebugLog.log($"addProject not executed because project '{id}' already in db", enLogType.ERROR);
        return 3;
      }

      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append("INSERT INTO projects\n");
      sqlCmd.Append("(id,date,recording_device,sw_version,recording_person,path_to_wavs,microphone_id,classifier,model,location,notes)\n");
      sqlCmd.Append("VALUES\n");


      string sw = r.Firmware;
      string pwav = string.IsNullOrEmpty(altWavPath) ? Path.Combine(prj.PrjDir, prj.WavSubDir) : altWavPath;
      pwav = pwav.Replace('\\', '/');
      string mic = prj.MicId;
      string clsf = prj.AvailableModelParams[prj.SelectedModelIndex].Name;
      string model = prj.AvailableModelParams[prj.SelectedModelIndex].DataSet;
      string person = prj.CreateBy;
      string location = prj.Location;
      string notes = prj.Notes;
      sqlCmd.Append($"('{id}','{date}','{recDev}','{sw}', '{person}', '{pwav}', '{mic}', ");
      sqlCmd.Append($"'{clsf}','{model}','{location}', '{notes}');\n");
      return execNonQuery(sqlCmd.ToString());
    }

    public static string createFileId(string devName, string wavName)
    {
      return devName + "_" + wavName.ToLower().Replace(".wav","");
    }

    /// <summary>
    /// add a file to table 'files'. Checks if the file is already present.
    /// if yes, execution of command is aborted
    /// </summary>
    /// <param name="prjId">unique project id</param>
    /// <param name="file">file information</param>
    /// <returns></returns>
    public int addFile(string deviceName, string prjId, AnalysisFile file)
    {
      if (!checkIfOpen("addFile"))
        return 1;

      string wav_name = file.Name;
      string id = createFileId(deviceName, wav_name);

      // check if file already in db
      string sqlc = $"SELECT id FROM projects WHERE id ='{id}';";
      List<sqlRow> res = execQuery(sqlc);
      if (res.Count > 0)
      {
        DebugLog.log($"addFile not executed because project '{id}' already in db", enLogType.ERROR);
        return 3;
      }

      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append("INSERT INTO files\n");
      sqlCmd.Append("(id,wav_name,lat,lon,project_id,temperature,humidity,");
      sqlCmd.Append("sampling_rate,file_length)\n");
      sqlCmd.Append("VALUES\n");
      id = id.Replace(".wav", "");
      id = id.Replace(".WAV", "");
      string lat = file.getDouble(Cols.LAT).ToString(CultureInfo.InvariantCulture);
      string lon = file.getDouble(Cols.LON).ToString(CultureInfo.InvariantCulture);
      string temp = file.getDouble(Cols.TEMPERATURE).ToString(CultureInfo.InvariantCulture);
      string hum = file.getDouble(Cols.HUMIDITY).ToString(CultureInfo.InvariantCulture);
      string sr = file.getInt(Cols.SAMPLERATE).ToString(CultureInfo.InvariantCulture);
      string fLen = file.getDouble(Cols.FILE_LEN).ToString(CultureInfo.InvariantCulture);
      sqlCmd.Append($"('{id}','{wav_name}',{lat}, {lon}, '{prjId}', {temp}, {hum},");
      sqlCmd.Append($"{sr}, {fLen});\n");
      return execNonQuery(sqlCmd.ToString());
    }

    public int addCall(string fileId, AnalysisCall call)
    {
      if (!checkIfOpen("addCall"))
        return 1;

      // check if call already in db
      string nr = call.getInt(Cols.NR).ToString("000");
      string id = fileId + "_" + nr;
      string sqlc = $"SELECT id FROM calls WHERE id ='{id}';";
      List<sqlRow> res = execQuery(sqlc);
      if (res.Count > 0)
      {
        DebugLog.log($"addFile not executed because call '{id}' already in db", enLogType.ERROR);
        return 3;
      }

      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append("INSERT INTO calls\n");
      sqlCmd.Append("(id,file_id,call_nr,freq_min,freq_max,freq_char,duration,");
      sqlCmd.Append("start_time,call_interval,bandwidth,snr,spec_auto,probability,spec_man,remarks)\n");
      sqlCmd.Append("VALUES\n");
      string fmin = call.getDouble(Cols.F_MIN).ToString(CultureInfo.InvariantCulture);
      string fmax = call.getDouble(Cols.F_MAX).ToString(CultureInfo.InvariantCulture);
      string fchar = call.getDouble(Cols.F_MAX_AMP).ToString(CultureInfo.InvariantCulture);
      string dur = call.getDouble(Cols.DURATION).ToString(CultureInfo.InvariantCulture);
      string ts = call.getDouble(Cols.START_TIME).ToString(CultureInfo.InvariantCulture);
      string cint = call.getDouble(Cols.CALL_INTERVALL).ToString(CultureInfo.InvariantCulture);
      string bw = call.getDouble(Cols.BANDWIDTH).ToString(CultureInfo.InvariantCulture);
      string snr = call.getDouble(Cols.SNR).ToString(CultureInfo.InvariantCulture);
      string spa = call.getString(Cols.SPECIES);
      string spm = call.getString(Cols.SPECIES_MAN);
      string rem = call.getString(Cols.REMARKS);
      string prob = call.getDouble(Cols.PROBABILITY).ToString(CultureInfo.InvariantCulture);
      sqlCmd.Append($"('{id}','{fileId}', {nr}, {fmin}, {fmax}, {fchar}, {dur},");
      sqlCmd.Append($"{ts}, {cint}, {bw}, {snr}, '{spa}', '{prob}', '{spm}', '{rem}');");
      return execNonQuery(sqlCmd.ToString());
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

    bool checkIfOpen(string cmd)
    {
      if (!_isOpen)
        DebugLog.log($"{cmd}: data base not open, connect first", enLogType.ERROR);
      return _isOpen;
    }
  }
}
