using System;
using System.Security.Cryptography;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Genpass4Win
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Configure the window
            ApplicationView.PreferredLaunchViewSize = new Size(520, 240);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(370, 220));

            InitializationDone = true;
        }

        // Variables used during operation
        bool InitializationDone = false;
        private static ulong PasswordLength = 12;
        private static int CharacterTypesAllowed = 0;
        readonly private static RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        // Handlers for changing the password length
        private void DecreasePasswordLength_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PasswordLength--;
            PasswordLengthBox.Text = PasswordLength.ToString();
        }
        private void IncreasePasswordLength_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PasswordLength++;
            PasswordLengthBox.Text = PasswordLength.ToString();
        }
        // Handlers for generating, copying and viewing the password
        private void GenPassButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ulong PasswordLengthOld = PasswordLength;
            // Get the password length
            try
            {
                PasswordLength = UInt64.Parse(PasswordLengthBox.Text);
            }
            catch (Exception)
            {
                PasswordLength = PasswordLengthOld;
                PasswordLengthBox.Text = PasswordLengthOld.ToString();
            }

            // Ensure OutputBox.PasswordRevealMode is set according to the checkbox
            OutputBox.PasswordRevealMode = !(bool)ShowPassCheckbox.IsChecked ? PasswordRevealMode.Hidden : PasswordRevealMode.Visible;

            // Get the allowed character types
            bool UseLetters = (bool)LettersCheckbox.IsChecked;
            bool UseNumbers = (bool)NumbersCheckbox.IsChecked;
            bool UseSymbols = (bool)SymbolsCheckbox.IsChecked;

            // Write the password
            string GeneratedPassword = "";
            byte[] GeneratedByte = { 0 };
            for (ulong i = 0UL; i < PasswordLength; i++)
            {
                rnd.GetBytes(GeneratedByte);
                GeneratedByte[0] %= 94;
                GeneratedByte[0] += 32;
                while (!ValidateChar((char)GeneratedByte[0], UseLetters, UseNumbers, UseSymbols))
                    rnd.GetBytes(GeneratedByte);
                GeneratedPassword += (char)GeneratedByte[0];
            }
            OutputBox.Password = GeneratedPassword;
        }
        private bool ValidateChar(in int chr, in bool UseLetters, in bool UseNumbers, in bool UseSymbols)
        {
            bool Accepted = false;
            // Test Letters
            if (((64 < chr && chr < 91) || (96 < chr && chr < 123))
                && UseLetters == true)
                Accepted = true;
            // Test Numbers
            if ((47 < chr && chr < 58)
                && UseNumbers == true)
                Accepted = true;
            // Test symbols
            if (((32 < chr && chr < 48) || (57 < chr && chr < 65) || (90 < chr && chr < 97) || (122 < chr && chr < 127))
                && UseSymbols == true)
                Accepted = true;
            return Accepted;
        }
        private void ClipPassButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Send the password to the clipboard
            var DataPackage = new DataPackage();
            DataPackage.SetText(OutputBox.Password);
            Clipboard.SetContent(DataPackage);
        }
        private void ShowPassCheckbox_Changed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Ensure OutputBox.PasswordRevealMode is set according to the checkbox
            OutputBox.PasswordRevealMode = !(bool)ShowPassCheckbox.IsChecked ? PasswordRevealMode.Hidden : PasswordRevealMode.Visible;
        }

        // Handlers for controlling character types
        private void LettersCheckbox_Changed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CharacterTypesAllowed += (bool)LettersCheckbox.IsChecked ? 1 : -1;
            if (CharacterTypesAllowed == 1)
                StopChangesToCharTypes();
            else
                ReleaseChangesToCharTypes();
        }
        private void NumbersCheckbox_Changed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CharacterTypesAllowed += (bool)NumbersCheckbox.IsChecked ? 1 : -1;
            if (CharacterTypesAllowed == 1)
                StopChangesToCharTypes();
            else
                ReleaseChangesToCharTypes();
        }
        private void SymbolsCheckbox_Changed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CharacterTypesAllowed += (bool)SymbolsCheckbox.IsChecked ? 1 : -1;
            if (CharacterTypesAllowed == 1)
                StopChangesToCharTypes();
            else
                ReleaseChangesToCharTypes();
        }

        private void StopChangesToCharTypes()
        {
            if (!InitializationDone) return;
            LettersCheckbox.IsEnabled = !(bool)LettersCheckbox.IsChecked;
            NumbersCheckbox.IsEnabled = !(bool)NumbersCheckbox.IsChecked;
            SymbolsCheckbox.IsEnabled = !(bool)SymbolsCheckbox.IsChecked;
        }
        private void ReleaseChangesToCharTypes()
        {
            if (!InitializationDone) return;
            LettersCheckbox.IsEnabled = true;
            NumbersCheckbox.IsEnabled = true;
            SymbolsCheckbox.IsEnabled = true;
        }
    }
}
