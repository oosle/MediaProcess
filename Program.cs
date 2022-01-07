using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;

namespace MediaProcess
{
    static class Program
    {
        static StringBuilder log = new StringBuilder();
        static String fileFilter = "";
        static Boolean mp3Mode = false;
        static Boolean metaMode = false;
        static Int16 cmdString = 1;
        static Int16 maxTasks = 1;
        static String audioRate = "128k";
        static String videoRate = "1200k";
        static String videoRes = "640x480";
        static String exParams = string.Empty;
        static readonly BlockingCollection<string> files = new BlockingCollection<string>();
        static Int32 fileCount = 0;

        static DateTime startTime = DateTime.Now;
        static DateTime endTime = DateTime.Now;
        static TimeSpan totalElapsed = startTime - endTime;

        static void LogComplete()
        {
            log.AppendLine("");
            log.AppendLine(String.Format("Media Files(s): {0}", fileCount));
            Console.WriteLine("Video Files(s): {0}", fileCount);
            log.AppendLine(String.Format("Elapsed Time  : {0}", totalElapsed));
            Console.WriteLine("Elapsed Time  : {0}", totalElapsed);
            Console.WriteLine("");
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate
            {
                endTime = DateTime.Now;
                totalElapsed = endTime - startTime;

                Console.WriteLine("--> TERMINATED");
                LogComplete();
                File.WriteAllText(Utility.sLogFile, log.ToString());
            };

            try
            {
                if (args.Count() > 0 && Utility.bCheckBinaryInstalled)
                {
                    fileFilter = args[args.Count() - 1];

                    if (fileFilter != "")
                    {
                        String[] values = null;

                        foreach (String arg in args)
                        {
                            if (arg.ToUpper() == "-MP3")
                                mp3Mode = true;
                            if (arg.ToUpper() == "-META")
                                metaMode = true;
                            else
                            {
                                values = arg.Split('=');

                                if (values[0].ToUpper() == "-C")
                                    cmdString = Convert.ToInt16(values[1]);
                                else if (values[0].ToUpper() == "-T")
                                    maxTasks = Convert.ToInt16(values[1]);
                                else if (values[0].ToUpper() == "-A")
                                    audioRate = values[1];
                                else if (values[0].ToUpper() == "-V")
                                    videoRate = values[1];
                                else if (values[0].ToUpper() == "-R")
                                    videoRes = values[1];
                                else if (cmdString == 2)
                                    exParams += arg + " ";
                            }
                        }

                        // Strip out the file path from the extra user defined FFMPEG parameters
                        if (exParams.IndexOf(fileFilter) > -1)
                        {
                            int idx = exParams.IndexOf(fileFilter);
                            exParams = exParams.Substring(0, idx - 1).Trim();
                        }

                        Console.WriteLine("");
                        log.AppendLine(String.Format("{0}: Media proceesing utility.", Utility.sTitleVersion));
                        Console.WriteLine("{0}: Media proceesing utility.", Utility.sTitleVersion);

                        if (!String.IsNullOrWhiteSpace(exParams))
                        {
                            log.AppendLine(String.Format("FFMPEG Extra Commands: {0}", exParams));
                            Console.WriteLine("FFMPEG Extra Commands: {0}", exParams);
                        }

                        Console.WriteLine("Building file tree, please wait...");

                        // Build recursive list of files to check for
                        var tasks = new List<Task>
                        {
                            Task.Factory.StartNew(() =>
                            {
                                ProcessTree(fileFilter);
                                files.CompleteAdding();
                            }, TaskCreationOptions.LongRunning),
                        };

                        // Start asynchronously processing video file
                        for (var i = 0; i < maxTasks; i++)
                        {
                            tasks.Add(Task.Factory.StartNew(Process(i), TaskCreationOptions.LongRunning));
                        }
                        Task.WaitAll(tasks.ToArray());

                        endTime = DateTime.Now;
                        totalElapsed = endTime - startTime;

                        if (fileCount > 0)
                        {
                            Console.WriteLine("");
                            LogComplete();
                            File.WriteAllText(Utility.sLogFile, log.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("{0}: Media proceesing utility.", Utility.sTitleVersion);
                        Console.WriteLine("Error: No file filter specified, please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("{0}: Media proceesing utility.", Utility.sTitleVersion);
                    Console.WriteLine("");
                    Console.WriteLine("Note: Matt's FFMPEG CLI simplification utility, processes MP3 audio and MP4 video.");
                    Console.WriteLine("Note: When using MP3 transcoding option this overwrites original, so have a backup handy!");
                    Console.WriteLine("Note: The META mode strips all metadata from the MP4 file, useful for torrent downloads!");
                    Console.WriteLine("Note: Iteratively processes tree of media files using FFMPEG utility, get latest version!");
                    Console.WriteLine("");
                    Console.WriteLine("Supported video formats = {0}", String.Join(",", Utility.sVideoFormats));
                    Console.WriteLine("");
                    Console.WriteLine("Syntax: {0} <options> <archive>", Utility.sTitle);
                    Console.WriteLine("");
                    Console.WriteLine("Example: {0} -c=1 -t=3 -a=128k -v=2500k -r=1280x720 C:\\Media", Utility.sTitle);
                    Console.WriteLine("         {0} -c=2 -t=2 -a=128k -v=2500k -r=1280x720 -map 0:v -map 1:a C:\\Media", Utility.sTitle);
                    Console.WriteLine("         {0} -mp3 -t=3 C:\\Media", Utility.sTitle);
                    Console.WriteLine("         {0} -meta -t=3 C:\\Media", Utility.sTitle);
                    Console.WriteLine("");
                    Console.WriteLine("Default: c=1 t={0} a={1} v={2} r={3}", maxTasks, audioRate, videoRate, videoRes);
                    Console.WriteLine("");
                    Console.WriteLine("Options: -mp3   = Enable MP3 transcoding option.");
                    Console.WriteLine("         -meta  = Enable MP4 metadata file stripping option.");
                    Console.WriteLine("         -c=<#> = Video command pattern to use.");
                    Console.WriteLine("         -t=<#> = Number of threads to spawn.");
                    Console.WriteLine("         -a=<#> = Audio bitrate to encode.");
                    Console.WriteLine("         -v=<#> = Video bitrate to encode.");
                    Console.WriteLine("         -r=<#> = Video resolution to encode.");
                    Console.WriteLine("");

                    // Check that the optimization binary exists (FFMPEG)
                    if (Utility.bCheckBinaryInstalled)
                        Console.WriteLine("FFMPEG installed.");
                    else
                        Console.WriteLine("FFMPEG not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("{0}: Media proceesing utility.", Utility.sTitleVersion);
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        #region -- Publisher code --

        // Recursively build a file list for processing from passed directory item
        private static void ProcessDirectory(DirectoryInfo directory)
        {
            foreach (var dir in directory.GetDirectoriesNoException())
            {
                ProcessDirectory(dir);
            }

            foreach (var file in directory.GetFilesNoException())
            {
                if (Utility.CheckFileTypes(file.Extension))
                {
                    files.Add(file.FullName);
                }
            }
        }

        // Recursively build a file list for processing of file archive for asynchronous operations
        private static void ProcessTree(String path)
        {
            var root = new DirectoryInfo(path);

            foreach (var dir in root.GetDirectoriesNoException())
            {
                ProcessDirectory(dir);
            }

            foreach (var file in root.GetFilesNoException())
            {
                if (Utility.CheckFileTypes(file.Extension))
                {
                    files.Add(file.FullName);
                }
            }
        }

        #endregion

        #region -- Consumer code --

        public static void ConvertMedia(String file)
        {
            try
            {
                String mediaCommand = Utility.sMediaBinary;
                String inFile = String.Empty;
                String outFile = String.Empty;
                String subFile = String.Empty;
                String cmdLine = String.Empty;

                if (mp3Mode)
                {
                    inFile = "\"" + file + "\"";
                    outFile = "\"" + file + ".new.mp3\"";
                    cmdLine = String.Format(Utility.sAudioParams, inFile, outFile);
                }
                else if (metaMode)
                {
                    inFile = "\"" + file + "\"";
                    outFile = "\"" + file + ".new.mp4\"";
                    cmdLine = String.Format(Utility.sMetaParams, inFile, outFile);
                }
                else
                {
                    inFile = "\"" + file + "\"";
                    outFile = "\"" + file + ".new.mp4\"";

                    if (cmdString == 2)
                        cmdLine = String.Format(Utility.sVideoParamsTwo, inFile, audioRate, videoRate, videoRes, exParams, outFile);
                    else
                        cmdLine = String.Format(Utility.sVideoParamsOne, inFile, audioRate, videoRate, videoRes, outFile);
                }

                using (Process p = new Process())
                {
                    try
                    {
                        p.StartInfo.FileName = mediaCommand;
                        p.StartInfo.Arguments = cmdLine;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = false;
                        p.Start();
                        p.WaitForExit();
                    }
                    finally
                    {
                        // Test the return code from the spawned process, FFMPEG application
                        if (p.ExitCode != 0)
                        {
                            log.AppendLine(String.Format("File: {0} -> Error code {1}.", file, p.ExitCode));
                            Console.WriteLine("File: {0} -> Error code {1}.", file, p.ExitCode);
                        }
                        else
                        {
                            fileCount++;

                            inFile = inFile.Trim(new Char[] { '"' });
                            outFile = outFile.Trim(new Char[] { '"' });

                            if (mp3Mode && File.Exists(outFile))
                            {
                                File.Delete(inFile);
                                File.Move(outFile, inFile);
                            }

                            log.AppendLine(String.Format("File: {0} -> Processed.", file));
                            Console.WriteLine("File: {0} -> Processed.", file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.AppendLine(String.Format("File: {0} -> Error: {1}", file, e.Message));
                Console.WriteLine("File: {0} -> Error: {1}", file, e.Message);
            }
        }

        static Action Process(Int32 id)
        {
            // Return a closure just so the id can get passed
            return () =>
            {
                String file;

                while (true)
                {
                    if (files.TryTake(out file, -1))
                    {
                        if (File.Exists(file))
                        {
                            ConvertMedia(file);
                        }
                    }
                    else if (files.IsAddingCompleted)
                    {
                        break; // Exit loop
                    }
                }
            };
        }

        #endregion
    }
}

