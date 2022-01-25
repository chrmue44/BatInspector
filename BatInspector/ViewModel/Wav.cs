using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;

namespace BatInspector
{
  //https://www.codeguru.com/dotnet/making-sounds-with-waves-using-c/
  public class WaveHeader
  {
    private const string FILE_TYPE_ID = "RIFF";
    private const string MEDIA_TYPE_ID = "WAVE";

    public string FileTypeId { get; private set; }
    public UInt32 FileLength { get; set; }
    public string MediaTypeId { get; private set; }

    public WaveHeader()
    {
      FileTypeId = FILE_TYPE_ID;
      MediaTypeId = MEDIA_TYPE_ID;
      // Minimum size is always 4 bytes
      FileLength = 4;
    }

    public byte[] GetBytes()
    {
      List<Byte> chunkData = new List<byte>();
      chunkData.AddRange(Encoding.ASCII.GetBytes(FileTypeId));
      chunkData.AddRange(BitConverter.GetBytes(FileLength));
      chunkData.AddRange(Encoding.ASCII.GetBytes(MediaTypeId));
      return chunkData.ToArray();
    }
  }

  public class FormatChunk
  {
    private ushort _bitsPerSample;
    private ushort _channels;
    private uint _frequency;
    private const string CHUNK_ID = "fmt ";

    public string ChunkId { get; private set; }
    public UInt32 ChunkSize { get; private set; }
    public UInt16 FormatTag { get; private set; }

    public UInt16 Channels
    {
      get { return _channels; }
      set { _channels = value; RecalcBlockSizes(); }
    }

    public UInt32 Frequency
    {
      get { return _frequency; }
      set { _frequency = value; RecalcBlockSizes(); }
    }

    public UInt32 AverageBytesPerSec { get; private set; }
    public UInt16 BlockAlign { get; private set; }

    public UInt16 BitsPerSample
    {
      get { return _bitsPerSample; }
      set { _bitsPerSample = value; RecalcBlockSizes(); }
    }

    public FormatChunk(ushort chans, uint freq)
    {
      ChunkId = CHUNK_ID;
      ChunkSize = 16;
      FormatTag = 1;       // MS PCM (Uncompressed wave file)
      Channels = chans;        // Default to stereo
      Frequency = freq;   // Default to 44100hz
      BitsPerSample = 16;  // Default to 16bits
      RecalcBlockSizes();
    }

    private void RecalcBlockSizes()
    {
      BlockAlign = (UInt16)(_channels * (_bitsPerSample / 8));
      AverageBytesPerSec = _frequency * BlockAlign;
    }

    public byte[] GetBytes()
    {
      List<Byte> chunkBytes = new List<byte>();

      chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
      chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
      chunkBytes.AddRange(BitConverter.GetBytes(FormatTag));
      chunkBytes.AddRange(BitConverter.GetBytes(Channels));
      chunkBytes.AddRange(BitConverter.GetBytes(Frequency));
      chunkBytes.AddRange(BitConverter.GetBytes(AverageBytesPerSec));
      chunkBytes.AddRange(BitConverter.GetBytes(BlockAlign));
      chunkBytes.AddRange(BitConverter.GetBytes(BitsPerSample));

      return chunkBytes.ToArray();
    }

    public UInt32 Length()
    {
      return (UInt32)GetBytes().Length;
    }

  }


  public class DataChunk
  {
    private const string CHUNK_ID = "data";

    public string ChunkId { get; private set; }
    public UInt32 ChunkSize { get; set; }
    public short[] WaveData { get; private set; }

    public DataChunk()
    {
      ChunkId = CHUNK_ID;
      ChunkSize = 0;  // Until we add some data
    }

    public UInt32 Length()
    {
      return (UInt32)GetBytes().Length;
    }

    public byte[] GetBytes()
    {
      List<Byte> chunkBytes = new List<Byte>();

      chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
      chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
      byte[] bufferBytes = new byte[WaveData.Length * 2];
      Buffer.BlockCopy(WaveData, 0, bufferBytes, 0,
         bufferBytes.Length);
      chunkBytes.AddRange(bufferBytes.ToList());

      return chunkBytes.ToArray();
    }

