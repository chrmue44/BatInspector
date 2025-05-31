/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;

namespace BatInspector
{

  struct FormulaData
  {
    public FormulaData(int i, string f, string r, string e)
    {
      Id = i;
      Formula = f;
      Result = r;
      Error = e;
    }

    public int Id;
    public string Formula;
    public string Result;
    public string Error;
  }

  public class Tests
  {
    ProcessRunner _proc;
    int _errors = 0;
    FormulaData[] _dataForm;

  public Tests()
   {
      _proc = new ProcessRunner();
      _dataForm = new FormulaData[]
        {
        new FormulaData(1, "substr(\"ABCDE\",0,2)", "AB",""),
        new FormulaData(2, "substr(\"ABCDE\",3,2)", "DE",""),
        new FormulaData(3, "substr(\"ABCDE\",4,2)", "ARG2_OUT_OF_RANGE","ARG2_OUT_OF_RANGE"),
        new FormulaData(4, "cast(2.5,\"INT\")", "2",""),
        new FormulaData(5, "1+3+4", "8",""),
        new FormulaData(6, "-1+3+4", "6",""),
        new FormulaData(7, "-5.0*(3.1 + 2.9)", "-30.0000",""),
        new FormulaData(8, "a=0d22-11-10T13:45:00", "0d22-11-10T13:45:00",""),
        new FormulaData(9, "a=0d22+11-10T13:45:00", "BAD_TOKEN","BAD_TOKEN"),
        new FormulaData(10, "0d22-11-10T13:45:00 + 60", "0d22-11-10T13:46:00",""),
        new FormulaData(11, "0d22-11-10T13:45:00 - 2", "0d22-11-10T13:44:58",""),
        new FormulaData(12, "0d22-11-10T13:45:00 < 0d22-11-10T13:46:00", "TRUE",""),
        new FormulaData(13, "0d22-11-10T13:45:00 < 0d22-11-10T13:44:00", "FALSE",""),
        new FormulaData(14, "0d22-11-10T13:45:00 < 0d22-11-10T13:45:00", "FALSE",""),
        new FormulaData(15, "0d22-11-10T13:45:00 > 0d22-11-10T13:46:00", "FALSE",""),
        new FormulaData(16, "0d22-11-10T13:45:00 > 0d22-11-10T13:44:00", "TRUE",""),
        new FormulaData(17, "0d22-11-10T13:45:00 > 0d22-11-10T13:45:00", "FALSE",""),
        new FormulaData(18, "1 + 4i + 2 - 2i", "3.0000 + 2.0000i",""),
        new FormulaData(19, "0t13:45:00 > 0t13:44:00", "TRUE",""),
        new FormulaData(20, "0t13:45:00 - 0t13:41:00", "0t00:04:00",""),
        new FormulaData(21, "tod(\"0d22-11-10T17:48:22\")", "0t17:48:22",""),
        new FormulaData(22, "0t13:45:00 + 0t01:01:02", "0t14:46:02",""),
        new FormulaData(23, "0d22-11-10T13:45:00 - 0t02:00:01", "0d22-11-10T11:44:59",""),
        new FormulaData(24, "indexOf(\"abcdef\",\"cde\")","2",""),
        new FormulaData(25, "indexOf(\"abcdef\",\"efg\")","-1",""),
        new FormulaData(26, "strListCnt(\"abc;def;ghi\")","3",""),
        new FormulaData(27, "strListItem(\"abc;def;ghi\",1)","def",""),
        new FormulaData(28, "strListItem(\"abc;def;ghi\",3)","ARG2_OUT_OF_RANGE","ARG2_OUT_OF_RANGE"),
        new FormulaData(29, "replace(\"PAUR(56%)\",\"(\",\"](\")", "PAUR](56%)",""),

        };

    }

    public void exec()
    {
      string wrkDir = Path.Combine(AppParams.Inst.AppRootPath, "../../../scripts");
      // testBioAcoustics();
      //testFft();
      //testDenoising();
      //testActivityDiagr();
      testNextStepDown();
      testIf(wrkDir);
      testWhile(wrkDir);
      testFor(wrkDir);
      testParser();
      testCsvFuncs(wrkDir);
      testClassifier();
      testSumReport();
      testHistogram();
      testGpx();
      testKml();
      testLocfileTxt();
      //testSignalForm();
      testSimCall();
      //testReportModelBatdetect2();
      //testCreatePrj();
      //updateSummaries();
      //string prjName = "E:/bat/2023/20230408_4_SW";
      //testCreatePrjInfoFiles(prjName, 49.7670333333333, 8.63353333333333);
      //adjustTimesInReport(prjName);
      //testQuery();
      testEffVal();
      //calcNoiseLevel();
      testRulerTicks();
      testRemoveSection();
      testCheckSpecAtLocation();
      //testSplitWavs();
      testCalcSnr();
      testCreatePngCpp();
      //testCreateMicSpectrumFromNoiseFile();
      //double soft = 1.0;
      // testSoundEditApplyFreqResponse("20250428_210228", soft);
      // testSoundEditApplyFreqResponse("20250428_210836", soft);
      // testSoundEditApplyFreqResponse("20250428_noise", soft);

      //checkIfWavFilesExist(); // not a test, a one time function
      //fixAnnotationIds(); // not a test, a one time function
      //testWriteModParams(); //not a test, a one time function
      //adjustJsonIds(); //not a test, a one time function
      //adjustJsonAnnotationCallsAtBorder(); //not a test, a one time function

      if (_errors == 0)
      {
        DebugLog.clear();
        DebugLog.log("Tests passed", enLogType.INFO);
      }
      else
        DebugLog.log("Tests failed", enLogType.INFO);
    }


