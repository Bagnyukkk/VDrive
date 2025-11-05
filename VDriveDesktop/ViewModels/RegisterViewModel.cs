using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VDriveDesktop.Services;

namespace VDriveDesktop.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _fullName;
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isBusy;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
            GoToLoginCommand = new Command(async () => await GoToLoginPageAsync());
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "All fields are required.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var (success, error) = await _apiService.RegisterAsync(FullName, Username, Password);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Registration successful! You can now log in.", "OK");
                    await GoToLoginPageAsync();
                }
                else
                {
                    ErrorMessage = $"Registration failed: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToLoginPageAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            (RegisterCommand as Command)?.ChangeCanExecute();
        }
    }
}
