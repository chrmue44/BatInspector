/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;

namespace libParser
{
  public class MthdResult
  {
    public MthdResult()
    {
      m_SaveResult = false;
    }

    // fuegt einen Parameter zum Ergebnis hinzu
    public void addValue(AnyType Val)
    {
      if (m_List.Count > 0)
        m_ResString += m_Delimiter;
      m_List.Add(Val);
      AnyType.tType Type = Val.getType();
      Val.changeType(AnyType.tType.RT_STR);
      if (Type == AnyType.tType.RT_STR)
      {
        m_ResString += "\"";
        m_ResString += Val.getString();
        m_ResString += "\"";
      }
      else
        m_ResString += Val.getString();
    }

    public void addValue(double Val)
    {
      AnyType AVal = new AnyType();

      if (m_List.Count > 0)
        m_ResString += m_Delimiter;
      m_ResString += Val.ToString();
      AVal.assign(Val);
      m_List.Add(AVal);
    }

    // void addValue(int32_t Val);
    // void addValue(uint32_t Val);
    public void addValue(UInt64 Val)
    {
      AnyType AVal = new AnyType();

      if (m_List.Count > 0)
        m_ResString += m_Delimiter;

      AVal.assign(Val);
      m_List.Add(AVal);
      AVal.changeType(AnyType.tType.RT_STR);
      m_ResString += AVal.getString();
    }


    public void addValue(string Val)
    {
      AnyType AVal = new AnyType();

      if (m_List.Count > 0)
        m_ResString += m_Delimiter;

      AVal.assign(Val);
      m_List.Add(AVal);
      m_ResString += "\"";
      m_ResString += AVal.getString();
      m_ResString += "\"";
    }

    public void addValue(string Val, int Len)
    {
      AnyType AVal = new AnyType();

      if (m_List.Count > 0)
        m_ResString += m_Delimiter;

      AVal.setString(Val, Len);
      m_List.Add(AVal);
      m_ResString += "\"";
      m_ResString += AVal.getString();
      m_ResString += "\"";
    }

  // liefert die Anzahl der Parameter der aktuellen Antwort (m_List)
    public int getParCount()
    {
      return m_List.Count;
    }

    // fuegt ein NewLine in die Antwort ein
    public void setDelimiter(string Delimiter)
    {
      m_Delimiter = Delimiter;
    }

    // liefert Zeiger auf Ergebnis-String
    public string getString()
    {
      return m_ResString;
    }

    // liefert den n-Ten Parameter der Antwort
    AnyType getPar(int Nr)
    {
      AnyType Val = new AnyType();
      if ((m_LastResult.Count >= Nr) && (Nr > 0))
        return m_LastResult[Nr - 1];
      else
      {
        Val.assign(0.0);
        return Val;
      }
    }


    // liefert den n-ten Parameter als string
    string getParString(int Nr)
    {
      if ((m_LastResult.Count >= Nr) && (Nr > 0))
        return m_LastResult[Nr - 1].getString();
      else
      {
        return null;
      }
    }

    // liefert die Größe des n-ten Parameters als string
    public int getParStringSize(int Nr)
    {
      if ((m_LastResult.Count >= Nr) && (Nr > 0))
        return m_LastResult[Nr - 1].getStrSize();
      else
      {
        return 0;
      }
    }

    // liefert die Anzahl der Parameter der letzten Antwort
    public int getLastResultParCount()
    {
      return m_LastResult.Count;
    }

    // Ergebnis loeschen
    public void flush()
    {
      m_ResString = "";
      m_Delimiter = ",";
      m_List.Clear();
    }

    // Ergebnis sichern
    public void storeResult()
    {
      if (!m_SaveResult)
      {
        m_LastResult.Clear();
        for (int i = 0; i < m_List.Count; i++)
        {
          m_LastResult.Add(m_List[i]);
        }
      }
      else
        m_SaveResult = false;
    }

    // 
    void signalizeSaveResult()
    {
      m_SaveResult = true;
    }

    #region Member
    // Ergebnis als String
    string m_ResString;

    // Trennzeichen
    string m_Delimiter;

    // Liste der Parameter
    List<AnyType> m_List = new List<AnyType>();

    // letztes Ergebnis
    List<AnyType> m_LastResult = new List<AnyType>();

    // wird gesetzt, wenn m_List nicht durch Funktionsaufruf ueberschrieben werden soll
    bool m_SaveResult;
    #endregion Member
  };
}
