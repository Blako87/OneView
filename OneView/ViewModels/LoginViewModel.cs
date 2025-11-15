using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace OneView.ViewModels
{
    /// <summary>
    /// ViewModel for the Login page
    /// Handles user authentication and navigation to main app
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string emailAddress = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool showError = false;

        /// <summary>
        /// Attempts to log in the user with provided credentials
        /// </summary>
        [RelayCommand]
        private async Task Login()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ShowErrorMessage("Bitte Benutzername und Passwort eingeben");
                return;
            }

            IsLoading = true;
            ShowError = false;

            try
            {
                // Attempt login
                bool success = App.LoginService.Login(Username, Password, EmailAddress);

                if (success)
                {
                    Debug.WriteLine($"? Login successful for user: {Username}");
                    
                    // Navigate to main page
                    await Shell.Current.GoToAsync("///MainTabs");
                }
                else
                {
                    ShowErrorMessage("Login fehlgeschlagen. Bitte versuchen Sie es erneut.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Login error: {ex.Message}");
                ShowErrorMessage($"Fehler: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Displays an error message to the user
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            ErrorMessage = message;
            ShowError = true;
        }

        /// <summary>
        /// Clears the error message
        /// </summary>
        [RelayCommand]
        private void ClearError()
        {
            ShowError = false;
            ErrorMessage = string.Empty;
        }
    }
}
