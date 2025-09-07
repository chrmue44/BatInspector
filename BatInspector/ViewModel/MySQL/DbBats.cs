using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace BatInspector
{
  public class DbBats
  {
    DataBase _db;
    List<sqlRow> _queryResult = new List<sqlRow>();
    bool _isOpen = false;

    public List<sqlRow> QueryResult { get { return _queryResult; } set { _queryResult = value; } }
    public DbBats(DataBase db)
    {
      _db = db;
    }

    public bool IsOpen { get { return _isOpen; } }

    public void setIsOpen(bool isOpen) { _isOpen = isOpen; }

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
      if ((prj.Analysis == null) || (prj.Analysis.Files.Count == 0))
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
        for (int c = 0; c < prj.Analysis.Files[i].Calls.Count; c++)
        {
          retVal = addCall(fileId, prj.Analysis.Files[i].Calls[c]);
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

      if (!_db.checkIfOpen("addPrj"))
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
      List<sqlRow> res = _db.execQuery(sqlc);
      if (res.Count > 0)
      {
        DebugLog.log($"addProject not executed because project '{id}' already in db", enLogType.ERROR);
        return 3;
      }

      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append("INSERT INTO projects\n");
      sqlCmd.Append($"(id,Date,RecordingDevice,SwVersion,PrjCreator,{DBBAT.PATH_TO_WAV},MicrophoneId,Classifier,Model,{DBBAT.LOC},{DBBAT.PRJ_NOTES})\n");
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
      return _db.execNonQuery(sqlCmd.ToString());
    }

    public static string createFileId(string devName, string wavName)
    {
      return devName + "_" + wavName.ToLower().Replace(".wav", "");
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
      if (!_db.checkIfOpen("addFile"))
        return 1;

      string wav_name = file.Name;
      string id = createFileId(deviceName, wav_name);

      // check if file already in db
      string sqlc = $"SELECT id FROM projects WHERE id ='{id}';";
      List<sqlRow> res = _db.execQuery(sqlc);
      if (res.Count > 0)
      {
        DebugLog.log($"addFile not executed because project '{id}' already in db", enLogType.ERROR);
        return 3;
      }

      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append("INSERT INTO files\n");
      sqlCmd.Append($"(id,ProjectId,{DBBAT.WAV_FILE_NAME},{DBBAT.RECORDING_TIME},{DBBAT.LAT},{DBBAT.LAT},{DBBAT.TEMP},{DBBAT.HUMI},");
      sqlCmd.Append($"{DBBAT.SAMPLE_RATE},{DBBAT.FILE_LENGTH})\n");
      sqlCmd.Append("VALUES\n");
      id = id.Replace(".wav", "");
      id = id.Replace(".WAV", "");
      string lat = file.getDouble(Cols.LAT).ToString(CultureInfo.InvariantCulture);
      DateTime time = AnyType.getDate(file.getString(Cols.REC_TIME));
      string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");
      string lon = file.getDouble(Cols.LON).ToString(CultureInfo.InvariantCulture);
      string temp = file.getDouble(Cols.TEMPERATURE).ToString(CultureInfo.InvariantCulture);
      string hum = file.getDouble(Cols.HUMIDITY).ToString(CultureInfo.InvariantCulture);
      string sr = file.getInt(Cols.SAMPLERATE).ToString(CultureInfo.InvariantCulture);
      string fLen = file.getDouble(Cols.FILE_LEN).ToString(CultureInfo.InvariantCulture);
      sqlCmd.Append($"('{id}','{prjId}','{wav_name}','{timeStr}',{lat}, {lon}, {temp}, {hum},");
      sqlCmd.Append($"{sr}, {fLen});\n");
      return _db.execNonQuery(sqlCmd.ToString());
    }

    public int addCall(string fileId, AnalysisCall call)
    {
      if (!_db.checkIfOpen("addCall"))
        return 1;

      // check if call already in db
      string nr = call.getInt(Cols.NR).ToString("000");
      string id = fileId + "_" + nr;
      string sqlc = $"SELECT id FROM calls WHERE id ='{id}';";
      List<sqlRow> res = _db.execQuery(sqlc);
      if (res.Count > 0)
      {
        DebugLog.log($"addFile not executed because call '{id}' already in db", enLogType.ERROR);
        return 3;
      }

      StringBuilder sqlCmd = new StringBuilder();
      sqlCmd.Append("INSERT INTO calls\n");
      sqlCmd.Append($"(id,FileId,{DBBAT.CALLNR},{DBBAT.FMIN},{DBBAT.FMAX},{DBBAT.FMAXAMP},{DBBAT.CALL_LEN},");
      sqlCmd.Append($"{DBBAT.START_TIME},{DBBAT.CALL_DST},{DBBAT.BWIDTH},SNR,{DBBAT.SPEC_AUTO},{DBBAT.PROB},{DBBAT.SPEC_MAN},{DBBAT.REM})\n");
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
      return _db.execNonQuery(sqlCmd.ToString());
    }

    public AnalysisFile fillAnalysisFromQuery(QueryItem item)
    {
      if (_queryResult.Count == 0)
        return null;

      int colDuration = _queryResult[0].findField(DBBAT.FILE_LENGTH);
      int colWavFile = _queryResult[0].findField(DBBAT.WAV_FILE_NAME);
      int colSampleRate = _queryResult[0].findField(DBBAT.SAMPLE_RATE);
      int colRecTime = _queryResult[0].findField(DBBAT.RECORDING_TIME);
      int colLat = _queryResult[0].findField(DBBAT.LAT);
      int colLon = _queryResult[0].findField(DBBAT.LON);
      int colTemp = _queryResult[0].findField(DBBAT.TEMP);
      int colHumi = _queryResult[0].findField(DBBAT.HUMI);
      int colCalNr = _queryResult[0].findField(DBBAT.CALLNR);
      int colFmin = _queryResult[0].findField(DBBAT.FMIN);
      int colFmax = _queryResult[0].findField(DBBAT.FMAX);
      int colFmaxAmp = _queryResult[0].findField(DBBAT.FMAXAMP);
      int colSpecMan = _queryResult[0].findField(DBBAT.SPEC_MAN);
      int colSpecAuto = _queryResult[0].findField(DBBAT.SPEC_AUTO);
      int colProb = _queryResult[0].findField(DBBAT.PROB);
      int colCallInt = _queryResult[0].findField(DBBAT.CALL_DST);
      int colStartTime = _queryResult[0].findField(DBBAT.START_TIME);
      int colCallLen = _queryResult[0].findField(DBBAT.CALL_LEN);
      int colRem = _queryResult[0].findField(DBBAT.REM);
      int colSnr = _queryResult[0].findField(DBBAT.SNR);

      if (
           (colDuration < 0) || (colWavFile < 0) || (colSampleRate < 0) || (colRecTime < 0) ||
           (colLat < 0) || (colLon < 0) || (colTemp < 0) || (colHumi < 0) || (colCalNr < 0) ||
           (colFmin < 0) || (colFmax < 0) || (colFmaxAmp < 0) || (colSpecMan < 0) || (colSpecAuto < 0) ||
           (colProb < 0) || (colCallInt < 0) || (colStartTime < 0) || (colCallLen < 0) || (colRem < 0) ||
           (colSnr < 0)
         )
      {
        DebugLog.log("could not show call because of mssing fields in query", enLogType.ERROR);
        return null;
      }
      Csv report = ModelBatDetect2.createReport(Cols.SPECIES);
      string csvName = AppParams.AppParamsPath + "/temAnalysis.csv";

      int idx = -1;
      for (int i = 0; i < _queryResult.Count; i++)
      {
        if (_queryResult[i].Fields[colWavFile].getString() == item.WavFileName)
        {
          idx = i;
          break;
        }
      }

      while ((_queryResult[idx].Fields[colWavFile].getString() == item.WavFileName) && (idx < _queryResult.Count))
      {
        report.addRow();
        int repRow = report.RowCnt;
        report.setCell(repRow, Cols.SAMPLERATE, _queryResult[idx].Fields[colSampleRate].getInt32());
        report.setCell(repRow, Cols.FILE_LEN, _queryResult[idx].Fields[colDuration].getFloat());
        report.setCell(repRow, Cols.REC_TIME, _queryResult[idx].Fields[colRecTime].getFloat());
        report.setCell(repRow, Cols.NAME, _queryResult[idx].Fields[colWavFile].getString());
        report.setCell(repRow, Cols.LAT, _queryResult[idx].Fields[colLat].getFloat());
        report.setCell(repRow, Cols.LON, _queryResult[idx].Fields[colLon].getFloat());
        report.setCell(repRow, Cols.TEMPERATURE, _queryResult[idx].Fields[colTemp].getFloat());
        report.setCell(repRow, Cols.HUMIDITY, _queryResult[idx].Fields[colHumi].getFloat());
        report.setCell(repRow, Cols.NR, _queryResult[idx].Fields[colHumi].getInt32());
        report.setCell(repRow, Cols.START_TIME, _queryResult[idx].Fields[colStartTime].getFloat());
        report.setCell(repRow, Cols.F_MIN, _queryResult[idx].Fields[colFmin].getFloat());
        report.setCell(repRow, Cols.F_MAX, _queryResult[idx].Fields[colFmax].getFloat());
        report.setCell(repRow, Cols.F_MAX_AMP, _queryResult[idx].Fields[colFmaxAmp].getFloat());
        report.setCell(repRow, Cols.DURATION, _queryResult[idx].Fields[colCallLen].getFloat());
        report.setCell(repRow, Cols.NR, _queryResult[idx].Fields[colCalNr].getInt32());
//           report.setCell(repRow, Cols.BANDWIDTH, bandwidth);
        report.setCell(repRow, Cols.CALL_INTERVALL, _queryResult[idx].Fields[colCallInt].getFloat());
        report.setCell(repRow, Cols.DURATION, _queryResult[idx].Fields[colCallLen].getFloat());
        report.setCell(repRow, Cols.SNR, _queryResult[idx].Fields[colSnr].getFloat());
        report.setCell(repRow, Cols.PROBABILITY, _queryResult[idx].Fields[colProb].getFloat());
        report.setCell(repRow, Cols.SPECIES, _queryResult[idx].Fields[colSpecAuto].getString());
        report.setCell(repRow, Cols.SPECIES_MAN, _queryResult[idx].Fields[colSpecMan].getString());
        report.setCell(repRow, Cols.REMARKS, _queryResult[idx].Fields[colRem].getString()); 
        idx++;
      }
//      report.saveAs(csvName);
      DateTime recTime = DateTime.Parse(item.RecordingTime);
      AnalysisFile file = new AnalysisFile(report, item.WavFileName,  2, recTime);
      for(int r = 2; r < report.RowCnt; r++)
      {
        AnalysisCall call = new AnalysisCall(report, r, false);
        file.addCall(call, true);
      }

      return file;

    }
  }
}