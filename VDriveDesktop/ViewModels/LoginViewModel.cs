using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VDriveDesktop.Services;
using VDriveDesktop.Views;

namespace VDriveDesktop.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isBusy;

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

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
            GoToRegisterCommand = new Command(async () => await GoToRegisterPageAsync());
        }

        public void OnAppearing()
        {
            Password = string.Empty;

            ErrorMessage = string.Empty;
        }

        private async Task GoToRegisterPageAsync()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var token = await _apiService.LoginAsync(Username, Password);

                if (!string.IsNullOrEmpty(token))
                {
                    await SecureStorage.SetAsync("auth_token", token);

                    await Shell.Current.GoToAsync(nameof(FilesPage));
                }
                else
                {
                    ErrorMessage = "Неправильний логін або пароль.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Сталася помилка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            (LoginCommand as Command)?.ChangeCanExecute();
        }
    }
}
