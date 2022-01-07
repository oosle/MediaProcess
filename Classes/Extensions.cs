using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace MediaProcess
{
    public static class Extensions
    {
        public static String ToTitleCase(this String str)
        {
            TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;
            return textInfo.ToTitleCase(str.ToLower());
        }

        public static Boolean In<String>(this String str, params String[] list)
        {
            return (list.Contains(str));
        }

        public static Boolean ValidPhone(this String str)
        {
            return (Regex.IsMatch(str, @"^0(\d ?){10}$"));
        }

        public static Boolean ValidWebsite(this String str)
        {
            return (Uri.IsWellFormedUriString(str, UriKind.Absolute));
        }

        public static Boolean ValidEmail(this String str)
        {
            try
            {
                var address = new MailAddress(str).Address;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IEnumerable<DirectoryInfo> GetDirectoriesNoException(this DirectoryInfo dir)
        {
            try
            {
                return dir.GetDirectories();
            }
            catch (Exception)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }
        }

        public static IEnumerable<FileInfo> GetFilesNoException(this DirectoryInfo dir)
        {
            try
            {
                return dir.GetFiles();
            }
            catch (Exception)
            {
                return Enumerable.Empty<FileInfo>();
            }
        }
    }
}
