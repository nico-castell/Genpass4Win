using System;
using System.Security.Cryptography;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

// MIT License - Copyright (c) 2021 Nicolás Castellán
// SPDX License identifier: MIT
// THE SOFTWARE IS PROVIDED "AS IS"
// Read the included LICENSE file for more information

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
		private readonly bool InitializationDone = false;
		private int PasswordLength = 12, PasswordLengthOld = 12;
		private readonly int MaxPasswordLength = 10000;
		private int CharacterTypesAllowed = 0;
		private readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		// Handlers for changing the password length
		private void DecreasePasswordLength_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			PasswordLengthOld = PasswordLength;
			try
			{
				PasswordLength = Int32.Parse(PasswordLengthBox.Text);
			}
			catch (Exception)
			{
				PasswordLength = PasswordLengthOld;
			}
			if (1 >= PasswordLength)
				PasswordLength = 1;
			else if (PasswordLength > MaxPasswordLength)
				PasswordLength = MaxPasswordLength;
			else
				PasswordLength--;
			PasswordLengthBox.Text = PasswordLength.ToString();
		}
		private void IncreasePasswordLength_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			PasswordLengthOld = PasswordLength;
			try
			{
				PasswordLength = Int32.Parse(PasswordLengthBox.Text);
			}
			catch (Exception)
			{
				PasswordLength = PasswordLengthOld;
			}
			if (1 > PasswordLength)
				PasswordLength = 1;
			else if (PasswordLength >= MaxPasswordLength)
				PasswordLength = MaxPasswordLength;
			else
				PasswordLength++;
			PasswordLengthBox.Text = PasswordLength.ToString();
		}
		// Handlers for generating, copying and viewing the password
		private void GenPassButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			PasswordLengthOld = PasswordLength;
			// Get the password length
			try
			{
				PasswordLength = Int32.Parse(PasswordLengthBox.Text);
			}
			catch (Exception)
			{
				PasswordLength = PasswordLengthOld;
			}
			if (1 >= PasswordLength)
				PasswordLength = 1;
			else if (PasswordLength >= MaxPasswordLength)
				PasswordLength = MaxPasswordLength;
			PasswordLengthBox.Text = PasswordLength.ToString();

			// Get the allowed character types
			bool UseLetters = (bool)LettersCheckbox.IsChecked;
			bool UseNumbers = (bool)NumbersCheckbox.IsChecked;
			bool UseSymbols = (bool)SymbolsCheckbox.IsChecked;

			string GeneratedPassword = "";

			// Use faster algorithm when all characters are allowed
			if (UseLetters & UseNumbers & UseSymbols != false)
			{
				byte[] ArrayGeneratedPassword = new byte[PasswordLength];
				rnd.GetBytes(ArrayGeneratedPassword, 0, PasswordLength);
				for (int i = 0; i < PasswordLength; i++)
				{
					ArrayGeneratedPassword[i] %= 95;
					ArrayGeneratedPassword[i] += 32;
					GeneratedPassword += (char)ArrayGeneratedPassword[i];
				}
				OutputBox.Password = GeneratedPassword;
				return;
			}

			// Get, process and dump the random data
			byte[] GeneratedByte = { 0 };
			for (int i = 0; i < PasswordLength; i++)
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
