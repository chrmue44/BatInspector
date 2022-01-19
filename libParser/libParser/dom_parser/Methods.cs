using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libParser;

namespace libParser
{
  public class Methods
  {

    const int PAR_HELP_CNT = 20;
    const int RET_HELP_CNT = 10;

    public Methods(MthdResult pResult)
    {
      m_Result = pResult;
    }

    // Funktion ausfuehren
    public tParseError executeFunction(
      // Name der Funktion
      string FuncName,
      // Anzahl der Parameter
      int ParCnt,
      // Liste der Parameter
      List<AnyType> argv,
      ref AnyType result)
    {
      tParseError RetVal = tParseError.METHOD;
      //bool Found = false;
      if (FuncName == "help")
      {
        help(ParCnt, argv);
        result.assign(m_Result.getString());
        return tParseError.SUCCESS;
      }
      else
      {
        foreach(MethodList m in m_List)
        {
          tParseError err = m.execMethod(FuncName, argv, out result);
          if (err != tParseError.METHOD)
          {
            RetVal = err;
            break;
          }
        }
      }
      return RetVal;
    }

    // fuegt eine Liste mit Methoden hinzu
    public void addMethodList(
      // Liste von Methoden
      MethodList pMthdList)
    {
      m_List.Add(pMthdList);
    }

  // Liste aus Zeigern auf Methodenlisten
  List<MethodList> m_List = new List<MethodList>();

    // Hilfe-Funktion
    void help(
      // Anzahl der Parameter
      int ParCnt,
      // Liste der Parameter
      List<AnyType> argv)
    {
      m_Result.flush();
      if (ParCnt == 0)
      {
        string Delimiter = "\r\n";
        m_Result.setDelimiter(Delimiter);
        m_Result.addValue("syntax: method(par1, par2, ...)");
        Delimiter = "\r\n  ";
        m_Result.setDelimiter(Delimiter);

        foreach (MethodList m in m_List)
        {
          List<FuncTabItem> pTab = m.getMthdList();
          m_Result.addValue(m.getMthdListHelp());
          foreach (FuncTabItem f in pTab)
            m_Result.addValue(f.pName);
        }
        m_Result.addValue("q");
      }
      else
      {
        bool Found = false;

        int i;
        for (i = 0; i < m_List.Count; i++)
        {
          string FuncName = argv[0].getString();
          bool RetVal = m_List[i].findMethod(FuncName);
          if (RetVal)
          {
            Found = true;
            break;
          }
        }
        if (Found)
        {
          foreach(HelpTabItem pItem in m_List[i].getHelpTab())
          {
            if (pItem.pMthdName == argv[0].getString())
            {
              string Delimiter = "\r\n";

              m_Result.setDelimiter(Delimiter);
              m_Result.addValue("");
              m_Result.addValue(pItem.pMthdName);
              m_Result.addValue(pItem.pMthdHelp);
              m_Result.addValue("\nParameters:");
              Delimiter = "\n  ";
              m_Result.setDelimiter(Delimiter);

              foreach(string s in pItem.pParHelp)
                 m_Result.addValue(s);

              Delimiter = "\r\n";
              m_Result.setDelimiter(Delimiter);
              m_Result.addValue("\nReturn values:");
              Delimiter = "\r\n  ";
              m_Result.setDelimiter(Delimiter);

              foreach (string s in pItem.pRetHelp)
                m_Result.addValue(s);

              break;
            }
          }
        }
      }
    }

    // Zeiger auf Antwort
    MthdResult m_Result;
  }
}
