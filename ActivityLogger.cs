﻿namespace DataImportClient
{
    internal class ActivityLogger
    {
        private static readonly ApplicationSettings.Runtime appRuntime = new();





        internal static void Log(string currentSection, string message, bool removePrefix = false)
        {
            try
            {
                string clientFolder = appRuntime.pathClientFolder;
                string logFile = appRuntime.pathLogFile;

                if (Directory.Exists(clientFolder) == false)
                {
                    Directory.CreateDirectory(clientFolder);
                }

                string prefix = $"[{DateTime.Now}] - [ProcessId: {appRuntime.processId}] - [Section: {currentSection}] - ";

                if (removePrefix == true)
                {
                    File.AppendAllText(logFile, $"{new string(' ', prefix.Length)}{message}\r\n");
                    return;
                }

                File.AppendAllText(logFile, $"{prefix}{message}\r\n");
            }
            catch
            {
                Console.Clear();
                Console.SetCursorPosition(0, 4);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("             WARNING\r\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             Failed to create crucial directorys/files.");
                Console.WriteLine("             Please read the manual on how to fix this error!");

                Thread.Sleep(10000);

                Environment.Exit(0);
            }
        }
    }
}