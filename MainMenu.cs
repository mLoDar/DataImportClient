using DataImportClient.Scripts;





namespace DataImportClient
{
    internal class MainMenu
    {
        private const string currentSection = "MainMenu";

        private static int _navigationXPosition = 1;
        private static readonly int _countOfMenuOptions = 5;



        internal static async Task Main()
        {
            ActivityLogger.Log(currentSection, "Entering main menu.");



            Console.Clear();

        LabelDrawUi:

            Console.SetCursorPosition(0, 4);



            ActivityLogger.Log(currentSection, "Starting to draw the main menu.");

            DisplayMenu();

            ActivityLogger.Log(currentSection, "Displayed main menu, waiting for key input.");



            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (_navigationXPosition + 1 <= _countOfMenuOptions)
                    {
                        _navigationXPosition += 1;
                        ActivityLogger.Log(currentSection, $"Changed menu option from '{_navigationXPosition - 1}' to '{_navigationXPosition}'.");
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (_navigationXPosition - 1 >= 1)
                    {
                        _navigationXPosition -= 1;
                        ActivityLogger.Log(currentSection, $"Changed menu option from '{_navigationXPosition + 1}' to '{_navigationXPosition}'.");
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



            ActivityLogger.Log(currentSection, $"Switching to module '{_navigationXPosition}'.");

            switch (_navigationXPosition)
            {
                case 1:
                    // TODO: Call main method of the selected module
                    break;

                case 2:
                    // TODO: Call main method of the selected module
                    break;

                case 3:
                    // TODO: Call main method of the selected module
                    break;

                case 4:
                    // TODO: Call main method of the selected module
                    break;

                case 5:
                    // TODO: Call main method of the selected module
                    break;
            }



            ActivityLogger.Log(currentSection, $"Redrawing main menu after returning from selected module.");

            Console.Clear();
            goto LabelDrawUi;
        }

        private static void DisplayMenu()
        {
            Console.WriteLine("              {0}                                              ", "\u001b[91m┳┓•  •   ┓ \u001b[97m┓┏┏┳┓┓  \u001b[91m┏┓         •   \u001b[97m");
            Console.WriteLine("              {0}                                              ", "\u001b[91m┃┃┓┏┓┓╋┏┓┃ \u001b[97m┣┫ ┃ ┃  \u001b[91m┣ ┏┓┏┓╋┏┓┏┓┓┏┓╋\u001b[97m");
            Console.WriteLine("              {0}                                              ", "\u001b[91m┻┛┗┗┫┗┗┗┻┗ \u001b[97m┛┗ ┻ ┗┛ \u001b[91m┻ ┗┛┗┛┗┣┛┛ ┗┛┗┗\u001b[97m");
            Console.WriteLine("              {0}                                              ", "\u001b[91m    ┛      \u001b[97m        \u001b[91m       ┛       \u001b[97m");
            Console.WriteLine("             ─────────────────────────────────────────         ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Modules                           State         ");
            Console.WriteLine("             └────────────────┐                  ┌───┐         ");
            Console.WriteLine("             {0} Weather                         │ {1}         ", $"[\u001b[91m{(_navigationXPosition == 1 ? ">" : " ")}\u001b[97m]", "  │");
            Console.WriteLine("             {0} Electricity                     │ {1}         ", $"[\u001b[91m{(_navigationXPosition == 2 ? ">" : " ")}\u001b[97m]", "  │");
            Console.WriteLine("             {0} DistrictHeat                    │ {1}         ", $"[\u001b[91m{(_navigationXPosition == 3 ? ">" : " ")}\u001b[97m]", "  │");
            Console.WriteLine("             {0} Photovoltaic                    │ {1}         ", $"[\u001b[91m{(_navigationXPosition == 4 ? ">" : " ")}\u001b[97m]", "  │");
            Console.WriteLine("                                                 └───┘         ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Application                                     ");
            Console.WriteLine("             └────────────────┐                                ");
            Console.WriteLine("             {0} Settings                                      ", $"[\u001b[91m{(_navigationXPosition == 5 ? ">" : " ")}\u001b[97m]");
        }
    }
}