using System.ComponentModel;
using System.Diagnostics;

namespace Spa_Interaction_Screen
{
    internal static class Program
    {
        public static bool runningParent = true;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            ApplicationConfiguration.Initialize();

#if !DEBUG
            Application.Run(new MainForm());
#else
            while (runningParent)
            {
                try
                {
                    Application.Run(new MainForm());
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
#endif
        }
    }
}