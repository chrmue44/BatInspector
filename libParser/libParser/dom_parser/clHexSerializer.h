#ifndef _CLHEXSERIALIZER_H_
#define _CLHEXSERIALIZER_H_
/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/
#include "clAnyType.h"

typedef union
{
    uint64_t ValUint64;
    int64_t ValInt64;
    int16_t ValInt16;
	REAL ValReal;
//	bool ValBool;
    uint8_t Byte[8];
} tComb;

// class to serialize/deserialize HexASCII-Data
class clHexSerializer
{
public:
  clHexSerializer();
  virtual ~clHexSerializer();

	// start serializing of data
    int32_t StartSerializing(
		// pointer to Buffer with HexASCII-Data
		char* pBuf, 
		// Size of the Buffer including trailing zero (strlen() + 1)!!
        uint32_t BufSize);

	// finalize serialization
    int32_t FinalizeSerializing();
	// Add value
//	int32_t Add(bool Val);
	// Add value
    int32_t Add(int16_t Val);
	// Add value
    int32_t Add(uint64_t Val);
	// Add value
    int32_t Add(int64_t Val);
	// Add value
    int32_t Add(REAL Val);
	// Add string
    int32_t Add(const tPM_Char* Val, uint32_t Len);

	// start serializing of data
    int32_t StartDeserializing(
		// pointer to Buffer with HexASCII-Data
		const char* pBuf,
		// Size of the Buffer
        uint32_t BufSize);

	// get value of specified type
    int32_t Get(
		// pointer to Buffer for value
		void* pVal, 
		// type of variable to read
		tType Type);

	// get a string value 
    int32_t GetStr(
		// pointer to string buffer
		tPM_Char* Str, 
		// length of string buffer
        uint32_t Len);

private:
    int32_t HexToAsc(uint32_t Size);
    int32_t AscToHex(uint32_t Size, tPM_Char* pDest = NULL);

	tPM_Char* m_pBuffer;
	tPM_Char* m_pBufStart;
    uint32_t m_Remaining;
    uint32_t m_Count;
    uint32_t m_BufSize;
  tComb m_Value;
};


#endif   //#ifndef _CLHEXSERIALIZER_H_