    private void testSumReport()
    {
      // calcDays()
      SumReport rep = new SumReport();
      DateTime dat = new DateTime(2022, 09, 15);
      DateTime end = new DateTime(2022, 09, 30);
      int days = rep.calcDays(dat, end, enPeriod.DAILY);
      assert("check daily", days == 1);
      days = rep.calcDays(dat, end, enPeriod.WEEKLY);
      assert("check full week", days == 7);
      end = new DateTime(2022, 09, 20);
      days = rep.calcDays(dat, end, enPeriod.WEEKLY);
      assert("check part week", days == 5);
      end = new DateTime(2022, 10, 01);
      days = rep.calcDays(dat, end, enPeriod.MONTHLY);
      assert("check part month", days == 16);
      dat = new DateTime(2022, 10, 01);
      end = new DateTime(2022, 11, 05);
      days = rep.calcDays(dat, end, enPeriod.MONTHLY);
      assert("check full month", days == 31);

      //incrementDate
      dat = new DateTime(2022, 07, 15);
      DateTime exp = new DateTime(2022, 07, 16);
      end = rep.incrementDate(dat, enPeriod.DAILY);
      assert("check inc day", end == exp);
      dat = new DateTime(2022, 10, 27);
      exp = new DateTime(2022, 11, 03);
      end = rep.incrementDate(dat, enPeriod.WEEKLY);
      assert("check inc week", end == exp);
      dat = new DateTime(2022, 11, 05);
      exp = new DateTime(2022, 12, 01);
      end = rep.incrementDate(dat, enPeriod.MONTHLY);
      assert("check inc month", end == exp);
      DateTime startD = new DateTime(2023, 03, 01);
      DateTime endD = new DateTime(2023, 04, 30);

      //string path = "G:/bat/2023";
      //rep.createReport(startD, endD, enPeriod.DAILY, path);
    }

    private void testCsvFuncs(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null);
      scr.runScript(Path.Combine(wrkDir, "test_csv.scr"), false);
      assert(scr.getVariable("c23"), "23");
      assert(scr.getVariable("c123"), "123");
      assert(scr.getVariable("c43"), "43");
      assert(scr.getVariable("c55"), "55");
      assert(scr.getVariable("c62"), "62");
    }

    private void testHistogram()
    {
      Histogram h = new Histogram(10);
      h.init(0, 10);
      h.add(5);
      h.add(6);
      h.add(7);
      h.add(5);
      h.add(6);
      h.add(7);
      assert("mean", h.Mean == 6);
      assert("count", h.Count == 6);
      assert("stddev", Math.Abs(h.StdDev - 0.894) < 0.001);
    }

