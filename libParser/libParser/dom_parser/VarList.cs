/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
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
using System.Linq;
using System.IO;
using System.Globalization;

namespace libParser
{
  public struct VarListItem
  {
    public string name;
    public AnyType value;
    public string formulaString;
  };

  public class VarName
  {
    string m_varName;
    VarList m_pVarList;
    Methods m_pMethods;
    bool m_isConst;

    public VarName(VarList pVarList, Methods pMethods, bool isConst)
    {
      m_pVarList = pVarList;
      m_pMethods = pMethods;
      m_isConst = isConst;
      m_Value = new List<AnyType>();
      AnyType p = new AnyType(m_pVarList, m_pMethods);
      m_Value.Add(p);
    }

    public Int32 setValue(Int32 Index, AnyType Val)
    {
      Int32 RetVal = 0;
      AnyType p = new AnyType(m_pVarList, m_pMethods);
      while (Index >= m_Value.Count())
        m_Value.Add(p);
      m_Value[Index] = Val;
      return RetVal;
    }


    public Int32 getValue(Int32 Index, ref AnyType Val)
    {
      Int32 RetVal = (Int32)tParseError.ARRAY_INDEX;
      if (Index < m_Value.Count)
      {
        Val.assign(m_Value[Index]);
        RetVal = 0;
      }
      else
      {
        Val = new AnyType();
        Val.assign((double)RetVal);
      }
      return RetVal;
    }

    public string getName()
    {
      return m_varName;
    }

    public void setName(string p)
    {
      m_varName = p;
    }
    public bool isConst()
    {
      return m_isConst;
    }

    public VarName Next;

    //  clAnyType value;
    List<AnyType> m_Value;
  };


  public class VarList
  {
    // Hash-Tabelle der Variablen
    List<VarName> m_Table;

    Methods m_pMethods;


    public VarList()
    {
      m_Table = new List<VarName>();
    }

    public void init()
    {
      m_Table.Clear();
    }

    public void addConstant(string name, double value, AnyType.tType type = AnyType.tType.RT_FLOAT)
    {
      VarName var = insert(name, true, m_pMethods);
      AnyType val = new AnyType();
      val.assign(value);
      val.changeType(type);
      var.setValue(0,val);
    }

    public VarName set(string name, string value, Methods methods = null, int index = 0)
    {
      VarName n = insert(name, false, methods);
      if (n != null)
      {
        AnyType v = new AnyType();
        v.setType(AnyType.tType.RT_STR);
        v.assign(value);
        n.setValue(index, v);
      }
      return n;
    }

    public void set(string name, double value, Methods methods = null, int index = 0)
    {
      VarName n = insert(name, false, methods);
      if (n != null)
      {
        AnyType v = new AnyType();
        v.setType(AnyType.tType.RT_FLOAT);
        v.assign(value);
        n.setValue(index, v);
      }
    }


    public void set(string name, int value, Methods methods = null, int index = 0)
    {
      VarName n = insert(name, false, methods);
      if (n != null)
      {
        AnyType v = new AnyType();
        v.setType(AnyType.tType.RT_INT64);
        v.assignInt64(value);
        n.setValue(index, v);
      }
    }

    // erzeugt einen Neueintrag in der Liste der Variablen
    private VarName insert(string s, bool isConst = false, Methods pMethods = null)
    {
      return look(s, pMethods, isConst, 1);
    }

    public VarName get(string p, Methods pMethods = null)
    {
      return look(p, pMethods);
    }


    // Zugriff auf die Symboltabelle
    private VarName look(
      // Zeiger auf Varialennamen
      string p,
      // Zeiger auf die Methodenliste (fur RT_FORMULA)
      Methods pMethods = null,
      bool isConst = false,
      // 1: Variable neu in die Tabelle eintragen
      Int32 ins = 0)
    {
      // Variable in der Tabelle suchen
      foreach (VarName v in m_Table)
      {
        if (v.getName() == p)
          return v;
      }

      if (ins == 0)
        return null;

      VarName nn = new VarName(this, pMethods, isConst);
      nn.setName(p);
      m_Table.Add(nn);
      return nn;
    }

