﻿
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BoxVR_Playlist_Manager.Helpers;

namespace BoxVR_Playlist_Manager.FitXr.BeatStructure
{
    public class MadmomProcess
    {
        public static string madmonOutputPath;
        public string[] _resultEntries;
        public string _output;
        private const bool SHOW_WINDOW = false;
        private string processPath;
        private string outputFileName;
        private string progressFileName;

        private string BinaryPath => Path.Combine(Paths.StreamingAssetsPath, @"DBNDownBeatTracker\DBNDownBeatTracker.exe");

        public MadmomProcess()
        {
            madmonOutputPath = Path.Combine(Paths.PersistentDataPath, "Temp");
            this.processPath = this.BinaryPath;
            if(Directory.Exists(MadmomProcess.madmonOutputPath))
            {
                return;
            }
            Directory.CreateDirectory(MadmomProcess.madmonOutputPath);
        }

        private string CreateCommandArgs(string musicPath, string outputPath) => string.Format("--beats_per_bar 4 single \"{0}\" -o \"{1}\" -j 1", (object)musicPath, (object)outputPath);

        public void Apply(string musicPath, Action successAction)
        {
            ProcessMadmom(musicPath, successAction);
        }

        private void ProcessMadmom(string musicPath, Action successAction)
        {
            outputFileName = Path.GetFileNameWithoutExtension(musicPath) + ".madmom.txt";
            progressFileName = Path.GetFileNameWithoutExtension(musicPath) + ".progress.madmom.txt";
            string resultOutPath = Path.Combine(MadmomProcess.madmonOutputPath, outputFileName);
            string progressPath = Path.Combine(MadmomProcess.madmonOutputPath, progressFileName);
            string ffmpegPath = Path.GetDirectoryName(FFmpegQueue.binaryPath);
            string arguments = this.CreateCommandArgs(musicPath, resultOutPath);
            File.Delete(resultOutPath);
            File.Delete(progressPath);
            bool isProcessComplete = false;
            App.logger.Debug(arguments);
            Process process = (Process)null;
            new Thread((ThreadStart)(() =>
            {
                App.logger.Debug("Madmom started");
                process = new Process()
                {
                    StartInfo = {
          FileName = this.processPath,
          UseShellExecute = false,
          EnvironmentVariables = {
            {
              "PROGRESS_PATH",
              progressPath
            }
          }
        }
                };
                process.StartInfo.EnvironmentVariables["PATH"] = process.StartInfo.EnvironmentVariables["PATH"] + ";" + ffmpegPath;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Arguments = arguments;
                isProcessComplete = false;
                process.Start();
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
                process.WaitForExit();
                isProcessComplete = true;
            })).Start();
            App.logger.Debug("Waiting for madmom");
            while(!isProcessComplete){ }
            while(!process.HasExited){ }
            int exitCode = process.ExitCode;
            process.Close();
            App.logger.Debug("Madmom finished : error code= " + (object)exitCode);
            this._resultEntries = new string[0];
            if(File.Exists(resultOutPath))
            {
                this._resultEntries = File.ReadAllLines(resultOutPath);
            }

            if(exitCode == -1 || this._resultEntries.Length == 0)
            {
                throw new System.Exception("Process error");
            }
            else
            {
                successAction.Invoke();
            }
            File.Delete(resultOutPath);
            File.Delete(progressPath);
        }
    }
}
