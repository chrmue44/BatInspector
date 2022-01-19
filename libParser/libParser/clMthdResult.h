// ****************************************************************************
// *                                                                          *
// *   (C)  Hottinger Baldwin Messtechnik GmbH                                *
// *        Im Tiefen See 45,                                                 *
// *        D-64293 Darmstadt                                                 *
// *        www.PM.com                                                       *
// *                                                                          *
// *                                                                          *
// ******* MODULE HISTORY *****************************************************
// *                                                                          *
// *   Ver     Date     Author      Remarks                                   *
// *   ---------------------------------------------------------------------- *
// *   000     18.10.05 Schreyer    implemented                               *
// ****************************************************************************
#ifndef CLMTHDRESULT_H
#define CLMTHDRESULT_H

#include "dom_parser/clAnyType.h"
#include "os/objhndlglib.h"

typedef tPM_vector<clAnyType> tParList;

class clMthdResult
{
  
public:
	clMthdResult();
  virtual ~clMthdResult();

  // fuegt einen Parameter zum Ergebnis hinzu
	void addValue(clAnyType Val);
  void addValue(double Val);
 // void addValue(int32_t Val);
 // void addValue(uint32_t Val);
  void addValue(uint64_t Val);
  void addValue(const char* Val);
  void addValue(const char* Val,uint32_t Len);

  // liefert die Anzahl der Parameter der aktuellen Antwort (m_List)
  uint32_t getParCount(void);

  // fuegt ein NewLine in die Antwort ein
  void setDelimiter(tPM_string& Delimiter);

  // liefert Zeiger auf Ergebnis-String
  const char* getString();

  // liefert den n-Ten Parameter der Antwort
  clAnyType getPar(uint32_t Nr);
	// liefert den n-ten Parameter als string
    const char* getParString(uint32_t Nr);
	// liefert die Größe des n-ten Parameters als string
    uint32_t getParStringSize(uint32_t Nr);
  // liefert die Anzahl der Parameter der letzten Antwort
  uint32_t getLastResultParCount();

  // Ergebnis loeschen
  void flush();

  // Ergebnis sichern
  void storeResult();

  // 
  void signalizeSaveResult();
  
private:
  // Ergebnis als String
  tPM_string m_ResString;

  // Trennzeichen
  tPM_string m_Delimiter;
  
  // Liste der Parameter
  tParList m_List;
  
  // letztes Ergebnis
  tParList m_LastResult;

  // wird gesetzt, wenn m_List nicht durch Funktionsaufruf ueberschrieben werden soll
  bool m_SaveResult;
};



#endif  //#ifndef CLMTHDRESULT_H
