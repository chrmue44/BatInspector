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
using System.Globalization;

namespace libParser
{
  public class AnyType
  {

    // Datentypen, die von clAnyType unterstuetzt werden
    const int TYPEFLAG_BOOL = 0x10;
    public const int TYPEFLAG_INT = 0x20;
    const int TYPEFLAG_FLOAT = 0x40;
    const int TYPEFLAG_CPLX = 0x80;
    const int TYPEFLAG_FORM = 0x100;
    const int TYPEFLAG_STRING = 0x800;

    public enum tType
    {
      RT_BOOL = TYPEFLAG_BOOL | 0x01,
      //  RT_UINT32  = TYPEFLAG_INT    | 0x01,
      //  RT_INT32   = TYPEFLAG_INT    | 0x02,
      RT_UINT64 = TYPEFLAG_INT | 0x03,
      RT_INT64 = TYPEFLAG_INT | 0x04,
      RT_HEXVAL = TYPEFLAG_INT | 0x05,
      RT_FLOAT = TYPEFLAG_FLOAT | 0x01,
      RT_COMPLEX = TYPEFLAG_CPLX | 0x01,
      RT_COMMENT = TYPEFLAG_STRING | 0x01,
      RT_STR = TYPEFLAG_STRING | 0x02,
      RT_FORMULA = TYPEFLAG_FORM | 0x01,
    };


    struct stComplex
    {
      public double re;
      public double im;
    };

    struct tValue
    {
      public stComplex Complex; ///< Wert als komplexe Zahl
      public double Double;     ///< Wert als Float
      public Int64 Int64;     ///< Wert als int64
      public UInt64 Uint64;   ///< Wert als uint64
      public bool Bool;         ///< Wert als Bool
      public string String;
    }


    // Klasse zur Darstellung von Zahlen in verschiedenen Typen und zur 
    // Konvertierung von einem Typ in den anderen. 
    // Ermoeglicht dem Parser das Parsen von Ausdruecken unabhaengig vom
    // Typ der beteiligten Variablen
    public AnyType()
    {
      m_Type = tType.RT_FLOAT;
      m_Val.String = "";
      m_pVarList = null;
      m_pMethods = null;
      m_Val.Double = 0.0;
    }

    public AnyType(AnyType x)
    {
      assign(x);
    }

    public AnyType(VarList pVarList, Methods pMethods)
    {
      m_Type = tType.RT_FLOAT;
      m_Val.String = "";
      m_pVarList = pVarList;
      m_pMethods = pMethods;
      m_Val.Double = 0.0;
    }

    // Zuweisungsoperatoren
    // ACHTUNG: dieser Zuweisungsoperator funktioniert nur mit o-terminierten
    // Strings, ansonsten setString() verwenden!
    public AnyType(string ValStr)
    {
      m_Type = tType.RT_STR;
      if (ValStr != null)
      {
        if (ValStr[0] == '=')
          m_Type = tType.RT_FORMULA;
        m_Val.String = ValStr;
      }
      else
        m_Val.String = "";
    }
    /*
    public AnyType(Int64 Val)
    {
      m_Type = tType.RT_INT64;
      m_Val.Int64 = Val;
    }

    public AnyType(UInt64 Val)
    {
      m_Type = tType.RT_UINT64;
      m_Val.Uint64 = Val;
    }

    public AnyType(double Val)
    {
      m_Type = tType.RT_FLOAT;
      m_Val.Double = Val;
    }

    public AnyType(bool Val)
    {
      m_Type = tType.RT_BOOL;
      m_Val.Bool = Val;
    }
    */
    // Vergleichsoperatoren
    public static bool operator <(AnyType a1, AnyType a2)
    {
      bool Result = false;

      if (a1.m_Type == tType.RT_UINT64)
        a1.changeType(tType.RT_INT64);
      if (a1.m_Type == tType.RT_FORMULA)
        a1.assignFormulaResult();

      a1.adaptType(ref a2);
      switch (a1.m_Type)
      {
        case tType.RT_INT64:
          Result = (a1.m_Val.Int64 < a2.getInt64());
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          Result = (a1.m_Val.Uint64 < a2.getUint64());
          break;

        case tType.RT_FLOAT:
          Result = (a1.m_Val.Double < a2.getFloat());
          break;
        case tType.RT_STR:
        case tType.RT_COMMENT:
        case tType.RT_BOOL:
        case tType.RT_COMPLEX:
        case tType.RT_FORMULA:
          Error.report(tParseError.LESS_NOT_SUPPORTED);
          break;
      }
      return Result;
    }

