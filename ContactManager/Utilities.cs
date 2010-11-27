using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ContactManager
{
    public static class StringX
    {
        public static string RightPadIf(this string input)
        {
            if(input == "" || input == null) return "";
            return input + " ";
        }
        public static string LeftPadIf(this string input)
        {
            if(input == "" || input == null) return "";
            return " " + input;
        }
        public static string NewLineAfterIf(this string input)
        {
            if(input == "" || input == null) return "";
            return input + Environment.NewLine;
        }
        public static string NewLineBeforeIf(this string input)
        {
            if(input == "" || input == null) return "";
            return Environment.NewLine + input;
        }
        public static string DefaultIfEmpty(string txt, string def)
        {
            if(txt == "") return def;
            return txt;
        }
    }

    public static class Utilities
    {
        public static IEnumerable<T> EnumerableUnit<T>(T item)
        {
            if(item != null) yield return item;
        }

        public static bool IsBackupFile(string path)
        {
            return Regex.IsMatch(path, @"\d{2}\.\d{2}\.\d{4}@\d{2}\.\d{2}.\d{2}\s*");
        }

        public static string GetBackupFileName(string path)
        {
            if(IsBackupFile(path)) throw new Exception("We don't backup backup files - please check with IsBackupFile before invoking GetBackupFileName");
            var fileName = Path.GetFileNameWithoutExtension(path);
            var rootPath = Path.GetDirectoryName(path);
            var ext = Path.GetExtension(path);
            var date = File.Exists(path) ? File.GetLastWriteTime(path) : DateTime.Now;
            return Path.Combine(rootPath, String.Format("{0:dd.MM.yyyy@HH.mm.ss} {1}{2}", date, fileName, ext));
        }
    }
}
