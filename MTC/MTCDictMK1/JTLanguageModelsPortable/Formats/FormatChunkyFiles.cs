using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.MediaInterfaces;

namespace JTLanguageModelsPortable.Formats
{
    // Chunky format class.
    //
    // file format:
    //
    //      Field       Size            Content
    //      Header ID   2               'CF'
    //      Version     variable 1-5    packed int
    //      Count       variable 1-5    Count of file chunks
    //      Chunks...   Count           chunk
    //
    // Chunk format:
    //
    //      Field       Size            Content
    //      Chunk Type  1               'F' for file
    //      Chunk Size  variable 1-5    Size of chunk data
    //      Chunk Data  from Chunk Size Chunk type dependent
    //
    // File chunk data format:
    //
    //      Field       Size            Content
    //      SLength     variable 1-5    Partial file path string size
    //      Path        from SLength    Partial file path string relative to common file directory
    //      Size        variable 1-5    Size of file (0 for missing files)
    //      File Data   from Size       File bytes

    public class FormatChunkyFiles : Format
    {
        protected int Version = 1;
        private static string FormatDescription = "Native JTLanguage multiple binary file transfer format based on typed and sized binary chunks.";
        public string CommonFilePath;               // The common file path root for the files
        public bool OverwriteExistingFiles;
        public List<string> FileList;
        private const byte HeaderByte0 = (byte)'C';
        private const byte HeaderByte1 = (byte)'F';
        private const byte FileChunkCode = (byte)'F';

        public FormatChunkyFiles(
                string commonFilePath,
                List<string> fileList)
            : base("JTLanguage Chunky Files", "FormatChunkyFiles", FormatDescription, String.Empty, String.Empty,
                  "application/octet-stream", ".jtc", null, null, null, null, null)
        {
            CommonFilePath = commonFilePath;
            OverwriteExistingFiles = true;
            FileList = fileList;
        }

        public FormatChunkyFiles(
                string commonFilePath,
                bool overwriteExistingFiles)
            : base("JTLanguage Chunky Files", "FormatChunkyFiles", FormatDescription, String.Empty, String.Empty,
                  "application/octet-stream", ".jtc", null, null, null, null, null)
        {
            CommonFilePath = commonFilePath;
            OverwriteExistingFiles = overwriteExistingFiles;
            FileList = null;
        }

        public FormatChunkyFiles(FormatChunkyFiles other)
            : base(other)
        {
            CommonFilePath = other.CommonFilePath;
            OverwriteExistingFiles = other.OverwriteExistingFiles;
            FileList = null;
        }

        public FormatChunkyFiles()
            : base("JTLanguage Chunky Files", "FormatChunkyFiles", FormatDescription, String.Empty, String.Empty,
                  "application/octet-stream", ".jtc", null, null, null, null, null)
        {
            CommonFilePath = null;
            OverwriteExistingFiles = true;
            FileList = null;
        }

        public override Format Clone()
        {
            return new FormatChunkyFiles(this);
        }

        public override void Read(Stream stream)
        {
            try
            {
                int b1 = stream.ReadByte();
                int b2 = stream.ReadByte();

                if ((b1 == HeaderByte0) && (b2 == HeaderByte1))
                {
                    ItemCount = 0;
                    ResetProgress();

                    //if (Timer != null)
                    //    Timer.Start();

                    int version;
                    int fileCount;

                    if (!ReadPackedInteger(stream, out version))
                        throw new ObjectException("No \"Version\" field.");

                    if (version != Version)
                        throw new ObjectException("Format is too new for this version of JTLanguage.");

                    if (!ReadPackedInteger(stream, out fileCount))
                        throw new ObjectException("No \"Count\" field.");

                    FileList = new List<string>(fileCount);

                    string fileRelativePath;

                    for (int index = 0; index < fileCount; index++)
                    {
                        ReadFileChunk(stream, CommonFilePath, out fileRelativePath);

                        if (!String.IsNullOrEmpty(fileRelativePath))
                            FileList.Add(fileRelativePath);
                    }

                    EndContinuedProgress();

                    if (Timer != null)
                    {
                        //Timer.Stop();
                        OperationTime = Timer.GetTimeInSeconds();
                    }
                }
                else
                    Error = "Read not supported for this file type.";
            }
            catch (Exception exception)
            {
                Error = "Exception: " + exception.Message;

                if (exception.InnerException != null)
                    Error += " (" + exception.InnerException.Message + ")";
            }

            if (!String.IsNullOrEmpty(Error))
                throw new ObjectException(Error);
        }

