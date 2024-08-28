using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace QuickMD5
{
    class Program
    {
        // Queue to hold progress messages
        private static readonly ConcurrentQueue<string> progressMessages = new ConcurrentQueue<string>();
        // Flag to indicate if processing is complete
        private static bool isProcessingComplete = false;

        [STAThread]
        static void Main(string[] args)
        {
            // Set the console title
            Console.Title = "QuickMD5 - No MD5 File Selected";
            // Print the logo
            PrintLogo();

            // Create the timer to detect how long it took
            Stopwatch stopwatch = Stopwatch.StartNew();

            string md5FilePath;
            string md5FileName;

            // Check if an MD5 file path is provided as an argument
            if (args.Length != 1)
            {
                // Open a file dialog to select an MD5 file
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "MD5 files (*.md5)|*.md5|All files (*.*)|*.*",
                    Title = "Select an MD5 file"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    md5FilePath = openFileDialog.FileName;
                    md5FileName = Path.GetFileName(md5FilePath);
                    Console.Title = $"QuickMD5 - Checking {md5FileName}";
                }
                else
                {
                    // If no file is selected, display an error and exit
                    Console.ForegroundColor = ConsoleColor.Red;
                    CenterText("No file selected. Exiting.");
                    System.Threading.Thread.Sleep(2000);
                    return;
                }
            }
            else
            {
                // If a file path is provided as an argument, use it
                md5FilePath = args[0];
                md5FileName = Path.GetFileName(md5FilePath);
                Console.Title = $"QuickMD5 - Checking {md5FileName}";
            }

            // Read the MD5 file and store the entries in a dictionary
            var md5Dict = ReadMD5File(md5FilePath);
            if (md5Dict.Count == 0)
            {
                CenterText("No valid entries found in the MD5 file. Exiting.");
                Console.ReadLine();
                return;
            }

            GoDownLine();
            int numThreads = GetNumberOfThreads();
            Console.Clear();
            PrintLogo();


            int totalFiles = md5Dict.Count;
            int successfulFiles = 0;
            int corruptedFiles = 0;
            int missingFiles = 0;
            int i = 0;

            // Concurrent collections to store missing and corrupted files
            ConcurrentBag<string> missingFileList = new ConcurrentBag<string>();
            ConcurrentBag<string> corruptedFileList = new ConcurrentBag<string>();

            // Start a task to process the files in parallel
            Task.Run(() =>
            {
                Parallel.ForEach(md5Dict, new ParallelOptions { MaxDegreeOfParallelism = numThreads }, entry =>
                {
                    string filePath = entry.Key;
                    string originalMD5 = entry.Value;

                    // Update the console title with the current file path
                    Console.Title = $"QuickMD5 - Checking {filePath}";

                    string currentMD5 = CalculateMD5(filePath);
                    if (currentMD5 == null)
                    {
                        Interlocked.Increment(ref missingFiles);
                        missingFileList.Add(filePath);
                    }
                    else if (currentMD5 == originalMD5)
                    {
                        Interlocked.Increment(ref successfulFiles);
                    }
                    else
                    {
                        Interlocked.Increment(ref corruptedFiles);
                        corruptedFileList.Add(filePath);
                    }

                    double percentageDone = (Interlocked.Increment(ref i) / (double)totalFiles) * 100;
                    progressMessages.Enqueue($"Checking Files: {percentageDone:F2}%");
                });

                isProcessingComplete = true;
            });

            string lastMessage = string.Empty;
            while (!isProcessingComplete || !progressMessages.IsEmpty)
            {
                while (progressMessages.TryDequeue(out string message))
                {
                    if (message != lastMessage)
                    {
                        Console.SetCursorPosition(0, 0);
                        PrintLogo();
                        GoDownLine();
                        GoDownLine();
                        Console.ForegroundColor = ConsoleColor.White;
                        CenterText(message);
                        CenterText("Sit back. This may take a while.");
                        GoDownLine();
                        CenterText($"[Total Files: {totalFiles}]");
                        Console.ForegroundColor = ConsoleColor.Green;
                        CenterText($"[Successful: {successfulFiles}]");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        CenterText($"[Corrupted: {corruptedFiles}]");
                        Console.ForegroundColor = ConsoleColor.Red;
                        CenterText($"[Missing: {missingFiles}]");
                        Console.ResetColor();
                        lastMessage = message;
                    }
                }

                Thread.Sleep(100);
            }

            if (missingFileList.Count > 0 || corruptedFileList.Count > 0)
            {
                if (missingFileList.Count > 0)
                {
                    GoDownLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    CenterText("Missing files:");
                    GoDownLine();
                    foreach (var file in missingFileList)
                    {
                        CenterText(file);
                    }
                }

                if (corruptedFileList.Count > 0)
                {
                    GoDownLine();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    CenterText("Corrupted files:");
                    GoDownLine();
                    foreach (var file in corruptedFileList)
                    {
                        CenterText(file);
                    }
                }

                GoDownLine();
                Console.ForegroundColor = ConsoleColor.Red;
                CenterText("Check failed. There are missing or corrupted files.");
                CenterText("Press enter or return to exit.");
                Console.ReadLine();
                Environment.Exit(1);
            }
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            string ElapsedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);

            Console.ForegroundColor = ConsoleColor.Green;
            GoDownLine();
            CenterText($"Check complete. {ElapsedTime}");
            Console.ReadLine();
        }

        // Prompt the user to input the number of threads to use then return the value the user entered
        static int GetNumberOfThreads()
        {
            int maxThreads = Environment.ProcessorCount;
            int numThreads;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                CenterText($"Enter the number of threads to use (1-{maxThreads}): ");
                Console.ResetColor();

                int leftPadding = (Console.WindowWidth - $"Enter the number of threads to use (1-{maxThreads}): ".Length) / 2;
                Console.SetCursorPosition(leftPadding + $"Enter the number of threads to use (1-{maxThreads}): ".Length, Console.CursorTop - 1);

                string input = Console.ReadLine();
                if (int.TryParse(input, out numThreads) && numThreads > 0 && numThreads <= maxThreads)
                {
                    break;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                CenterText($"Invalid input. Please enter a number between 1 and {maxThreads}.");
                Console.ResetColor();
            }
            return numThreads;
        }

        // Reads the MD5 file and returns a dictionary with file paths and their corresponding MD5 hashes
        static Dictionary<string, string> ReadMD5File(string md5FilePath)
        {
            var md5Dict = new Dictionary<string, string>();
            foreach (var line in File.ReadLines(md5FilePath))
            {
                var parts = line.Split(new[] { " *" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    md5Dict[parts[1]] = parts[0];
                }
                else
                {
                    CenterText("Line format incorrect.");
                }
            }
            return md5Dict;
        }

        // Calculates the MD5 hash of a file
        static string CalculateMD5(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        // Centers text in the console window
        static void CenterText(string text)
        {
            int windowWidth = Console.WindowWidth;
            int textLength = text.Length;
            int leftPadding = (windowWidth - textLength) / 2;

            Console.SetCursorPosition(leftPadding, Console.CursorTop);
            Console.WriteLine(text);
        }

        // Moves the console cursor down one line
        static void GoDownLine()
        {
            Console.WriteLine();
        }

        // Prints the logo and description
        static void PrintLogo()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            string logopart1 = "   ___       _    _   __  __ ___  ___ ";
            string logopart2 = "  / _ \\ _  _(_)__| |_|  \\/  |   \\| __|";
            string logopart3 = " | (_) | || | / _| / / |\\/| | |) |__ \\";
            string logopart4 = "  \\__\\_\\\\_,_|_\\__|_\\_\\_|  |_|___/|___/";
            string by = "Made with love by: FortNbreak";
            string description = "A fast, free, and open-source MD5 file checker.";
            string url = "https://github.com/FortNbreak/QuickMD5";

            CenterText(logopart1);
            CenterText(logopart2);
            CenterText(logopart3);
            CenterText(logopart4);
            CenterText(by);
            CenterText(description);
            CenterText(url);
        }
    }
}
