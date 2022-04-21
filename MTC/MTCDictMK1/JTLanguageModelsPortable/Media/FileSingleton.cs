using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.MediaInterfaces;

namespace JTLanguageModelsPortable.Media
{
    public static class FileSingleton
    {
        public static void AppendAllText(string path, string contents)
        {
            PortableFile.Singleton.AppendAllText(path, contents);
        }

        public static void AppendAllText(string path, string contents, Encoding encoding)
        {
            PortableFile.Singleton.AppendAllText(path, contents, encoding);
        }

        public static StreamWriter AppendText(string path)
        {
            return PortableFile.Singleton.AppendText(path);
        }

        public static void Copy(string sourceFileName, string destFileName)
        {
            PortableFile.Singleton.Copy(sourceFileName, destFileName);
        }

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            PortableFile.Singleton.Copy(sourceFileName, destFileName, overwrite);
        }

        public static Stream Create(string path)
        {
            return PortableFile.Singleton.Create(path);
        }

        public static StreamWriter CreateText(string path)
        {
            return PortableFile.Singleton.CreateText(path);
        }

        public static void Delete(string path)
        {
            PortableFile.Singleton.Delete(path);
        }

        public static bool Exists(string path)
        {
            return PortableFile.Singleton.Exists(path);
        }

        public static long FileSize(string path)
        {
            return PortableFile.Singleton.FileSize(path);
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            PortableFile.Singleton.Move(sourceFileName, destFileName);
        }

        public static Stream Open(string path, PortableFileMode mode)
        {
            return PortableFile.Singleton.Open(path, mode);
        }

        public static Stream Open(string path, PortableFileMode mode, PortableFileAccess access)
        {
            return PortableFile.Singleton.Open(path, mode, access);
        }

        public static Stream OpenRead(string path)
        {
            return PortableFile.Singleton.OpenRead(path);
        }

        public static StreamReader OpenText(string path)
        {
            return PortableFile.Singleton.OpenText(path);
        }

        public static StreamReader OpenText(string path, Encoding encoding)
        {
            return PortableFile.Singleton.OpenText(path, encoding);
        }

        public static Stream OpenWrite(string path)
        {
            return PortableFile.Singleton.OpenWrite(path);
        }

        public static byte[] ReadAllBytes(string path)
        {
            return PortableFile.Singleton.ReadAllBytes(path);
        }

        public static string ReadAllText(string path)
        {
            return PortableFile.Singleton.ReadAllText(path);
        }

        public static string ReadAllText(string path, Encoding encoding)
        {
            return PortableFile.Singleton.ReadAllText(path, encoding);
        }

        public static string[] ReadAllLines(string path)
        {
            return PortableFile.Singleton.ReadAllLines(path);
        }

        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            return PortableFile.Singleton.ReadAllLines(path, encoding);
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            PortableFile.Singleton.WriteAllBytes(path, bytes);
        }

        public static void WriteAllText(string path, string contents)
        {
            PortableFile.Singleton.WriteAllText(path, contents);
        }

        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            PortableFile.Singleton.WriteAllText(path, contents, encoding);
        }

        public static void WriteAllLines(string path, string[] contents)
        {
            PortableFile.Singleton.WriteAllLines(path, contents);
        }

        public static void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            PortableFile.Singleton.WriteAllLines(path, contents, encoding);
        }

        public static void Close(Stream stream)
        {
            PortableFile.Singleton.Close(stream);
        }

        public static void Close(StreamReader streamReader)
        {
            PortableFile.Singleton.Close(streamReader);
        }

        public static void Close(StreamWriter streamWriter)
        {
            PortableFile.Singleton.Close(streamWriter);
        }

        public static bool DirectoryExistsCheck(string path)
        {
            return PortableFile.Singleton.DirectoryExistsCheck(path);
        }

        public static bool DirectoryExists(string path)
        {
            return PortableFile.Singleton.DirectoryExists(path);
        }

        public static void CreateDirectory(string path)
        {
            PortableFile.Singleton.CreateDirectory(path);
        }

        public static void RenameDirectory(string oldPath, string newPath)
        {
            PortableFile.Singleton.RenameDirectory(oldPath, newPath);
        }

        public static void DeleteDirectory(string path)
        {
            PortableFile.Singleton.DeleteDirectory(path);
        }

        public static bool DeleteEmptyDirectory(string path)
        {
            return PortableFile.Singleton.DeleteEmptyDirectory(path);
        }

        public static void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath)
        {
            PortableFile.Singleton.CopyDirectory(sourceDirectoryPath, destDirectoryPath);
        }

        public static void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath, bool overwrite)
        {
            PortableFile.Singleton.CopyDirectory(sourceDirectoryPath, destDirectoryPath, overwrite);
        }

        public static void MoveDirectory(string sourceDirectoryPath, string destDirectoryPath)
        {
            PortableFile.Singleton.MoveDirectory(sourceDirectoryPath, destDirectoryPath);
        }

        public static void MoveDirectory(string sourceDirectoryPath, string destDirectoryPath, bool overwrite)
        {
            PortableFile.Singleton.MoveDirectory(sourceDirectoryPath, destDirectoryPath, overwrite);
        }

        public static string GetTemporaryFileName(string fileExtension)
        {
            return PortableFile.Singleton.GetTemporaryFileName(fileExtension);
        }

        public static List<string> GetFiles(string path)
        {
            return PortableFile.Singleton.GetFiles(path);
        }

        public static List<string> GetDirectories(string path)
        {
            return PortableFile.Singleton.GetDirectories(path);
        }

        public static IArchiveFile Archive()
        {
            return PortableFile.Singleton.Archive();
        }

        public static long GetFileSize(string path)
        {
            return PortableFile.Singleton.GetFileSize(path);
        }

        public static DateTime GetFileDate(string path)
        {
            return PortableFile.Singleton.GetFileDate(path);
        }

        public static long GetDirectorySize(string path, List<string> extensions, bool recurse)
        {
            return PortableFile.Singleton.GetDirectorySize(path, extensions, recurse);
        }
    }
}