        public override void Write(Stream stream)
        {
            ItemCount = 0;
            ProgressCount = ProgressIndex = 0;

            try
            {
                stream.WriteByte((byte)HeaderByte0);
                stream.WriteByte((byte)HeaderByte1);

                WritePackedInteger(stream, Version);

                int fileCount = (FileList != null ? FileList.Count() : 0);

                WritePackedInteger(stream, fileCount);

                foreach (string fileRelativePath in FileList)
                    WriteFileChunk(stream, CommonFilePath, fileRelativePath);

                stream.Flush();
            }
            catch (Exception exception)
            {
                Error = "Exception: " + exception.Message;

                if (exception.InnerException != null)
                    Error += " (" + exception.InnerException.Message + ")";
            }

            if (!String.IsNullOrEmpty(Error))
                throw new ObjectException(Error);
        }

        public bool ReadFileChunk(Stream stream, string directory, out string relativeFilePath)
        {
            int chunkType = stream.ReadByte();
            int chunkSize;
            string localFilePath;
            int fileLength;
            bool returnValue = true;

            relativeFilePath = null;

            if (chunkType != FileChunkCode)
                return false;

            if (!ReadPackedInteger(stream, out chunkSize))
                return false;

            if (!ReadString(stream, out relativeFilePath))
                return false;

            if (!ReadPackedInteger(stream, out fileLength))
                return false;

            if (ApplicationData.PlatformPathSeparator != @"\")
                localFilePath = relativeFilePath.Replace(@"\", ApplicationData.PlatformPathSeparator);
            else
                localFilePath = relativeFilePath;

            int relativeFilePathsize = GetStringFieldLength(relativeFilePath);
            int referenceChunkSize = relativeFilePathsize + fileLength + GetPackedIntegerSize(fileLength);

            if (referenceChunkSize != chunkSize)
                throw new Exception("Bad chunk size in file chunk.");

            if (fileLength == 0)
            {
                relativeFilePath = null;
                return true;
            }

            localFilePath = MediaUtilities.ConcatenateFilePath(CommonFilePath, localFilePath);

            FileSingleton.DirectoryExistsCheck(localFilePath);

            if (!OverwriteExistingFiles && FileSingleton.Exists(localFilePath))
            {
                try
                {
                    if (stream.CanSeek)
                        stream.Seek(fileLength, SeekOrigin.Current);
                    else
                    {
                        int readSize = fileLength;
                        const int bufferSize = 0x1000;
                        byte[] buffer = new byte[bufferSize];
                        int read;
                        int blockSize;

                        while (readSize > 0)
                        {
                            blockSize = (readSize < bufferSize ? readSize : bufferSize);

                            read = stream.Read(buffer, 0, blockSize);

                            if (read == 0)
                                throw new Exception("ReadFileChunk: Reading beyond end of stream.");

                            readSize -= read;
                        }
                    }
                }
                catch (Exception)
                {
                    returnValue = false;
                }
            }
            else
            {
                using (Stream fileStream = FileSingleton.Open(localFilePath, PortableFileMode.Create))
                {
                    try
                    {
                        int readSize = fileLength;
                        const int bufferSize = 0x1000;
                        byte[] buffer = new byte[bufferSize];
                        int read;
                        int blockSize;

                        while (readSize > 0)
                        {
                            blockSize = (readSize < bufferSize ? readSize : bufferSize);

                            read = stream.Read(buffer, 0, blockSize);

                            if (read == 0)
                                throw new Exception("ReadFileChunk: Reading beyond end of stream.");

                            fileStream.Write(buffer, 0, read);

                            readSize -= read;
                        }
                    }
                    catch (Exception)
                    {
                        returnValue = false;
                    }
                    finally
                    {
                        FileSingleton.Close(fileStream);
                    }
                }
            }

            return returnValue;
        }

        public bool WriteFileChunk(Stream stream, string mediaDir, string relativeFilePath)
        {
            string localFilePath;
            int fileSize = 0;
            bool returnValue = true;

            localFilePath = MediaUtilities.ConcatenateFilePath(CommonFilePath, relativeFilePath);

            try
            {
                fileSize = (int)FileSingleton.GetFileSize(localFilePath);
            }
            catch (Exception)
            {
                fileSize = 0;
            }

            int relativeFilePathSize = GetStringFieldLength(relativeFilePath);
            int chunkSize = relativeFilePathSize + fileSize + GetPackedIntegerSize(fileSize);

            stream.WriteByte(FileChunkCode);

            if (!WritePackedInteger(stream, chunkSize))
                return false;

            if (!WriteString(stream, relativeFilePath))
                return false;

            if (!WritePackedInteger(stream, fileSize))
                return false;

            if (fileSize != 0)
            {
                using (Stream fileStream = FileSingleton.OpenRead(localFilePath))
                {
                    try
                    {
                        const int bufferSize = 0x1000;
                        byte[] buffer = new byte[bufferSize];
                        int read;
                        int readAccum = 0;

                        while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                        {
                            stream.Write(buffer, 0, read);
                            readAccum += read;
                        }

                        if (readAccum != fileSize)
                            return false;
                    }
                    catch (Exception)
                    {
                        returnValue = false;
                    }
                    finally
                    {
                        FileSingleton.Close(fileStream);
                    }
                }
            }

            return returnValue;
        }

        public bool ReadPackedInteger(Stream stream, out int value)
        {
            int b;
            int shift = 0;

            value = 0;

            do
            {
                if (shift > 32)
                    return false;

                b = stream.ReadByte();

                if (b == -1)
                    return false;

                value += ((b & 0x7f) << shift);
                shift += 7;
            }
            while ((b & 0x80) == 0);

            return true;
        }

        public bool WritePackedInteger(Stream stream, int value)
        {
            uint data = (uint)value;
            byte b;

            do
            {
                b = (byte)(data & 0x7f);
                data >>= 7;

                if (data == 0)
                    b |= 0x80;

                stream.WriteByte(b);
            }
            while (data != 0);

            return true;
        }

        public int GetPackedIntegerSize(int value)
        {
            uint data = (uint)value;
            int size = 0;

            do
            {
                data >>= 7;
                size++;
            }
            while (data != 0);

            return size;
        }

        public bool ReadString(Stream stream, out string value)
        {
            int length = 0;

            value = null;

            if (!ReadPackedInteger(stream, out length))
                return false;

            byte[] data = new byte[length];

            if (!ReadData(stream, length, data))
                return false;

            value = ApplicationData.Encoding.GetString(data, 0, length);

            return true;
        }

        public bool WriteString(Stream stream, string value)
        {
            byte[] data = ApplicationData.Encoding.GetBytes(value);
            int length = data.Length;

            if (!WritePackedInteger(stream, length))
                return false;

            stream.Write(data, 0, length);

            return true;
        }

        public int GetStringFieldLength(string value)
        {
            byte[] data = ApplicationData.Encoding.GetBytes(value);
            int length = data.Length;

            length += GetPackedIntegerSize(length);

            return length;
        }

        public bool ReadData(Stream stream, int size, byte[] data)
        {
            int readSize = 0;

            while (readSize < size)
            {
                int readCount = stream.Read(data, readSize, size - readSize);

                if (readCount == 0)
                    return false;

                readSize += readCount;
            }

            return true;
        }

        public bool WriteData(Stream stream, int size, byte[] data)
        {
            stream.Write(data, 0, size);
            return true;
        }
    }
}
