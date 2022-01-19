/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#if !defined(AFX_CLVARLIST_H__95F7BE80_4DAF_4EFB_A48F_BAA089240CCA__INCLUDED_)
#define AFX_CLVARLIST_H__95F7BE80_4DAF_4EFB_A48F_BAA089240CCA__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "clAnyType.h"
#include "os/objhndlglib.h"

// Groesse der Hash-Table fuer Variablen
#define TBLSZ     2500

typedef tPM_vector<clAnyType> tArray;
typedef tArray::iterator tArrayIter;

struct stVarListItem {
  std::string name;
  clAnyType value;
  std::string formulaString;
};

class clName
{

public:
  clName(clVarList* pVarList, clMethods* pMethods, bool isConst);
  int32_t setValue(uint32_t Index, clAnyType Val);
  int32_t getValue(uint32_t Index, clAnyType& Val);
  char* getName() {return Str; }
  void setName(char* p) {Str = p; }
  bool isConst() { return m_isConst; }
  virtual ~clName();
  clName* Next;
//  clAnyType value;
  tArray m_Value;

protected:
  clName();

private:
  char* Str;
  clVarList* m_pVarList;
  clMethods* m_pMethods;
  bool m_isConst;
};

class clVarList  
{
public:
  clVarList();
  virtual ~clVarList();
  // erzeugt einen Neueintrag in der Liste der Variablen
  clName* insert(const char* s, bool isConst = false, clMethods* pMethods = NULL);
  // Zugriff auf die Symboltabelle
  clName* look(
    // Zeiger auf Varialennamen
    const char* p,
    // Zeiger auf die Methodenliste (fur RT_FORMULA)
    clMethods* pMethods = NULL,
    bool isConst = false,
    // 1: Variable neu in die Tabelle eintragen
    int32_t ins = 0);

  /**
   * @brief dump list of variables
   * @param isConst true: dump constants, false: dump read/write vars
   */
  std::string& dumpVarList(bool isConst);
  /**
   * @brief clVarList::getVarList fill a list with variables
   * @param isConst
   * @param list
   */
  void getVarList(bool isConst, std::vector<stVarListItem>& list);

  int32_t save(const char* fileName);
  /**
   * @brief load load list of variables from file
   * @param fileName
   * @param pMethods
   * @param varConst true: load only consts, false load only read/write vars
   * @return
   */
  int32_t load(const char* fileName, clMethods* pMethods, bool varConst);
private:
  // Hash-Tabelle der Variablen
  clName* m_Table[TBLSZ];

  std::string m_listStr;
  clMethods* m_pMethods;
};

#endif // !defined(AFX_CLVARLIST_H__95F7BE80_4DAF_4EFB_A48F_BAA089240CCA__INCLUDED_)
