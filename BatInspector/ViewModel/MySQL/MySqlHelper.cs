using libParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace BatInspector
{
  public enum enSqlType
  {
    VARCHAR = 0,
    INT = 1,
    FLOAT = 2,
    DATE = 3,
    DATETIME = 4,
    BIGINT = 5,
    TIMESTAMP = 6
  }
  public class DBBAT
  {
    public const string ID = "id";
    public const string PRJID = "ProjectId";
    public const string SWVER = "SwVersion";
    public const string MODEL = "Model";
    public const string CLASSI = "Classifier";
    public const string MICID = "MicrophoneId";
    public const string PRJCREATOR = "PrjCreator";
    public const string RECDEV = "RecordingDevice";
    public const string DATE = "Date";
    public const string FILEID = "FileId";
    public const string LOC = "Location";
    public const string PRJ_NOTES = "Notes";
    public const string TRIG_SET = "TriggerSettings";
    public const string WAV_FILE_NAME = "WavFileName";
    public const string FILE_LENGTH = "FileLength";
    public const string RECORDING_TIME = "RecordingTime";
    public const string PATH_TO_WAV = "PathToWavs";
    public const string SAMPLE_RATE = "SamplingRate";
    public const string LAT = "Latitude";
    public const string LON = "Longitude";
    public const string TEMP = "Temperature";
    public const string HUMI = "Humidity";
    public const string CALLNR = "CallNr";
    public const string FMIN = "FreqMin";
    public const string FMAX = "FreqMax";
    public const string FMAXAMP = "FreqMaxAmp";
    public const string SPEC_MAN = "SpeciesMan";
    public const string SPEC_AUTO = "SpeciesAuto";
    public const string PROB = "Probability";
    public const string CALL_DST = "CallInterval";
    public const string START_TIME = "StartTime";
    public const string CALL_LEN = "DurationCall";
    public const string BWIDTH = "Bandwidth";
    public const string REM = "Remarks";
    public const string SNR = "SNR";

  }

  public class sqlField
  {
    public string Name { get; set; }
    public enSqlType FieldType { get; set; }


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
        case enSqlType.BIGINT:
          return _valInt32;
        case enSqlType.DATE:
        case enSqlType.DATETIME:
        case enSqlType.TIMESTAMP:
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
        case enSqlType.BIGINT:
          return _valInt32.ToString(CultureInfo.InvariantCulture);
        case enSqlType.DATE:
          return _date.ToString("dd.MM.yyyy");
        case enSqlType.TIMESTAMP:
        case enSqlType.DATETIME:
          return _date.ToString("dd.MM.yyyy HH:mm:ss");
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
      return _date.ToString("dd.MM.yyyy");
    }

    public DateTime getDateTime()
    {
      return _date;
    }

    public string getDateTimeAsString()
    {
      return _date.ToString("dd.MM.yyyy HH:mm:ss");
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
      switch (type)
      {
        case enSqlType.FLOAT:
          _valFloat = (float)value;
          break;
        case enSqlType.INT:
        case enSqlType.BIGINT:
          _valInt32 = (int)value;
          break;
        case enSqlType.DATE:
        case enSqlType.DATETIME:
        case enSqlType.TIMESTAMP:
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
      foreach (sqlField f in row.Fields)
      {
        sqlField fn = new sqlField(f);
        Fields.Add(fn);
      }
    }

    public sqlRow getCopy(sqlRow row)
    {
      return new sqlRow(row);
    }

    public void addField(enSqlType 
    type, string name)
    {
      sqlField field = new sqlField(type, name);
      Fields.Add(field);
    }

    public int findField(string name)
    {
      int retVal = -1;
      for(int i = 0; i < Fields.Count; i++)
      {
        if (Fields[i].Name == name)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }
  }


  public class SqlQueryBuilder
  {
    public string Command { get { return _str.ToString() + ";"; } }

    StringBuilder _str;
    bool _select = false;
    bool _from = false;
    bool _where = false;
    bool _order = false;
    bool _limit = false;
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
      _limit = false;
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
      if (rows > 0)
        addStatement("LIMIT", ref _limit, rows.ToString());
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


  public class QueryItem
  {
    public int line { get; set; }
    public string Date { get; set; }
    public string Location { get; set; }
    public string RecordingDevice { get; set; }
    public string MicrophoneId { get; set; }
    public string PrjCreator { get; set; }
    public string projects___Notes { get; set; }
    public string Classifier { get; set; }
    public string Model { get; set; }
    public string PathToWavs { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string WavFileName { get; set; }
    public int SamplingRate { get; set; }
    public float FileLength { get; set; }
    public string RecordingTime { get; set; }
    public string Temperature { get; set; }
    public string Humidity { get; set; }
    public int CallNr { get; set; }

    public float StartTime { get; set; }
    public string SNR { get; set; }

    public string SpeciesMan { get; set; }
    public string SpeciesAuto { get; set; }
    public string Probability { get; set; }
    public string FreqMin { get; set; }
    public string FreqMax { get; set; }
    public string FreqMaxAmp { get; set; }
    public string DurationCall { get; set; }
    public string CallInterval { get; set; }

    public string calls___Remarks { get; set; }
    public void setValues(sqlRow row)
    {
      foreach (sqlField f in row.Fields)
      {
        if (f.Name == "Date")
          Date = f.getDateAsString();
        else if (f.Name == DBBAT.LOC)
          Location = f.getString();
        else if (f.Name == "PrjCreator")
          PrjCreator = f.getString();
        else if (f.Name == "Classifier")
          Classifier = f.getString();
        else if (f.Name == "RecordingDevice")
          RecordingDevice = f.getString();
        else if (f.Name == "MicrophoneId")
          MicrophoneId = f.getString();
        else if (f.Name == "Model")
          Model = f.getString();
        else if (f.Name == DBBAT.PATH_TO_WAV)
          PathToWavs = f.getString();
        else if (f.Name == DBBAT.LAT)
          Latitude = f.getFloat().ToString("0.000000", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.LON)
          Longitude = f.getFloat().ToString("0.000000", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.WAV_FILE_NAME)
          WavFileName = f.getString();
        else if (f.Name == DBBAT.SAMPLE_RATE)
          SamplingRate = f.getInt32();
        else if (f.Name == DBBAT.FILE_LENGTH)
          FileLength = f.getFloat();
        else if (f.Name == DBBAT.RECORDING_TIME)
          RecordingTime = f.getDateTimeAsString();
        else if (f.Name == DBBAT.START_TIME)
          StartTime = f.getFloat();
        else if (f.Name == DBBAT.SNR)
          SNR = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.SPEC_MAN)
          SpeciesMan = f.getString();
        else if (f.Name == DBBAT.SPEC_AUTO)
          SpeciesAuto = f.getString();
        else if (f.Name == DBBAT.PROB)
          Probability = f.getFloat().ToString("0.00", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.FMIN)
          FreqMin = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.FMAX)
          FreqMax = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.FMAXAMP)
          FreqMaxAmp = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.CALL_LEN)
          DurationCall = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.CALL_DST)
          CallInterval = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.CALLNR)
          CallNr = f.getInt32();
        else if (f.Name == DBBAT.TEMP)
          Temperature = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.HUMI)
          Humidity = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == DBBAT.PRJ_NOTES)
          projects___Notes = f.getString();
        else if (f.Name == DBBAT.REM)
          calls___Remarks = f.getString();
      }
    }
  }
}
