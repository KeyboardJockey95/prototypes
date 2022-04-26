using System;
using System.Collections.Generic;
using System.IO;
#if SILVERLIGHT
using System.IO.IsolatedStorage;
#endif
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.MediaInterfaces;
//using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Media
{
    public class FileBuffer : BaseObject, IDataBuffer
    {
        protected string _FilePath;
        protected int _Length;
        protected bool _Modified;
        protected bool _Opened;

        // For temporary use while reading or writing.
        protected Stream _Stream;
        protected Stream _TempStream;
        protected int _Offset;
        protected bool _Writing;

        // Get data from stream.
        public FileBuffer(string filePath, Stream stream)
        {
            ClearFileBuffer();
            _FilePath = filePath;
            _Length = (int)stream.Length;
            Stream outStream = GetWriteableStream();
            StreamTransfer(stream, outStream);
            CompleteWrite(outStream);
        }

        // Create from data buffer.
        public FileBuffer(string filePath, byte[] data)
        {
            ClearFileBuffer();
            _FilePath = filePath;
            _Length = data.Count();
            Stream outStream = GetWriteableStream();
            outStream.Write(data, 0, _Length);
            CompleteWrite(outStream);
        }

        // Create from data buffer.
        public FileBuffer(string filePath, byte[] data, int offset, int length)
        {
            ClearFileBuffer();
            _FilePath = filePath;
            _Length = length;
            Stream outStream = GetWriteableStream();
            outStream.Write(data, offset, length);
            CompleteWrite(outStream);
        }

        // Copy from other buffer.
        public FileBuffer(string filePath, IDataBuffer other)
        {
            ClearFileBuffer();
            _FilePath = filePath;
            SetFrom(other, 0, 0, other.Length);
        }


        // Just set the name.
        public FileBuffer(string filePath)
        {
            ClearFileBuffer();
            _FilePath = filePath;
        }

        // Empty.
        public FileBuffer()
        {
            ClearFileBuffer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearFileBuffer();
        }

        public void ClearFileBuffer()
        {
            _FilePath = null;
            _Length = -1;
            _Modified = false;
            _Opened = false;
            _Stream = null;
            _TempStream = null;
            _Offset = 0;
            _Writing = false;
        }

        public override IBaseObject Clone()
        {
            return new FileBuffer(_FilePath, this);
        }

        public virtual int Length
        {
            get
            {
                if (_Length == -1)
                {
                    if (_Stream != null)
                        _Length = (int)_Stream.Length;
                    else if (Open(PortableFileMode.Open))
                    {
                        if (_Stream != null)
                            _Length = (int)_Stream.Length;
                        Close();
                    }
                    else
                        return 0;
                }
                return _Length;
            }
            set
            {
                if (value != _Length)
                {
                    _Length = value;
                    _Modified = true;
                }
            }
        }

        public virtual bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                _Modified = value;
            }
        }

        // Stream functions - create readable or writeable stream.

        public virtual Stream GetReadableStream()
        {
            Stream stream = null;

            if (!String.IsNullOrEmpty(_FilePath))
            {
                stream = FileSingleton.OpenRead(_FilePath);
            }

            return stream;
        }

        public virtual bool CompleteRead(Stream readableStream)
        {
            FileSingleton.Close(readableStream);
            return true;
        }

        public virtual Stream GetWriteableStream()
        {
            Stream stream = null;

            if (!String.IsNullOrEmpty(_FilePath))
            {
                FileSingleton.DirectoryExistsCheck(_FilePath);
                stream = FileSingleton.Create(_FilePath);
            }

            return stream;
        }

        public virtual bool CompleteWrite(Stream writeableStream)
        {
            _Length = (int)writeableStream.Length;
            FileSingleton.Close(writeableStream);
            return true;
        }

        // Open/close functions.

        public virtual bool Open(PortableFileMode mode)
        {
            if (_Opened)
                return false;

            switch (mode)
            {
                case PortableFileMode.CreateNew:
                case PortableFileMode.Create:
                case PortableFileMode.OpenOrCreate:
                case PortableFileMode.Truncate:
                    FileSingleton.DirectoryExistsCheck(_FilePath);
                    break;
                default:
                    break;
            }

            try
            {
                _Stream = FileSingleton.Open(_FilePath, mode);
            }
            catch (Exception)
            {
                _Length = 0;
                _Offset = 0;
                _Opened = false;
                return false;
            }

            _Length = (int)_Stream.Length;
            _Offset = 0;
            _Opened = true;
            return true;
        }

        public virtual bool Close()
        {
            if (!_Opened)
                return false;

            if (_Stream != null)
            {
                FileSingleton.Close(_Stream);
                _Stream = null;
            }

            _Opened = false;

            return true;
        }

        public virtual bool IsOpen()
        {
            return _Opened;
        }

        public virtual bool Exists()
        {
            if (!String.IsNullOrEmpty(_FilePath))
            {
                return FileSingleton.Exists(_FilePath);
            }

            return false;
        }

        protected bool ReadSetup(int offset)
        {
            if (_Stream != null)
            {
                if (_Offset != offset)
                {
                    if (_Stream.CanSeek)
                        _Offset = (int)_Stream.Seek(offset, SeekOrigin.Begin);
                    else
                        return false;
                }
            }

            return true;
        }

        protected bool WriteSetup(int offset)
        {
            if (_Stream != null)
            {
                if (_Offset != offset)
                {
                    if (_Stream.CanSeek)
                        _Offset = (int)_Stream.Seek(offset, SeekOrigin.Begin);
                    else
                        return false;
                }
            }

            return true;
        }

        protected bool StreamTransfer(Stream inStream, Stream outStream)
        {
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            int read;

            while ((read = inStream.Read(buffer, 0, bufferSize)) > 0)
                outStream.Write(buffer, 0, read);

            return true;
        }

        protected bool StreamTransfer(Stream inStream, Stream outStream, int length)
        {
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            int offset;
            int read;

            for (offset = 0; offset < length; )
            {
                int toRead = length - offset;

                if (toRead > bufferSize)
                    toRead = bufferSize;

                read = inStream.Read(buffer, 0, toRead);

                if (read != toRead)
                    return false;

                if (read == 0)
                    return false;

                outStream.Write(buffer, 0, read);
                offset += read;
            }

            return true;
        }

        // Access functions for use between Open and Close.

        public virtual int GetByte(int offset)
        {
            lock (this)
            {
                if (_Opened && (offset >= 0) && (offset < _Length) && ReadSetup(offset))
                {
                    int value = _Stream.ReadByte();
                    _Offset++;
                    return value;
                }
            }
            return -1;
        }

        public virtual byte[] GetBytes()
        {
            return GetBytes(0, _Length);
        }

        public virtual byte[] GetBytes(int offset, int length)
        {
            lock (this)
            {
                if (_Opened && (offset >= 0) && (offset < _Length) && (length >= 0) && (offset + length <= _Length) && ReadSetup(offset))
                {
                    byte[] buffer = new byte[length];
                    if (_Stream.Position != offset)
                    {
                        _Offset = (int)_Stream.Seek(offset, SeekOrigin.Begin);
                        if (_Offset == -1)
                            return null;
                    }
                    int read = (int)_Stream.Read(buffer, 0, length);
                    _Offset += read;
                    if (read != length)
                        return null;
                    return buffer;
                }
            }
            return null;
        }

        public virtual bool GetBytes(byte[] buffer, int destinationOffset, int sourceOffset, int length)
        {
            lock (this)
            {
                if (_Opened && (buffer != null) && (sourceOffset >= 0) && (sourceOffset < _Length) && (length >= 0) && (sourceOffset + length <= _Length)
                    && (destinationOffset >= 0) && (destinationOffset + length < buffer.Count()) && ReadSetup(sourceOffset))
                {
                    int read = (int)_Stream.Read(buffer, destinationOffset, length);
                    _Offset += read;
                    if (read != length)
                        return false;
                    return true;
                }
            }
            return false;
        }

        public virtual int GetInteger(int offset, int size, bool littleEndian, bool signExtend)
        {
            int value = 0;

            lock (this)
            {
                if (_Opened && (offset >= 0) && (offset < _Length) && (size >= 0) && (offset + size <= _Length) && ReadSetup(offset))
                {
                    if (littleEndian)
                    {
                        for (int i = 0; i < size; i++)
                            value += (_Stream.ReadByte() << (8 * i));
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                            value += (_Stream.ReadByte() << (8 * (size - (i + 1))));
                    }

                    _Offset += size;

                    if (signExtend && (size < sizeof(int)) && ((value & (1 << ((8 * size) - 1))) != 0))
                        value |= ~((1 << (8 * size)) - 1);

                    return value;
                }
            }

            return -1;
        }

        public virtual bool SetByte(int data, int offset)
        {
            if (_Opened && (offset >= 0) && (offset < _Length) && WriteSetup(offset))
            {
                _Stream.WriteByte((byte)data);
                _Offset++;
                return true;
            }
            else if ((offset == _Length) && WriteSetup(offset))
            {
                _Stream.WriteByte((byte)data);
                _Offset++;
                _Length++;
                return true;
            }

            return false;
        }

        public virtual bool FillByte(int data, int offset, int length)
        {
            byte b = (byte)data;

            if (offset > _Length)
                return false;

            if (_Opened && (offset >= 0) && (offset <= _Length) && WriteSetup(offset))
            {
                while (length-- > 0)
                {
                    _Stream.WriteByte(b);
                    _Offset++;

                    if (_Offset > _Length)
                        _Length++;
                }

                return true;
            }

            return false;
        }

        public virtual bool SetBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length)
        {
            if (length == 0)
                return true;

            lock (this)
            {
                if (_Opened && (buffer != null) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= buffer.Count())
                    && (destinationOffset >= 0) && (destinationOffset <= _Length) && WriteSetup(destinationOffset))
                {
                    _Stream.Write(buffer, sourceOffset, length);

                    _Offset += length;

                    if (_Length < _Offset)
                        _Length = _Offset;

                    return true;
                }
            }
            return false;
        }

        public bool SetInteger(int data, int offset, int size, bool littleEndian)
        {
            lock (this)
            {
                if (_Opened && (offset >= 0) && (offset <= _Length) && (size >= 0) && WriteSetup(offset))
                {
                    if (littleEndian)
                    {
                        for (int i = 0; i < size; i++)
                            _Stream.WriteByte((byte)((data >> (8 * i)) & 0xff));
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                            _Stream.WriteByte((byte)((data >> (8 * (size - (i + 1))))));
                    }

                    _Offset += size;

                    if (_Length < _Offset)
                        _Length = _Offset;

                    return true;
                }
            }

            return false;
        }

        protected void GetLengthCheck()
        {
            if (_Length == -1)
            {
                if (_Opened)
                    return;

                if (String.IsNullOrEmpty(_FilePath))
                    return;

                if (FileSingleton.Exists(_FilePath))
                {
                    Open(PortableFileMode.Open);
                    Close();
                }
                else
                    _Length = 0;
            }
        }

        // Access functions for use outside of Open and Close.

        public virtual byte[] GetAllBytes()
        {
            if (_Opened)
                return null;

            GetLengthCheck();

            if (Open(PortableFileMode.Open))
            {
                byte[] returnValue = GetBytes();
                Close();
                return returnValue;
            }

            return null;
        }

        public virtual bool SetAllBytes(byte[] buffer)
        {
            if (_Opened)
                return false;

            if (buffer == null)
                return false;

            GetLengthCheck();

            if (Open(PortableFileMode.OpenOrCreate))
            {
                bool returnValue = SetBytes(buffer, 0, 0, buffer.Count());
                Close();
                return returnValue;
            }

            return false;
        }

        public virtual bool InsertBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length)
        {
            if (_Opened)
                return false;

            if (length == 0)
                return true;

            GetLengthCheck();

            if (destinationOffset == _Length)
            {
                if (Open(PortableFileMode.OpenOrCreate))
                {
                    bool returnValue = SetBytes(buffer, sourceOffset, destinationOffset, length);
                    Close();
                    return returnValue;
                }
                else
                    return false;
            }

            lock (this)
            {
                if ((buffer != null) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= buffer.Count())
                    && (destinationOffset >= 0) && (destinationOffset <= _Length) && Open(PortableFileMode.OpenOrCreate))
                {
                    BeginTempWrite();

                    if (destinationOffset > 0)
                        StreamTransfer(_Stream, _TempStream, destinationOffset);

                    _TempStream.Write(buffer, sourceOffset, length);

                    StreamTransfer(_Stream, _TempStream, _Length - destinationOffset);

                    _Length += length;

                    EndTempWrite();

                    return true;
                }
            }
            return false;
        }

        public virtual bool ReplaceBytes(byte[] buffer, int sourceOffset, int sourceLength, int destinationOffset, int destinationLength)
        {
            if (destinationLength <= 0)
                return InsertBytes(buffer, sourceOffset, destinationOffset, sourceLength);
            else if (sourceLength <= 0)
                return DeleteBytes(destinationOffset, destinationLength);

            lock (this)
            {
                if (_Opened)
                    return false;

                GetLengthCheck();

                if ((buffer != null) && (sourceOffset >= 0) && (sourceOffset + sourceLength <= buffer.Count())
                    && (destinationOffset >= 0) && (destinationOffset <= _Length) && Open(PortableFileMode.OpenOrCreate))
                {
                    BeginTempWrite();

                    if (destinationOffset > 0)
                        StreamTransfer(_Stream, _TempStream, destinationOffset);

                    _TempStream.Write(buffer, sourceOffset, sourceLength);

                    _Stream.Seek(destinationLength, SeekOrigin.Current);

                    if ((destinationOffset + destinationLength) < _Length)
                        StreamTransfer(_Stream, _TempStream, _Length - (destinationOffset + destinationLength));

                    _Length = (int)_TempStream.Length;

                    EndTempWrite();

                    return true;
                }
            }
            return false;
        }

        public virtual bool DeleteBytes(int destinationOffset, int length)
        {
            if (length == 0)
                return true;

            lock (this)
            {
                GetLengthCheck();

                if (!_Opened && (length >= 0) && (destinationOffset >= 0) && (destinationOffset + length <= _Length) && Open(PortableFileMode.Open))
                {
                    BeginTempWrite();

                    if (destinationOffset > 0)
                        StreamTransfer(_Stream, _TempStream, destinationOffset);

                    _Stream.Seek(length, SeekOrigin.Current);

                    if ((destinationOffset + length) < _Length)
                        StreamTransfer(_Stream, _TempStream, _Length - (destinationOffset + length));

                    _Length = (int)_TempStream.Length;

                    EndTempWrite();

                    return true;
                }
            }
            return false;
        }

        public virtual bool ClearBytes(int destinationOffset, int length)
        {
            if (length == 0)
                return true;

            lock (this)
            {
                GetLengthCheck();

                if (!_Opened && (length >= 0) && (destinationOffset >= 0) && (destinationOffset + length <= _Length))
                {
                    int endIndex = destinationOffset + length;

                    if (Open(PortableFileMode.OpenOrCreate))
                    {
                        _Stream.Seek(destinationOffset, SeekOrigin.Begin);

                        for (int index = destinationOffset; index < endIndex; index++)
                            _Stream.WriteByte(0);

                        _Length = (int)_Stream.Length;

                        Close();

                        return true;
                    }
                }
            }
            return false;
        }

        public virtual bool CropBytes(int frontOffset, int rearOffset)
        {
            if ((frontOffset == 0) && (rearOffset == _Length))
                return true;

            lock (this)
            {
                GetLengthCheck();

                if (!_Opened && (frontOffset >= 0) && (frontOffset <= _Length) && (rearOffset >= frontOffset) && (rearOffset <= _Length)
                    && Open(PortableFileMode.Open))
                {
                    BeginTempWrite();
                    _Stream.Seek(frontOffset, SeekOrigin.Begin);
                    StreamTransfer(_Stream, _TempStream, rearOffset - frontOffset);
                    _Length = (int)_TempStream.Length;
                    EndTempWrite();
                    return true;
                }
            }
            return false;
        }

        public virtual bool FadeBytes(int frontOffset, int rearOffset, int fadeInCount, int fadeOutCount)
        {
            lock (this)
            {
                if (_Opened)
                    return false;

                if ((frontOffset == 0) && (rearOffset == _Length))
                    return true;

                if ((frontOffset >= 0) && (frontOffset <= _Length) && (rearOffset >= frontOffset) && (rearOffset <= _Length)
                    && ((fadeInCount + fadeOutCount) < (rearOffset - frontOffset))
                    && Open(PortableFileMode.Open))
                {
                    if (fadeInCount != 0)
                    {
                        int endIndex = frontOffset + fadeInCount;
                        int fadeIndex = 0;

                        for (int index = frontOffset; index < endIndex; index++, fadeIndex++)
                        {
                            _Stream.Seek(index, SeekOrigin.Begin);
                            byte data = (byte)_Stream.ReadByte();
                            data = (byte)(((int)(sbyte)data * fadeIndex) / fadeInCount);
                            _Stream.Seek(index, SeekOrigin.Begin);
                            _Stream.WriteByte(data);
                        }
                    }

                    if (fadeOutCount != 0)
                    {
                        int fadeIndex = fadeOutCount - 1;

                        for (int index = rearOffset - fadeOutCount; index < rearOffset; index++, fadeIndex--)
                        {
                            _Stream.Seek(index, SeekOrigin.Begin);
                            byte data = (byte)_Stream.ReadByte();
                            data = (byte)(((int)(sbyte)data * fadeIndex) / fadeInCount);
                            _Stream.Seek(index, SeekOrigin.Begin);
                            _Stream.WriteByte(data);
                        }
                    }

                    Close();

                    return true;
                }
            }
            return false;
        }

        public virtual bool SetFrom(IDataBuffer other, int sourceOffset, int destinationOffset, int length)
        {
            if (length == 0)
                return true;

            lock (this)
            {
                GetLengthCheck();

                if (!_Opened && (other != null) && (other.Length != 0) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= other.Length)
                    && (destinationOffset >= 0) && (destinationOffset <= _Length) && Open(PortableFileMode.OpenOrCreate))
                {
                    Stream inStream = other.GetReadableStream();

                    inStream.Seek(sourceOffset, SeekOrigin.Begin);
                    _Stream.Seek(destinationOffset, SeekOrigin.Begin);

                    StreamTransfer(inStream, _Stream, length);
                    _Length = (int)_Stream.Length;

                    other.CompleteRead(inStream);

                    Close();

                    return true;
                }
            }
            return false;
        }

        public virtual bool InsertFrom(IDataBuffer other, int sourceOffset, int destinationOffset, int length)
        {
            bool returnValue = false;

            if (length == 0)
                return true;

            if (destinationOffset == _Length)
                return SetFrom(other, sourceOffset, destinationOffset, length);

            lock (this)
            {
                GetLengthCheck();

                if (!_Opened && (other != null) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= other.Length)
                    && (destinationOffset >= 0) && (destinationOffset <= _Length) && Open(PortableFileMode.OpenOrCreate))
                {
                    Stream sourceStream = other.GetReadableStream();
                    sourceStream.Seek(sourceOffset, SeekOrigin.Begin);

                    BeginTempWrite();

                    if (destinationOffset > 0)
                        StreamTransfer(_Stream, _TempStream, destinationOffset);

                    StreamTransfer(sourceStream, _TempStream, length);

                    StreamTransfer(_Stream, _TempStream, _Length - destinationOffset);

                    other.CompleteRead(sourceStream);

                    _Length = (int)_TempStream.Length;

                    EndTempWrite();

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public virtual bool ReplaceFrom(IDataBuffer other, int sourceOffset, int sourceLength, int destinationOffset, int destinationLength)
        {
            bool returnValue = false;

            lock (this)
            {
                GetLengthCheck();

                if (!_Opened && (other != null) && (sourceOffset >= 0) && (sourceOffset + sourceLength <= other.Length)
                    && (destinationOffset >= 0) && (destinationOffset <= _Length) && Open(PortableFileMode.OpenOrCreate))
                {
                    Stream sourceStream = other.GetReadableStream();
                    sourceStream.Seek(sourceOffset, SeekOrigin.Begin);

                    BeginTempWrite();

                    if (destinationOffset > 0)
                        StreamTransfer(_Stream, _TempStream, destinationOffset);

                    StreamTransfer(sourceStream, _TempStream, sourceLength);

                    _Stream.Seek(destinationLength, SeekOrigin.Current);

                    if ((destinationOffset + destinationLength) < _Length)
                        StreamTransfer(_Stream, _TempStream, _Length - (destinationOffset + destinationLength));

                    other.CompleteRead(sourceStream);

                    _Length = (int)_TempStream.Length;

                    EndTempWrite();

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XAttribute("FilePath", _FilePath));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "FilePath":
                    _FilePath = childElement.Value;
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        protected void BeginTempWrite()
        {
            string filePath = _FilePath + ".tmp";
            _TempStream = FileSingleton.Create(filePath);
        }

        protected void EndTempWrite()
        {
            Close();
            if (_TempStream != null)
            {
                FileSingleton.Close(_TempStream);
                _TempStream = null;
            }
            string filePath = _FilePath + ".tmp";
            FileSingleton.Delete(_FilePath);
            FileSingleton.Copy(filePath, _FilePath);
            FileSingleton.Delete(filePath);
        }

        public bool Delete()
        {
            FileSingleton.Delete(_FilePath);
            return true;
        }
    }
}
