#ifndef _CLRESULTPARSER_H_
#define _CLRESULTPARSER_H_

#include "dom_parser/clCondParser.h"
#include "clMthdResult.h"

class clResultParser
{
public:
	clResultParser();
    int32_t Parse(char* pBuf, uint32_t BufLen);
    uint32_t GetParCount();
    int64_t GetInt64Par(uint32_t Nr);
    uint64_t GetUint64Par(uint32_t Nr);
    REAL GetRealPar(uint32_t Nr);
  const tPM_Char* GetStringPar(uint32_t Nr);
  uint32_t GetStringParSize(uint32_t Nr);

private:
	clCondParser m_Parser;
	clMthdResult m_Result;
};


#endif //#ifndef _CLRESULTPARSER_H_
