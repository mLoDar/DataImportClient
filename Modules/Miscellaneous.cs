using System.Diagnostics;

using DataImportClient.Scripts;
using DataImportClient.Ressources;





namespace DataImportClient.Modules
{
    class Miscellaneous
    {
        private const string _currentSection = "Miscellaneous";

        private static int _navigationXPosition = 1;
        private static readonly int _countOfMenuOptions = 6;

        private static readonly ApplicationSettings.Paths _appPaths = new();

        internal ErrorCache errorCache = new();



        internal async Task Main()
        {
            ActivityLogger.Log(_currentSection, "Entering the section 'Miscellaneous'.");

            Console.Clear();



        LabelDrawUi:

            Console.SetCursorPosition(0, 4);



            ActivityLogger.Log(_currentSection, "Starting to draw the main menu.");

            DisplayMenu();

            ActivityLogger.Log(_currentSection, "Displayed main menu, waiting for key input.");



            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (_navigationXPosition + 1 <= _countOfMenuOptions)
                    {
                        _navigationXPosition += 1;
                        ActivityLogger.Log(_currentSection, $"Changed menu option from '{_navigationXPosition - 1}' to '{_navigationXPosition}'.");
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (_navigationXPosition - 1 >= 1)
                    {
                        _navigationXPosition -= 1;
                        ActivityLogger.Log(_currentSection, $"Changed menu option from '{_navigationXPosition + 1}' to '{_navigationXPosition}'.");
                    }
                    break;

                case ConsoleKey.Backspace:
                    return;

                case ConsoleKey.Escape:
                    return;

                default:
                    break;
            }



            if (pressedKey != ConsoleKey.Enter)
            {
                goto LabelDrawUi;
            }



            switch (_navigationXPosition)
            {
                case 1:
                    try
                    {
                        string configurationFile = _appPaths.configurationFile;
                        Process.Start("notepad.exe", configurationFile);

                        ActivityLogger.Log(_currentSection, "Opened the configuration file of the application.");
                    }
                    catch (Exception exception)
                    {
                        ActivityLogger.Log(_currentSection, "[ERROR] Failed to open the configuration file of the application.");
                        ActivityLogger.Log(_currentSection, exception.Message, true);

                        string title = "Failed to perform this action.";
                        string description = "Please check the error log for detailed information.";

                        await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);
                    }
                    break;

                case 2:
                    bool currentState = MainMenu.EmailAlerts;
                    MainMenu.EmailAlerts = !currentState;

                    goto LabelDrawUi;

                case 3:
                    ActivityLogger.Log(_currentSection, "Opening a minimalistic error cache view.");

                    errorCache.DisplayMinimalistic();
                    break;

                case 4:
                    ActivityLogger.Log(_currentSection, "Opening a detailed error cache view.");

                    await errorCache.DisplayDetailed();
                    break;

                case 5:
                    try
                    {
                        string logsFolder = _appPaths.logsFolder;
                        Process.Start("explorer.exe", logsFolder);

                        ActivityLogger.Log(_currentSection, "Opened the configuration folder of the applications log files.");
                    }
                    catch (Exception exception)
                    {
                        ActivityLogger.Log(_currentSection, "[ERROR] Failed to open the folder of the applications log files.");
                        ActivityLogger.Log(_currentSection, exception.Message, true);

                        Console.Clear();

                        Console.SetCursorPosition(0, 4);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("             [ERROR] Failed to perform this action.              ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("                                                                 ");
                        Console.WriteLine("             Please check the error log for detailed information.");

                        await Task.Delay(3000);
                    }
                    break;

                case 6:
                    ActivityLogger.Log(_currentSection, "Returning to the main menu.");
                    return;
            }



            Console.Clear();
            goto LabelDrawUi;
        }

        private static void DisplayMenu()
        {
            string formattedEmailAlerts = GetFormattedEmailAlertState();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("              ┳┳┓•    ┓┓                                       ");
            Console.WriteLine("              ┃┃┃┓┏┏┏┓┃┃┏┓┏┓┏┓┏┓┓┏┏                            ");
            Console.WriteLine("              ┛ ┗┗┛┗┗ ┗┗┗┻┛┗┗ ┗┛┗┻┛                            ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ─────────────────────────                         ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Adjust settings                  State          ");
            Console.WriteLine("             └────────────────────────────┐     ┌───┐          ");
            Console.WriteLine("             {0} Open configuration file        │   │          ", $"[\u001b[91m{(_navigationXPosition == 1 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("             {0} Email alerts                   │{1}│          ", $"[\u001b[91m{(_navigationXPosition == 2 ? ">" : " ")}\u001b[97m]", formattedEmailAlerts);
            Console.WriteLine("                                                └───┘          ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Error handling                                  ");
            Console.WriteLine("             └────────────────────────────┐                    ");
            Console.WriteLine("             {0} Minimalistic error cache                      ", $"[\u001b[91m{(_navigationXPosition == 3 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("             {0} Detailed error cache                          ", $"[\u001b[91m{(_navigationXPosition == 4 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("             {0} Open log files                                ", $"[\u001b[91m{(_navigationXPosition == 5 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Application                                     ");
            Console.WriteLine("             └────────────────────────────┐                    ");
            Console.WriteLine("             {0} MainMenu                                      ", $"[\u001b[91m{(_navigationXPosition == 6 ? ">" : " ")}\u001b[97m]");
        }

        private static string GetFormattedEmailAlertState()
        {
            bool featureState = MainMenu.EmailAlerts;

            return featureState == true ? "\x1B[92m √ \x1B[97m" : "\x1B[91m x \x1B[97m";
        }
    }
}