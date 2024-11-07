#pragma once
#include <cstdint>
#include <string>
#include <vector>

union uData
{
  char i8[8];
  uint16_t i16[4];
  int32_t i32[2];
  int64_t i64;
  float f32[2];
  double f63;
} ;

class DataArray
{
public:
  static void copy(const char* pSrc, char* pDst, int nr);
  static int32_t getInt32(const char* pSrc);
  static uint16_t getUint16(const char* pSrc);
};

#pragma pack(4)
struct stHeader
{
  char FileTypeId[4];
  uint32_t FileLength;
  char MediaTypeId[4];
};

struct stFormatChunk
{
  char                fmt[4];         // FMT header       
  unsigned long       Subchunk1Size;  // Size of the fmt chunk                                
  unsigned short      AudioFormat;    // Audio format 1=PCM,6=mulaw,7=alaw, 257=IBM Mu-Law, 258=IBM A-Law, 259=ADPCM 
  unsigned short      Channels;       // Number of channels 1=Mono 2=Sterio                   
  unsigned long       Frequency;      // Sampling Frequency in Hz                             
  unsigned long       AverageBytesPerSec;    // bytes per second 
  unsigned short      BlockAlign;     // 2=16-bit mono, 4=16-bit stereo 
  unsigned short      BitsPerSample;  // Number of bits per sample      
};


class WaveHeader
{
  const char FILE_TYPE_ID[4] = { 'R','I','F','F' };
  const char MEDIA_TYPE_ID[4] = { 'W','A','V','E' };

public:
  WaveHeader();
  void set(const char* pData);
  const char* getBytes();
  stHeader* getData() { return &_data; }
  int getLength() { return sizeof(stHeader); }

private:
  stHeader _data;
};


class FormatChunk
{
public:
  void set(const char* data);
  const char* getBytes();
  int getLength() { return sizeof(stFormatChunk); }
  stFormatChunk* getData() { return &_data; }

private:
  void RecalcBlockSizes();
  stFormatChunk _data;
};


struct stDataChunk
{
  char ChunkId[4];
  uint32_t ChunkSize;
};

class DataChunk
{
  const char CHUNK_ID[4] = { 'd','a','t','a' };

public:
  DataChunk();
  virtual ~DataChunk();
    void set(const char* data);
  int getLength() { return sizeof(stDataChunk); }
  stDataChunk* getData() { return &_data; }
  bool isData();
  void addSampleData(const char* data, int offs, int len, int bitsPerSample, int chanCount);
  int getSampleCnt() { return _samples.size(); }
  int16_t* getSamples() { return &_samples[0]; }

private:
  stDataChunk _data;
  std::vector<int16_t> _samples;
};





class WavFile
{
public:
  int readFile(const char* name);
  bool isOpen() { return _isOpen; }
  int getSampleRate() { return _formatChunk.getData()->Frequency; }
  int getSampleCnt() { return _dataChunk.getSampleCnt(); }
  int16_t* getSamples() { return _dataChunk.getSamples(); }
private:
  WaveHeader _header;
  FormatChunk _formatChunk;
  DataChunk _dataChunk;
  const char* _fName = nullptr;
  bool _isOpen = false;

};