    public void AddSampleData(double[] leftBuffer,
       double[] rightBuffer, int idxStart, int idxEnd)
    {
      WaveData = new short[leftBuffer.Length +
         rightBuffer.Length];
      int bufferOffset = 0;
      for (int index = 0; index < WaveData.Length; index += 2)
      {
        WaveData[index] = (short)(leftBuffer[bufferOffset] * 32767.0);
        WaveData[index + 1] = (short)(rightBuffer[bufferOffset] * 32767.0);
        bufferOffset++;
      }
      ChunkSize = (UInt32)WaveData.Length * 2;
    }

    public void AddSampleData(List<short> leftBuffer, int idxStart, int idxEnd)
    {
      WaveData = new short[leftBuffer.Count];

      int bufferOffset = 0;
      for (int index = idxStart; index < idxEnd; index++)
      {
        WaveData[index] = leftBuffer[bufferOffset];
        bufferOffset++;
      }
      ChunkSize = (UInt32)WaveData.Length * 2;
    }

    public void AddSampleData(double[] leftBuffer, int idxStart, int idxEnd)
    {
      WaveData = new short[idxEnd - idxStart + 2];
      if (idxEnd >= leftBuffer.Length)
        idxEnd = leftBuffer.Length - 1;

      int bufferOffset = 0;
      for (int index = idxStart; index < idxEnd; index++)
      {
        WaveData[bufferOffset] = (short)(leftBuffer[index] * 32767.0);
        bufferOffset++;
      }
      ChunkSize = (UInt32)WaveData.Length * 2;
    }

  }

  public class WavFile
  {
    private byte[] _waveData;

    Audio _audio;
    public WavFile()
    {
      _audio = new Audio();
    }

    public void play(ushort chanCount, int sampleRate, int idxStart, int idxEnd, double[] left, double[] right = null)
    {
      WaveHeader header = new WaveHeader();
      FormatChunk format = new FormatChunk(chanCount, (uint)sampleRate);
      DataChunk data = new DataChunk();
      List<Byte> tempBytes = new List<byte>();

      if (chanCount == 1)
        data.AddSampleData(left, idxStart, idxEnd);
      else
        data.AddSampleData(left, right, idxStart, idxEnd);

      tempBytes.AddRange(header.GetBytes());
      tempBytes.AddRange(format.GetBytes());
      tempBytes.AddRange(data.GetBytes());
      _waveData = tempBytes.ToArray();
      _audio.Play(_waveData, AudioPlayMode.WaitToComplete);
      _waveData = null;
      tempBytes = null;
    }
  }

/*
  class WavTest
  {
    static Audio myAudio = new Audio();
    private static byte[] myWaveData;

    // Sample rate (Or number of samples in one second)
    private const int SAMPLE_FREQUENCY = 44100;
    // 60 seconds or 1 minute of audio
    private const int AUDIO_LENGTH_IN_SECONDS = 1;

    static void Main()
    {
      List<Byte> tempBytes = new List<byte>();

      WaveHeader header = new WaveHeader();
      FormatChunk format = new FormatChunk();
      DataChunk data = new DataChunk();

      // Create 1 second of tone at 697Hz
      SineGenerator leftData = new SineGenerator(697.0f,
         SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS);
      // Create 1 second of tone at 1209Hz
      SineGenerator rightData = new SineGenerator(1209.0f,
         SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS);

      data.AddSampleData(leftData.Data, rightData.Data);

      header.FileLength += format.Length() + data.Length();

      tempBytes.AddRange(header.GetBytes());
      tempBytes.AddRange(format.GetBytes());
      tempBytes.AddRange(data.GetBytes());

      myWaveData = tempBytes.ToArray();

      myAudio.Play(myWaveData, AudioPlayMode.WaitToComplete);

    }

  }
*/
}