    public static bool operator <=(AnyType a1, AnyType a2)
    {
      return !(a1 > a2);
    }


    public static bool operator >(AnyType a1, AnyType a2)
    {
      bool Result = false;

      if (a1.m_Type == tType.RT_UINT64)
        a1.changeType(tType.RT_INT64);
      if (a1.m_Type == tType.RT_FORMULA)
        a1.assignFormulaResult();

      a1.adaptType(ref a2);
      switch (a1.m_Type)
      {
        case tType.RT_INT64:
          Result = (a1.m_Val.Int64 > a2.getInt64());
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          Result = (a1.m_Val.Uint64 > a2.getUint64());
          break;

        case tType.RT_FLOAT:
          Result = (a1.m_Val.Double > a2.getFloat());
          break;
        case tType.RT_STR:
        case tType.RT_COMMENT:
        case tType.RT_BOOL:
        case tType.RT_COMPLEX:
        case tType.RT_FORMULA:
          Error.report(tParseError.LESS_NOT_SUPPORTED);
          break;
      }
      return Result;
    }

    public static bool operator >=(AnyType a1, AnyType a2)
    {
      return !(a1 < a2);
    }
    public override bool Equals(object obj)
    {
      AnyType x;
      try
      {

        x = (AnyType)obj;
        return this == x;
      }
      catch
      {
        return false;
      }
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public static bool operator ==(AnyType a1, AnyType a2)
    {
      bool Result = false;

      if (a1.m_Type == tType.RT_UINT64)
        a1.changeType(tType.RT_INT64);
      if (a1.m_Type == tType.RT_FORMULA)
        a1.assignFormulaResult();

      a1.adaptType(ref a2);
      switch (a1.m_Type)
      {
        case tType.RT_INT64:
          Result = (a1.m_Val.Int64 == a2.getInt64());
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          Result = (a1.m_Val.Uint64 == a2.getUint64());
          break;

        case tType.RT_FLOAT:
          Result = (a1.m_Val.Double == a2.getFloat());
          break;

        case tType.RT_STR:
          Result = (a1.m_Val.String == a2.m_Val.String);
          break;

        case tType.RT_BOOL:
          Result = (a1.getBool() == a2.getBool());
          break;

        case tType.RT_COMPLEX:
          Result = ((a1.getComplexRe() == a2.getComplexRe()) && (a1.getComplexIm() == a2.getComplexIm()));
          break;

        case tType.RT_COMMENT:
        case tType.RT_FORMULA:
          Error.report(tParseError.LESS_NOT_SUPPORTED);
          break;
      }
      return Result;
    }

    public static bool operator !=(AnyType a1, AnyType a2)
    {
      return !(a1 == a2);
    }


    public static AnyType operator& (AnyType s1, AnyType s2)
{
      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      s1.adaptType(ref s2);
      switch (s1.m_Type)
      {
        case tType.RT_INT64:
          s1.m_Val.Int64 &= s2.getInt64();
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          s1.m_Val.Uint64 &= s2.getUint64();
          break;

        case tType.RT_BOOL:
          s1.m_Val.Bool &= s2.getBool();
          break;

        case tType.RT_STR:
          Error.report(tParseError.STRING_AND);
          break;

        case tType.RT_FLOAT:
          Error.report(tParseError.FLOAT_AND);
          break;

        case tType.RT_COMMENT:
        case tType.RT_FORMULA:
        case tType.RT_COMPLEX:
          Error.report(tParseError.AND_NOT_SUPPORTED);
          break;
      }
      return s1;
    }


    public static AnyType operator| (AnyType s1, AnyType s2)
{
      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      s1.adaptType(ref s2);
      switch (s1.m_Type)
      {
        case tType.RT_INT64:
          s1.m_Val.Int64 |= s2.getInt64();
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          s1.m_Val.Uint64 |= s2.getUint64();
          break;

        case tType.RT_BOOL:
          s1.m_Val.Bool |= s2.getBool();
          break;

        case tType.RT_STR:
          Error.report(tParseError.STRING_OR);
          break;

        case tType.RT_COMMENT:
        case tType.RT_FORMULA:
        case tType.RT_COMPLEX:
          Error.report(tParseError.OR_NOT_SUPPORTED);
          break;

        case tType.RT_FLOAT:
          Error.report(tParseError.FLOAT_OR);
          break;
      }
      return s1;
    }


    // Mathematik
    public static AnyType operator +(AnyType s1, AnyType s2)
    {
      AnyType Zero = new AnyType();
      Zero.assign((UInt64)0);
      if ((s2.getType() != tType.RT_COMPLEX) && (s2.getType() == tType.RT_INT64))
      {
        if ((s2 < Zero) && ((s1.m_Type == tType.RT_UINT64) || (s1.m_Type == tType.RT_HEXVAL)))
          s1.changeType(tType.RT_INT64);
      }

      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      if (s2.getType() == tType.RT_FORMULA)
        s2.assignFormulaResult();

      s1.adaptType(ref s2);
      switch (s1.m_Type)
      {
        case tType.RT_INT64:
          s1.m_Val.Int64 += s2.getInt64();
          break;

        case tType.RT_UINT64:
          s1.m_Val.Uint64 += s2.getUint64();
          break;

        case tType.RT_HEXVAL:
          s1.m_Val.Uint64 += s2.getUint64();
          break;

        case tType.RT_BOOL:
          s1.m_Val.Bool |= s2.getBool();
          break;

        case tType.RT_STR:
          s1.m_Val.String += s2.getString();
          break;

        case tType.RT_FLOAT:
          s1.m_Val.Double += s2.getFloat();
          break;

        case tType.RT_FORMULA:
        case tType.RT_COMMENT:
          Error.report(tParseError.PLUS_NOT_SUPPORTED);
          break;
        case tType.RT_COMPLEX:
          s1.m_Val.Complex.re += s2.getComplexRe();
          s1.m_Val.Complex.im += s2.getComplexIm();
          break;
      }
      return s1;
    }


    public static AnyType operator -(AnyType s1, AnyType s2)
    {
      AnyType Zero = new AnyType();
      Zero.assign((UInt64)0);
      if ((s2.getType() != tType.RT_COMPLEX) && (s2.getType() == tType.RT_INT64))
      {
        if ((s2 < Zero) && ((s1.m_Type == tType.RT_UINT64) || (s1.m_Type == tType.RT_HEXVAL)))
          s1.changeType(tType.RT_INT64);
      }

      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      if (s2.getType() == tType.RT_FORMULA)
        s2.assignFormulaResult();

      s1.adaptType(ref s2);
      switch (s1.m_Type)
      {
        case tType.RT_INT64:
          s1.m_Val.Int64 -= s2.getInt64();
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          if (s1 < s2)
          {
            s1.changeType(tType.RT_INT64);
            s2.changeType(tType.RT_INT64);
            s1.m_Val.Int64 -= s2.getInt64();
          }
          else
            s1.m_Val.Uint64 -= s2.getUint64();
          break;

        case tType.RT_FLOAT:
          s1.m_Val.Double -= s2.getFloat();
          break;

        case tType.RT_FORMULA:
        case tType.RT_COMMENT:
        case tType.RT_STR:
        case tType.RT_BOOL:
          Error.report(tParseError.MINUS_NOT_SUPPORTED);
          break;
        case tType.RT_COMPLEX:
          s1.m_Val.Complex.re += s2.getComplexRe();
          s1.m_Val.Complex.im += s2.getComplexIm();
          break;
      }
      return s1;
    }


    public static AnyType operator* (AnyType s1, double m)
{
      AnyType s = new AnyType();

      // wenn vorzeichenbehaftete Zahl multipliziert wird, Typ in INT wandeln
      if ((s1.m_Type == tType.RT_UINT64) || (s1.m_Type == tType.RT_HEXVAL))
        s1.changeType(tType.RT_INT64);
      s.assign(m);
      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      s1.adaptType(ref s);
      return s;
    }


    public static AnyType operator*(AnyType s1, AnyType s2)
    {
      AnyType Zero = new AnyType();
      Zero.assign((UInt64)0);
      if ((s2.getType() != tType.RT_COMPLEX) && (s2.getType() == tType.RT_INT64))
      {
        if ((s2 < Zero) && ((s1.m_Type == tType.RT_UINT64) || (s1.m_Type == tType.RT_HEXVAL)))
          s1.changeType(tType.RT_INT64);
      }

      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      if (s2.getType() == tType.RT_FORMULA)
        s2.assignFormulaResult();

      s1.adaptType(ref s2);
      switch (s1.m_Type)
      {
        case tType.RT_INT64:
          s1.m_Val.Int64 *= s2.getInt64();
          break;

        case tType.RT_UINT64:
          s1.m_Val.Uint64 *= s2.getUint64();
          break;

        case tType.RT_HEXVAL:
          s1.m_Val.Uint64 *= s2.getUint64();
          break;

        case tType.RT_FLOAT:
          s1.m_Val.Double *= s2.getFloat();
          break;

        case tType.RT_FORMULA:
        case tType.RT_COMMENT:
        case tType.RT_STR:
        case tType.RT_BOOL:
          Error.report(tParseError.PLUS_NOT_SUPPORTED);
          break;
        case tType.RT_COMPLEX:
          double re = s1.m_Val.Complex.re * s2.getComplexRe() - s1.m_Val.Complex.im * s2.getComplexIm();
          s1.m_Val.Complex.im = s1.m_Val.Complex.im * s2.getComplexRe() + s1.m_Val.Complex.re * s2.getComplexIm();
          s1.m_Val.Complex.re = re;
          break;
      }
      return s1;
    }


    public static AnyType operator/(AnyType s1, AnyType s2)
    {
      AnyType Zero = new AnyType();
      Zero.assign((UInt64)0);
      if ((s2.getType() != tType.RT_COMPLEX) && (s2.getType() == tType.RT_INT64))
      {
        if ((s2 < Zero) && ((s1.m_Type == tType.RT_UINT64) || (s1.m_Type == tType.RT_HEXVAL)))
          s1.changeType(tType.RT_INT64);
      }

      if (s1.m_Type == tType.RT_FORMULA)
        s1.assignFormulaResult();
      if (s2.getType() == tType.RT_FORMULA)
        s2.assignFormulaResult();

      s1.adaptType(ref s2);
      switch (s1.m_Type)
      {
        case tType.RT_INT64:
          s1.m_Val.Int64 /= s2.getInt64();
          break;

        case tType.RT_UINT64:
          s1.m_Val.Uint64 /= s2.getUint64();
          break;

        case tType.RT_HEXVAL:
          s1.m_Val.Uint64 /= s2.getUint64();
          break;

        case tType.RT_FLOAT:
          s1.m_Val.Double /= s2.getFloat();
          break;

        case tType.RT_FORMULA:
        case tType.RT_COMMENT:
        case tType.RT_STR:
        case tType.RT_BOOL:
          Error.report(tParseError.PLUS_NOT_SUPPORTED);
          break;
        case tType.RT_COMPLEX:
          double a = s1.m_Val.Complex.re;
          double b = s1.m_Val.Complex.im;
          double c = s2.getComplexRe();
          double e = s2.getComplexIm();
          if ((c != 0) || (e != 0))
          {
            double re = (a * c + b * e) / (c * c + e * e);
            double im = (b * c - a * e) / (c * c + e * e);
            s1.m_Val.Complex.re = re;
            s1.m_Val.Complex.im = im;
          }
          else
            Error.report(tParseError.DIV);
          break;
      }
      return s1;
    }

    /*    public static void operator+=(long s)
        {

        }
        public static void operator-=(AnyType s)
        {

        }
        public static void operator*=(double m)
        {

        }

        public static void operator*=(AnyType m)
        {

        }
        public static void operator/=(AnyType d)
        {

        }

        // logische Verknuepfungen
        public static void operator&=(AnyType a)
        {

        }

        public static void operator|=(AnyType a)
        {

        } */

    public static AnyType operator !(AnyType a)
    {
      AnyType r = new AnyType();
      switch (a.m_Type)
      {
        case tType.RT_INT64:
          r.assignInt64(~a.m_Val.Int64);
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          r.assign(~a.m_Val.Uint64);
          break;

        case tType.RT_BOOL:
          r.assignBool(!a.m_Val.Bool);
          break;

        case tType.RT_STR:
          Error.report(tParseError.STRING_NOT);
          break;

        case tType.RT_COMMENT:
        case tType.RT_FORMULA:
        case tType.RT_COMPLEX:
          Error.report(tParseError.NOT_NOT_SUPPORTED);
          break;

        case tType.RT_FLOAT:
          Error.report(tParseError.FLOAT_NOT);
          break;
      }
      return r;
    }

    // Typ setzen
    public void setType(tType Type)
    {
      m_Type = Type;
    }
    // in anderen Typ konvertieren und den Wert beibehalten
    public void changeType(tType NewType, uint decimals = 4)
    {
      switch (m_Type)
      {
        case tType.RT_COMMENT:
          Error.report(tParseError.TYPE_CONVERSION);
          break;

        case tType.RT_BOOL:
          switch (NewType)
          {
            case tType.RT_BOOL:
              break;
            case tType.RT_INT64:
              m_Val.Int64 = m_Val.Bool ? 1 : 0;
              break;
            case tType.RT_UINT64:
            case tType.RT_HEXVAL:
              m_Val.Uint64 = (UInt64)(m_Val.Bool ? 1 : 0);
              break;
            case tType.RT_STR:
              if (m_Val.Bool)
                m_Val.String = "TRUE";
              else
                m_Val.String = "FALSE";
              break;
            case tType.RT_FLOAT:
              m_Val.Double = m_Val.Bool ? 1.0 : 0.0;
              break;
            case tType.RT_FORMULA:
            case tType.RT_COMPLEX:
            case tType.RT_COMMENT:
              Error.report(tParseError.TYPE_CONVERSION);
              break;
          }
          break;

        case tType.RT_INT64:
          switch (NewType)
          {
            case tType.RT_BOOL:
              m_Val.Bool = (m_Val.Int64 != 0);
              break;
            case tType.RT_INT64:
              break;
            case tType.RT_UINT64:
            case tType.RT_HEXVAL:
              m_Val.Uint64 = (UInt64)(m_Val.Int64);
              break;
            case tType.RT_STR:
              m_Val.String = m_Val.Int64.ToString();
              break;
            case tType.RT_FLOAT:
              m_Val.Double = (double)(m_Val.Int64);
              break;
            case tType.RT_COMPLEX:
              m_Val.Complex.re = (double)(m_Val.Int64);
              m_Val.Complex.im = 0.0;
              break;
            case tType.RT_FORMULA:
            case tType.RT_COMMENT:
              Error.report(tParseError.TYPE_CONVERSION);
              break;

          }
          break;

        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
          switch (NewType)
          {
            case tType.RT_BOOL:
              m_Val.Bool = (m_Val.Uint64 != 0);
              break;
            case tType.RT_INT64:
              m_Val.Int64 = (Int64)(m_Val.Uint64);
              break;
            case tType.RT_UINT64:
            case tType.RT_HEXVAL:
              break;
            case tType.RT_STR:
              if (m_Type == tType.RT_UINT64)
                m_Val.String = m_Val.Uint64.ToString();
              else
                m_Val.String = "0x" + m_Val.Uint64.ToString("x");
              break;
            case tType.RT_FLOAT:
              m_Val.Int64 = (Int64)(m_Val.Uint64);
              m_Val.Double = (double)(m_Val.Int64);
              break;

            case tType.RT_COMPLEX:
              m_Val.Int64 = (Int64)(m_Val.Uint64);
              m_Val.Complex.re = (double)(m_Val.Int64);
              m_Val.Complex.im = 0.0;
              break;
            case tType.RT_COMMENT:
            case tType.RT_FORMULA:
              Error.report(tParseError.TYPE_CONVERSION);
              break;

          }
          break;


        case tType.RT_STR:
          switch (NewType)
          {
            case tType.RT_BOOL:
              if (m_Val.String == "TRUE")
                m_Val.Bool = true;
              else
                m_Val.Bool = false;
              break;

            case tType.RT_INT64:
              Int64.TryParse(m_Val.String, out m_Val.Int64);
              break;
            case tType.RT_UINT64:
              UInt64.TryParse(m_Val.String, out m_Val.Uint64);
              break;
            case tType.RT_HEXVAL:
              UInt64.TryParse(m_Val.String, out m_Val.Uint64);
              break;
            case tType.RT_STR:
              break;
            case tType.RT_FLOAT:
              try
              {
                m_Val.Double = double.Parse(m_Val.String,CultureInfo.InvariantCulture);
              }
              catch
              {

              }
              break;
            case tType.RT_COMPLEX:
            case tType.RT_FORMULA:
            case tType.RT_COMMENT:
              Error.report(tParseError.TYPE_CONVERSION);
              break;

          }
          break;

        case tType.RT_FLOAT:
          switch (NewType)
          {
            case tType.RT_BOOL:
              m_Val.Bool = ((m_Val.Double > 0.1) || (m_Val.Double < -0.1));
              break;

            case tType.RT_INT64:
              m_Val.Int64 = (Int64)(m_Val.Double);
              break;

            case tType.RT_UINT64:
            case tType.RT_HEXVAL:
              m_Val.Uint64 = (UInt64)(m_Val.Double);
              break;

            case tType.RT_STR:
              m_Val.String = doubleToString(m_Val.Double, decimals);
              break;

            case tType.RT_FLOAT:
              break;

            case tType.RT_COMPLEX:
              m_Val.Complex.re = m_Val.Double;
              m_Val.Complex.im = 0.0;
              break;
            case tType.RT_COMMENT:
            case tType.RT_FORMULA:
              Error.report(tParseError.TYPE_CONVERSION);
              break;

          }
          break;

        case tType.RT_FORMULA:
          {
            AnyType resu = new AnyType(m_pVarList, m_pMethods);
            resu = getFormulaResult();
            resu.changeType(NewType);
            switch (NewType)
            {
              case tType.RT_BOOL:
                m_Val.Bool = resu.getBool();
                break;

              case tType.RT_INT64:
                m_Val.Int64 = resu.getInt64();
                break;

              case tType.RT_UINT64:
              case tType.RT_HEXVAL:
                m_Val.Uint64 = resu.getUint64();
                break;
              case tType.RT_FLOAT:
                m_Val.Double = resu.getFloat();
                break;
              case tType.RT_STR:
                m_Val.String = resu.getString();
                break;
              case tType.RT_COMPLEX:
                m_Val.Complex.re = resu.getComplexRe();
                m_Val.Complex.im = resu.getComplexIm();
                break;

              case tType.RT_COMMENT:
                Error.report(tParseError.TYPE_CONVERSION);
                break;
              case tType.RT_FORMULA:
                break;
            }
          }
          break;

        case tType.RT_COMPLEX:
          {
            string val = doubleToString(m_Val.Complex.re, decimals);
            m_Val.String = val;
            if (m_Val.Complex.im >= 0.0)
              m_Val.String += " + ";
            val = doubleToString(m_Val.Complex.im, decimals);
            m_Val.String += val;
            m_Val.String += "i";
          }
          break;
      }

      m_Type = NewType;
      switch (m_Type)
      {
        case tType.RT_STR:
        case tType.RT_COMMENT:
          m_Val.Double = 0;
          m_Val.Complex.im = 0;
          break;
        case tType.RT_BOOL:
        case tType.RT_INT64:
        case tType.RT_UINT64:
        case tType.RT_HEXVAL:
        case tType.RT_FLOAT:
        case tType.RT_COMPLEX:
        case tType.RT_FORMULA:
          m_Val.String = "";
          break;
      }
    }
    // String in Variable kopieren (Funktion wie operator=() mit dem Unterschied,
    // dass die Laenge explizit angegeben werden kann. Hilfreich bei Strings, die
    // nicht o-terminiert sind

    // typen adaptieren
    public void adaptType(ref AnyType s)
    {
      if (m_Type != s.getType())
      {
        if ((Int32)(m_Type) > (Int32)(s.getType()))
          s.changeType(m_Type);
        else
          changeType(s.getType());
      }
    }


    public void assignFormulaResult()
    {
      AnyType res = getFormulaResult();
      switch (res.getType())
      {
        case tType.RT_INT64:
          m_Val.Int64 = res.getInt64();
          break;
        case tType.RT_HEXVAL:
        case tType.RT_UINT64:
          m_Val.Uint64 = res.getUint64();
          break;
        case tType.RT_FLOAT:
          m_Val.Double = res.getFloat();
          break;
        case tType.RT_BOOL:
          m_Val.Bool = res.getBool();
          break;
        case tType.RT_COMPLEX:
          m_Val.Complex.re = res.getComplexRe();
          m_Val.Complex.im = res.getComplexIm();
          break;
        case tType.RT_STR:
        case tType.RT_COMMENT:
          m_Val.String = res.getString();
          break;
        case tType.RT_FORMULA:
          break;
      }
      m_Type = res.getType();
    }


    static string stripFollowingZeroes(string pBuf)
    {
      string retVal = pBuf;
      int pos = retVal.IndexOf('.');
      if (pos >= 0)
      {
        int pos0 = retVal.LastIndexOf('0');
        while (pos0 > pos)
        {
          retVal = retVal.Substring(0, pos0);
          pos0 = retVal.LastIndexOf('0');
        }
      }
      return retVal;
    }

    public static string doubleToString(double val, UInt32 decimals)
    {
      string retVal = "";
      if (((Math.Abs(val) < 1e-4) || (Math.Abs(val) > 1e7)) && (val != 0))
      {
        string formatStr = "E";
        retVal = val.ToString(formatStr, CultureInfo.InvariantCulture);
      }
      else
      {
        string formatStr = "N" + decimals.ToString();
        retVal = val.ToString(formatStr, CultureInfo.InvariantCulture);
        //retVal = stripFollowingZeroes(retVal);
      }
      retVal = retVal.Replace(",", "");
      return retVal;
    }

    public void assign(AnyType val)
    {
      m_Type = val.m_Type;
      m_pMethods = val.m_pMethods;
      m_pVarList = val.m_pVarList;
      m_Val = val.m_Val;
    }

    public void negate()
    {
      switch(m_Type)
      {
        case tType.RT_FLOAT:
          m_Val.Double *= -1;
          break;
        case tType.RT_INT64:
          m_Val.Int64 *= 1;
          break;
        case tType.RT_UINT64:
          m_Val.Int64 = -(Int64)m_Val.Uint64;
          m_Type = tType.RT_INT64;
          break;
        default:
          Error.report(tParseError.MINUS_NOT_SUPPORTED);
          break;
      }
    }
    public void assign(double val)
    {
      m_Type = tType.RT_FLOAT;
      m_Val.Double = val;
    }

    public void assignInt64(Int64 val)
    {
      m_Type = tType.RT_INT64;
      m_Val.Int64 = val;
    }

    public void assignBool(bool val)
    {
      m_Type = tType.RT_BOOL;
      m_Val.Bool = val;
    }

    public void assign(UInt64 val)
    {
      m_Type = tType.RT_UINT64;
      m_Val.Uint64 = val;
    }

    public void assign(string val)
    {
      m_Type = tType.RT_STR;
      m_Val.String = val;
    }

    public void setString(string pData, int Len = 0)
    {
      m_Type = tType.RT_STR;
      m_Val.String = "";
      if (pData != null)
      {
        if (Len != 0)
          m_Val.String = pData.Substring(0, Len);
        else
          m_Val.String = pData;
      }
    }


    public void setComplex(double re, double im)
    {
      m_Val.Complex.re = re;
      m_Val.Complex.im = im;
    }


    public void setComplexPolar(double abs, double arg)
    {
      m_Val.Complex.re = abs * Math.Cos(arg);
      m_Val.Complex.im = abs * Math.Sin(arg);
    }


    public double getFloat()
    {
      return m_Val.Double;
    }


    public double getComplexIm()
    {
      return m_Val.Complex.im;
    }


    public double getComplexRe()
    {
      return m_Val.Complex.re;
    }


    public double getComplexAbs()
    {
      double abs = Math.Sqrt(m_Val.Complex.re * m_Val.Complex.re + m_Val.Complex.im * m_Val.Complex.im);
      return abs;
    }


    public double getComplexArg()
    {
      double arg = 0.0;
      double abs = getComplexAbs();
      arg = Math.Acos(m_Val.Complex.re / abs);
      if (m_Val.Complex.im < 0)
        arg *= -1.0;
      if (arg < 0)
        arg += Math.PI * 2;
      return arg;
    }

    public Int64 getInt64()
    {
      return m_Val.Int64;
    }
    public UInt64 getUint64()
    {
      return m_Val.Uint64;
    }
    public tType getType()
    {
      return m_Type;
    }

     public override string ToString()
    {
      switch (m_Type)
      {
        case tType.RT_INT64:
          return m_Val.Int64.ToString();
        case tType.RT_UINT64:
          return m_Val.Uint64.ToString();
        case tType.RT_FLOAT:
        default:
          return m_Val.Double.ToString(CultureInfo.InvariantCulture);
        case tType.RT_FORMULA:
          return m_Val.String; ;
        case tType.RT_BOOL:
          return m_Val.Bool.ToString();
        case tType.RT_COMPLEX:
          return m_Val.Complex.ToString();
        case tType.RT_COMMENT:
          return "COMMENT";
        case tType.RT_STR:
          return m_Val.String;
        case tType.RT_HEXVAL:
          return m_Val.Uint64.ToString(); ;
      }
    }


    public string getTypeString()
    {
      switch (m_Type)
      {
        case tType.RT_INT64:
          return "INT64";
        case tType.RT_UINT64:
          return "UINT64";
        case tType.RT_FLOAT:
        default:
          return "FLOAT";
        case tType.RT_FORMULA:
          return "FORMULA";
        case tType.RT_BOOL:
          return "BOOL";
        case tType.RT_COMPLEX:
          return "COMPLEX";
        case tType.RT_COMMENT:
          return "COMMENT";
        case tType.RT_STR:
          return "STRING";
        case tType.RT_HEXVAL:
          return "HEX";
      }
    }

    public bool getBool()
    {
      return m_Val.Bool;
    }
    public string getString()
    {
      return m_Val.String;
    }


    public AnyType getFormulaResult()
    {
      AnyType retVal = new AnyType(m_pVarList, m_pMethods);
      CondParser p = new CondParser(m_pVarList, m_pMethods);
      if (m_Val.String.Length >= 1)
      {
        string str = m_Val.String.Substring(1);
        retVal = p.parse(str);
      }
      return retVal;
    }

    public int getStrSize()
    {
      return m_Val.String.Length;
    }

    public static tType stringToType(string typeString)
    {
      if ("INT64" == typeString)
        return tType.RT_INT64;
      else if ("UINT64" == typeString)
        return tType.RT_UINT64;
      else if ("FLOAT" == typeString)
        return tType.RT_FLOAT;
      else if ("FORMULA" == typeString)
        return tType.RT_FORMULA;
      else if ("BOOL" == typeString)
        return tType.RT_BOOL;
      else if ("COMPLEX" == typeString)
        return tType.RT_COMPLEX;
      else if ("COMMENT" == typeString)
        return tType.RT_COMMENT;
      else if ("STRING" == typeString)
        return tType.RT_STR;
      else if ("HEX" == typeString)
        return tType.RT_HEXVAL;

      return tType.RT_FLOAT;
    }

    // Datentyp
    tType m_Type;
    tValue m_Val;

    VarList m_pVarList;
    Methods m_pMethods;
  }
}
