/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using BatInspector.Forms;
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;


namespace BatInspector
{
  public class PrjInfo
  {
    public string Name { get; set; }
    public string SrcDir { get;set; }
    public string DstDir { get;set; }
    public int MaxFileCnt { get; set; } 
    public double MaxFileLenSec { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Weather { get; set; } 
    public string Landscape { get; set; } 
    public string GpxFile { get; set; }
    public bool LocSourceGpx { get; set; }
    public bool LocSourceKml { get; set; }
    public bool LocSourceTxt { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsProjectFolder { get; set; } 
    public bool OverwriteLocation { get; set; }
    public bool RemoveSource { get; set; }
    public string WavSubDir { get; set; } = AppParams.DIR_WAVS;

  }


  public abstract class PrjBase
  {
    protected List<string> _speciesList;

    protected List<SpeciesInfos> _speciesInfo;

    protected Analysis _analysis;
    protected BatSpeciesRegions _batSpecRegions;

    public List<SpeciesInfos> SpeciesInfos { get { return _speciesInfo; } }
    public List<string> Species { get { return _speciesList; } }
    public Analysis Analysis { get { return _analysis; } }

    public PrjBase(List<SpeciesInfos> speciesInfo, BatSpeciesRegions batSpecRegions, DlgUpdateFile dlgUpdate)
    {
      _speciesList = new List<string>();
      _speciesInfo = speciesInfo;
      _analysis = new Analysis(this.SpeciesInfos, dlgUpdate);
      _batSpecRegions = batSpecRegions;
    }

    public abstract string getFullFilePath(string path);
    public abstract BatExplorerProjectFileRecordsRecord[] getRecords(); 

    /// <summary>
    /// init the list of species valid for the project.
    /// The list is depending on the region where the first file of the project was recorded
    /// </summary>
    protected void initSpeciesList()
    {
      BatExplorerProjectFileRecordsRecord[] records = getRecords();
      if (records.Length > 0)
      {
        string fName = getFullFilePath(records[0].File);
        fName = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        BatRecord info = ElekonInfoFile.read(fName);
        ElekonInfoFile.parsePosition(info, out double lat, out double lon);
        ParRegion reg = _batSpecRegions.findRegion(lat, lon);
        _speciesList = new List<string>();
        if (reg != null)
        {
          foreach (string sp in reg.Species)
            _speciesList.Add(sp);
          _speciesList.Add("?");
          _speciesList.Add("Social");
        }
        else
        {
          foreach (SpeciesInfos sp in _speciesInfo)
            _speciesList.Add(sp.Abbreviation);
        }
        _speciesList.Add("todo");
      }
    }

  }

  public class Project : PrjBase
  {
    private string _prjFileName;
    private string _wavSubDir;
    private bool _ok;
    private string _selectedDir;
    private BatExplorerProjectFile _batExplorerPrj;
    private bool _reloadInGui;
    
    public bool Ok { get { return _ok; } }

    public BatExplorerProjectFileRecordsRecord[] Records
    {
      get {
        if (_batExplorerPrj.Records != null)
          return _batExplorerPrj.Records;
        else
          return null;
      }
    }
    public string Name { get { return _prjFileName; } }
    public string WavSubDir { get { return _wavSubDir; } }
    public string Notes
    {
      get
      {
        return _batExplorerPrj != null ? _batExplorerPrj.Notes : "";
      }
      set
      {
        if (_batExplorerPrj != null)
        {
          _batExplorerPrj.Notes = value;
          _changed = true;
        }
      }
    }
    public string Created
    {
      get
      {
        if (_batExplorerPrj != null)
          return _batExplorerPrj.Created;
        else
          return "";
      }
      set { _batExplorerPrj.Created = value; } }
    public string PrjDir { get { return _selectedDir; } }


    public string ReportName { get { return getReportName(_selectedDir); } }

    public string SummaryName { get { return getSummaryName(_selectedDir); } }

    public bool ReloadInGui { get { return _reloadInGui; } set { _reloadInGui = value; } }


    bool _changed = false;

    public Project(BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo, DlgUpdateFile dlgUpdate, string wavSubDir = "")
    : base(speciesInfo, regions, dlgUpdate)
    {
      _wavSubDir = wavSubDir;
    }


    public static string getReportName(string dir)
    {
      return Path.Combine (dir ,AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir, AppParams.PRJ_REPORT);
    }

    public static string getSummaryName(string dir)
    {
      return Path.Combine(dir, AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir, AppParams.PRJ_SUMMARY);
    }

    /// <summary>
    /// checks wether a directory contains a project
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string containsProject(DirectoryInfo dir)
    {
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_PRJ,
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          return files[0];
      }
      catch { }
      return "";
    }

    /// <summary>
    /// checks if a directory contains a report
    /// </summary>
    /// <param name="dir">directory info of directory to check</param>
    /// <param name="repName">naem of the report to search for</param>
    /// <returns></returns>
    public static string containsReport(DirectoryInfo dir, string repName)
    {
      string dirName = dir.FullName + "/" + AppParams.Inst.Models[AppParams.Inst.SelectedModel].Dir;
      if (Directory.Exists(dirName))
      {
        try
        {
          string[] files = System.IO.Directory.GetFiles(dirName, "*.*",
                           System.IO.SearchOption.TopDirectoryOnly);
          foreach (string file in files)
          {
            if (file.IndexOf(repName) >= 0)
              return file;
          }
        }
        catch { }
      }
      return "";
    }

    /// <summary>
    /// checks wether a directory contains wav files
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool containsWavs(DirectoryInfo dir)
    {
      bool retVal = false;
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_WAV,
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          retVal = true;
      }
      catch { }
      return retVal;
    }

