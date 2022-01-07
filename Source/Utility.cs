using System;
using System.IO;
using System.Reflection;

namespace MediaProcess
{
    public static class Utility
    {
        #region -- Public properties (mainly configuration) --

        public static String[] sVideoFormats = {"MP4","WMV","AVI","MPEG","FLC","FLV","MOV","MKV","MPG","WEBM","DIVX","VOB","M4V","MP3"};
        public static String sAudioParams = @"-y -i {0} -id3v2_version 3 -write_id3v1 1 -codec:a libmp3lame -qscale:a 2 -ar 44100 -ac 2 -af ""volume=6dB"" {1}";
        public static String sMetaParams = @"-i {0} -map 0 -map_metadata -1 -c copy {1}";
        public static String sVideoParamsOne = @"-i {0} -acodec mp3 -b:a {1} -map 0:v -map 0:a -ar 48000 -af ""volume=1.5"" -movflags +faststart -vcodec mpeg4 -b:v {2} -s {3} {4}";
        public static String sVideoParamsTwo = @"-i {0} {4} -acodec mp3 -b:a {1} -ar 48000 -af ""volume=1.5"" -movflags +faststart -vcodec mpeg4 -b:v {2} -s {3} {5}";
        public static String sLogFile = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase) + ".log";

        public static String sTitleVersion
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];

                    if (titleAttribute.Title != "")
                    {
                        String sVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                        return (titleAttribute.Title + " v" + sVersion);
                    }
                }
                return (Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase));
            }
        }

        public static String sTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];

                    if (titleAttribute.Title != "")
                    {
                        return (titleAttribute.Title);
                    }
                }
                return (Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase));
            }
        }

        public static String sAppStartPath
        {
            get
            {
                return (Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
            }
        }

        public static String sMediaBinary
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);

                return (Path.GetDirectoryName(path) + @"\ffmpeg.exe");
            }
        }

        public static Boolean bCheckBinaryInstalled
        {
            get
            {
                return (File.Exists(sMediaBinary));
            }
        }

        #endregion

        #region -- Public methods --

        public static Boolean CheckFileTypes(String ext)
        {
            String fileType = ext.ToUpper();
            Int32 period = fileType.IndexOf(".");

            if (period > -1)
            {
                fileType = fileType.Substring(period + 1);
            }

            return (fileType.In(sVideoFormats));
        }

        public static void Log(String msg)
        {
            StreamWriter log = null;

            if (!File.Exists(sLogFile))
            {
                log = new StreamWriter(sLogFile);
            }
            else
            {
                log = File.AppendText(sLogFile);
            }

            log.WriteLine(msg);
            log.Close();
        }

        #endregion
    }
}
