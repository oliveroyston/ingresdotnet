namespace ConnectionStringGenerator
{
    using System;
    using System.Windows.Forms;
 
    /// <summary>
    /// The main program class for the Ingres Connection String generator.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
