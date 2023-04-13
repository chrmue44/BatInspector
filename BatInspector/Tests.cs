using libParser;
using libScripter;
using System;
using System.Collections.Generic;


namespace BatInspector
{

  struct stFormulaData
  {
    public stFormulaData(int i, string f, string r, string e)
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
    stFormulaData[] _dataForm;
    ViewModel _model;

  public Tests(ViewModel model)
    {
      _proc = new ProcessRunner();
      _model = model;
      _dataForm = new stFormulaData[]
        {
        new stFormulaData(1, "substr(\"ABCDE\",0,2)", "AB",""),
        new stFormulaData(2, "substr(\"ABCDE\",3,2)", "DE",""),
        new stFormulaData(3, "substr(\"ABCDE\",4,2)", "ARG2_OUT_OF_RANGE","ARG2_OUT_OF_RANGE"),
        new stFormulaData(4, "cast(2.5,\"INT\")", "2",""),
        new stFormulaData(5, "1+3+4", "8",""),
        new stFormulaData(6, "-1+3+4", "6",""),
        new stFormulaData(7, "-5.0*(3.1 + 2.9)", "-30.0000",""),
        new stFormulaData(8, "a=0d22-11-10T13:45:00", "0d22-11-10T13:45:00",""),
        new stFormulaData(9, "a=0d22+11-10T13:45:00", "BAD_TOKEN","BAD_TOKEN"),
        new stFormulaData(10, "0d22-11-10T13:45:00 + 60", "0d22-11-10T13:46:00",""),
        new stFormulaData(11, "0d22-11-10T13:45:00 - 2", "0d22-11-10T13:44:58",""),
        new stFormulaData(12, "0d22-11-10T13:45:00 < 0d22-11-10T13:46:00", "TRUE",""),
        new stFormulaData(13, "0d22-11-10T13:45:00 < 0d22-11-10T13:44:00", "FALSE",""),
        new stFormulaData(14, "0d22-11-10T13:45:00 < 0d22-11-10T13:45:00", "FALSE",""),
        new stFormulaData(15, "0d22-11-10T13:45:00 > 0d22-11-10T13:46:00", "FALSE",""),
        new stFormulaData(16, "0d22-11-10T13:45:00 > 0d22-11-10T13:44:00", "TRUE",""),
        new stFormulaData(17, "0d22-11-10T13:45:00 > 0d22-11-10T13:45:00", "FALSE",""),
        new stFormulaData(18, "1 + 4i + 2 - 2i", "3.0000 + 2.0000i",""),
        new stFormulaData(19, "0t13:45:00 > 0t13:44:00", "TRUE",""),
        new stFormulaData(20, "0t13:45:00 - 0t13:41:00", "0t00:04:00",""),
        new stFormulaData(21, "tod(\"0d22-11-10T17:48:22\")", "0t17:48:22",""),
        new stFormulaData(22, "0t13:45:00 + 0t01:01:02", "0t14:46:02",""),
        new stFormulaData(23, "0d22-11-10T13:45:00 - 0t02:00:01", "0d22-11-10T11:44:59",""),
        new stFormulaData(24, "indexOf(\"abcdef\",\"cde\")","2",""),
        new stFormulaData(25, "indexOf(\"abcdef\",\"efg\")","-1",""),

        };

    }

    public void exec()
    {
      string wrkDir = AppParams.Inst.AppRootPath + "/../../../scripts";
  //    testBioAcoustics();
      testIf(wrkDir);
      testWhile(wrkDir);
      testParser();
      testCsvFuncs(wrkDir);
      testClassifier();
      testSumReport();
  //    testSignalForm();
      testSimCall();
      //   testReportModelBatdetect2();
      // testCreatePrj();
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
      SumReport rep = new SumReport(null);
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
    }

    private void testCsvFuncs(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null, null);
      scr.RunScript("test_csv.scr", false);
      assert(scr.getVariable("c23"), "23");
      assert(scr.getVariable("c123"), "123");
      assert(scr.getVariable("c43"), "43");
      assert(scr.getVariable("c55"), "55");
      assert(scr.getVariable("c62"), "62");
    }