    /// <summary>
    /// checks wether the manual evaluation of calls is completed (no more todo entries) or not
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool evaluationDone(DirectoryInfo dir)
    {
      bool retVal = false;
      string sumName = Project.getSummaryName(dir.FullName);
      if (File.Exists(sumName))
      {
        Csv csv = new Csv();
        csv.read(sumName, ";", true);
        int col = csv.findInCol("todo", Cols.SPECIES_MAN);
        retVal = (col == 0);
      }
      else
      {
        string fName = Project.getReportName(dir.FullName);
        if (File.Exists(fName))
        {
          Csv csv = new Csv();
          {
            csv.read(fName, ";", true);
            int colSp = csv.findInRow(1, Cols.SPECIES_MAN);
            if (colSp > 0)
            {
              int row = csv.findInCol("todo", colSp);
              if (row == 0)
                retVal = true;
            }
          }
        }
      }

      return retVal;
    }


    /// <summary>
    /// returns the selected files according to search pattern and file creation time
    /// </summary>
    /// <param name="prjInfo"></param>
    /// <param name="searchPattern"></param>
    /// <returns>a list of file names</returns>
    private static string[] getSelectedFiles(PrjInfo prjInfo, string searchPattern)
    {
      string[] files = Directory.GetFiles(prjInfo.SrcDir, searchPattern);
      List<string> strings = new List<string>();
      foreach (string file in files)
      {
        DateTime fileTime = ElekonInfoFile.getDateTimeFromFileName(file);
        if ((prjInfo.StartTime <= fileTime) && (fileTime <= prjInfo.EndTime))
          strings.Add(file);
      }
      return strings.ToArray();
    }

    /// <summary>
    /// create one or multiple projects from a directory containing a project
    /// </summary>
    /// <param name="info">parameters to specify project</param>
    /// <param name="regions"></param>
    /// <param name="speciesInfo"></param>
   /* public static void splitPrj(PrjInfo info, BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      string wavDir = Path.Combine(info.SrcDir, AppParams.DIR_WAVS);
      string wavSubDir = "";
      if (Directory.Exists(wavDir))
        wavSubDir = AppParams.DIR_WAVS;

      Project prjSrc = new Project(regions, speciesInfo, null, wavSubDir);
      string fName = Path.Combine(info.SrcDir, info.Name) + ".bpr";
      prjSrc.readPrjFile(fName);
      string[] notes = prjSrc.Notes.Split('\n');

      info.SrcDir = Path.Combine(info.SrcDir, prjSrc.WavSubDir);
      info.Landscape = notes[0];
      info.StartTime = new DateTime(1900, 1, 1);
      info.EndTime = new DateTime(2100, 1, 1);
      if (notes.Length > 1)
        info.Weather = notes[1];

      createPrj(info, regions, speciesInfo);
    } */

    private static bool removeAppleTempFiles(PrjInfo info)
    {
      bool retVal = true;
      string[] files = getSelectedFiles(info, "._*.wav");
      if (files.Length > 0)
      {
        MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.msgDeleteIntermediate,
                                               BatInspector.Properties.MyResources.msgQuestion,
                                               MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res == MessageBoxResult.Yes)
        {
          foreach (string file in files)
            File.Delete(file);
        }
        else
        {
          DebugLog.log("project creation aborted", enLogType.INFO);
          retVal = false;
        }
      }
      return retVal;
    }

