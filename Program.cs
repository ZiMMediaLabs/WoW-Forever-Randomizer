using System;
using System.Windows.Forms;
using WoWForeverRandomizer.Forms;

namespace WoWForeverRandomizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(Navigator.Start(new MainForm()));
        }
    }

    // Switches screens in place. Closing the current screen exits the app.
    public static class Navigator
    {
        private static ApplicationContext _context;

        public static ApplicationContext Start(Form firstForm)
        {
            _context = new ApplicationContext(firstForm);
            return _context;
        }

        public static void GoTo(Form currentForm, Form nextForm)
        {
            nextForm.StartPosition = FormStartPosition.Manual;
            nextForm.Location = currentForm.Location;

            // Set the new main form before closing the old one so the app doesn't exit.
            _context.MainForm = nextForm;
            nextForm.Show();
            currentForm.Close();
        }
    }
}
