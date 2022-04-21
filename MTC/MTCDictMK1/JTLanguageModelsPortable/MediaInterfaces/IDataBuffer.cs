using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.MediaInterfaces
{
    public interface IDataBuffer : IBaseObject
    {
        int Length { get; set;  }
        bool Modified { get; set; }

        // Stream functions - create readable or writeable stream.
        Stream GetReadableStream();
        bool CompleteRead(Stream readableStream);
        Stream GetWriteableStream();
        bool CompleteWrite(Stream writeableStream);

        // Open/close functions.
        bool Open(PortableFileMode mode);
        bool Close();
        bool IsOpen();
        bool Exists();

        // Access functions for use between Open and Close.
        int GetByte(int offset);
        byte[] GetBytes();
        byte[] GetBytes(int offset, int length);
        bool GetBytes(byte[] buffer, int destinationOffset, int sourceOffset, int length);
        int GetInteger(int offset, int size, bool littleEndian, bool signExtend);
        bool SetByte(int data, int offset);
        bool FillByte(int data, int offset, int length);
        bool SetBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length);
        bool SetInteger(int data, int offset, int size, bool littleEndian);

        // Access functions for use outside of Open and Close.
        byte[] GetAllBytes();
        bool SetAllBytes(byte[] buffer);
        bool InsertBytes(byte[] buffer, int sourceOffset, int destinationOffset, int length);
        bool ReplaceBytes(byte[] buffer, int sourceOffset, int sourceLength, int destinationOffset, int destinationLength);
        bool DeleteBytes(int destinationOffset, int length);
        bool ClearBytes(int destinationOffset, int length);
        bool CropBytes(int frontOffset, int rearOffset);
        bool FadeBytes(int frontOffset, int rearOffset, int fadeInCount, int fadeOutCount);
        bool SetFrom(IDataBuffer other, int sourceOffset, int destinationOffset, int length);
        bool InsertFrom(IDataBuffer other, int sourceOffset, int destinationOffset, int length);
        bool ReplaceFrom(IDataBuffer other, int sourceOffset, int sourceLength, int destinationOffset, int destinationLength);
        bool Delete();
    }
}
