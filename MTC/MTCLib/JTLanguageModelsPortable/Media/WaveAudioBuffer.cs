using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Media
{
    public class WaveAudioBuffer
    {
        protected IDataBuffer _Storage;
        protected bool _Modified;
        protected int _NumberOfChannels;
        protected int _BytesPerSample;
        protected int _SampleSize;
        protected int _SampleCount;
        protected int _SampleRate;
        protected const int ChunkIDOffset = 0;
        protected const int ChunkIDSize = 4;
        public const string ChunkIDValue = "RIFF";
        protected const int ChunkSizeOffset = 4;
        protected const int ChunkSizeSize = 4;
        protected const int ChunkFormatOffset = 8;
        protected const int ChunkFormatSize = 4;
        public const string ChunkFormatValue = "WAVE";
        protected int SubChunk1IDOffset = 12;
        protected const int SubChunk1IDSize = 4;
        public const string SubChunk1IDValue = "fmt ";
        protected int SubChunk1SizeOffset = 16;
        protected const int SubChunk1SizeSize = 4;
        protected int AudioFormatOffset = 20;
        protected const int AudioFormatSize = 2;
        public const int AudioFormatPCM = 1;
        protected int NumberOfChannelsOffset = 22;
        protected const int NumberOfChannelsSize = 2;
        protected int SampleRateOffset = 24;
        protected const int SampleRateSize = 4;
        protected int ByteRateOffset = 28;
        protected const int ByteRateSize = 4;
        protected int BlockAlignOffset = 32;
        protected const int BlockAlignSize = 2;
        protected int BitsPerSampleOffset = 34;
        protected const int BitsPerSampleSize = 2;
        protected int SubChunk2IDOffset = 36;
        protected const int SubChunk2IDSize = 4;
        public const string SubChunk2IDValue = "data";
        protected int SubChunk2SizeOffset = 40;
        protected const int SubChunk2SizeSize = 4;
        public int DataOffset = 44;

        public WaveAudioBuffer(IDataBuffer storage)
        {
            _Storage = storage;
            _Modified = false;

            if ((storage != null) && storage.Exists())
            {
                bool opened = false;

                if (!_Storage.IsOpen())
                    opened = _Storage.Open(PortableFileMode.Open);
                while (GetStringField(SubChunk1IDOffset, SubChunk1IDSize) != SubChunk1IDValue)
                {
                    int size = GetInteger(SubChunk1SizeOffset, SubChunk1SizeSize, true, false);
                    if (size == -1)
                        throw new Exception("Wave format error.");
                    size += 8;
                    SubChunk1IDOffset += size;
                    SubChunk1SizeOffset += size;
                    AudioFormatOffset += size;
                    NumberOfChannelsOffset += size;
                    SampleRateOffset += size;
                    ByteRateOffset += size;
                    BlockAlignOffset += size;
                    BitsPerSampleOffset += size;
                    SubChunk2IDOffset += size;
                    SubChunk2SizeOffset += size;
                    DataOffset += size;
                }
                if (storage.Length != 0)
                {
                    _NumberOfChannels = NumberOfChannels;
                    _BytesPerSample = BytesPerSample;
                    _SampleSize = SampleSize;
                    _SampleCount = -1;
                    _SampleRate = SampleRate;
                }
                else
                {
                    _NumberOfChannels = 0;
                    _BytesPerSample = 0;
                    _SampleSize = 0;
                    _SampleCount = -1;
                    _SampleRate = 0;
                }
                if (SubChunk1Size != 16)
                {
                    int offset = SubChunk1Size - 16;
                    SubChunk2IDOffset += offset;
                    SubChunk2SizeOffset += offset;
                    DataOffset += offset;
                }
                while (GetStringField(SubChunk2IDOffset, SubChunk2IDSize) != SubChunk2IDValue)
                {
                    int size = GetInteger(SubChunk2SizeOffset, SubChunk2SizeSize, true, false);
                    if (size == -1)
                        throw new Exception("Wave format error.");
                    size += 8;
                    SubChunk2IDOffset += size;
                    SubChunk2SizeOffset += size;
                    DataOffset += size;
                }
                if (opened)
                    _Storage.Close();
            }
            else
            {
                _NumberOfChannels = 0;
                _BytesPerSample = 0;
                _SampleSize = 0;
                _SampleCount = -1;
                _SampleRate = 0;
            }
        }

        public WaveAudioBuffer(int numberOfChannels, int sampleRate, int bitsPerSample, IDataBuffer data,
            bool dataIsWavformat)
        {
            _Storage = null;
            _Modified = false;
            Initialize(numberOfChannels, sampleRate, bitsPerSample, data, dataIsWavformat);
        }

        public WaveAudioBuffer(IDataBuffer storage, int numberOfChannels, int sampleRate, int bitsPerSample,
            IDataBuffer data, bool dataIsWavformat)
        {
            _Storage = storage;
            _Modified = false;
            Initialize(numberOfChannels, sampleRate, bitsPerSample, data, dataIsWavformat);
            _Storage.Modified = false;
            _Modified = false;
        }

        public WaveAudioBuffer(IDataBuffer storage, WaveAudioBuffer other)
        {
            _Storage = storage;
            _Modified = false;
            Initialize(other, 0, other.SampleCount);
            _Storage.Modified = false;
            _Modified = false;
        }

        public WaveAudioBuffer(IDataBuffer storage, WaveAudioBuffer other, int sampleIndex, int sampleCount)
        {
            _Storage = storage;
            _Modified = false;
            Initialize(other, sampleIndex, sampleCount);
            _Storage.Modified = false;
            _Modified = false;
        }

        public IDataBuffer Storage
        {
            get
            {
                return _Storage;
            }
            set
            {
                if (_Storage != value)
                {
                    _Modified = true;
                    _Storage = value;
                }
            }
        }

        public virtual bool Modified
        {
            get
            {
                if (_Storage != null)
                {
                    if (_Storage.Modified)
                        return true;
                }
                return _Modified;
            }
            set
            {
                if (_Storage != null)
                    _Storage.Modified = false;
               _Modified = value;
            }
        }

        // Open/close functions.

        public virtual bool Open(PortableFileMode mode)
        {
            if (_Storage != null)
                return _Storage.Open(mode);

            return false;
        }

        public virtual bool Close()
        {
            if (_Storage != null)
                return _Storage.Close();

            return false;
        }

        public virtual bool IsOpen()
        {
            if (_Storage != null)
                return _Storage.IsOpen();

            return false;
        }

        public virtual bool Exists()
        {
            if (_Storage != null)
                return _Storage.Exists();

            return false;
        }

        public int Length
        {
            get
            {
                if (_Storage != null)
                    return _Storage.Length;
                return 0;
            }
        }

        public int GetByte(int offset)
        {
            if (_Storage.IsOpen())
                return _Storage.GetByte(offset);
            else if (_Storage.Open(PortableFileMode.Open))
            {
                int data = _Storage.GetByte(offset);
                _Storage.Close();
                return data;
            }
            return -1;
        }

        public byte[] GetBytes(int offset, int length)
        {
            if (_Storage.IsOpen())
                return _Storage.GetBytes(offset, length);
            else if (_Storage.Open(PortableFileMode.Open))
            {
                byte[] data = _Storage.GetBytes(offset, length);
                _Storage.Close();
                return data;
            }
            return null;
        }

        public virtual bool GetBytes(byte[] buffer, int destinationOffset, int sourceOffset, int length)
        {
            if (_Storage.IsOpen())
                return _Storage.GetBytes(buffer, destinationOffset, sourceOffset, length);
            else if (_Storage.Open(PortableFileMode.Open))
            {
                bool returnValue = _Storage.GetBytes(buffer, destinationOffset, sourceOffset, length);
                _Storage.Close();
                return returnValue;
            }
            return false;
        }

        public virtual int GetInteger(int offset, int size, bool littleEndian, bool signExtend)
        {
            if (_Storage.IsOpen())
                return _Storage.GetInteger(offset, size, littleEndian, signExtend);
            else if (_Storage.Open(PortableFileMode.Open))
            {
                int data = _Storage.GetInteger(offset, size, littleEndian, signExtend);
                _Storage.Close();
                return data;
            }
            return -1;
        }

        public bool SetByte(int data, int offset)
        {
            if (_Storage.IsOpen())
                return _Storage.SetByte(data, offset);
            else if (_Storage.Open(PortableFileMode.OpenOrCreate))
            {
                bool returnValue = _Storage.SetByte(data, offset);
                _Storage.Close();
                return returnValue;
            }
            return false;
        }

        public virtual bool SetBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length)
        {
            if (_Storage.IsOpen())
                return _Storage.SetBytes(buffer, sourceOffset, destinationOffset, length);
            else if (_Storage.Open(PortableFileMode.OpenOrCreate))
            {
                bool returnValue = _Storage.SetBytes(buffer, sourceOffset, destinationOffset, length);
                _Storage.Close();
                return returnValue;
            }
            return false;
        }

        public bool FillByte(int byteValue, int offset, int count)
        {
            if (_Storage.IsOpen())
            {
                _Storage.FillByte(byteValue, offset, count);
                return true;
            }
            else if (_Storage.Open(PortableFileMode.OpenOrCreate))
            {
                _Storage.FillByte(byteValue, offset, count);
                _Storage.Close();
                return true;
            }
            return false;
        }

        public bool SetInteger(int data, int offset, int size, bool littleEndian)
        {
            if (_Storage.IsOpen())
                return _Storage.SetInteger(data, offset, size, littleEndian);
            else if (_Storage.Open(PortableFileMode.OpenOrCreate))
            {
                bool returnValue = _Storage.SetInteger(data, offset, size, littleEndian);
                _Storage.Close();
                return returnValue;
            }
            return false;
        }

        protected string GetStringField(int offset, int length)
        {
            byte[] fieldData = GetBytes(offset, length);
            string str = TextUtilities.GetStringFromBytes(fieldData, 0, length);
            return str;
        }

        protected void SetStringField(string value, int offset, int length)
        {
            byte[] fieldData = TextUtilities.GetBytesFromString(value);
            SetBytes(fieldData, 0, offset, length);
        }

        public string ChunkID
        {
            get
            {
                return GetStringField(ChunkIDOffset, ChunkIDSize);
            }
            set
            {
                SetStringField(value, ChunkIDOffset, ChunkIDSize);
            }
        }

        public int ChunkSize
        {
            get
            {
                return GetInteger(ChunkSizeOffset, ChunkSizeSize, true, false);
            }
            set
            {
                SetInteger(value, ChunkSizeOffset, ChunkSizeSize, true);
            }
        }

        public string ChunkFormat
        {
            get
            {
                return GetStringField(ChunkFormatOffset, ChunkFormatSize);
            }
            set
            {
                SetStringField(value, ChunkFormatOffset, ChunkFormatSize);
            }
        }

        public string SubChunk1ID
        {
            get
            {
                return GetStringField(SubChunk1IDOffset, SubChunk1IDSize);
            }
            set
            {
                SetStringField(value, SubChunk1IDOffset, SubChunk1IDSize);
            }
        }

        public int SubChunk1Size
        {
            get
            {
                return GetInteger(SubChunk1SizeOffset, SubChunk1SizeSize, true, false);
            }
            set
            {
                SetInteger(value, SubChunk1SizeOffset, SubChunk1SizeSize, true);
            }
        }

        public int AudioFormat
        {
            get
            {
                return GetInteger(AudioFormatOffset, AudioFormatSize, true, false);
            }
            set
            {
                SetInteger(value, AudioFormatOffset, AudioFormatSize, true);
            }
        }

        public bool IsPCM
        {
            get
            {
                return AudioFormat == AudioFormatPCM;
            }
            set
            {
                AudioFormat = (value ? AudioFormatPCM : 0);
            }
        }

        public int NumberOfChannels
        {
            get
            {
                return GetInteger(NumberOfChannelsOffset, NumberOfChannelsSize, true, false);
            }
            set
            {
                _NumberOfChannels = value;
                SetInteger(value, NumberOfChannelsOffset, NumberOfChannelsSize, true);
            }
        }

        public int SampleRate
        {
            get
            {
                if (_SampleRate != 0)
                    return _SampleRate;
                return _SampleRate = GetInteger(SampleRateOffset, SampleRateSize, true, false);
            }
            set
            {
                _SampleRate = value;
                SetInteger(value, SampleRateOffset, SampleRateSize, true);
            }
        }

        public int ByteRate
        {
            get
            {
                return GetInteger(ByteRateOffset, ByteRateSize, true, false);
            }
            set
            {
                SetInteger(value, ByteRateOffset, ByteRateSize, true);
            }
        }

        public int BlockAlign
        {
            get
            {
                return GetInteger(BlockAlignOffset, BlockAlignSize, true, false);
            }
            set
            {
                _SampleSize = value;
                SetInteger(value, BlockAlignOffset, BlockAlignSize, true);
            }
        }

        public int BitsPerSample
        {
            get
            {
                if (_BytesPerSample != 0)
                    return _BytesPerSample * 8;
                return GetInteger(BitsPerSampleOffset, BitsPerSampleSize, true, false);
            }
            set
            {
                _BytesPerSample = value / 8;
                SetInteger(value, BitsPerSampleOffset, BitsPerSampleSize, true);
            }
        }

        public int BytesPerSample
        {
            get
            {
                if (_BytesPerSample != 0)
                    return _BytesPerSample;
                return BitsPerSample / 8;
            }
            set
            {
                _BytesPerSample = value;
                SetInteger(value * 8, BitsPerSampleOffset, BitsPerSampleSize, true);
            }
        }

        public string SubChunk2ID
        {
            get
            {
                return GetStringField(SubChunk2IDOffset, SubChunk2IDSize);
            }
            set
            {
                SetStringField(value, SubChunk2IDOffset, SubChunk2IDSize);
            }
        }

        public int SubChunk2Size
        {
            get
            {
                int size = GetInteger(SubChunk2SizeOffset, SubChunk2SizeSize, true, false);
                if (size == -1)
                    return 0;
                int sampleSize = 0;
                if ((BytesPerSample > 0) && (NumberOfChannels > 0))
                    sampleSize = BytesPerSample* NumberOfChannels;
                if (sampleSize == 0)
                    return 0;
                _SampleCount = size / sampleSize;
                return size;
            }
            set
            {
                SetInteger(value, SubChunk2SizeOffset, SubChunk2SizeSize, true);
                if ((BytesPerSample > 0) && (NumberOfChannels > 0))
                    _SampleCount = value / (BytesPerSample * NumberOfChannels);
                else
                    _SampleCount = 0;
            }
        }

        public int SampleCount
        {
            get
            {
                if (_SampleCount < 0)
                {
                    int sampleSize = BytesPerSample * NumberOfChannels;
                    if (sampleSize == 0)
                        return 0;
                    _SampleCount = SubChunk2Size / sampleSize;
                }
                return _SampleCount;
            }
        }

        public TimeSpan TimeLength
        {
            get
            {
                int sampleCount = SampleCount;
                int sampleRate = SampleRate;
                if (sampleRate == 0)
                    return TimeSpan.Zero;
                long ticks = (sampleCount * 10000000L /* ticks per second */) / sampleRate;
                TimeSpan timeLength = new TimeSpan(ticks);
                return timeLength;
            }
        }

        public int SampleSize
        {
            get
            {
                return BlockAlign;
            }
        }

        public bool GetSample(int sampleIndex, out int leftSample, out int rightSample)
        {
            int offset = DataOffset + (sampleIndex * _SampleSize);
            if (_NumberOfChannels == 2)
            {
                leftSample = GetInteger(offset, _BytesPerSample, true, true);
                rightSample = GetInteger(offset + _BytesPerSample, _BytesPerSample, true, true);
                if (offset + (2 * _SampleSize) > _Storage.Length)
                    return false;
            }
            else
            {
                leftSample = GetInteger(offset, _BytesPerSample, true, true);
                rightSample = leftSample;
                if (offset + _SampleSize > _Storage.Length)
                    return false;
            }
            return true;
        }

        public int GetMonoSample(int sampleIndex)
        {
            int offset = DataOffset + (sampleIndex * _SampleSize);
            if (_NumberOfChannels == 2)
            {
                int leftSample = GetInteger(offset, _BytesPerSample, true, true)/2;
                int rightSample = GetInteger(offset + _BytesPerSample, _BytesPerSample, true, true)/2;
                return leftSample + rightSample;
            }
            else
                return _Storage.GetInteger(offset, _BytesPerSample, true, true);
        }

        public int GetMonoSampleAbsoluteValue(int sampleIndex)
        {
            int value = GetMonoSample(sampleIndex);

            if (value < 0)
                value = -value;

            return value;
        }

        public void MultiplySample(int sampleIndex, double factor)
        {
            int offset = DataOffset + (sampleIndex * _SampleSize);
            if (_NumberOfChannels == 2)
            {
                int leftSample = GetInteger(offset, _BytesPerSample, true, true);
                int rightSample = GetInteger(offset + _BytesPerSample, _BytesPerSample, true, true);
                leftSample = (int)(leftSample * factor);
                rightSample = (int)(rightSample * factor);
                SetInteger(leftSample, offset, _BytesPerSample, true);
                SetInteger(rightSample, offset + _BytesPerSample, _BytesPerSample, true);
            }
            else
            {
                int sample = GetInteger(offset, _BytesPerSample, true, true);
                sample = (int)(sample * factor);
                SetInteger(sample, offset, _BytesPerSample, true);
            }
        }

        public bool GetSamples(IDataBuffer sampleBuffer, int sampleIndex, int sampleCount)
        {
            int offset = DataOffset + (sampleIndex * _SampleSize);
            int length = sampleCount * _SampleSize;
            return sampleBuffer.SetFrom(_Storage, offset, 0, length);
        }

        public bool SetSamples(IDataBuffer sampleBuffer, int sourceSampleIndex, int destinationSampleIndex, int sampleCount)
        {
            int sourceOffset = sourceSampleIndex * _SampleSize;
            int destinationOffset = DataOffset + (destinationSampleIndex * _SampleSize);
            int length = sampleCount * _SampleSize;
            bool returnValue = _Storage.SetFrom(sampleBuffer, sourceOffset, destinationOffset, length);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool InsertSamples(IDataBuffer sampleBuffer, int sourceSampleIndex, int destinationSampleIndex, int sampleCount)
        {
            int sourceOffset = sourceSampleIndex * _SampleSize;
            int destinationOffset = DataOffset + (destinationSampleIndex * _SampleSize);
            int length = sampleCount * _SampleSize;
            bool returnValue = _Storage.InsertFrom(sampleBuffer, sourceOffset, destinationOffset, length);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool ReplaceSamples(IDataBuffer sampleBuffer, int sourceSampleIndex, int sourceSampleCount,
            int destinationSampleIndex, int destinationSampleCount)
        {
            int sourceOffset = sourceSampleIndex * _SampleSize;
            int sourceLength = sourceSampleCount * _SampleSize;
            int destinationOffset = DataOffset + (destinationSampleIndex * _SampleSize);
            int destinationLength = destinationSampleCount * _SampleSize;
            bool returnValue = _Storage.ReplaceFrom(sampleBuffer, sourceOffset, sourceLength, destinationOffset, destinationLength);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool DeleteSamples(int sampleDestinationIndex, int sampleDestinationCount)
        {
            int destinationOffset = DataOffset + (sampleDestinationIndex * _SampleSize);
            int destinationLength = sampleDestinationCount * _SampleSize;
            bool returnValue = _Storage.DeleteBytes(destinationOffset, destinationLength);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool ClearSamples(int sampleDestinationIndex, int sampleDestinationCount)
        {
            int destinationOffset = DataOffset + (sampleDestinationIndex * _SampleSize);
            int destinationLength = sampleDestinationCount * _SampleSize;
            bool returnValue = _Storage.ClearBytes(destinationOffset, destinationLength);
            return returnValue;
        }

        public bool CropSamples(int startSampleIndex, int endSampleIndex)
        {
            int startOffset = DataOffset + (startSampleIndex * _SampleSize);
            int endOffset = DataOffset + (endSampleIndex * _SampleSize);
            int length = _Storage.Length;
            bool returnValue = _Storage.DeleteBytes(endOffset, length - endOffset);
            returnValue = returnValue && _Storage.DeleteBytes(DataOffset, startOffset - DataOffset);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool SquelchSamples(
            int startSampleIndex,
            int stopSampleIndex,
            int threshold,
            int minimumSampleWidth)
        {
            int startIndex = 0;
            int endIndex = 0;
            int subSampleCount = 0;
            int sampleCount = 0;
            int sampleIndex;
            bool inBelow = false;
            bool returnValue = true;
            if (_Storage.Open(PortableFileMode.Open))
            {
                if (startSampleIndex == -1)
                    startSampleIndex = 0;
                if (stopSampleIndex == -1)
                    stopSampleIndex = SampleCount;
                sampleCount = SampleCount;
                for (sampleIndex = startSampleIndex; sampleIndex < stopSampleIndex; sampleIndex++)
                {
                    int sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    if (sample <= threshold)
                    {
                        if (!inBelow)
                        {
                            inBelow = true;
                            startIndex = sampleIndex;
                        }
                    }
                    else
                    {
                        if (inBelow)
                        {
                            inBelow = false;
                            endIndex = sampleIndex;
                            subSampleCount = endIndex - startIndex;
                            if ((minimumSampleWidth == 0) || (subSampleCount >= minimumSampleWidth))
                                ClearSamples(startIndex, subSampleCount);
                        }
                    }
                }
                if (inBelow)
                {
                    subSampleCount = endIndex - startIndex;
                    if ((minimumSampleWidth == 0) || (subSampleCount >= minimumSampleWidth))
                        ClearSamples(startIndex, subSampleCount);
                }
                _Storage.Close();
            }
            return returnValue;
        }

        public bool TrimSamples(int threshold, int leadingSampleCount, int trailingSampleCount)
        {
            int startSampleIndex = 0;
            int endSampleIndex = 0;
            int sampleCount = 0;
            int sampleIndex;
            bool returnValue = false;
            if (_Storage.Open(PortableFileMode.Open))
            {
                sampleCount = SampleCount;
                for (sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    int sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    if (sample > threshold)
                        break;
                }
                startSampleIndex = sampleIndex - leadingSampleCount;
                if (startSampleIndex < 0)
                    startSampleIndex = 0;
                for (sampleIndex = sampleCount - 1; sampleIndex >= 0; sampleIndex--)
                {
                    int sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    if (sample > threshold)
                        break;
                }
                endSampleIndex = sampleIndex + trailingSampleCount;
                if (endSampleIndex > sampleCount)
                    endSampleIndex = sampleCount;
                _Storage.Close();
            }
            if (endSampleIndex < startSampleIndex)
                returnValue = DeleteSamples(0, sampleCount);
            else if ((startSampleIndex != 0) || (endSampleIndex != sampleCount))
                returnValue = CropSamples(startSampleIndex, endSampleIndex);
            else
                returnValue = true;
            return returnValue;
        }

        public bool GetTrimIndexes(
            int threshold,
            int startSampleIndex,
            int endSampleIndex,
            int leadingSampleCount,
            int trailingSampleCount,
            out int newStartIndex,
            out int newEndIndex)
        {
            int sampleIndex;
            bool returnValue = false;
            newStartIndex = startSampleIndex;
            newEndIndex = endSampleIndex;
            if (_Storage.Open(PortableFileMode.Open))
            {
                for (sampleIndex = newStartIndex; sampleIndex < newEndIndex; sampleIndex++)
                {
                    int sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    if (sample > threshold)
                        break;
                }
                newStartIndex = sampleIndex - trailingSampleCount;
                if (newStartIndex < startSampleIndex)
                    newStartIndex = startSampleIndex;
                for (sampleIndex = newEndIndex - 1; sampleIndex >= newStartIndex; sampleIndex--)
                {
                    int sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    if (sample > threshold)
                        break;
                }
                newEndIndex = sampleIndex + trailingSampleCount;
                if (newEndIndex > endSampleIndex)
                    newEndIndex = endSampleIndex;
                if (newStartIndex >= newEndIndex)
                {
                    newStartIndex = startSampleIndex;
                    newEndIndex = endSampleIndex;
                }
                _Storage.Close();
                returnValue = true;
            }
            return returnValue;
        }

        public bool GetLeadInSampleCount(
            int threshold,
            int startSampleIndex,
            int endSampleIndex,
            out int leadInEndIndex)
        {
            bool returnValue = false;
            leadInEndIndex = startSampleIndex;
            if (endSampleIndex < startSampleIndex)
                endSampleIndex = SampleCount;
            if (_Storage.Open(PortableFileMode.Open))
            {
                for (leadInEndIndex = startSampleIndex; leadInEndIndex < endSampleIndex; leadInEndIndex++)
                {
                    int sample = GetMonoSampleAbsoluteValue(leadInEndIndex);
                    if (sample > threshold)
                        break;
                }
                _Storage.Close();
                returnValue = true;
            }
            return returnValue;
        }

        public bool GetLeadInDuration(
            int threshold,
            int startSampleIndex,
            int endSampleIndex,
            out TimeSpan leadInDuration)
        {
            int leadInEndIndex;
            bool returnValue = GetLeadInSampleCount(threshold, startSampleIndex, endSampleIndex, out leadInEndIndex);
            leadInDuration = GetSampleTime(leadInEndIndex - startSampleIndex);
            return returnValue;
        }

        // Finds the first silence;
        public bool FindFirstSilence(
            int startSampleIndex,
            int stopSampleIndex,
            int amplitudeThreshold,
            int widthThreshold,
            out int silenceStartIndex,
            out int silenceStopIndex)
        {
            int sampleIndex = startSampleIndex;
            int sample;
            int nextSample;
            bool inSilence;
            bool newInSilence;
            int silenceStart;
            int silenceLength;
            bool returnValue = false;

            if (stopSampleIndex < startSampleIndex)
            {
                int tmp = startSampleIndex;
                startSampleIndex = stopSampleIndex;
                stopSampleIndex = tmp;
            }

            silenceStartIndex = startSampleIndex;
            silenceStopIndex = startSampleIndex;

            if (_Storage.Open(PortableFileMode.Open))
            {
                sample = GetMonoSample(sampleIndex);
                nextSample = GetMonoSample(sampleIndex + 1);

                if ((sample <= amplitudeThreshold) && (nextSample <= amplitudeThreshold))
                    inSilence = true;
                else
                    inSilence = false;

                silenceStart = startSampleIndex;

                for (sampleIndex = startSampleIndex + 2; sampleIndex < stopSampleIndex - 1; sampleIndex += 2)
                {
                    sample = GetMonoSample(sampleIndex);
                    nextSample = GetMonoSample(sampleIndex + 1);
                    newInSilence = ((sample <= amplitudeThreshold) && (nextSample <= amplitudeThreshold));

                    if (newInSilence != inSilence)
                    {
                        if (!newInSilence)
                        {
                            silenceLength = (sampleIndex - silenceStart) + 1;

                            if (silenceLength > widthThreshold)
                            {
                                silenceStartIndex = silenceStart;
                                silenceStopIndex = sampleIndex + 1;
                                returnValue = true;
                                break;
                            }
                        }
                        else
                            silenceStart = sampleIndex;

                        inSilence = newInSilence;
                    }
                }

                if (inSilence)
                {
                    silenceLength = (sampleIndex - silenceStart) + 1;

                    if (silenceLength > widthThreshold)
                    {
                        silenceStartIndex = silenceStart;
                        silenceStopIndex = sampleIndex + 1;
                        returnValue = true;
                    }
                }

                _Storage.Close();
            }

            return returnValue;
        }

        // Finds the longest silence;
        public bool FindLongestSilence(
            int startSampleIndex,
            int stopSampleIndex,
            int amplitudeThreshold,
            int widthThreshold,
            out int silenceStartIndex,
            out int silenceStopIndex)
        {
            int sampleIndex = startSampleIndex;
            int sample;
            int nextSample;
            bool inSilence;
            bool newInSilence;
            int silenceStart;
            int silenceLength;
            int bestLongWidth = 0;
            bool returnValue = false;

            if (stopSampleIndex < startSampleIndex)
            {
                int tmp = startSampleIndex;
                startSampleIndex = stopSampleIndex;
                stopSampleIndex = tmp;
            }

            silenceStartIndex = startSampleIndex;
            silenceStopIndex = startSampleIndex;

            if (_Storage.Open(PortableFileMode.Open))
            {
                sample = GetMonoSampleAbsoluteValue(sampleIndex);
                nextSample = GetMonoSampleAbsoluteValue(sampleIndex + 1);

                if ((sample <= amplitudeThreshold) && (nextSample <= amplitudeThreshold))
                    inSilence = true;
                else
                    inSilence = false;

                silenceStart = startSampleIndex;

                for (sampleIndex = startSampleIndex + 2; sampleIndex < stopSampleIndex - 1; sampleIndex += 2)
                {
                    sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    nextSample = GetMonoSampleAbsoluteValue(sampleIndex + 1);
                    newInSilence = ((sample <= amplitudeThreshold) && (nextSample <= amplitudeThreshold));

                    if (newInSilence != inSilence)
                    {
                        if (!newInSilence)
                        {
                            silenceLength = (sampleIndex - silenceStart) + 1;

                            if (silenceLength > widthThreshold)
                            {
                                if (silenceLength > bestLongWidth)
                                {
                                    bestLongWidth = silenceLength;
                                    silenceStartIndex = silenceStart;
                                    silenceStopIndex = sampleIndex + 1;
                                }
                            }
                        }
                        else
                            silenceStart = sampleIndex;

                        inSilence = newInSilence;
                    }
                }

                if (inSilence)
                {
                    silenceLength = (sampleIndex - silenceStart) + 1;

                    if (silenceLength > widthThreshold)
                    {
                        if (silenceLength > bestLongWidth)
                        {
                            bestLongWidth = silenceLength;
                            silenceStartIndex = silenceStart;
                            silenceStopIndex = sampleIndex + 1;
                        }
                    }
                }

                _Storage.Close();

                if (bestLongWidth != 0)
                    returnValue = true;
            }

            return returnValue;
        }

        // Finds the longest silence;
        public bool FindSilenceRange(
            int sampleMidIndex,
            int amplitudeThreshold,
            out int silenceStartIndex,
            out int silenceStopIndex)
        {
            int sampleIndex;
            int sampleCount = SampleCount;
            int sample;
            int nextSample;
            bool returnValue = false;

            silenceStartIndex = sampleMidIndex;
            silenceStopIndex = sampleMidIndex;

            if (_Storage.Open(PortableFileMode.Open))
            {
                sample = GetMonoSampleAbsoluteValue(sampleMidIndex);
                nextSample = GetMonoSampleAbsoluteValue(sampleMidIndex + 1);

                if ((sample > amplitudeThreshold) || (nextSample > amplitudeThreshold))
                    return false;

                for (sampleIndex = sampleMidIndex + 2; sampleIndex < sampleCount - 1; sampleIndex += 2)
                {
                    sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    nextSample = GetMonoSampleAbsoluteValue(sampleIndex + 1);

                    if (((sample > amplitudeThreshold) || (nextSample > amplitudeThreshold)))
                    {
                        silenceStopIndex = sampleIndex + 1;
                        break;
                    }
                }

                for (sampleIndex = sampleMidIndex - 2; sampleIndex >= 0; sampleIndex -= 2)
                {
                    sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    nextSample = GetMonoSampleAbsoluteValue(sampleIndex + 1);

                    if ((sample > amplitudeThreshold) || (nextSample > amplitudeThreshold))
                    {
                        silenceStartIndex = sampleIndex;
                        break;
                    }
                }

                _Storage.Close();
                returnValue = true;
            }

            return returnValue;
        }

        public bool FindSilenceRuns(
            int startSampleIndex,
            int stopSampleIndex,
            int amplitudeThreshold,
            int widthThreshold,
            List<int> silenceRuns,  // array of start/stop pairs.
            out int longestIndex,
            out int shortestIndex)
        {
            int sampleIndex = startSampleIndex;
            int sample;
            int nextSample;
            bool inSilence;
            bool newInSilence;
            int silenceStart;
            int silenceLength;
            int bestLongWidth = 0;
            int bestShortWidth = stopSampleIndex - startSampleIndex;
            bool returnValue = false;

            longestIndex = -1;
            shortestIndex = -1;

            if (_Storage.Open(PortableFileMode.Open))
            {
                sample = GetMonoSampleAbsoluteValue(sampleIndex);
                nextSample = GetMonoSampleAbsoluteValue(sampleIndex + 1);

                if ((sample <= amplitudeThreshold) && (nextSample <= amplitudeThreshold))
                    inSilence = true;
                else
                    inSilence = false;

                silenceStart = startSampleIndex;

                for (sampleIndex = startSampleIndex + 2; sampleIndex < stopSampleIndex - 1; sampleIndex += 2)
                {
                    sample = GetMonoSampleAbsoluteValue(sampleIndex);
                    nextSample = GetMonoSampleAbsoluteValue(sampleIndex + 1);
                    newInSilence = ((sample <= amplitudeThreshold) && (nextSample <= amplitudeThreshold));

                    if (newInSilence != inSilence)
                    {
                        if (!newInSilence)
                        {
                            silenceLength = (sampleIndex - silenceStart) + 1;

                            if (silenceLength > widthThreshold)
                            {
                                if (silenceLength > bestLongWidth)
                                {
                                    bestLongWidth = silenceLength;
                                    longestIndex = silenceRuns.Count();
                                }

                                if (silenceLength < bestShortWidth)
                                {
                                    bestShortWidth = silenceLength;
                                    shortestIndex = silenceRuns.Count();
                                }

                                silenceRuns.Add(silenceStart);
                                silenceRuns.Add(sampleIndex + 1);
                            }
                        }
                        else
                            silenceStart = sampleIndex;

                        inSilence = newInSilence;
                    }
                }

                if (inSilence)
                {
                    silenceLength = (sampleIndex - silenceStart) + 1;

                    if (silenceLength > widthThreshold)
                    {
                        if (silenceLength > bestLongWidth)
                        {
                            bestLongWidth = silenceLength;
                            longestIndex = silenceRuns.Count();
                        }

                        if (silenceLength < bestShortWidth)
                        {
                            bestShortWidth = silenceLength;
                            shortestIndex = silenceRuns.Count();
                        }

                        silenceRuns.Add(silenceStart);
                        silenceRuns.Add(sampleIndex + 1);
                    }
                }

                _Storage.Close();

                if (longestIndex != -1)
                    returnValue = true;
            }

            return returnValue;
        }

        public bool FadeInOutSamples(int startSampleIndex, int endSampleIndex, int fadeInCount, int fadeOutCount)
        {
            int startOffset = DataOffset + (startSampleIndex * _SampleSize);
            int endOffset = DataOffset + (endSampleIndex * _SampleSize);
            int fadeInLength = fadeInCount * _SampleSize;
            int fadeOutLength = fadeOutCount * _SampleSize;
            bool returnValue = _Storage.FadeBytes(startOffset, endOffset, fadeInLength, fadeOutLength);
            return returnValue;
        }

        // Find peak amplitude;
        public bool FindPeakAmplitude(
            int startSampleIndex,
            int stopSampleIndex,
            int averageCount,
            out int peakAmplitude)
        {
            int blockIndex;
            int sampleIndex = startSampleIndex;
            int sample;
            int bestAmplitude = 0;
            bool returnValue = false;

            peakAmplitude = 0;

            if (startSampleIndex == -1)
                startSampleIndex = 0;

            if (stopSampleIndex == -1)
                stopSampleIndex = SampleCount;

            if (stopSampleIndex < startSampleIndex)
            {
                int tmp = startSampleIndex;
                startSampleIndex = stopSampleIndex;
                stopSampleIndex = tmp;
            }

            if (averageCount < 1)
                averageCount = 1;

            if (_Storage.Open(PortableFileMode.Open))
            {
                for (blockIndex = startSampleIndex; blockIndex < stopSampleIndex; blockIndex += averageCount)
                {
                    int blockEnd = blockIndex + averageCount;
                    int blockSum = 0;
                    int blockCount = 0;

                    if (blockEnd > stopSampleIndex)
                        blockEnd = stopSampleIndex;

                    for (sampleIndex = blockIndex; sampleIndex < blockEnd; sampleIndex++, blockCount++)
                    {
                        sample = GetMonoSampleAbsoluteValue(sampleIndex);
                        blockSum += sample;
                    }

                    if (blockCount != 0)
                    {
                        int blockAverage = blockSum / blockCount;

                        if (blockAverage > bestAmplitude)
                            bestAmplitude = blockAverage;
                    }
                }

                peakAmplitude = bestAmplitude;

                returnValue = true;

                _Storage.Close();
            }

            return returnValue;
        }

        // Find peak amplitude;
        public bool AdjustAmplitude(
            int startSampleIndex,
            int stopSampleIndex,
            int averageCount,
            int desiredPeakAmplitude,
            int noGoAmplitudeThreshold)
        {
            int blockIndex;
            int sampleIndex = startSampleIndex;
            int sample;
            int bestAmplitude = 0;
            bool returnValue = false;

            if (startSampleIndex == -1)
                startSampleIndex = 0;

            if (stopSampleIndex == -1)
                stopSampleIndex = SampleCount;

            if (stopSampleIndex < startSampleIndex)
            {
                int tmp = startSampleIndex;
                startSampleIndex = stopSampleIndex;
                stopSampleIndex = tmp;
            }

            if (averageCount < 1)
                averageCount = 1;

            if ((desiredPeakAmplitude < 0) || (desiredPeakAmplitude < noGoAmplitudeThreshold))
                return false;

            if (_Storage.Open(PortableFileMode.Open))
            {
                for (blockIndex = startSampleIndex; blockIndex < stopSampleIndex; blockIndex += averageCount)
                {
                    int blockEnd = blockIndex + averageCount;
                    int blockSum = 0;
                    int blockCount = 0;

                    if (blockEnd > stopSampleIndex)
                        blockEnd = stopSampleIndex;

                    for (sampleIndex = blockIndex; sampleIndex < blockEnd; sampleIndex++, blockCount++)
                    {
                        sample = GetMonoSampleAbsoluteValue(sampleIndex);
                        blockSum += sample;
                    }

                    if (blockCount != 0)
                    {
                        int blockAverage = blockSum / blockCount;

                        if (blockAverage > bestAmplitude)
                            bestAmplitude = blockAverage;
                    }
                }

                if (bestAmplitude > noGoAmplitudeThreshold)
                {
                    int amplitudeDifference = desiredPeakAmplitude - bestAmplitude;

                    double factor = (double)desiredPeakAmplitude / bestAmplitude;

                    for (sampleIndex = startSampleIndex; sampleIndex < stopSampleIndex; sampleIndex++)
                        MultiplySample(sampleIndex, factor);

                    returnValue = true;
                }

                _Storage.Close();
            }

            return returnValue;
        }

        public bool SetFrom(WaveAudioBuffer other, int sourceSampleIndex, int destinationSampleIndex, int sampleCount)
        {
            IDataBuffer sampleBuffer = other.Storage;
            int sourceOffset = DataOffset + (sourceSampleIndex * _SampleSize);
            int destinationOffset = DataOffset + (destinationSampleIndex * _SampleSize);
            int length = sampleCount * _SampleSize;
            bool returnValue = _Storage.SetFrom(sampleBuffer, sourceOffset, destinationOffset, length);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool SetFromSpeedChange(WaveAudioBuffer other, int sourceSampleIndex, int destinationSampleIndex, int sampleCount, float speedMultiplier)
        {
            IDataBuffer storage = new MemoryBuffer();
            WaveAudioBuffer waveData = new WaveAudioBuffer(storage);
            waveData.Initialize(other.NumberOfChannels, other.SampleRate, other.BitsPerSample, null, false);
            if (!waveData.SetFrom(other, sourceSampleIndex, 0, sampleCount))
                return false;
            if (!MediaConvertSingleton.SpeedChange(waveData, waveData, speedMultiplier, false))
                return false;
            return SetFrom(waveData, 0, destinationSampleIndex, waveData.SampleCount);
        }

        public bool InsertFrom(WaveAudioBuffer other, int sourceSampleIndex, int destinationSampleIndex, int sampleCount)
        {
            IDataBuffer sampleBuffer = other.Storage;
            int sourceOffset = DataOffset + (sourceSampleIndex * _SampleSize);
            int destinationOffset = DataOffset + (destinationSampleIndex * _SampleSize);
            int length = sampleCount * _SampleSize;
            bool returnValue = _Storage.InsertFrom(sampleBuffer, sourceOffset, destinationOffset, length);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool ReplaceFrom(WaveAudioBuffer other, int sourceSampleIndex, int sourceSampleCount,
            int destinationSampleIndex, int destinationSampleCount)
        {
            IDataBuffer sampleBuffer = other.Storage;
            int sourceOffset = DataOffset + (sourceSampleIndex * _SampleSize);
            int sourceLength = sourceSampleCount * _SampleSize;
            int destinationOffset = DataOffset + (destinationSampleIndex * _SampleSize);
            int destinationLength = destinationSampleCount * _SampleSize;
            bool returnValue = _Storage.ReplaceFrom(sampleBuffer, sourceOffset, sourceLength, destinationOffset, destinationLength);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool Insert(WaveAudioBuffer other, int destinationSampleIndex)
        {
            int sourceSampleIndex = 0;
            int sampleCount = other.SampleCount;
            return InsertFrom(other, sourceSampleIndex, destinationSampleIndex, sampleCount);
        }

        public bool Append(WaveAudioBuffer other)
        {
            int sourceSampleIndex = 0;
            int destinationSampleIndex = SampleCount;
            int sampleCount = other.SampleCount;
            return InsertFrom(other, sourceSampleIndex, destinationSampleIndex, sampleCount);
        }

        public bool AppendSilence(double seconds)
        {
            int count = (int)(seconds * _SampleRate) * _SampleSize;
            int destinationOffset = _Storage.Length;
            bool returnValue = FillByte(0, destinationOffset, count);
            returnValue = returnValue && UpdateSizes();
            return returnValue;
        }

        public bool IsCompatible(WaveAudioBuffer other)
        {
            if (other.SampleRate != SampleRate)
                return false;

            if (other.NumberOfChannels != NumberOfChannels)
                return false;

            if (other.BitsPerSample != BitsPerSample)
                return false;

            return true;
        }

        public bool MakeCompatible(WaveAudioBuffer other)
        {
            if (other.SampleRate != SampleRate)
            {
                if (!MediaConvertSingleton.RateChange(this, this, other.SampleRate, false))
                    return false;
            }

            if (other.NumberOfChannels != NumberOfChannels)
            {
                if (!MediaConvertSingleton.NumberOfChannelsChange(this, this, other.NumberOfChannels, false))
                    return false;
            }

            if (other.BitsPerSample != BitsPerSample)
                return false;

            return true;
        }

        public bool SpeedChange(float speedMultiplier)
        {
            return MediaConvertSingleton.SpeedChange(this, this, speedMultiplier, false);
        }

        protected bool UpdateSizes()
        {
            if (_Storage != null)
            {
                int length = _Storage.Length;
                int newChunkSize = length - 8;
                int newSubChunk2Size = length - DataOffset;
                if (_Storage.IsOpen())
                {
                    ChunkSize = newChunkSize;
                    SubChunk2Size = newSubChunk2Size;
                }
                else if (_Storage.Open(PortableFileMode.OpenOrCreate))
                {
                    ChunkSize = newChunkSize;
                    SubChunk2Size = newSubChunk2Size;
                    _Storage.Close();
                }
                return true;
            }
            return false;
        }

        protected void SetDefaultStorage(int capacity)
        {
            if (_Storage == null)
                _Storage = new MemoryBuffer(capacity);
        }

        public TimeSpan Duration
        {
            get
            {
                int sampleRate = SampleRate;
                if (sampleRate == 0)
                    return TimeSpan.Zero;
                long sampleCount = SampleCount;
                long ticks = (sampleCount * 10000000L) / SampleRate;
                TimeSpan duration = TimeSpan.FromTicks(ticks);
                return duration;
            }
        }

        public TimeSpan GetSampleTime(int sampleCount)
        {
            int sampleRate = SampleRate;
            if (sampleRate == 0)
                return TimeSpan.Zero;
            long ticks = (sampleCount * 10000000L) / SampleRate;
            TimeSpan duration = TimeSpan.FromTicks(ticks);
            return duration;
        }

        public int GetSampleIndexFromTime(TimeSpan timeOffset)
        {
            int sampleCount = SampleCount;
            TimeSpan duration = Duration;

            if (duration.Ticks == 0)
                return 0;

            int index = (int)((timeOffset.Ticks * sampleCount) / duration.Ticks);

            if (index < 0)
                index = 0;
            else if (index > sampleCount)
                index = sampleCount;

            return index;
        }

        public bool Initialize(int numberOfChannels, int sampleRate, int bitsPerSample, IDataBuffer data,
            bool dataIsWavformat)
        {
            int blockAlign = numberOfChannels * (bitsPerSample/8);
            int dataLength = (((data != null) && data.Exists()) ? data.Length : 0);
            int sourceOffset = 0;
            int subChunk1Size = 16;
            int subChunk2Size = dataLength;
            int sampleCount = dataLength / blockAlign;
            int capacity = DataOffset + dataLength;

            if (dataIsWavformat)
            {
                dataLength -= DataOffset;
                sourceOffset += DataOffset;
            }

            SetDefaultStorage(capacity);

            if (_Storage.Open(PortableFileMode.OpenOrCreate))
            {
                ChunkID = ChunkIDValue;
                ChunkSize = 4 + (8 + subChunk1Size) + (8 + subChunk2Size);
                ChunkFormat = ChunkFormatValue;
                SubChunk1ID = SubChunk1IDValue;
                SubChunk1Size = subChunk1Size;
                AudioFormat = AudioFormatPCM;
                NumberOfChannels = numberOfChannels;
                SampleRate = sampleRate;
                ByteRate = sampleRate * numberOfChannels * (bitsPerSample / 8);
                BlockAlign = blockAlign;
                BitsPerSample = bitsPerSample;
                SubChunk2ID = SubChunk2IDValue;
                SubChunk2Size = subChunk2Size;
                _Storage.Close();
            }

            if (data != null)
                _Storage.SetFrom(data, sourceOffset, DataOffset, dataLength);

            return true;
        }

        public bool Initialize(WaveAudioBuffer other, int sampleIndex, int sampleCount)
        {
            if (other == null)
                return false;

            other.Storage.Open(PortableFileMode.Open);

            int numberOfChannels = other.NumberOfChannels;
            int sampleRate = other.SampleRate;
            int bitsPerSample = other.BitsPerSample;
            int blockAlign = numberOfChannels * (bitsPerSample / 8);
            int dataLength = sampleCount * blockAlign;
            int dataOffset = DataOffset + (sampleIndex * blockAlign);
            int subChunk1Size = 16;
            int subChunk2Size = dataLength;
            int newLength = dataLength + DataOffset;

            other.Storage.Close();

            SetDefaultStorage(newLength);

            if (_Storage.Open(PortableFileMode.OpenOrCreate))
            {
                ChunkID = ChunkIDValue;
                ChunkSize = newLength - 8;
                ChunkFormat = ChunkFormatValue;
                SubChunk1ID = SubChunk1IDValue;
                SubChunk1Size = subChunk1Size;
                AudioFormat = AudioFormatPCM;
                NumberOfChannels = numberOfChannels;
                SampleRate = sampleRate;
                ByteRate = sampleRate * numberOfChannels * (bitsPerSample / 8);
                BlockAlign = blockAlign;
                BitsPerSample = bitsPerSample;
                SubChunk2ID = SubChunk2IDValue;
                SubChunk2Size = subChunk2Size;
                _Storage.Close();
            }

            return _Storage.SetFrom(other.Storage, dataOffset, DataOffset, dataLength);
        }

        public virtual bool Delete()
        {
            if (_Storage != null)
                return _Storage.Delete();
            return false;
        }
    }
}
