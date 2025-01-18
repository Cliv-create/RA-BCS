using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RA_BCS
{
    internal static class FileHandler
    {
        /// <summary>
        /// Async. Gets files from directory using System.IO.Directory.GetFiles.
        /// </summary>
        /// <param name="directory_path">Directory path, from which string[] of files will be returned.</param>
        /// <returns>String array of files in a directory</returns>
        public static async Task<string[]> GetFilesFromDirectory(string directory_path)
        {
            return await Task.Run(() => Directory.GetFiles(directory_path));
        }

        /// <summary>
        /// Async. Moves files from one directory to another. Concatenates destination file names (target_directory + current_file_name). Has two options - int[] array with numbers that will correspond to index in string array of file names in origin_directory. Second option - move_all: true moves all files from origin_directory to target directory, concatenating file names. Has internal try-catch block, that will catch exceptions. If catched, false will get returned, and exception Message property will get printed using Console.WriteLine.
        /// </summary>
        /// <param name="origin_directory">From which directory files will be moved.</param>
        /// <param name="target_directory">To which directory files will be moved.</param>
        /// <param name="files_to_move">Array of integers parameter. Numbers in array will correspond to index of files in target_directory string array, starting from 0. If number will be less then 0, equal or bigger then amount of files in origin_directory, number will be skipped.</param>
        /// <param name="move_all">Bool parameter. If set to true, will move all files from origin_directory to target_directory.</param>
        /// <returns>True when files were moved successfully, false if parameters were not recognized or exception has occured. Uses Console.WriteLine to print exception Message property.</returns>
        public static async Task<bool> MoveFilesToDirectory(string origin_directory, string target_directory, int[] files_to_move = null, bool move_all = false)
        {
            string[] directory_from = await FileHandler.GetFilesFromDirectory(origin_directory);
            return await Task.Run(() =>
            {
                try
                {
                    if (move_all == true)
                    {
                        for (int i = directory_from.Length - 1; i >= 0 ; i--)
                        {
                            string destination_path = Path.Combine(target_directory, Path.GetFileName(directory_from[i]));
                            System.IO.File.Move(directory_from[i], destination_path);
                        }
                        Console.WriteLine("Moved 'move_all' group files successfully.");
                        return true;
                    }
                    if (files_to_move != null)
                    {
                        for (int i = files_to_move.Length - 1; i >= 0 ; i--)
                        {
                            if (files_to_move[i] < 0 || files_to_move[i] >= directory_from.Length)
                            {
                                continue;
                            }
                            string source_file = directory_from[files_to_move[i]];
                            string destination_path = Path.Combine(target_directory, Path.GetFileName(source_file));
                            System.IO.File.Move(source_file, destination_path);
                        }
                        Console.WriteLine("Moved 'files_to_move' (array) group files successfully.");
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when moving files: {ex.Message}");
                    return false;
                }
            });
        }
    }
}