    private void testWhile(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null, null);
      scr.SetVariable("Limit", "9");
      scr.SetVariable("b", "0");
      scr.RunScript("test_while.scr", false, false);
      assert(scr.getVariable("sum"), "9");
      scr.SetVariable("Limit", "23");
      scr.SetVariable("b", "0");
      scr.RunScript("test_while.scr", false, false);
      assert(scr.getVariable("sum"), "23");
      scr.SetVariable("Limit", "9");
      scr.SetVariable("b", "1");
      scr.RunScript("test_while.scr", false, false);
      assert(scr.getVariable("sum"), "5");
      scr.SetVariable("Limit", "23");
      scr.SetVariable("b", "1");
      scr.RunScript("test_while.scr", false, false);
      assert(scr.getVariable("sum"), "5");
    }

    private void testIf(string wrkDir)
    {
      ScriptRunner scr = new ScriptRunner(ref _proc, wrkDir, null, null);

      scr.SetVariable("A", "55");
      scr.SetVariable("B", "34");
      scr.RunScript("test_if.scr", false, false);
      assert(scr.getVariable("Result"), "\"AlowBhigh\"");
      assert(scr.getVariable("Res2"), "\"AlowBhigh\"");

      scr.SetVariable("A", "55");
      scr.SetVariable("B", "32");
      scr.RunScript("test_if.scr", false, false);
      assert(scr.getVariable("Result"), "\"AlowBlow\"");
      assert(scr.getVariable("Res2"), "\"AlowBlow\"");

      scr.SetVariable("A", "101");
      scr.SetVariable("B", "34");
      scr.RunScript("test_if.scr", false, false);
      assert(scr.getVariable("Result"), "\"AhighBhigh\"");
      assert(scr.getVariable("Res2"), "\"AhighBhigh\"");

      scr.SetVariable("A", "101");
      scr.SetVariable("B", "33");
      scr.RunScript("test_if.scr", false, false);
      assert(scr.getVariable("Result"), "\"AhighBlow\"");
      assert(scr.getVariable("Res2"), "\"AhighBhigh\"");
    }

    int testParser()
    {

      int retVal = 0;
      DebugLog.log("Testing parser", enLogType.INFO);
      foreach (stFormulaData f in _dataForm)
      {
        string form = f.Formula;
        Expression exp = new Expression(null);
        if (f.Id == 22)
          retVal = 0;
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
      int sampleRate = 0;
      double duration = 0;
      ThresholdDetectItem[]items = BioAcoustics.analyzeCalls(wavFile, out sampleRate, out duration);
      return 0;
    }

    private int testClassifier()
    {
      int retVal = 0;
      List<ParLocation> locs = new List<ParLocation>();
      locs.Add(new ParLocation(49.963175, 8.563220));
      locs.Add(new ParLocation(49.727209, 8.539645));
      locs.Add(new ParLocation(49.728493, 8.788927));
      locs.Add(new ParLocation(49.839054, 9.030958));
      locs.Add(new ParLocation(49.995876, 9.010885));

      ParLocation pos = new ParLocation(49.753933, 8.632084);
      bool ret = _model.Regions.inside(pos, locs);
      assert("loc inside", ret == true);
      pos = new ParLocation(50.1, 8.632084);
      ret = _model.Regions.inside(pos, locs);
      assert("loc inside", ret == false);
      pos = new ParLocation(49.8, 9.1);
      ret = _model.Regions.inside(pos, locs);
      assert("loc inside", ret == false);
      pos = new ParLocation(49.8, 8.4);
      ret = _model.Regions.inside(pos, locs);
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
      List<FreqItem> l = new List<FreqItem>();
      l.Add(new FreqItem(100000, 0, 0));
      l.Add(new FreqItem(100000, 5e-3, 0));
      l.Add(new FreqItem(95000, 7e-3, 0.02));
      l.Add(new FreqItem(80000, 10e-3, 0.05));
      l.Add(new FreqItem(60000, 15e-3, 0.08));
      l.Add(new FreqItem(40000, 25e-3, 0.1));
      l.Add(new FreqItem(25000, 28e-3, 0.08));
      l.Add(new FreqItem(22000, 29e-3, 0));
      l.Add(new FreqItem(22000, 500e-3, 0));
      SimCall call = new SimCall(l, 384000);
    }

    private void testCreatePrj()
    {
      PrjInfo prj = new PrjInfo();
      prj.Name = "Test";
      prj.SrcDir = "G:\\bat\\src";
      prj.DstDir = "G:\\bat\\test";
      prj.MaxFileLenSec = 5;
      prj.MaxFileCnt = 30;
      prj.Weather = "12°C, bedeckt";
      prj.Landscape = "Uferbereich Waldweiher";
      prj.Latitude = 49.123;
      prj.Longitude = 8.123;
      Project.createPrj(prj, _model.Regions, _model.SpeciesInfos);
    }

    private void testReportModelBatdetect2()
    {
      List <SpeciesInfos> species = BatInfo.load().Species;
      ModelBatDetect2 model = new ModelBatDetect2(1);
      string WavDir = "G:\\bat\\2022\\20220326\\Records";
      string AnnotationDir = "G:\\bat\\2022\\20220326\\ann";
      string reportName = "G:\\bat\\2022\\20220326\\report2.csv";
      model.createReportFromAnnotations(0.5, species, WavDir, AnnotationDir, reportName);
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
