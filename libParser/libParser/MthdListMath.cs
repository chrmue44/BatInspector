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
using System;
using System.Collections.Generic;

namespace libParser
{
  public class MthdListMath : MethodList
  {
    public MthdListMath() : base()
    {
      m_pMethods = null;
    }

    // Initialisiert die Funktionstabelle
    public override void initMthdTab()
    {
      m_Inst = this;
      MathHelpTab = new List<HelpTabItem>();
      addMethod(new FuncTabItem("sqrt", sqrt));
      MathHelpTab.Add(new HelpTabItem("sqrt", "returns square root of a number",
                      new List<string> { "1: number" }, new List<string> { "1: square root of number" }));
      addMethod(new FuncTabItem("sin", sin));
      MathHelpTab.Add(new HelpTabItem("sin", "returns sinus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: sinus of number" }));
      addMethod(new FuncTabItem("asin", asin));
      MathHelpTab.Add(new HelpTabItem("asin", "returns arcus sinus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: arcus sinus of number" }));
      addMethod(new FuncTabItem("sinh", sinh));
      MathHelpTab.Add(new HelpTabItem("sinh", "returns sinus hyperbolicus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: sinus hyperbolicus of number" }));
      addMethod(new FuncTabItem("cos", cos));
      MathHelpTab.Add(new HelpTabItem("cos", "returns cosinus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: cosinus of number" }));
      addMethod(new FuncTabItem("acos", acos));
      MathHelpTab.Add(new HelpTabItem("acos", "returns arcus cosinus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: arcus cosinus of number" }));
      addMethod(new FuncTabItem("cosh", cosh));
      MathHelpTab.Add(new HelpTabItem("cosh", "returns cosinus hyberbolicus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: cosinus hyperbolicus of number" }));
      addMethod(new FuncTabItem("tan", tan));
      MathHelpTab.Add(new HelpTabItem("tan", "returns tangens of a number",
                      new List<string> { "1: number" }, new List<string> { "1: tangens of number" }));
      addMethod(new FuncTabItem("atan", atan));
      MathHelpTab.Add(new HelpTabItem("atan", "returns arcus tangens of a number",
                      new List<string> { "1: number" }, new List<string> { "1: arcus tangens of number" }));
      addMethod(new FuncTabItem("tanh", tanh));
      MathHelpTab.Add(new HelpTabItem("tanh", "returns tangens hyperbolicus of a number",
                      new List<string> { "1: number" }, new List<string> { "1: tangens hyperolicus of number" }));
      addMethod(new FuncTabItem("pow", pow));
      MathHelpTab.Add(new HelpTabItem("pow", "power",
                      new List<string> { "1: number", "2: exponent" }, new List<string> { "1: power of number" }));
      addMethod(new FuncTabItem("ln", ln));
      MathHelpTab.Add(new HelpTabItem("ln", "logarithm",
                      new List<string> { "1: number" }, new List<string> { "1: logarithm of number" }));
      addMethod(new FuncTabItem("abs", abs));
      MathHelpTab.Add(new HelpTabItem("abs", "absolut value",
                      new List<string> { "1: number" }, new List<string> { "1: absolut value of number(real or complex)" }));
      addMethod(new FuncTabItem("arg", arg));
      MathHelpTab.Add(new HelpTabItem("arg", "argument of complex number",
                      new List<string> { "1: number" }, new List<string> { "1: argument of complex number" }));
      addMethod(new FuncTabItem("exp", exp));
      MathHelpTab.Add(new HelpTabItem("exp", "exponential e^x",
                      new List<string> { "1: number" }, new List<string> { "1: e^x" }));
      addMethod(new FuncTabItem("vars", vars));
      MathHelpTab.Add(new HelpTabItem("vars", "dump list of variables",
                      new List<string>(), new List<string> { "1: list of variables" }));
      addMethod(new FuncTabItem("consts", consts));
      MathHelpTab.Add(new HelpTabItem("consts", "dump list of constants",
                      new List<string>(), new List<string> { "1: list of constants" }));
      addMethod(new FuncTabItem("cast", cast));
      MathHelpTab.Add(new HelpTabItem("cast", "cast the type of an expression to the desired type",
                      new List<string>{"1 : Expr:   any valid expression","2 : Type:   desired type\n" +
      "            FLOAT = 1, INT32 = 2, UINT32 = 3, INT64 = 4,UINT64 = 5,\n" +
      "            HEXVAL = 6,STR = 7, BOOL = 8"}, new List<string> { "1: result in desired type", }));
      addMethod(new FuncTabItem("if", if_func));
      MathHelpTab.Add(new HelpTabItem("if", "returns argument 2 or 3 depending in boolean value of argument 1",
                      new List<string>{"1 : condition:  boolean expression","2 : return value if condition is true",
                                       "3 : return value if condition is false" }, new List<string> { "1: result depending on condition", }));
      addMethod(new FuncTabItem("plot", plot));
      MathHelpTab.Add(new HelpTabItem("plot", "plot values of given function",
                     new List<string> {"1:name of variable with formula",
                                       "2: name of variable with argument",
                                       "3: min value for argument",
                                       "4: max value for argument",
                                       "5: nr of steps"}, new List<string> {"1: list of values" }));
      MathHelpTab.Add(new HelpTabItem("addbit", "add bits to a PLC address",
                      new List<string> { "1: PLC address", "2: bits: number of bits to add" }, new List<string> { "1: PLC address" }));
      addMethod(new FuncTabItem("addbit", addbit));
      MathHelpTab.Add(new HelpTabItem("select", "select element from list",
                      new List<string> { "1: PLC address", "2: bits: number of bits to add" }, new List<string> { "1: PLC address" }));
      addMethod(new FuncTabItem("select", select));
      MathHelpTab.Add(new HelpTabItem("strlen", "returns the length of a string if the variable type is STRING otherwise 0",
                      new List<string> { "1: a string" }, new List<string> { "1: length of string" }));
      addMethod(new FuncTabItem("strlen", strlen));
      MathHelpTab.Add(new HelpTabItem("substr", "returns substring if the variable type is STRING otherwise 0",
                      new List<string> { "1: a string", "2:index of first char", "3: length of substring" }, new List<string> { "1: length of string" }));
      addMethod(new FuncTabItem("substr", substr));
    }

    // Ueberschrift fuer Hilfe zur Methodenliste
    public override string getMthdListHelp()
    {
      return "\nlist of math commands";
    }

    // liefert die Tabelle mit detaillierter Hilfe zu den Methoden
    public override List<HelpTabItem> getHelpTab()
    {
      return MathHelpTab;

    }

    void setMethods(Methods pMethods)
    {
      m_pMethods = pMethods;
    }

    /* inline void setInst(CMthdListMath* pInst)
     {
       m_Inst = pInst;
     }*/

    static tParseError sqrt(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        if (((int)argv[0].getType() & (int)AnyType.TYPEFLAG_INT) != 0)
          argv[0].changeType(AnyType.tType.RT_FLOAT);
        if ((argv[0].getType() == AnyType.tType.RT_FLOAT) && (argv[0].getFloat() < 0))
          argv[0].changeType(AnyType.tType.RT_COMPLEX);
        if (argv[0].getType() == AnyType.tType.RT_FLOAT)
          result.assign(Math.Sqrt(argv[0].getFloat()));
        else if (argv[0].getType() == AnyType.tType.RT_COMPLEX)
        {
          double abs = argv[0].getComplexAbs();
          double arg = argv[0].getComplexArg();
          arg = arg / 2.0 + Math.PI;
          abs = Math.Sqrt(abs);
          result.setType(AnyType.tType.RT_COMPLEX);
          result.setComplexPolar(abs, arg);
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError sin(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Sin(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError asin(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Asin(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError sinh(List<AnyType> argv, out AnyType result)
{
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Sinh(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError cos(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Cos(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError acos(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Acos(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError cosh(List<AnyType> argv, out AnyType result)
    {
      {
        tParseError err = 0;
        result = new AnyType();
        if (argv.Count == 1)
        {
          argv[0].changeType(AnyType.tType.RT_FLOAT);
          result.assign(Math.Cosh(argv[0].getFloat()));
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
        return err;
      }
    }


    static tParseError tan(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Tan(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError atan(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Atan(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError tanh(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Tanh(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError pow(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 2)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        argv[1].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Pow(argv[0].getFloat(), argv[1].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;

    }


    static tParseError ln(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Log(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError exp(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        if (argv[0].getType() == AnyType.tType.RT_COMPLEX)
        {
          double re = Math.Exp(argv[0].getComplexRe()) * Math.Cos(argv[0].getComplexIm());
          double im = Math.Exp(argv[0].getComplexRe()) * Math.Sin(argv[0].getComplexIm());
          result.setType(AnyType.tType.RT_COMPLEX);
          result.setComplex(re, im);
        }
        else {
          argv[0].changeType(AnyType.tType.RT_FLOAT);
          result.assign(Math.Exp(argv[0].getFloat()));
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError plot(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 5)
      {
        argv[0].changeType(AnyType.tType.RT_STR);      ///< name of variable with formula
        argv[1].changeType(AnyType.tType.RT_STR);      ///< name of variable with argument
        argv[2].changeType(AnyType.tType.RT_FLOAT);    ///< min value for argument
        argv[3].changeType(AnyType.tType.RT_FLOAT);    ///< max value for argument
        argv[4].changeType(AnyType.tType.RT_UINT64);   ///< nr of steps
        do
        {
          AnyType func = new AnyType();
          AnyType funcArg = new AnyType();
          string resultString;
          VarName nameFunc = m_Inst.m_pVarList.look(argv[0].getString());
          if (nameFunc != null)
            err = (tParseError)nameFunc.getValue(0, ref func);
          else
            err = tParseError.VARIABLE;
          if (err != tParseError.SUCCESS)
            break;

          VarName nameFuncArg = m_Inst.m_pVarList.look(argv[1].getString());
          if (nameFuncArg != null)
            err = (tParseError)nameFuncArg.getValue(0, ref funcArg);
          else
            err = tParseError.VARIABLE;
          if (err != tParseError.SUCCESS)
            break;
          resultString = "";
          AnyType step = new AnyType();
          step.assign((argv[3].getFloat() - argv[2].getFloat()) / (argv[4].getUint64()));
          AnyType arg = new AnyType();
          arg.assign(argv[2].getFloat());
          for (; arg.getFloat() <= argv[3].getFloat(); arg += step)
          {
            nameFuncArg.setValue(0, arg);
            nameFunc.getValue(0, ref func);
            func.changeType(AnyType.tType.RT_STR);
            resultString += "\n";
            resultString += nameFunc.getName();
            resultString += "(";
            nameFuncArg.getValue(0, ref funcArg);
            funcArg.changeType(AnyType.tType.RT_STR);
            resultString += funcArg.getString();
            resultString += ")= ";
            resultString += func.getString();
          }
          result.assign(resultString);
        } while (false);
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;

    }


    static tParseError vars(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      string str = m_Inst.m_pVarList.dumpVarList(false);
      result.assign(str);
      return err;
    }


    static tParseError consts(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      string str = m_Inst.m_pVarList.dumpVarList(true);
      result.assign(str);
      return err;
    }


    static tParseError abs(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        result.assign(Math.Abs(argv[0].getFloat()));
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError arg(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        switch (argv[0].getType())
        {
          case AnyType.tType.RT_COMPLEX:
            result.assign(argv[0].getComplexArg());
            break;
          case AnyType.tType.RT_FLOAT:
          case AnyType.tType.RT_INT64:
          case AnyType.tType.RT_UINT64:
            result.assign(0.0);
            break;
          default:
            err = tParseError.ARG_NOT_SUPPORTED;
            break;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError cast(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();

      if (argv.Count == 2)
      {
        result.assign(argv[0]);
        AnyType.tType Type = AnyType.tType.RT_INT64;
        Enum.TryParse(argv[0].getString(), out Type);
        result.changeType(Type);
      }
      else
      {
        err = tParseError.NR_OF_ARGUMENTS;
      }
      return err;
    }

    static tParseError if_func(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();

      if (argv.Count == 3)
      {
        if (argv[0].getType() == AnyType.tType.RT_BOOL)
        {
          if(argv[0].getBool())
            result.assign(argv[1]);
          else
            result.assign(argv[2]);
        }
        else
         err = tParseError.ARGUMENT_TYPE_ARG1;
      }
      else
      {
        err = tParseError.NR_OF_ARGUMENTS;
      }
      return err;
    }

    static tParseError addbit(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 2)
      {
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        double x = argv[0].getFloat();
        argv[1].changeType(AnyType.tType.RT_FLOAT);
        double b = argv[1].getFloat();
        double res = (x * 1.25 - Math.Floor(x) / 4 + b / 8) * 0.8 + Math.Floor(x * 1.25 - Math.Floor(x) / 4 + b / 8) * 0.2;
        result.assign(AnyType.doubleToString(res, 1));        
      }
      else
      {
        err = tParseError.NR_OF_ARGUMENTS;
      }
      return err;
    }

    static tParseError strlen(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        double len;
        if (argv[0].getType() == AnyType.tType.RT_STR)
          len = argv[0].getString().Length;
        else
          len = 0;
        result.assign(len);
      }
      else
      {
        err = tParseError.NR_OF_ARGUMENTS;
      }
      return err;
    }

    static tParseError substr(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 3)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string str = argv[0].getString();
        argv[1].changeType(AnyType.tType.RT_INT64);
        int start = (int)argv[1].getInt64();
        argv[2].changeType(AnyType.tType.RT_INT64);
        int len = (int)argv[2].getInt64();
        if ((start + len) <= str.Length)
        {
          string resultStr = str.Substring(start, len);
          result.assign(resultStr);
        }
        else
          err = tParseError.ARG2_OUT_OF_RANGE;
      }
      else
      {
        err = tParseError.NR_OF_ARGUMENTS;
      }
      return err;
    }

    static tParseError select(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 3)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string list = argv[0].getString();
        argv[1].changeType(AnyType.tType.RT_STR);
        string s = argv[1].getString();
        argv[2].changeType(AnyType.tType.RT_INT64);
        long i = argv[2].getInt64();
        string[] split = list.Split(s[0]);
        if ((i >= 1) && (i <= split.Length))
          result.assign(split[i-1]);
        else
          err = tParseError.ARG3_OUT_OF_RANGE;
      }
      else
      {
        err = tParseError.NR_OF_ARGUMENTS;
      }
      return err;
    }

    // static tParseError unittest(List<AnyType> argv, out AnyType result);


    static tParseError savevars(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      m_Inst.m_pVarList.save("vars.csv");
      return err;
    }

  

    static tParseError loadvars(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      m_Inst.m_pVarList.load("vars.csv", m_Inst.m_pMethods, false);
      return err;
    }

    Methods m_pMethods;
    static MthdListMath m_Inst;
    List<HelpTabItem> MathHelpTab = new List<HelpTabItem>();
  };
}