    private static bool readGpxFile(PrjInfo info, out gpx gpxFile)
    {
      bool retVal = true;
      gpxFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceGpx)
        {
          gpxFile = gpx.read(info.GpxFile);
          if (gpxFile == null)
          {
            DebugLog.log("gpx file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    private static bool readKmlFile(PrjInfo info, out kml kmlFile)
    {
      bool retVal = true;
      kmlFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceKml)
        {
          kmlFile = kml.read(info.GpxFile);
          if (kmlFile == null)
          {
            DebugLog.log("kml file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;
    }

    private static bool readLoctxtFile(PrjInfo info, LocFileSettings pars, out LocFileTxt txtFile)
    {
      bool retVal = true;
      txtFile = null;
      // read gpx file if needed
      if (info.OverwriteLocation)
      {
        if (info.LocSourceTxt)
        {
          txtFile = LocFileTxt.read(info.GpxFile, pars);
          if (txtFile == null)
          {
            DebugLog.log("txt location file not readable: " + info.GpxFile, enLogType.ERROR);
            retVal = false;
          }
        }
      }
      return retVal;

    }

    private static bool createPrjDirStructure(string prjDir, out string wavDir, string wavSubDir)
    {
      bool retVal = true;
      DebugLog.log("creating project " + prjDir, enLogType.INFO);
      if (string.IsNullOrEmpty(wavSubDir))
        wavDir = prjDir;
      else
        wavDir =Path.Combine(prjDir,wavSubDir);
      if (Directory.Exists(prjDir))
      {
        DebugLog.log("directory '" + prjDir + "' already exists, project creation of folder structure aborted!", enLogType.WARNING);
        retVal = false;
      }
      else
        Directory.CreateDirectory(prjDir);
      if (!Directory.Exists(wavDir))
        Directory.CreateDirectory(wavDir);
      return retVal;
    }

    /// <summary>
    /// create one or multiple projects from a directory containing WAV files
    /// </summary>
    /// <param name="info">parameters to specify project</param>
    /// <param name="regions"></param>
    /// <param name="speciesInfo"></param>
    public static void createPrjFromWavs(PrjInfo info, BatSpeciesRegions regions, List<SpeciesInfos> speciesInfo)
    {
      if ((info.MaxFileCnt == 0) || info.MaxFileLenSec == 0)
      {
        DebugLog.log("error creating project, maxFiles or maxFileLen == 0", enLogType.ERROR);
        return;
      }
      try
      {
        DebugLog.log("start creating project(s): " + info.Name, enLogType.INFO);
        
        // in case of project folder get infos from project
        if (info.IsProjectFolder)
        {
          string wavDir = Path.Combine(info.SrcDir, info.WavSubDir);

          Project prjSrc = new Project(regions, speciesInfo, null, info.WavSubDir);
          string fName = Path.Combine(info.SrcDir, info.Name) + ".bpr";
          prjSrc.readPrjFile(fName);
          string[] notes = prjSrc.Notes.Split('\n');

          info.SrcDir = Path.Combine(info.SrcDir, prjSrc.WavSubDir);
          info.Landscape = notes[0];
          info.StartTime = new DateTime(1900, 1, 1);
          info.EndTime = new DateTime(2100, 1, 1);
          if (notes.Length > 1)
            info.Weather = notes[1];
        }

        string[] files = getSelectedFiles(info, "*.wav");
        if (files.Length > 0)
        {
          // copy files to a single project at destination and create project
          DebugLog.log("copy wav files...", enLogType.INFO);
          string fullDir = Path.Combine(info.DstDir, info.Name);
          createPrjDirStructure(fullDir, out string wavDir, info.WavSubDir);
          Utils.copyFiles(files, wavDir, info.RemoveSource);
          string[] xmlFiles = getSelectedFiles(info, "*.xml");
          DebugLog.log("copy xml files...", enLogType.INFO);
          Utils.copyFiles(xmlFiles, wavDir, info.RemoveSource);

          // create project
          Project prj = new Project(regions, speciesInfo, null);
          DirectoryInfo dir = new DirectoryInfo(fullDir);
          DebugLog.log("creating project...", enLogType.INFO);
          prj.fillFromDirectory(dir, info.WavSubDir, info.Landscape + "\n" + info.Weather);

          // copy location files is present
          DebugLog.log("copy location files...", enLogType.INFO);
          string[] gpxFiles = Directory.GetFiles(info.SrcDir, "*.gpx");
          Utils.copyFiles(gpxFiles, prj.PrjDir, info.RemoveSource);
          string[] kmlFiles = Directory.GetFiles(info.SrcDir, "*.kml");
          Utils.copyFiles(kmlFiles, prj.PrjDir, info.RemoveSource);
          string[] txtFiles = Directory.GetFiles(info.SrcDir, "*.txt");
          Utils.copyFiles(txtFiles, prj.PrjDir, info.RemoveSource);

          if (!info.IsProjectFolder)
            prj.createXmlInfoFiles(info);

          // replace gpx locations
          xmlFiles = Directory.GetFiles(wavDir, "*.xml");
          if (info.OverwriteLocation && info.LocSourceGpx)
          {
            bool ok = readGpxFile(info, out gpx gpxFile);
            if (ok)
              replaceLocations(xmlFiles, wavDir, gpxFile);
            else
              DebugLog.log("error reading GPX file, could not generate location information", enLogType.ERROR);
          }
          else if (info.OverwriteLocation && info.LocSourceKml)
          {
            bool ok = readKmlFile(info, out kml kmlFile);
            if (ok)
              replaceLocations(xmlFiles, wavDir, kmlFile);
            else
              DebugLog.log("error reading KML file, could not generate location information", enLogType.ERROR);
          }
          else if (info.OverwriteLocation && info.LocSourceTxt)
          {
            bool ok = readLoctxtFile(info, AppParams.Inst.LocFileSettings, out LocFileTxt txtFile);
            if (ok)
              replaceLocations(xmlFiles, wavDir, txtFile);
            else
              DebugLog.log("error reading TXT file, could not generate location information", enLogType.ERROR);
          }
          else if (info.OverwriteLocation)
            replaceLocations(xmlFiles, wavDir, info.Latitude, info.Longitude);

          splitFiles(prj, info);
        }
        else
          DebugLog.log("No WAV files in directory " + info.SrcDir, enLogType.ERROR);
        // remove src project
        if(info.RemoveSource)
          Directory.Delete(info.SrcDir, true);
      }
      catch (Exception e)
      {
        DebugLog.log("error creating Project " + info.Name + " " + e.ToString(), enLogType.ERROR);
      }
      return;
    }

    /// <summary>
    /// split all files in prj that are longer than the specified max. length
    /// </summary>
    /// <param name="prj"></param>
    /// <param name="info"></param>
    private static void splitFiles(Project prj, PrjInfo info)
    {
      DebugLog.log("split files exceeding max. length", enLogType.INFO);

      string prjWavDir = Path.Combine(prj.PrjDir, prj.WavSubDir);
      string[] files = Directory.GetFiles(prjWavDir, "*" + AppParams.EXT_WAV);
      WavFile wavFile = new WavFile();
      wavFile.readFile(files[0]);
      uint sampleRate = wavFile.FormatChunk.Frequency;
      int maxFileLen = (int)((double)sampleRate * 2 * info.MaxFileLenSec + 2048);

      foreach (string f in files)
      {
        FileInfo fileInfo = new FileInfo(f);
        if (fileInfo.Length > maxFileLen)
        {
          string[] names = Project.splitWav(f, info.MaxFileLenSec);
          Project.createSplitXmls(f, names, info.MaxFileLenSec);
          string oldXml = f.Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
          if (File.Exists(oldXml))
            File.Delete(oldXml);
        }
      }
      prj.fillFromDirectory(new DirectoryInfo(prj.PrjDir), prj.WavSubDir, info.Landscape + "\n" + info.Weather);
    }


    /// <summary>
    /// split a project into multiple projects
    /// </summary>
    /// <param name="prj">the project to split</param>
    public static List<string> splitProject(Project prj, int prjCnt, BatSpeciesRegions regions)
    {
      List<string> retVal = new List<string>();
      string wavSrc = Path.Combine(prj.PrjDir, prj.WavSubDir);
      string[] files = Directory.GetFiles(wavSrc, "*.wav");
      int fileCnt = files.Length / prjCnt;

      for (int p = 0; p < prjCnt; p++)
      { 
        int iFirst = p * fileCnt;
        if((p + 1) == prjCnt) 
          fileCnt = files.Length - iFirst;
        string dirName = prj.PrjDir + "_" + p.ToString("D2");
        bool ok = createPrjDirStructure(dirName, out string wavDir, prj.WavSubDir);
        if (!ok)
          return retVal;
        DebugLog.log("Creating project " + dirName, enLogType.INFO);

        string[] prjFiles = new string[fileCnt];
        Array.Copy(files, iFirst, prjFiles, 0, fileCnt);
        Utils.copyFiles(prjFiles, wavDir, true);

        string[] xmlFiles = new string[prjFiles.Length];
        for (int i = 0; i < xmlFiles.Length; i++)
          xmlFiles[i] = prjFiles[i].ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        Utils.copyFiles(xmlFiles, wavDir, true);

        string[] gpxFiles = Directory.GetFiles(prj.PrjDir, "*.gpx");
        Utils.copyFiles(gpxFiles, dirName);
        string[] kmlFiles = Directory.GetFiles(prj.PrjDir, "*.kml");
        Utils.copyFiles(kmlFiles, dirName);
        string[] txtFiles = Directory.GetFiles(prj.PrjDir, "*.txt");
        Utils.copyFiles(txtFiles, dirName);

        Project dstprj = new Project(regions, prj.SpeciesInfos, null);
        dstprj.fillFromDirectory(new DirectoryInfo(dirName), AppParams.DIR_WAVS, prj.Notes);

        copyAnalysisPart(prj, dstprj);
        retVal.Add(dirName);
      }
      return retVal;
    }

    private static void copyAnalysisPart(Project prjSrc, Project prjDest)
    {
      foreach(BatExplorerProjectFileRecordsRecord rec in prjDest.Records)
      {
        AnalysisFile f = prjSrc.Analysis.getAnalysis(rec.File);
        if(f != null)
        {
          prjDest.Analysis.addFile(f, true);
        }
      }
      string dir = Path.GetDirectoryName(prjDest.ReportName);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
      prjDest.Analysis.save(prjDest.ReportName, prjDest.Notes);
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, gpx gpxFile)
    {
      DebugLog.log("replace locations from gpx file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      { 
        BatRecord f = ElekonInfoFile.read(fName);
        DateTime t = ElekonInfoFile.getDateTimeFromFileName(fName);
        double[] pos = gpxFile.getPosition(t);
        if ((pos == null) || (pos.Length < 2) || ((pos[0] == 0.0) && (pos[1] == 0.0)))
          DebugLog.log("no position found for " + fName + ", timestamp: " + t.ToString(), enLogType.ERROR);
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, kml kmlFile)
    {
      DebugLog.log("replace locations from kml file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
		    double[] posOld = {90,0};
        double[] pos = kmlFile.getPosition(fName);
	      if(pos[0] < 1e-6)
	        pos = posOld;
        else
          posOld = pos;
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, LocFileTxt txtFile)
    {
      DebugLog.log("replace locations from txt file...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        double[] posOld = { 90, 0 };
        DateTime t = AnyType.getDate(f.DateTime);
        double[] pos = txtFile.getPosition(fName, t);
        if (pos[0] < 1e-6)
          pos = posOld;
        else
          posOld = pos;
        f.GPS.Position = pos[0].ToString(CultureInfo.InvariantCulture) + " " + pos[1].ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    private static void replaceLocations(string[] xmlfiles, string wavDir, double lat, double lon)
    {
      DebugLog.log("replace locations with fix location...", enLogType.INFO);
      foreach (string fName in xmlfiles)
      {
        BatRecord f = ElekonInfoFile.read(fName);
        f.GPS.Position = lat.ToString(CultureInfo.InvariantCulture) + " " + lon.ToString(CultureInfo.InvariantCulture);
        string dstName = Path.GetFileName(fName);
        dstName = Path.Combine(wavDir, dstName);
        ElekonInfoFile.write(dstName, f);
      }
    }

    public void readPrjFile(string fName)
    {
      try
      {
        _prjFileName = fName;
        _selectedDir = Path.GetDirectoryName(fName);
        string xml = File.ReadAllText(fName);
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        _speciesList.Clear();
        TextReader reader = new StringReader(xml);
        _batExplorerPrj = (BatExplorerProjectFile)serializer.Deserialize(reader);
        if (_batExplorerPrj.Created == null)
          _batExplorerPrj.Created = "";
        if (_batExplorerPrj.Notes == null)
          _batExplorerPrj.Notes = "";
        if (Directory.Exists(Path.Combine(_selectedDir, AppParams.DIR_WAVS)))
          _wavSubDir = AppParams.DIR_WAVS;
        else
          _wavSubDir = "";
        initSpeciesList();
        _ok = true;
        _changed = false;
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading project file: " + fName + " :" + ex.ToString(), enLogType.ERROR);
        _ok = false;
      }
    }

    public void writePrjFile()
    {
      if ((_batExplorerPrj != null) && _changed)
      {
        _batExplorerPrj.FileVersion = "3";
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        TextWriter writer = new StreamWriter(_prjFileName);
        serializer.Serialize(writer, _batExplorerPrj);
        writer.Close();
        _changed = false;
        DebugLog.log("project '" + _prjFileName + "' saved", enLogType.INFO);
      }
    }

    public void removeFile(string wavName)
    {
      List<BatExplorerProjectFileRecordsRecord> list = _batExplorerPrj.Records.ToList();
      foreach (BatExplorerProjectFileRecordsRecord rec in list)
      {
        if (rec.File == wavName)
        {
          list.Remove(rec);
          _changed = true;
          break;
        }
      }
      _batExplorerPrj.Records = list.ToArray();
    }

    public void removeFilesNotInReport()
    {
      List<BatExplorerProjectFileRecordsRecord> newList = new List<BatExplorerProjectFileRecordsRecord>();
      string destDir = Path.Combine(this.PrjDir, AppParams.DIR_DEL);
      if (!Directory.Exists(destDir))
        Directory.CreateDirectory(destDir);

      foreach (BatExplorerProjectFileRecordsRecord rec in _batExplorerPrj.Records)
      {
        if (Analysis.find(rec.Name) == null)
        {
          string name = Path.Combine(PrjDir, WavSubDir, rec.File);
          DebugLog.log("delete file " + name, enLogType.DEBUG);
          try
          {
            if (File.Exists(name))
              File.Move(name, Path.Combine(destDir, Path.GetFileName(name)));
            name = name.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
            if (File.Exists(name))
              File.Move(name, Path.Combine(destDir, Path.GetFileName(name)));
            name = name.Replace(AppParams.EXT_INFO, AppParams.EXT_IMG);
            if (File.Exists(name))
              File.Move(name, Path.Combine(destDir, Path.GetFileName(name)));
            _changed = true;
          }
          catch (Exception ex)
          {
            DebugLog.log("error removing files from project: " + name + ", " + ex.ToString(), enLogType.ERROR);
          }
        }
        else
          newList.Add(rec);
      }
      _batExplorerPrj.Records = newList.ToArray();
      writePrjFile();
    }


    /// <summary>
    /// find a file in the project
    /// </summary>
    /// <param name="fileName">name of the file (full path or just file name)</param>
    /// <returns>record containing the file information</returns>
    public BatExplorerProjectFileRecordsRecord find(string fileName)
    {
      BatExplorerProjectFileRecordsRecord retVal = null;
      if (fileName != null)
      {
        foreach (BatExplorerProjectFileRecordsRecord r in _batExplorerPrj.Records)
        {
          if (fileName.ToLower().Contains(r.File.ToLower()))
          {
            retVal = r;
            break;
          }
        }
      }
      return retVal;
    }


    public void addFiles(string[] files, bool removeSrc = false)
    {
      List<BatExplorerProjectFileRecordsRecord> list = new List<BatExplorerProjectFileRecordsRecord>();
      foreach (BatExplorerProjectFileRecordsRecord rec in _batExplorerPrj.Records)
        list.Add(rec);
      foreach (string file in files)
      {
        _changed = true;
        addRecord(file, ref list);
        if (_analysis?.IsEmpty == false)
        {
          WavFile wFile = new WavFile();
          wFile.readFile(file);
          _analysis.addFile(ReportName, file, (int)wFile.FormatChunk.Frequency,
                            (double)wFile.AudioSamples.Length / wFile.FormatChunk.Frequency);
        }
      }

      _batExplorerPrj.Records = list.ToArray();
      Array.Sort(_batExplorerPrj.Records);

      string dstPath = Path.Combine(PrjDir, _wavSubDir);
      List<string> fileList = new List<string>();
      foreach (string file in files)
      {
        fileList.Add(file);
        string xmlFile = file.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        if (File.Exists(xmlFile))
          fileList.Add(xmlFile);
      }
      Utils.copyFiles(fileList.ToArray(), dstPath, removeSrc);
      writePrjFile();
      if(_analysis?.IsEmpty == false)
        _analysis?.save(ReportName, Notes);
      _reloadInGui = true;
    }


    private void addRecord(string filePath, ref List<BatExplorerProjectFileRecordsRecord> list)
    {
      BatExplorerProjectFileRecordsRecord rec = new BatExplorerProjectFileRecordsRecord();
      string name = Path.GetFileName(filePath);
      rec.File = name;
      name = Path.GetFileNameWithoutExtension(filePath);
      rec.Name = name;
      list.Add(rec);
    }



    private static void createSplitXmls(string fName, string[] newNames, double maxLen)
    {
      fName = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      double deltaT = 0;
      DateTime t = new DateTime(1900,1,1);
      BatRecord rec = ElekonInfoFile.read(fName);
      foreach (string newName in newNames)
      {
        string xmlName = newName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
        if(rec != null)
        {
          if(t.Year < 1910)
            t = AnyType.getDate(rec.DateTime);
          t =  t.AddMilliseconds(deltaT * 1000);
          deltaT += maxLen;
          rec.DateTime = ElekonInfoFile.getDateString(t);
          ElekonInfoFile.write(xmlName, rec);
        }
      }
    }

    private static string[] splitWav(string fName, double size)
    {
      WavFile wav = new WavFile();
      string[] retVal = null;
      try
      {
        wav.readFile(fName);
        double len = wav.AudioSamples.Length / wav.FormatChunk.Frequency / size;
        int cnt = (int)len;
        if ((double)cnt < len)
          cnt++;
        retVal = new string[cnt];
        for (int i = 0; i < cnt; i++)
        {
          int iStart = (int)((double)i * wav.FormatChunk.Frequency * size);
          int iEnd = iStart + (int)((double)wav.FormatChunk.Frequency * size) - 1;
          if (iEnd > (wav.AudioSamples.Length - 1))
            iEnd = wav.AudioSamples.Length - 1;
          short[] samples = new short[iEnd - iStart + 1];
          Array.Copy(wav.AudioSamples, iStart, samples, 0, iEnd - iStart + 1);
          WavFile newWav = new WavFile();
          newWav.createFile((int)wav.FormatChunk.Frequency, 0, samples.Length - 1, samples);
          string name = Path.GetDirectoryName(fName) + "/" +
                        Path.GetFileNameWithoutExtension(fName) + "_" + i.ToString("D3") + Path.GetExtension(fName);
          newWav.saveFileAs(name);
          retVal[i] = name;
        }
        File.Delete(fName);
      }
      catch (Exception ex)
      {
        DebugLog.log("Error splitting wav file: " + fName + ":" + ex.ToString(), enLogType.ERROR);
      }
      return retVal;
    }


    /// <summary>
    /// create an elekon compatible project from a directory that contains wav files
    /// </summary>
    /// <param name="dir">directory info of prj directory</param>
    /// <param name="wavSubDir">subdir with leading '/' for wav files</param>
    public void fillFromDirectory(DirectoryInfo dir, string wavSubDir = "", string notes = "")
    {
      try
      {
        string dirName = Path.Combine(dir.FullName, wavSubDir);
        string[] files = System.IO.Directory.GetFiles(dirName, "*.wav",
                         System.IO.SearchOption.TopDirectoryOnly);
        List<BatExplorerProjectFileRecordsRecord> records = new List<BatExplorerProjectFileRecordsRecord>();
        _selectedDir = dir.FullName;
        for (int i = 0; i < files.Length; i++)
          addRecord(files[i], ref records);

        _batExplorerPrj = new BatExplorerProjectFile("wavs", records);
        _wavSubDir = wavSubDir;
        _ok = true;
        _prjFileName = Path.GetFileName(dir.FullName);
        _batExplorerPrj.Name = _prjFileName;
        _prjFileName = Path.Combine(dir.FullName , _prjFileName + ".bpr");
        _changed = true;
        Created = DateTime.Now.ToString(AppParams.GPX_DATETIME_FORMAT);
        Notes = notes;
        writePrjFile();
      }
      catch (Exception ex)
      {
        DebugLog.log("error creating prject: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void createXmlInfoFiles(PrjInfo info)
    {
      bool replaceAll = false;
      DebugLog.log("creating xml infofcreate files...", enLogType.INFO);

      //gpx gpxFile = gpx.read(info.GpxFile);
      foreach (BatExplorerProjectFileRecordsRecord record in _batExplorerPrj.Records)
      {
        bool create = replaceAll;
        string fullName = Path.Combine(_selectedDir, _wavSubDir, record.File);
        if (File.Exists(fullName))
        {
          if (!replaceAll && File.Exists(fullName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO)))
          {
            MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.ProjectmsgReplaceInfo, 
                                                   BatInspector.Properties.MyResources.msgQuestion, 
                                                   MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
              replaceAll = true;
              create = true;
            }
          }
          else
            create = true;
          if (create)
          {
            DateTime time = ElekonInfoFile.getDateTimeFromFileName(fullName);
            double[] pos = new double[2];
            pos[0] = info.Latitude;
            pos[1] = info.Longitude;
       //     if (gpxFile != null)
       //       pos = gpxFile.getPosition(time);
            ElekonInfoFile.create(fullName, pos[0], pos[1], time);
          }
        }
      }
    }

    public void recovery(bool delFiles, bool changedFiles)
    {
      if (changedFiles)
      {
        ColorTable colorTable = new ColorTable();
        colorTable.createColorLookupTable();

        string srcDir = Path.Combine(PrjDir, AppParams.DIR_ORIG);
        string dstDir = Path.Combine(PrjDir, WavSubDir);
        DirectoryInfo dir = new DirectoryInfo(srcDir);
        foreach (FileInfo file in dir.GetFiles())
        {
          string wavName = "";
          if (Path.GetExtension(file.FullName).ToLower() == AppParams.EXT_CSV)
          {
            wavName = Path.GetFileNameWithoutExtension(file.FullName) + AppParams.EXT_WAV;
            _analysis.undo(file.FullName);
            File.Delete(file.FullName);
            _analysis.save(ReportName, Notes);
            _analysis.read(ReportName);
            _analysis.updateControls(wavName);
          }
          else
          {
            string dstFile = Path.Combine(dstDir, file.Name);
            if (File.Exists(dstFile))
              File.Delete(dstFile);
            File.Copy(file.FullName, dstFile);
            File.Delete(file.FullName);
            if (Path.GetExtension(file.FullName).ToLower() == AppParams.EXT_WAV)
            {
              string pngName = file.FullName.Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
              ViewModel.createPng(file.FullName, pngName, AppParams.FFT_WIDTH, colorTable);
            }
          }
        }
        DebugLog.log("original files of Prj '" + Name + "' recovered", enLogType.INFO);
      }
      if (delFiles)
      {
        string srcDir = Path.Combine(PrjDir, AppParams.DIR_DEL);
        DirectoryInfo dir = new DirectoryInfo(srcDir);
        List<string> files = new List<string>();
        foreach (FileInfo file in dir.GetFiles("*.wav"))
          files.Add(file.FullName);
        addFiles(files.ToArray(), true);
        DebugLog.log("deleted files of Prj '" + Name + "' recovered", enLogType.INFO);
      }
    }

    public override string getFullFilePath(string wavName)
    {
      string retVal = Path.Combine(_selectedDir, _wavSubDir, wavName);
      return retVal;
    }

    public override BatExplorerProjectFileRecordsRecord[] getRecords() 
    {
      return _batExplorerPrj.Records;
    }

    public static bool parseLatitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "N", "S", 90);
    }


    public static bool parseLongitude(string coordStr, out double coord)
    {
      return parseGeoCoord(coordStr, out coord, "E", "W", 180);
    }

    /// <summary>
    /// parse geographical coordinates, two formats are allowed:
    /// 1.: plain double e.g. 49.657489, -33.5679864
    /// 2.: degrees and minutes e.g. "49° 38.012 N", "8° 37.443 W"
    /// </summary>
    /// <param name="coordStr">coordinat as string</param>
    /// <param name="coord">output coordinate</param>
    /// <param name="hem1">hemesphere character positive direction</param>
    /// <param name="hem2">hemisphere character negative direction</param>
    /// <param name="maxDeg">max value for degrees</param>
    /// <returns></returns>
    static bool parseGeoCoord(string coordStr, out double coord, string hem1, string hem2, int maxDeg)
    {
      bool retVal = true;
      bool ok = double.TryParse(coordStr, NumberStyles.Any, CultureInfo.InvariantCulture, out coord);
      if (ok)
      {
        if ((coord < -maxDeg) || (coord > maxDeg))
          retVal = false;
      }
      else
      {
        int deg = 0;
        int sign = 0;
        int pos = coordStr.IndexOf("°");
        if (pos >= 0)
        {
          string degStr = coordStr.Substring(0, pos);
          retVal = int.TryParse(degStr, out deg);
        }
        else
          retVal = false;
        coord = 0;
        int pos2 = coordStr.IndexOf(hem1);
        if (pos2 < 0)
        {
          pos2 = coordStr.IndexOf(hem2);
          if (pos2 >= 0)
            sign = 1;
          else
            retVal = false;
        }

        if ((deg < -maxDeg) || (deg > maxDeg))
          retVal = false;

        if (retVal)
        {
          string minStr = coordStr.Substring(pos + 1, pos2 - pos - 1);
          retVal = double.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double minval);
          if ((minval >= 60) || (minval < 0))
            retVal = false;
          if (retVal)
          {
            coord = deg + minval / 60;
            if (sign == 1)
              coord *= -1;
          }
        }
      }
      return retVal;
    }
  }
}
