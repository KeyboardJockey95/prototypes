using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.MediaInterfaces
{
    // Substitute for FileMode
    public enum PortableFileMode
    {
        CreateNew = 1,
        Create = 2,
        Open = 3,
        OpenOrCreate = 4,
        Truncate = 5,
        Append = 6
    }

    public enum PortableFileAccess
    {
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }

    public interface IPortableFile
    {
        void AppendAllText(string path, string contents);
        void AppendAllText(string path, string contents, Encoding encoding);
        StreamWriter AppendText(string path);
        void Copy(string sourceFileName, string destFileName);
        void Copy(string sourceFileName, string destFileName, bool overwrite);
        Stream Create(string path);
        StreamWriter CreateText(string path);
        void Delete(string path);
        bool Exists(string path);
        void Move(string sourceFileName, string destFileName);
        Stream Open(string path, PortableFileMode mode);
        Stream Open(string path, PortableFileMode mode, PortableFileAccess access);
        Stream OpenRead(string path);
        StreamReader OpenText(string path);
        StreamReader OpenText(string path, Encoding encoding);
        Stream OpenWrite(string path);
        byte[] ReadAllBytes(string path);
        string ReadAllText(string path);
        string ReadAllText(string path, Encoding encoding);
        string[] ReadAllLines(string path);
        string[] ReadAllLines(string path, Encoding encoding);
        void WriteAllBytes(string path, byte[] bytes);
        void WriteAllText(string path, string contents);
        void WriteAllText(string path, string contents, Encoding encoding);
        void WriteAllLines(string path, string[] contents);
        void WriteAllLines(string path, string[] contents, Encoding encoding);
        void Close(Stream stream);
        void Close(StreamReader streamReader);
        void Close(StreamWriter streamWriter);
        bool DirectoryExistsCheck(string path);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        void RenameDirectory(string oldPath, string newPath);
        void DeleteDirectory(string path);
        bool DeleteEmptyDirectory(string path);
        void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath);
        void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath, bool overwrite);
        void MoveDirectory(string sourceDirectoryPath, string destDirectoryPath);
        void MoveDirectory(string sourceDirectoryPath, string destDirectoryPath, bool overwrite);
        List<string> GetFiles(string path);
        List<string> GetDirectories(string path);
        string GetTemporaryFileName(string fileExtension);
        IArchiveFile Archive();
        long GetFileSize(string path);
        DateTime GetFileDate(string path);
    }
}
