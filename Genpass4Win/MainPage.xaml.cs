using System;
using Windows.ApplicationModel.DataTransfer;
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
        }

        private static ulong PasswordLength = 12;
        private static int CharacterTypesAllowed = 0;

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
            // Get the password length
            PasswordLength = UInt64.Parse(PasswordLengthBox.Text);

            // Ensure OutputBox.PasswordRevealMode is set according to the checkbox
            OutputBox.PasswordRevealMode = !(bool)ShowPassCheckbox.IsChecked ? PasswordRevealMode.Hidden : PasswordRevealMode.Visible;

            // Write the password
            // TODO: Implement the friggin` randomizer
            OutputBox.Password = PasswordLength.ToString();
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
            if (LettersCheckbox == null || NumbersCheckbox == null || SymbolsCheckbox == null) return;
            LettersCheckbox.IsEnabled = !(bool)LettersCheckbox.IsChecked;
            NumbersCheckbox.IsEnabled = !(bool)NumbersCheckbox.IsChecked;
            SymbolsCheckbox.IsEnabled = !(bool)SymbolsCheckbox.IsChecked;
        }
        private void ReleaseChangesToCharTypes()
        {
            if (LettersCheckbox == null || NumbersCheckbox == null || SymbolsCheckbox == null) return;
            LettersCheckbox.IsEnabled = true;
            NumbersCheckbox.IsEnabled = true;
            SymbolsCheckbox.IsEnabled = true;
        }
    }
}
