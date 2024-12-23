using DataImportClient.Scripts;





namespace DataImportClient
{
    class Miscellaneous
    {
        private const string _currentSection = "Miscellaneous";

        private static int _navigationXPosition = 1;
        private static readonly int _countOfMenuOptions = 5;



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
                    // TODO: Open the applications configuration file
                    break;

                case 2:
                    // TODO: Show a minimalistic error cache within the console
                    break;

                case 3:
                    // TODO: Show a detailed error cache in a text file
                    break;

                case 4:
                    // TODO: Open folder for of the applications log files
                    break;

                case 5:
                    ActivityLogger.Log(_currentSection, "Returning to the main menu.");
                    return;
            }



            Console.Clear();
            goto LabelDrawUi;
        }

        private static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("              ┳┳┓•    ┓┓                                       ");
            Console.WriteLine("              ┃┃┃┓┏┏┏┓┃┃┏┓┏┓┏┓┏┓┓┏┏                            ");
            Console.WriteLine("              ┛ ┗┗┛┗┗ ┗┗┗┻┛┗┗ ┗┛┗┻┛                            ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ─────────────────────────                         ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Adjust settings                                 ");
            Console.WriteLine("             └────────────────────────────┐                    ");
            Console.WriteLine("             {0} Open configuration file                       ", $"[\u001b[91m{(_navigationXPosition == 1 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Error handling                                  ");
            Console.WriteLine("             └────────────────────────────┐                    ");
            Console.WriteLine("             {0} Minimalistic error cache                      ", $"[\u001b[91m{(_navigationXPosition == 2 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("             {0} Detailed error cache                          ", $"[\u001b[91m{(_navigationXPosition == 3 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("             {0} Open log files                                ", $"[\u001b[91m{(_navigationXPosition == 4 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Application                                     ");
            Console.WriteLine("             └────────────────────────────┐                    ");
            Console.WriteLine("             {0} MainMenu                                      ", $"[\u001b[91m{(_navigationXPosition == 5 ? ">" : " ")}\u001b[97m]");
        }
    }
}