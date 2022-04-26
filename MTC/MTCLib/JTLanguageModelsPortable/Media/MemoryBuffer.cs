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
    public class MemoryBuffer : BaseObject, IDataBuffer
    {
        protected byte[] _Data;
        protected int _Capacity;
        protected int _Length;
        protected bool _Modified;
        protected bool _Opened;

        // Get data from file.
        public MemoryBuffer(string filePath)
        {
            ClearMemoryBuffer();

            _Data = FileSingleton.ReadAllBytes(filePath);

            int length = _Data.Length;
            _Capacity = length;
            _Length = length;
        }

        // Get data from full stream.
        public MemoryBuffer(Stream stream)
        {
            ClearMemoryBuffer();

            int length = (int)stream.Length;
            _Data = new byte[length];

            if (stream.Read(_Data, 0, length) != length)
                throw new ObjectException("Stream read didn't complete.");

            _Capacity = length;
            _Length = length;
        }

        // Adopt data buffer.
        public MemoryBuffer(byte[] data, int length, int capacity)
        {
            ClearMemoryBuffer();

            _Data = data;

            if (_Data != null)
            {
                _Capacity = capacity;
                _Length = length;
            }
        }

        // Adopt data buffer.
        public MemoryBuffer(byte[] data)
        {
            ClearMemoryBuffer();

            _Data = data;

            if (_Data != null)
            {
                _Capacity = _Data.Count();
                _Length = _Capacity;
            }
        }

        // Presize.
        public MemoryBuffer(int capacity)
        {
            ClearMemoryBuffer();
            _Data = new byte[capacity];
            _Capacity = capacity;
        }

        // Copy from other buffer.
        public MemoryBuffer(IDataBuffer other)
        {
            ClearMemoryBuffer();
            SetFrom(other, 0, 0, other.Length);
        }

        // Empty.
        public MemoryBuffer()
        {
            ClearMemoryBuffer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMemoryBuffer();
        }

        public void ClearMemoryBuffer()
        {
            _Data = null;
            _Capacity = 0;
            _Length = 0;
            _Opened = false;
        }

        public override IBaseObject Clone()
        {
            return new MemoryBuffer(this);
        }

        public virtual int Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (value > _Capacity)
                    Capacity = value;

                _Length = value;
                _Modified = true;
            }
        }

        public virtual int Capacity
        {
            get
            {
                return _Capacity;
            }
            set
            {
                if (value == _Capacity)
                    return;

                byte[] newData = new byte[value];

                if (_Data != null)
                {
                    if (value < _Length)
                    {
                        System.Array.Copy(_Data, 0, newData, 0, value);
                        _Length = value;
                    }
                    else
                        System.Array.Copy(_Data, 0, newData, 0, _Length);
                }

                _Data = newData;
                _Capacity = value;
                _Modified = true;
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
            if (_Opened)
                return null;

            if (_Data != null)
                return new MemoryStream(_Data, 0, _Length);
            else
                return new MemoryStream();
        }

        public virtual bool CompleteRead(Stream readableStream)
        {
            if (_Opened)
                return false;

            return true;
        }

        public virtual Stream GetWriteableStream()
        {
            if (_Opened)
                return null;

            return new MemoryStream();
        }

        public virtual bool CompleteWrite(Stream writeableStream)
        {
            if (_Opened)
                return false;

            if (writeableStream != null)
            {
                lock (writeableStream)
                {
                    writeableStream.Seek(0, SeekOrigin.Begin);
                    _Capacity = _Length = (int)writeableStream.Length;
                    _Data = new byte[_Capacity];
                    writeableStream.Read(_Data, 0, _Length);
                }
                return true;
            }
            return false;
        }

        // Open/close functions.

        public virtual bool Open(PortableFileMode mode)
        {
            if (_Opened)
                return false;

            _Opened = true;
            return true;
        }

        public virtual bool Close()
        {
            if (!_Opened)
                return false;

            _Opened = false;
            return true;
        }

        public virtual bool IsOpen()
        {
            return _Opened;
        }

        public virtual bool Exists()
        {
            return _Data != null;
        }

        // Access functions for use between Open and Close.

        public virtual int GetByte(int offset)
        {
            if (_Opened && (_Data != null) && (offset >= 0) && (offset < _Length))
                return _Data[offset];
            return -1;
        }

        public virtual byte[] GetBytes()
        {
            lock (this)
            {
                if (_Opened)
                    return _Data;
            }
            return null;
        }

        public virtual byte[] GetBytes(int offset, int length)
        {
            lock (this)
            {
                if (_Opened && (offset >= 0) && (offset < _Length) && (length >= 0) && (offset + length <= _Length))
                {
                    byte[] buffer = new byte[length];
                    System.Array.Copy(_Data, offset, buffer, 0, length);
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
                    && (destinationOffset >= 0) && (destinationOffset + length < buffer.Count()))
                {
                    System.Array.Copy(_Data, sourceOffset, buffer, destinationOffset, length);
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
                if (_Opened && (offset >= 0) && (offset < _Length) && (size >= 0) && (offset + size <= _Length))
                {
                    if (littleEndian)
                    {
                        for (int i = 0; i < size; i++)
                            value += (_Data[offset + i] << (8 * i));
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                            value += (_Data[offset + i] << (8 * (size - (i + 1))));
                    }

                    if (signExtend && (size < sizeof(int)) && ((value & (1 << ((8 * size) - 1))) != 0))
                        value |= ~((1 << (8 * size)) - 1);

                    return value;
                }
            }

            return -1;
        }

        public virtual bool SetByte(int data, int offset)
        {
            lock (this)
            {
                if (!_Opened)
                    return false;

                if ((_Data != null) && (offset >= 0) && (offset < _Length))
                {
                    _Data[offset] = (byte)data;
                    return true;
                }
                else if (offset == _Length)
                {
                    if (offset >= _Capacity)
                        Capacity = _Length + 1;

                    _Data[offset] = (byte)data;
                    _Length++;
                    return true;
                }
            }

            return false;
        }

        public virtual bool FillByte(int data, int offset, int length)
        {
            byte b = (byte)data;

            lock (this)
            {
                if (!_Opened)
                    return false;

                if (offset > _Length)
                    return false;

                if ((_Data != null) && (offset >= 0) && (offset + length <= _Length))
                {
                    for (int i = 0; i < length; i++)
                        _Data[offset + i] = b;
                }
                else
                {
                    int requiredCapacity = offset + length;

                    if (offset >= _Capacity)
                        Capacity = _Length + 1;

                    if (requiredCapacity > _Capacity)
                        Capacity = requiredCapacity;

                    for (int i = 0; i < length; i++)
                        _Data[offset + i] = b;

                    if (offset + length > _Length)
                        _Length = offset + length;
                }
            }

            return true;
        }

        public virtual bool SetBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length)
        {
            lock (this)
            {
                if (!_Opened)
                    return false;

                if (length == 0)
                    return true;

                if ((buffer != null) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= buffer.Count())
                    && (destinationOffset >= 0) && (destinationOffset <= _Length))
                {
                    int requiredCapacity = destinationOffset + length;

                    if (requiredCapacity > _Capacity)
                        Capacity = requiredCapacity;

                    System.Array.Copy(buffer, sourceOffset, _Data, destinationOffset, length);

                    if (_Length < requiredCapacity)
                        _Length = requiredCapacity;

                    return true;
                }
            }
            return false;
        }

        public bool SetInteger(int data, int offset, int size, bool littleEndian)
        {
            lock (this)
            {
                if (!_Opened)
                    return false;

                if ((offset >= 0) && (offset <= _Length) && (size >= 0))
                {
                    int requiredCapacity = offset + size;

                    if (requiredCapacity > _Capacity)
                        Capacity = requiredCapacity;

                    if (littleEndian)
                    {
                        for (int i = 0; i < size; i++)
                            _Data[offset + i] = (byte)((data >> (8 * i)) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                            _Data[offset + i] = (byte)((data >> (8 * (size - (i + 1)))));
                    }

                    if (offset + size > _Length)
                        _Length = offset + size;

                    return true;
                }
            }

            return false;
        }

        public virtual byte[] GetAllBytes()
        {
            lock (this)
            {
                if (!_Opened)
                    return _Data;
            }
            return null;
        }

        public virtual bool SetAllBytes(byte[] buffer)
        {
            lock (this)
            {
                if (!_Opened)
                {
                    _Data = buffer;
                    return true;
                }
            }
            return false;
        }

        public virtual bool InsertBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length)
        {
            lock (this)
            {
                if (_Opened)
                    return false;

                if (length == 0)
                    return true;

                if ((buffer != null) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= buffer.Count())
                    && (destinationOffset >= 0) && (destinationOffset <= _Length))
                {
                    int requiredCapacity = _Length + length;

                    if (requiredCapacity > _Capacity)
                        Capacity = requiredCapacity;

                    if (destinationOffset < _Length)
                        System.Array.Copy(_Data, destinationOffset, _Data, destinationOffset + length, _Length - destinationOffset);

                    System.Array.Copy(buffer, sourceOffset, _Data, destinationOffset, length);
                    _Length += length;

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

                if ((buffer != null) && (sourceOffset >= 0) && (sourceOffset + sourceLength <= buffer.Count())
                    && (destinationOffset >= 0) && (destinationOffset <= _Length))
                {
                    int growth = sourceLength - destinationLength;
                    int capacityGrowth = 0;

                    if (sourceLength > destinationLength)
                    {
                        int requiredCapacity = _Length + growth;

                        if (destinationOffset + destinationLength > _Length)
                        {
                            requiredCapacity += (_Length - (destinationOffset + destinationLength));
                            capacityGrowth = (requiredCapacity - _Capacity);
                        }

                        if (requiredCapacity > _Capacity)
                            Capacity = requiredCapacity;

                        if (destinationOffset + destinationLength < _Length)
                            System.Array.Copy(_Data, destinationOffset + destinationLength,
                                _Data, destinationOffset + destinationLength + growth, _Length - (destinationOffset + destinationLength));

                        growth += capacityGrowth;
                    }
                    else if (destinationOffset + destinationLength < _Length)
                    {
                        System.Array.Copy(_Data, destinationOffset + destinationLength,
                            _Data, destinationOffset + sourceLength, _Length - (destinationOffset + destinationLength));
                    }
                    else if (destinationOffset + destinationLength > _Length)
                    {
                        int requiredCapacity = destinationOffset + sourceLength;

                        if (requiredCapacity > _Capacity)
                        {
                            capacityGrowth = (requiredCapacity - _Capacity);;
                            Capacity = requiredCapacity;
                            growth += capacityGrowth;
                        }
                    }

                    System.Array.Copy(buffer, sourceOffset, _Data, destinationOffset, sourceLength);
                    _Length += growth;

                    return true;
                }
            }
            return false;
        }

        public virtual bool DeleteBytes(int destinationOffset, int length)
        {
            lock (this)
            {
                if (_Opened)
                    return false;

                if (length == 0)
                    return true;

                if ((_Data != null) && (length >= 0) && (destinationOffset >= 0) && (destinationOffset + length <= _Length))
                {
                    if (destinationOffset + length < _Length)
                        System.Array.Copy(_Data, destinationOffset + length, _Data, destinationOffset, _Length - (destinationOffset + length));

                    _Length -= length;

                    return true;
                }
            }
            return false;
        }

        public virtual bool ClearBytes(int destinationOffset, int length)
        {
            lock (this)
            {
                if (_Opened)
                    return false;

                if (length == 0)
                    return true;

                if ((_Data != null) && (length >= 0) && (destinationOffset >= 0) && (destinationOffset + length <= _Length))
                {
                    int endIndex = destinationOffset + length;

                    for (int index = destinationOffset; index < endIndex; index++)
                        _Data[index] = 0;

                    return true;
                }
            }
            return false;
        }

        public virtual bool CropBytes(int frontOffset, int rearOffset)
        {
            lock (this)
            {
                if (_Opened)
                    return false;

                if ((frontOffset == 0) && (rearOffset == _Length))
                    return true;

                if ((_Data != null) && (frontOffset >= 0) && (frontOffset <= _Length) && (rearOffset >= frontOffset) && (rearOffset <= _Length))
                {
                    if (frontOffset != 0)
                        System.Array.Copy(_Data, frontOffset, _Data, 0, rearOffset - frontOffset);

                    _Length = rearOffset - frontOffset;

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

                if ((_Data != null) && (frontOffset >= 0) && (frontOffset <= _Length) && (rearOffset >= frontOffset) && (rearOffset <= _Length)
                    && ((fadeInCount + fadeOutCount) < (rearOffset - frontOffset)))
                {
                    if (fadeInCount != 0)
                    {
                        int endIndex = frontOffset + fadeInCount;
                        int fadeIndex = 0;

                        for (int index = frontOffset; index < endIndex; index++, fadeIndex++)
                            _Data[index] = (byte)(((int)(sbyte)_Data[index] * fadeIndex)/fadeInCount);
                    }

                    if (fadeOutCount != 0)
                    {
                        int fadeIndex = fadeOutCount - 1;

                        for (int index = rearOffset - fadeOutCount; index < rearOffset; index++, fadeIndex--)
                            _Data[index] = (byte)(((int)(sbyte)_Data[index] * fadeIndex) / fadeInCount);
                    }

                    return true;
                }
            }
            return false;
        }

        public virtual bool SetFrom(IDataBuffer other, int sourceOffset, int destinationOffset, int length)
        {
            lock (this)
            {
                if (_Opened)
                    return false;

                if (length == 0)
                    return true;

                if ((other != null) && (other.Length != 0) && (length >= 0) && (sourceOffset >= 0) && (sourceOffset + length <= other.Length)
                    && (destinationOffset >= 0) && (destinationOffset <= _Length))
                {
                    int requiredCapacity = destinationOffset + length;

                    if (requiredCapacity > _Capacity)
                        Capacity = requiredCapacity;

                    if (other is MemoryBuffer)
                    {
                        MemoryBuffer otherMemoryBuffer = other as MemoryBuffer;
                        System.Array.Copy(otherMemoryBuffer._Data, sourceOffset, _Data, destinationOffset, length);
                    }
                    else if (other.Open(PortableFileMode.Open))
                    {
                        byte[] buffer = other.GetBytes(sourceOffset, length);
                        System.Array.Copy(buffer, 0, _Data, destinationOffset, length);
                        other.Close();
                    }

                    if (_Length < requiredCapacity)
                        _Length = requiredCapacity;

                    return true;
                }
            }
            return false;
        }

        public virtual bool InsertFrom(IDataBuffer other, int sourceOffset, int destinationOffset, int length)
        {
            bool returnValue = false;

            lock (this)
            {
                if (other is MemoryBuffer)
                {
                    MemoryBuffer otherMemoryBuffer = other as MemoryBuffer;
                    returnValue = InsertBytes(otherMemoryBuffer._Data, sourceOffset, destinationOffset, length);
                }
                else
                {
                    bool otherWasOpen = other.IsOpen();
                    if (!otherWasOpen)
                    {
                        if (!other.Open(PortableFileMode.Open))
                            return false;
                    }
                    byte[] buffer = other.GetBytes(sourceOffset, length);
                    if (!otherWasOpen)
                        other.Close();
                    returnValue = InsertBytes(buffer, 0, destinationOffset, length);
                }
            }

            return returnValue;
        }

        public virtual bool ReplaceFrom(IDataBuffer other, int sourceOffset, int sourceLength, int destinationOffset, int destinationLength)
        {
            bool returnValue = false;

            lock (this)
            {
                if (_Opened)
                    return false;

                if (other is MemoryBuffer)
                {
                    MemoryBuffer otherMemoryBuffer = other as MemoryBuffer;
                    returnValue = ReplaceBytes(otherMemoryBuffer._Data, sourceOffset, sourceLength, destinationOffset, destinationLength);
                }
                else
                {
                    byte[] buffer = other.GetBytes(sourceOffset, sourceLength);
                    returnValue = ReplaceBytes(buffer, sourceOffset, sourceLength, destinationOffset, destinationLength);
                }
            }

            return returnValue;
        }

        public virtual bool Delete()
        {
            Clear();
            return true;
        }

        public override XElement GetElement(string name)
        {
            if (_Data == null)
                return new XElement(name);

            XElement element = new XElement(name, ObjectUtilities.GetDataStringFromByteArray(_Data, _Length, true));
            return element;
        }

        public override void OnElement(XElement element)
        {
            Clear();
            _Data = ObjectUtilities.GetByteArrayFromDataString(element.Value);
            _Capacity = _Length = _Data.Count();
        }
    }
}
