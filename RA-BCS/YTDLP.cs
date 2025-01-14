using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
         * Directory, containing server .exe file must have yt_dlp_path.txt
         * It should have one line only - path to yt_dlp.exe file on your computer
         * Path should not be in "" symbols.
        */
        // TODO: Change this.
        private static string ytdlp_path = "";
        
        /// <summary>
        /// YTDLP constructor withouts parameters. Requires ytdlp_path.txt file with yt_dlp.exe file path.
        /// </summary>
        static YTDLP()
        {
            try
            {
                if (!System.IO.File.Exists("ytdlp_path.txt"))
                {
                    Console.WriteLine("ytdlp_path.txt not found!");
                    System.IO.File.Create("ytdlp_path.txt").Dispose(); // if file doesn't exist - create new ytdlp_path.txt file and immediatly close FileStream (otherwise file will be left open)
                    throw new Exception("File not found. Created an empty file.");
                    // return;
                }
                // TODO: Change this for settings.json later
                ytdlp_path = Convert.ToString(System.IO.File.ReadAllText("ytdlp_path.txt")); // If file exists - grab all lines (ytdlp_path.txt should have 1 line only (token))
                Console.WriteLine("Provied file path: {0}", ytdlp_path);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File not found!\n" + ex.ToString());
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("Directory not found!\n" + ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            if (ytdlp_path == "" || ytdlp_path == null)
            {
                Console.WriteLine("Empty or null token detected!\nExiting...");
                System.Environment.ExitCode = -1;
            }
        }

        // TODO: Change arguments list
        // string url (mandatory), string[] arguments (optional, but can be used more frequently), IProgress<string> progress (optional)
        public async Task StartDownloadAsync(/*string ytdlp_path,*/ string url, /*string[] arguments,*/ IProgress<string> progress = null)
        {
            // string[] args = { "1", "2" };
            Process process = new Process();
            // int args_lenght = arguments.GetLength(0);
            // if (arguments.GetLength(0) == 0) {}

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
