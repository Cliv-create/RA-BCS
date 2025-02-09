﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticAssets;

namespace RA_BCS
{
    internal class YTDLP
    {
        /*
         * YTDLP handling logic here.
         * Will be called by every server instance.
         * All addiitonal arguments will be passed when launching process.
         * Will call SendTextMessage method if available.
         * TODO: Add a proper path getting
         * TODO: Ensure Server classes will have certain methods. (Interface implementation)
         * 
         * Directory, containing server .exe file must have config.json file.
         * yt-dlp_path property should be set to yt_dlp.exe path on your computer. WARNING: Correctly escape "\" charachters in config.json.
         * Path should not be in "" symbols, and "\" symbols escaped properly.
        */
        // TODO: Change this.
        private static string ytdlp_path = "";
        
        /// <summary>
        /// YTDLP constructor withouts parameters. Requires ytdlp_path.txt file with yt_dlp.exe file path.
        /// </summary>
        static YTDLP()
        {
            ytdlp_path = ConfigManager.Get("yt-dlp_path");

            if (ytdlp_path == "" || ytdlp_path == null)
            {
                Console.WriteLine("Empty or null token detected!\nExiting...");
                System.Environment.ExitCode = -1;
            }
        }

        // TODO: Change arguments list
        // string url (mandatory), string[] arguments (optional, but can be used more frequently), IProgress<string> progress (optional)

        /// <summary>
        /// StartDownload (async method). Uses IProgress<string>, by redirecting console output, updates IProgress<string> with latest output. Has default arguments, passed to console application.
        /// </summary>
        /// <param name="url">Video URL to be downloaded.</param>
        /// <param name="progress">IProgress<string>, which will be updated with latest output from console application.</param>
        /// <returns></returns>
        public async Task StartDownload(string url, string additional_arguments = "", IProgress<string> progress = null)
        {
            Process process = new Process();

            // TODO: Uncomment string[] arguments
            // Implement StringBuilder that will build all default arguments + user-defined arguments
            // If same arguments were found, prefer user-defined arguments
            process.StartInfo.FileName = ytdlp_path;
            // Using arguments
            process.StartInfo.Arguments = $"--progress-template \"download:[download] %(progress._percent_str)s of %(progress._total_bytes_str)s. ETA: %(progress._eta_str)s\"" + // Progress template for console output
                                          " --sleep-requests 1.5" + // Sleep between data extraction requests
                                          " --min-sleep-interval 5" + // Sleep before each download
                                          " --max-sleep-interval 10" + // Slepp before each download
                                          " --sleep-subtitles 5" + // Sleep before each subtitle download
                                          // "-o \"%(title)s.%(ext)s\"" + // Output name template
                                          $" {additional_arguments}" +
                                          $" {url}"; // URL from user
            
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            

            // Because of IProgress<string> progress parameter passed, all output from console will be sent there using Report()
            // Data received handler
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    progress?.Report(e.Data); // Sending data to progress instance
                    Console.WriteLine($"\nReceived update: {e.Data}"); // TODO: Remove. Temporary
                }
            };

            // Error received handler
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    progress?.Report(e.Data); // Sending data to progress instance
                    Console.WriteLine($"\nReceived error: {e.Data}"); // TODO: Remove. Temporary
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }

        internal static string Path
        {
            get => ytdlp_path;
        }

        internal static string DownloadPath
        {
            get => ConfigManager.Get("yt-dlp_download_path");
        }

        public string GetDownloadDirectoryPath()
        {
            return ConfigManager.Get("yt-dlp_download_path");
        }

        /* Unused
        /// <summary>
        /// Matches youtube ID link using Regex and returns it.
        /// </summary>
        /// <param name="input_link">Input string for ID check.</param>
        /// <returns>String with matched ID. Returns empty string is nothning was found.</returns>
        public string MatchID(string input_link)
        {
            
            // If checks failed
            return "";
        }
        */
    }
}
