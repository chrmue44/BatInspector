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
using System.Collections.Generic;

namespace libParser
{
  public delegate tParseError tpFunc(List<AnyType> argv, out AnyType result);

  public struct HelpTabItem
  {
    public HelpTabItem(string mthName, string help, List<string> parHelp, List<string> retHelp)
    {
      pMthdName = mthName;
      pMthdHelp = help;
      pParHelp = parHelp;
      pRetHelp = retHelp;
    }

    public string pMthdName;
    public string pMthdHelp;
    public List<string> pParHelp;
    public List<string> pRetHelp;
  };

  public struct FuncTabItem
  {
    public string pName;
    public tpFunc pFunc;
    public FuncTabItem(string n, tpFunc f)
    {
      pName = n;
      pFunc = f;
    }
  };

  public abstract class MethodList
  {
    public MethodList()
    {
      m_pVarList = null;
    }

    // sucht einen Methodennamen in der Liste und gibt einen Iterator
    // auf die Methode zurueck. Wird die Methode nicht gefunden, wird false
    // zurueckgegeben
    public bool findMethod(
      // Name der Methode
      string MthdName)
    {
      bool Found = false;
      foreach(FuncTabItem f in m_Mthds)
      {
        if (MthdName == f.pName)
        {
          Found = true;
          break;
        }
      }
      return Found;
    }

    public tParseError execMethod(string name, List<AnyType> pars, out AnyType result)
    {
      tParseError retVal = tParseError.SUCCESS;
      result = new AnyType();
      result.assign((double)1.0);
      bool found = false;
      foreach (FuncTabItem f in m_Mthds)
      {
        if (name == f.pName)
        {
          f.pFunc(pars, out result);
          found = true;
          break;
        }
      }
      if (!found)
        retVal = tParseError.METHOD;

      return retVal;
    }

    // Initialisiert die Funktionstabelle
    public abstract void initMthdTab();

    // Ueberschrift fuer Hilfe zur Methodenliste
    public abstract string getMthdListHelp();

    // liefert die Tabelle mit detaillierter Hilfe zu den Methoden
    public abstract List<HelpTabItem> getHelpTab();
    // Zeiger auf Result-Objekt setzen
    public void setpResult(MthdResult pResult)
    {
      m_pResult = pResult;
    }

    // Zeiger auf VariablenListe setzen
    public void setVarList(VarList varList)
    {
      m_pVarList = varList;
    }

    public List<FuncTabItem> getMthdList()
    {
      return m_Mthds;
    }

    protected void addMethod(FuncTabItem Item)
    {
      m_Mthds.Add(Item);
    }

    // Liste der Methoden
    protected List<FuncTabItem> m_Mthds= new List<FuncTabItem>();
    // Zeiger auf Ergbenis
    protected MthdResult m_pResult = new MthdResult();

    protected VarList m_pVarList = new VarList();
  };
}