    /**
     * @brief dump list of variables
     * @param isConst true: dump constants, false: dump read/write vars
     */
    public string dumpVarList(bool isConst)
    {
      string retVal;

      if (isConst)
        retVal = "LIST OF CONSTANTS: \n";
      else
        retVal = "LIST OF VARIABLES: \n";
      foreach (VarName v in m_Table)
      {
        if (v.isConst() != isConst)
          continue;
        AnyType val = new AnyType(this, m_pMethods);
        v.getValue(0, ref val);
        retVal += v.getName();
        retVal += " = ";
        string formulaString = "";
        bool isFormula = false;
        string type = val.getTypeString();
        if (val.getType() == AnyType.tType.RT_FORMULA)
        {
          formulaString = val.getString();
          isFormula = true;
        }
        val.changeType(AnyType.tType.RT_STR);
        retVal += val.getString();
        retVal += "  ";
        retVal += type;
        if (isFormula)
        {
          retVal += "  { ";
          retVal += formulaString;
          retVal += " }";
        }
        retVal += "\n";
      }

      return retVal;

    }


    /**
     * @brief clVarList::getVarList fill a list with variables
     * @param isConst
     * @param list
     */
    public List<VarListItem> getVarList(bool isConst)
    {
      List<VarListItem> list = new List<VarListItem>();
      foreach (VarName v in m_Table)
      {
        VarListItem item = new VarListItem();
        if (v.isConst() != isConst)
          continue;
        AnyType val = new AnyType(this, m_pMethods);
        v.getValue(0, ref val);
        item.name = v.getName();
        item.value = val;
        if (val.getType() == AnyType.tType.RT_FORMULA)
          item.formulaString = val.getString();
        list.Add(item);
      }
      return list;
    }

    public Int32 save(string fileName)
    {
      Int32 retVal = 0;

      string outStr = "";
      foreach (VarName v in m_Table)
      {
          AnyType val = new AnyType();
          /*int32_t errCode = */
          v.getValue(0, ref val);
          outStr += v.getName() + ";";
          AnyType.tType type = val.getType();
          outStr += val.getTypeString() + ";";
          outStr += v.isConst().ToString() + ";";
          switch (type)
          {
            case AnyType.tType.RT_FORMULA:
              outStr += val.getString();
              break;
            case AnyType.tType.RT_COMPLEX:
              outStr += val.getComplexRe() + ";" + val.getComplexIm();
              break;
            default:
              val.changeType(AnyType.tType.RT_STR);
              outStr += val.getString();
              break;
          }
          outStr += "\n";
      }
      try
      {
        File.WriteAllText(fileName, outStr);
      }
      catch
      {
        retVal = 1;
      }
      return retVal;
    }

    /**
     * @brief load load list of variables from file
     * @param fileName
     * @param pMethods
     * @param varConst true: load only consts, false load only read/write vars
     * @return
     */
    public Int32 load(string fileName, Methods pMethods, bool varConst)
    {
      Int32 err = 0;
      try
      {
        string[] inStr = File.ReadAllLines(fileName);
        m_pMethods = pMethods;
        foreach (string line in inStr)
        {
          string[] tokens = line.Split(';');
          if (tokens.Length > 1)
          {
            bool isConst = tokens[2] != "0" ? true : false;
            if (isConst != varConst)
              continue;
            VarName varName = insert(tokens[0], isConst, pMethods);
            AnyType.tType type = AnyType.stringToType(tokens[1]);
            AnyType var = new AnyType(this, pMethods);
            double varDouble;
            double valRe;
            double valIm;
            Int64 varInt64;
            switch (type)
            {
              case AnyType.tType.RT_FLOAT:

                Double.TryParse(tokens[3], NumberStyles.Any, CultureInfo.InvariantCulture, out varDouble);
                var.assign(varDouble);
                break;
              case AnyType.tType.RT_INT64:
              case AnyType.tType.RT_UINT64:
              case AnyType.tType.RT_HEXVAL:
              case AnyType.tType.RT_BOOL:
                Int64.TryParse(tokens[3], out varInt64);
                var.assignInt64(varInt64);
                break;
              case AnyType.tType.RT_STR:
              case AnyType.tType.RT_FORMULA:
              case AnyType.tType.RT_COMMENT:
                var.setString(tokens[3]);
                break;
              case AnyType.tType.RT_COMPLEX:
                Double.TryParse(tokens[3], NumberStyles.Any, CultureInfo.InvariantCulture, out valRe);
                Double.TryParse(tokens[4], NumberStyles.Any, CultureInfo.InvariantCulture, out valIm);
                var.setComplex(valRe, valIm);
                break;
            }
            var.setType(type);
            varName.setValue(0, var);
          }
        }
      }
      catch
      {
        err = 1;
      }
      return err;
    }

    public void remove(string name)
    {
      VarName v = look(name);
      if (v != null)
        m_Table.Remove(v);
    }
  }
}