    private void testWhile(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null);
      scr.SetVariable("Limit", "9");
      scr.SetVariable("b", "0");
      scr.runScript(Path.Combine(wrkDir,"test_while.scr"), false, false);
      assert(scr.getVariable("sum"), "9");
      scr.SetVariable("Limit", "23");
      scr.SetVariable("b", "0");
      scr.runScript(Path.Combine(wrkDir, "test_while.scr"), false, false);
      assert(scr.getVariable("sum"), "23");
      scr.SetVariable("Limit", "9");
      scr.SetVariable("b", "1");
      scr.runScript(Path.Combine(wrkDir, "test_while.scr"), false, false);
      assert(scr.getVariable("sum"), "5");
      scr.SetVariable("Limit", "23");
      scr.SetVariable("b", "1");
      scr.runScript(Path.Combine(wrkDir, "test_while.scr"), false, false);
      assert(scr.getVariable("sum"), "5");
    }

    private void testFor(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null);
      scr.runScript(Path.Combine(wrkDir, "test_for.scr"), false, false);
      assert(scr.getVariable("sum1"), "12");
      assert(scr.getVariable("sum2"), "0");

    }

    private void testIf(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null);

      scr.SetVariable("A", "55");
      scr.SetVariable("B", "34");
      scr.runScript(Path.Combine(wrkDir, "test_if.scr"), false, false);
      assert(scr.getVariable("Result"), "\"AlowBhigh\"");
      assert(scr.getVariable("Res2"), "\"AlowBhigh\"");

      scr.SetVariable("A", "55");
      scr.SetVariable("B", "32");
      scr.runScript(Path.Combine(wrkDir, "test_if.scr"), false, false);
      assert(scr.getVariable("Result"), "\"AlowBlow\"");
      assert(scr.getVariable("Res2"), "\"AlowBlow\"");

      scr.SetVariable("A", "101");
      scr.SetVariable("B", "34");
      scr.runScript(Path.Combine(wrkDir, "test_if.scr"), false, false);
      assert(scr.getVariable("Result"), "\"AhighBhigh\"");
      assert(scr.getVariable("Res2"), "\"AhighBhigh\"");

      scr.SetVariable("A", "101");
      scr.SetVariable("B", "33");
      scr.runScript(Path.Combine(wrkDir, "test_if.scr"), false, false);
      assert(scr.getVariable("Result"), "\"AhighBlow\"");
      assert(scr.getVariable("Res2"), "\"AhighBhigh\"");
    }

    int testParser()
    {
      int retVal = 0;
      DebugLog.log("Testing parser", enLogType.INFO);
      foreach (FormulaData f in _dataForm)
      {
        string form = f.Formula;
        Expression exp = new Expression(null);
        MthdListScript mthdListScript = new MthdListScript( "");
        exp.addMethodList(mthdListScript);
        string res = exp.parseToString(form);
        if((f.Result != res) || ((f.Error != res) && (exp.Errors > 0)))
        {
          DebugLog.log("Error calculation of formula '" + f.Formula + "'.  expected: " + f.Result + ", got: " + res, enLogType.ERROR);
          retVal++;
          _errors++;
        }
      }
      return retVal;
    }

    private int testBioAcoustics()
    {
      //  string wavFile = "C:/Users/chrmu/bat/2022/20220816/Records/20220816_0027.wav";
      string wavFile = "C:/Users/chrmu/bat/2022/20220906/Records/20220906_0005.wav";
#pragma warning disable IDE0059 // Unnecessary assignment of a value
      ThresholdDetectItem[]items = BioAcoustics.analyzeCalls(wavFile, out int sampleRate, out double duration);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
      return 0;
    }

    private int testClassifier()
    {
      int retVal = 0;
      List<ParLocation> locs = new List<ParLocation>
      {
        new ParLocation(49.963175, 8.563220),
        new ParLocation(49.727209, 8.539645),
        new ParLocation(49.728493, 8.788927),
        new ParLocation(49.839054, 9.030958),
        new ParLocation(49.995876, 9.010885)
      };

      ParLocation pos = new ParLocation(49.753933, 8.632084);
      bool ret = BatSpeciesRegions.inside(pos, locs);
      assert("loc inside", ret == true);
      pos = new ParLocation(50.1, 8.632084);
      ret = BatSpeciesRegions.inside(pos, locs);
      assert("loc inside", ret == false);
      pos = new ParLocation(49.8, 9.1);
      ret = BatSpeciesRegions.inside(pos, locs);
      assert("loc inside", ret == false);
      pos = new ParLocation(49.8, 8.4);
      ret = BatSpeciesRegions.inside(pos, locs);
      assert("loc inside", ret == false);

      return retVal;
    }

    private int testSignalForm()
    {
      Csv csv = new Csv();
      csv.read("E:/bat/2022/20220905/report.csv", ";", true);
      
      Csv res = new Csv();
      res.initColNames("F00;F25;F50;F75;F100;Form", true);

      int rows = csv.RowCnt;
      for(int r = 2; r <= rows; r++)
      {
        double[] f = new double[5];
        f[0] = csv.getCellAsDouble(r, Cols.F_START);
        f[1] = csv.getCellAsDouble(r, Cols.F_25);
        f[2] = csv.getCellAsDouble(r, Cols.F_CENTER);
        f[3] = csv.getCellAsDouble(r, Cols.F_75);
        f[4] = csv.getCellAsDouble(r, Cols.F_END);
        enSigStructure sig = ClassifierBarataud.getSigStructure(f);
        res.addRow();
        res.setCell(r, "F00", f[0]);
        res.setCell(r, "F25", f[1]);
        res.setCell(r, "F50", f[2]);
        res.setCell(r, "F75", f[3]);
        res.setCell(r, "F100", f[4]);
        res.setCell(r, "Form", sig.ToString());
      }
      res.saveAs("sig.csv");

      return 0;
    }

    private void testSimCall()
    {
      List<FreqItem> l = new List<FreqItem>
      {
        new FreqItem(100000, 0, 0),
        new FreqItem(100000, 5e-3, 0),
        new FreqItem(95000, 7e-3, 0.02),
        new FreqItem(80000, 10e-3, 0.05),
        new FreqItem(60000, 15e-3, 0.08),
        new FreqItem(40000, 25e-3, 0.1),
        new FreqItem(25000, 28e-3, 0.08),
        new FreqItem(22000, 29e-3, 0),
        new FreqItem(22000, 500e-3, 0)
      };
#pragma warning disable IDE0059 // Unnecessary assignment of a value
      SimCall call = new SimCall(l, 384000);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }

    private void testCreatePrj()
    {
      PrjInfo prj = new PrjInfo
      {
        Name = "Test",
        SrcDir = "F:\\bat\\src",
        DstDir = "F:\\bat\\test",
        MaxFileLenSec = 5,
        MaxFileCnt = 30,
        Notes = "12°C, bedeckt; Uferbereich Waldweiher",
        Latitude = 49.123,
        Longitude = 8.123,
        StartTime = new DateTime(2022,7,12),
        EndTime = new DateTime(2022,7,14),
        WavSubDir = AppParams.DIR_WAVS,
      };
      Project.createPrjFromWavs(prj, App.Model.DefaultModelParams[0]);
    }

    private void testReportModelBatdetect2()
    {
      List <SpeciesInfos> species = BatInfo.loadFrom("F:/BatInspector/dat").Species;
      ModelBatDetect2 model = new ModelBatDetect2(1);
      string WavDir = "G:\\bat\\2022\\20220326\\Records";
      string AnnotationDir = "G:\\bat\\2022\\20220326\\ann";
      string reportName = "G:\\bat\\2022\\20220326\\bd2\\report.csv";
      model.createReportFromAnnotations(0.5, species, WavDir, AnnotationDir, reportName, enRepMode.REPLACE);
      ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
      Project prj = new Project(false, modelParams, App.Model.DefaultModelParams.Length);
      Analysis a = new Analysis(false, enModel.BAT_DETECT2);
      a.read(reportName, App.Model.DefaultModelParams);
      a.save(reportName, prj.Notes, prj.SummaryName);
    }

    private void testQuery()
    {
      string srcDir = "D:\\bat\\2023\\Ententeich";
      string dstDir = "D:\\bat\\Queries";
      string name = "test";
      string query = "(FreqMin > 18000) && (FreqMin < 21000) && (SpeciesAuto == \"NLEI\")";
      ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
      Query qry = new Query(name, srcDir, dstDir, query,  
                            modelParams, App.Model.DefaultModelParams.Length);
      qry.evaluate();      
    }

    private void updateSummaries()
    {
      string rootDir = "G:/bat/2023";
      DirectoryInfo dirInfo = new DirectoryInfo(rootDir);
      foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
      {
        string repName = Project.containsReport(subDir, AppParams.PRJ_REPORT, App.Model.DefaultModelParams[0]);
        if (repName != "")
        {
          ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
          Project p = new Project(false, modelParams, App.Model.DefaultModelParams.Length);
          Analysis a = new Analysis(false, enModel.BAT_DETECT2);
          a.read(repName, App.Model.DefaultModelParams);
          string sumName = repName.Replace(AppParams.PRJ_REPORT, AppParams.PRJ_SUMMARY);
          a.createSummary(sumName, p.Notes);
        }
      }
    }

    private void testCreatePrjInfoFiles(string prjName, double lat, double lon)
    {
      PrjInfo i = new PrjInfo();
      i.Latitude = lat;
      i.Longitude = lon;
      DirectoryInfo dirInfo = new DirectoryInfo(prjName);
      App.Model.initProject(dirInfo, false);
      App.Model.Prj.createXmlInfoFiles(i);
    }


    private void testFft()
    {
      uint size = 256;
      double[] samples = new double[size];
      int scale = 100;
      Random random = new Random();
      Csv src = new Csv();
      for (int i = 0; i < size; i++)
      {
        double phi = 5.0 * 2 * Math.PI * i / size;
        samples[i] = Math.Cos(phi) * scale + random.NextDouble() * scale / 30;
        src.addRow();
        src.setCell(i, 1, samples[i]);
      }
      src.saveAs("src.csv");
      int handle = BioAcoustics.getFft(size, enWIN_TYPE.HANN);
      double[] specC = BioAcoustics.calculateFftComplexOut(handle, samples);
      BioAcoustics.applyBandpassFilterComplex(ref specC, 2, 10, 256);
      handle = BioAcoustics.getFft(size, enWIN_TYPE.HANN);
      double[] reversed = BioAcoustics.calculateFftReversed(handle, specC);
      Csv csv = new Csv();
      for (int i = 0; i < reversed.Length; i++)
      {
        csv.addRow();
        csv.setCell(i + 1, 1, reversed[i]);
      }
      csv.saveAs("test.csv");
    }

    private void testDenoising()
    {
      WavFile w = new WavFile();
      string file = AppParams.DriveLetter + "bat\\2023\\Ententeich\\20230520_SW\\Records\\JA_N_20230520_212801.wav";
      
      w.readFile(file);
      SoundEdit result = new SoundEdit((int)w.FormatChunk.Frequency, w.AudioSamples.Length);
      result.copySamples(w.AudioSamples);
      result.FftForward();
      result.bandpass(15000, 40000);
      result.FftBackward();
      string newName = file.ToLower().Replace(".wav", "_edit.wav");
      result.saveAs(newName, "Records");
    }

    private void testSoundEditApplyFreqResponse(string name, double softening)
    {
      WavFile w = new WavFile();

      string prjDir = "F:\\prj\\BatInspector\\TestData\\wavTest";
      string wavDir = prjDir;
      string file = wavDir + "\\" + name + ".wav";
      string newFile = wavDir + "\\" + name + "_new.wav";
      string prjFile = prjDir + "\\20250428_BAT.batspy";
      w.readFile(file);
      SoundEdit result = new SoundEdit((int)w.FormatChunk.Frequency, w.AudioSamples.Length);
      result.copySamples(w.AudioSamples);

      string xml = File.ReadAllText(prjFile);
      TextReader reader = new StringReader(xml);
      XmlSerializer ser = new XmlSerializer(typeof(BatExplorerProjectFile));
      BatExplorerProjectFile batExplorerPrj = (BatExplorerProjectFile)ser.Deserialize(reader);

      result.applyFreqResponse(batExplorerPrj.Microphone.FrequencyResponse, softening);
      
      WavFile outFile = new WavFile();
      outFile.createFile(1, (int)w.FormatChunk.Frequency, 0, result.Samples.Length - 1, result.Samples);
      outFile.saveFileAs(newFile);
    }

    private void testCreateMicSpectrumFromNoiseFile()
    {
      WavFile w = new WavFile();
      string prjDir = "F:\\prj\\BatInspector\\TestData\\wavTest";
      string wavDir = prjDir;
      string file = wavDir + "\\20250428_noise.wav";
      string prjFile = prjDir + "\\20250428_BAT.batspy";
      string micFile = prjDir + "\\testMicResponse.txt";
      w.readFile(file);
      SoundEdit result = new SoundEdit((int)w.FormatChunk.Frequency, w.AudioSamples.Length);
      result.copySamples(w.AudioSamples);

      FreqResponseRecord[] r=  result.createMicSpectrumFromNoiseFile();
      string xml = File.ReadAllText(prjFile);
      TextReader reader = new StringReader(xml);
      XmlSerializer ser = new XmlSerializer(typeof(BatExplorerProjectFile));
      BatExplorerProjectFile batExplorerPrj = (BatExplorerProjectFile)ser.Deserialize(reader);
      batExplorerPrj.Microphone.FrequencyResponse = r;

      TextWriter writer = new StreamWriter(prjFile);
      ser.Serialize(writer, batExplorerPrj);
      writer.Close();

      string text = "";
      foreach (FreqResponseRecord rec in r)
      {
        text += $"{rec.Frequency.ToString("0.0", CultureInfo.InvariantCulture)}, {rec.Amplitude.ToString("0.0", CultureInfo.InvariantCulture)}\n".ToString(System.Globalization.CultureInfo.InvariantCulture);
      }
      File.WriteAllText(micFile, text);
    }

    private void testCalcSnr()
    {
      WavFile w = new WavFile();
      string prjDir = "F:\\prj\\BatInspector\\TestData\\20220906";
      string file = "20220906_0005.wav";
      w.readFile(Path.Combine(prjDir, "Records", file));
      string report = Path.Combine(prjDir, "bd2", "report_BatDetect2_GermanBats.pth.tar.csv");
      Analysis a = new Analysis(false, enModel.BAT_DETECT2);
      a.read(report, App.Model.DefaultModelParams);
      AnalysisFile f = a.find(file);
      assert("open test data", f != null);

      double tStart = f.Calls[0].getDouble(Cols.START_TIME);
      double tEnd = tStart + f.Calls[0].getDouble(Cols.DURATION) / 1000;
      double snr = w.calcSnr(tStart, tEnd);
      assert("SNR call 1", Math.Abs(snr - 15.9) < 0.1);

      tStart = f.Calls[9].getDouble(Cols.START_TIME);
      tEnd = tStart + f.Calls[7].getDouble(Cols.DURATION) / 1000;
      snr = w.calcSnr(tStart, tEnd);
      assert("SNR call 8", Math.Abs(snr - 3.1) < 0.1);
    }

    private void testRemoveSection()
    {
      SoundEdit s = new SoundEdit(1, 100);
      double[] samples = new double[100];
      for (int i = 0; i < samples.Length; i++)
      {
        samples[i] = 0.01 * i;
      }
      s.copySamples(samples);
      s.removeSection(10, 20);
      assert("Length", s.Samples.Length == 90);
      assert("sample[11]", s.Samples[11] == 0.21);
      assert("Last sample", s.Samples[89] == 0.99);
    }

    private void testEffVal()
    {
      WavFile w = new WavFile();
      w.createSineWave(1000, 384000, 0.5, 0.2);
      double v = w.calcEffVoltage();
      assert("effective voltage" , (v - Math.Sqrt(2)/4) < 0.0001);
      w.createSineWave(1000, 384000, 0, 0.2);
      v = w.calcEffVoltage(0, 1, false);
      assert("effective voltage", (v - 0.2 ) < 0.0001);
    }

    private void testRulerTicks()
    {
      RulerData ruler = new RulerData();
      ruler.setRange(0, 191);
      double[] ticks = GraphHelper.createTicks(9, ruler.Min, ruler.Max);
      ruler.setRange(20, 170);
      double[] ticks1 = GraphHelper.createTicks(9, ruler.Min, ruler.Max);
      ruler.setRange(47, 67);
      double[] ticks2 = GraphHelper.createTicks(9, ruler.Min, ruler.Max);
      ruler.setRange(67, 126);
      double[] ticks3 = GraphHelper.createTicks(9, ruler.Min, ruler.Max);
    }

    private void testNextStepDown()
    {
      double v = GraphHelper.nextStepDown(67);
      assert("Step 67 -> 50", v == 50);
      v = GraphHelper.nextStepDown(17);
      assert("Step 17 -> 10", v == 10);
      v = GraphHelper.nextStepDown(47);
      assert("Step 47 -> 20", v == 20);
      v = GraphHelper.nextStepDown(6700);
      assert("Step 6700 -> 5000", v == 5000);
      v = GraphHelper.nextStepDown(0.006);
      assert("Step 0.006 -> 0.005", v == 0.005);
      v = GraphHelper.nextStepDown(6);
      assert("Step 6 -> 5", v == 5);
      v = GraphHelper.nextStepDown(0.3);
      assert("Step 0.3 -> 0.2", v == 0.2);
    }

    private void testGpx()
    {
      string fName = "F:\\prj\\BatInspector\\TestData\\20230822\\Track_2023-08-22_GrubeMessel.gpx";
      gpx g = gpx.read(fName);

      DateTime t = DateTime.Parse("2023/08/22 18:29:37");
      double[] pos = g.getPosition(t);
      assert("GPX lat", Math.Abs(pos[0] - 49.9187) < 0.0001);
      assert("GPX lon", Math.Abs(pos[1] - 8.7558) < 0.0001);

      t = DateTime.Parse("2023/08/22 18:43:19");
      pos = g.getPosition(t);
      assert("GPX lat", Math.Abs(pos[0] - 49.9185) < 0.0001);
      assert("GPX lon", Math.Abs(pos[1] - 8.7583) < 0.0001);
    }

    private void testKml()
    {
      string fName = "F:\\prj\\BatInspector\\TestData\\20230928\\Session_20230928_195716.kml";
      kml k = kml.read(fName);

      double[] pos = k.getPosition("20230928_195928.wav");
      assert("GPX lat", Math.Abs(pos[0] - 49.8973) < 0.0001);
      assert("GPX lon", Math.Abs(pos[1] - 8.6742) < 0.0001);

      pos = k.getPosition("20230928_200232.wav");
      assert("GPX lat", Math.Abs(pos[0] - 49.8974) < 0.0001);
      assert("GPX lon", Math.Abs(pos[1] - 8.6759) < 0.0001);
    }

    private void testLocfileTxt()
    {

      if (AppParams.Inst.LocFileSettings == null)
        AppParams.Inst.LocFileSettings = new LocFileSettings();
    
      // test search for file name  
      LocFileSettings lo = new LocFileSettings(AppParams.Inst.LocFileSettings);
      AppParams.Inst.LocFileSettings.ColFilename = 0;
      AppParams.Inst.LocFileSettings.Mode = enLocFileMode.FILE_NAME;
      AppParams.Inst.LocFileSettings.ColNS = 1;
      AppParams.Inst.LocFileSettings.ColLatitude = 1;
      AppParams.Inst.LocFileSettings.ColWE = 2;
      AppParams.Inst.LocFileSettings.ColLongititude = 2;
      AppParams.Inst.LocFileSettings.ColTime = 0;
      AppParams.Inst.LocFileSettings.Delimiter = '\t';

      string fName = "F:\\prj\\BatInspector\\TestData\\20140908\\JB-_gps.txt";
      LocFileTxt l = LocFileTxt.read(fName, AppParams.Inst.LocFileSettings);
      double[] pos = l.getPosition("JB-___20140908_222847.wav", DateTime.Now);
      assert("check latitude TXT, FileMode",Math.Abs(pos[0] - 49.76643) < 0.00001);
      assert("check longitude TXT, FileMode", Math.Abs(pos[1] - 8.63371) < 0.00001);

      // test search for date time
      Expression exp = new Expression(null);
      MthdListScript mthdListScript = new MthdListScript("");
      exp.addMethodList(mthdListScript);
      exp.parse("setTxtLocfilePars(0,2,4,1,3,5,0,1,\",\")");
      fName = "F:\\prj\\BatInspector\\TestData\\Peter\\PTG01_Summary.txt";
      l = LocFileTxt.read(fName, AppParams.Inst.LocFileSettings);
      
      DateTime tTest = DateTime.Parse("2024/04/22 01:18:30");
      pos = l.getPosition("", tTest);
      AppParams.Inst.LocFileSettings = lo;
      assert("check latitude TXT, TimeMode", Math.Abs(pos[0] - 51.605) < 0.00001);
      assert("check longitude TXT, TimeMode", Math.Abs(pos[1] + 3.005) < 0.00001);
      
      tTest = DateTime.Parse("2024/04/22 01:30:00");
      pos = l.getPosition("", tTest);
      assert("check latitude TXT, TimeMode", Math.Abs(pos[0] - 49.000) < 0.00001);
      assert("check longitude TXT, TimeMode", Math.Abs(pos[1] - 8.1000) < 0.00001);

      AppParams.Inst.LocFileSettings = lo;

    }

    private void calcNoiseLevel()
    {
      WavFile w = new WavFile();
      w.readFile("G:\\bat\\2023\\TestRecording\\20230715\\Records\\20230715_224211.wav");
      double v = w.calcEffVoltage(0.1, 0.3);
      double db = Math.Log10(v) * 20;
      w.readFile("G:\\bat\\2023\\TestRecording\\20230715\\Records\\20230716_170418.wav");
      v = w.calcEffVoltage(0.1, 0.3);
      db = Math.Log10(v) * 20;
      w.readFile("G:\\bat\\2023\\TestRecording\\20230715\\Records\\20230716_171555.wav");
      v = w.calcEffVoltage(0.1, 0.3);
      db = Math.Log10(v) * 20;
       
      /* result for Rev B:
       * -80dB for ADC only (shortcut pin 3-5 header to analog board
       * -73dB for analog board (shortcut mic input on sub-D)
       * -57dB for mic pre amp (shortcut mic input on pre amp)
       */
    }

    private void adjustTimesInReport()
    {
      string rootDir = "G:/bat/2023";
      DirectoryInfo dirInfo = new DirectoryInfo(rootDir);
      foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
      {
        string repName = Project.containsReport(subDir, AppParams.PRJ_REPORT, App.Model.DefaultModelParams[0]);
        if (repName != "")
        {
          App.Model.initProject(subDir, false);
          foreach(AnalysisFile f in App.Model.Prj.Analysis.Files)
          {
            string info = Path.Combine(App.Model.Prj.PrjDir, App.Model.Prj.WavSubDir, f.Name.ToLower().Replace(".wav", ".xml"));
            BatRecord rec = ElekonInfoFile.read(info);
            foreach(AnalysisCall c in f.Calls)
              c.setString(Cols.REC_TIME, rec.DateTime);
          }
          App.Model.Prj.Analysis.save(App.Model.Prj.ReportName, App.Model.Prj.Notes, App.Model.Prj.SummaryName);
        }
      }
    }

    private void adjustJsonIds()
    {
      string dirName = "F:\\bat\\trainingBd2\\ann";
      DirectoryInfo dir = new DirectoryInfo(dirName);
      FileInfo[] files = dir.GetFiles();
      foreach(FileInfo file in files)
      {
        Bd2AnnFile f = Bd2AnnFile.loadFrom(file.FullName);
        f.id = file.Name.Replace(".json","");
        f.save();
      }
    }

    private void adjustJsonAnnotationCallsAtBorder()
    {
      string dirName = "F:\\bat\\trainingBd2\\ann";
      DirectoryInfo dir = new DirectoryInfo(dirName);
      FileInfo[] files = dir.GetFiles();
      int count = 0;
      int countEcho = 0;
      int countSocial = 0;
      foreach (FileInfo file in files)
      {
        Bd2AnnFile f = Bd2AnnFile.loadFrom(file.FullName);
        foreach (Bd2Annatation a in f.Annatations)
        {
          if(a.Event == "Echolocation")
            countEcho++;
          if(a.Event == "Social")
            countSocial++;
          if ((a.start_time < 0.0001) && (a.Event == "Echolocation") && (a.Class.ToLower() == "unknown"))
          {
            count++;
            a.Class = "Bat";
            DebugLog.log("file " + f.id + "1st call: " + a.Event + ", " + a.Class, enLogType.INFO);
          }
          if (((f.duration - a.end_time) < 0.0001) && (a.Event == "Echolocation") && (a.Class.ToLower() == "unknown"))
          {
            count++;
            a.Class = "Bat";
            DebugLog.log("file " + f.id + "last call: " + a.Event + ", " + a.Class, enLogType.INFO);
          }
        }
      //  f.save();
      }
      DebugLog.log("echolocation: "+ countEcho.ToString() + "social: " + countSocial.ToString() + " edge cases:" + count.ToString(), enLogType.INFO);
      
    }

    private void adjustTimesInReport(string prjName)
    {
      DirectoryInfo subDir = new DirectoryInfo(prjName);
      
      string repName = Project.containsReport(subDir, AppParams.PRJ_REPORT, App.Model.DefaultModelParams[0]);
      if (repName != "")
      {
        App.Model.initProject(subDir, false);
        foreach (AnalysisFile f in App.Model.Prj.Analysis.Files)
        {
          string info = Path.Combine(App.Model.Prj.PrjDir, App.Model.Prj.WavSubDir, f.Name.Replace(".wav", ".xml"));
          BatRecord rec = ElekonInfoFile.read(info);
          foreach (AnalysisCall c in f.Calls)
            c.setString(Cols.REC_TIME, rec.DateTime);
        }
        App.Model.Prj.Analysis.save(App.Model.Prj.ReportName, App.Model.Prj.Notes, App.Model.Prj.SummaryName);
      }
    }


    struct stSpecLoc
    {
      public string Species;
      public double Lat;
      public double Lon;
      public bool Occurs;

      public stSpecLoc(string species, double lat, double lon, bool occurs)
      {
        Species = species; 
        Lat = lat; 
        Lon = lon; 
        Occurs = occurs;
      }
    }

    private void testCheckSpecAtLocation()
    {
      List<stSpecLoc> tests = new List<stSpecLoc>();
      tests.Add(new stSpecLoc("BBAR", 49.849, 8.66, true));  //DA-DI
      tests.Add(new stSpecLoc("RFER", 49.849, 8.66, false)); // DA-DI
      tests.Add(new stSpecLoc("RFER", 49.483, 9.640, true));  // Germany
      tests.Add(new stSpecLoc("RFER", 61.523, 8.718, false));  // Norway
      tests.Add(new stSpecLoc("ENIL", 61.523, 8.718, true));  // Norway
      tests.Add(new stSpecLoc("MDAU", 51.71, -1.348, true));    //GB
      tests.Add(new stSpecLoc("MMYO", 51.71, -1.348, false));    //GB


      foreach (stSpecLoc s in tests)
      {
        assert("occorsAtLocation() fails", s.Occurs == App.Model.Regions.occursAtLocation(s.Species, s.Lat, s.Lon));
      }
    }



    

    private void testActivityDiagr()
    {
      string bmpName = "F:\bat\bmptest.bmp";
      frmActivity frm = new frmActivity(bmpName);
      frm.Show();
      frm.setup();
      int ticksPerHour = 60 / 5;
      ActivityData data = new ActivityData(ticksPerHour);
      data.StartDate = new DateTime(2024, 3, 25);
      data.EndDate = new DateTime(2024, 6, 25);
      DateTime currDay = data.StartDate;
      int d = 0;
      while(currDay < data.EndDate)
      {
        int t = 24 * ticksPerHour;
        DailyActivity dailyData = new DailyActivity(currDay, ticksPerHour);
        data.Add(dailyData);
        for(int h = 0; h < t; h++)
        {
          int val = 0;
          if(h < 6 * ticksPerHour)
            val = 5 + (12 * ticksPerHour - 2*h);
          else if(h > 20 * ticksPerHour)
            val = 2 + h - 19 * ticksPerHour;
          data.Days[d].Counter[h] = val;
        }
        d++;
        currDay = currDay.AddDays(1);
      }
      frm.createPlot(data);
    }

    private void testWriteModParams()
    {
      ModelParams[] mp = new ModelParams[2];
      mp[0] = new ModelParams();
      mp[0].Name = "BatDetect2";
      mp[0].Type = enModel.BAT_DETECT2;
      mp[0].DataSet = "UK_Bat_Species";
      mp[0].Enabled = true;
      mp[0].Parameters = new ModelParItem[1];
      mp[0].Parameters[0] = new ModelParItem()
      {
        Name = "minimal Probability",
        Type = enDataType.DOUBLE,
        Decimals = 2,
        Value = "0.5"
      };
      mp[0].AvailableDataSets = new string[1];
      mp[0].AvailableDataSets[0] = "UK_Bat_Species";

      mp[1] = new ModelParams();
      mp[1].Name = "BattyBirdNET";
      mp[1].Type = enModel.BATTY_BIRD_NET;
      mp[1].DataSet = "BattyBirdNET-Bavaria-256kHz";
      mp[1].Enabled = false; 
      
      mp[1].Parameters = new ModelParItem[2];
      mp[1].AvailableDataSets = new string[15];
      mp[1].AvailableDataSets[0] = "BattyBirdNET-Bavaria-256kHz";
      mp[1].AvailableDataSets[1] = "BattyBirdNET-Bavaria-144kHz";
      mp[1].AvailableDataSets[2] = "BattyBirdNET-EU-256kHz";
      mp[1].AvailableDataSets[3] = "BattyBirdNET-EU-144kHz";
      mp[1].AvailableDataSets[4] = "BattyBirdNET-MarinCounty-256kHz";
      mp[1].AvailableDataSets[5] = "BattyBirdNET-MarinCounty-144kHz";
      mp[1].AvailableDataSets[6] = "BattyBirdNET-Scotland-256kHz";
      mp[1].AvailableDataSets[7] = "BattyBirdNET-Scotland-144kHz";
      mp[1].AvailableDataSets[8] = "BattyBirdNET-SouthWales-256kHz";
      mp[1].AvailableDataSets[9] = "BattyBirdNET-Sweden-256kHz";
      mp[1].AvailableDataSets[10] = "BattyBirdNET-Sweden-144kHz";
      mp[1].AvailableDataSets[11] = "BattyBirdNET-UK-256kHz";
      mp[1].AvailableDataSets[12] = "BattyBirdNET-UK-144kHz";
      mp[1].AvailableDataSets[13] = "BattyBirdNET-USA-256kHz";
      mp[1].AvailableDataSets[14] = "BattyBirdNET-USA-144kHz";

      mp[1].Parameters[0] = new ModelParItem()
      {
        Name = "minimal Probability",
        Type = enDataType.DOUBLE,
        Decimals = 2,
        Value = "0.5"
      };
      mp[1].Parameters[1] = new ModelParItem()
      {
        Name = "Sensitivity",
        Type = enDataType.DOUBLE,
        Decimals = 2,
        Value = "1.0"
      };

      BaseModel.writeModelParams(mp, "F:\\batInspector\\dat\\default_model_params.xml");
    }

    private void testSplitWavs()
    {
      string path = "F:/prj/BatInspector/TestData/wavTest";
      string wavName = path + "/Eptesicus_serotinus_Ski0113_S1_From2475052ms_To2501559ms.wav";
      string jsonName = path + "/Eptesicus_serotinus_Ski0113_S1_From2475052ms_To2501559ms.wav.json";
      double splitLengh = 0.7;
      WavFile.splitWav(wavName, splitLengh, true);
      Bd2AnnFile.splitAnnotation(jsonName, splitLengh, true);
    }

    private void testCreatePngCpp()
    {
      string path = "F:/prj/BatInspector/TestData/wavTest";
      string wavName = path + "/Eptesicus_serotinus_Ski0113_S1_From2475052ms_To2501559ms.wav";
//      WavFile wav = new WavFile();
//      wav.readFile(wavName);
      BioAcoustics.setColorTable();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      BioAcoustics.createPngFromWav(wavName, 512, 512, 30);
      sw.Stop();
    }

    private void fixAnnotationIds()
    {
      //      string[] dirs = { "Bbar", "Enil", "Eser", "Hsav", "Malc", "Mbec", "Mbra", "Mdas","Mdau","Mema", "Mmyo", "Mmys", "Mnat","Nlei",
      //                        "Nnoc", "Pkuh", "Plecotus", "Pnat", "Ppip", "Ppyg", "Rfer", "Rhip", "Vmur" };
      string[] dirs = { "Rfer" };
      string root = "F:\\bat\\trainingBd2\\raw";
      foreach (string dir in dirs)
      {
        string[] files = Directory.GetFiles(Path.Combine(root, dir, "annotations"), "*.json");
        foreach (string fileName in files)
        {
          Bd2AnnFile.checkAndFixId(fileName);
        }
      }
    }

    private void checkIfWavFilesExist()
    {
      string root = "F:\\bat\\trainingBd2";
      string[] files = Directory.GetFiles(Path.Combine(root, "ann"), "*.json");
      foreach (string fileName in files)
      {
        Bd2AnnFile file = Bd2AnnFile.loadFrom(fileName);
        if (file != null)
        {
          string wavFile = Path.Combine(root, "wav", file.id);
          if (!File.Exists(wavFile))
          {
            _errors++;
            DebugLog.log($"file not existing: {wavFile} ", enLogType.ERROR);
          }
        }
        else
        {
          _errors++;
          DebugLog.log("erroneous annotation File: {fileName}", enLogType.ERROR);
        }
      }
    }


    private void assert(string a, string exp)
    {
      if( a != exp)
      {
        DebugLog.log("assertion error: " + exp + " != " + a, enLogType.ERROR);
        _errors++;
      }
    }

    private void assert(string a, bool exp)
    {
      if (!exp)
      {
        DebugLog.log("assertion error: " + a, enLogType.ERROR);
        _errors++;
      }
    }
  }
}
