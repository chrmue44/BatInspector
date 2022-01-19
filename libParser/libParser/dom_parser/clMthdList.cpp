/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#include "clMthdList.h"
#include "clMthdResult.h"
#include "clVarList.h"
#include "clAnyType.h"
//#include "iFrame.h"
#include "util/clstreamutils.h"
#include "clError.h"


//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

clMthdList::clMthdList() :
 m_pVarList(NULL)
{

}

clMthdList::~clMthdList()
{

}


bool clMthdList::findMethod( tFuncTabIter& RetIter,  tPM_string& MthdName)
{
  tFuncTabIter i;
  bool Found = false;
  for(i = m_Mthds.begin(); i != m_Mthds.end(); i++)
  {
    if(MthdName == i->pName)
    {
      Found = true;
      RetIter = i;
      break;
    }
  }

  return Found;
}


