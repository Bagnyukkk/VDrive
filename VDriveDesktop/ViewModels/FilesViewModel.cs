using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VDriveDesktop.Services;
using VDriveDesktop.Views;

namespace VDriveDesktop.ViewModels
{
    public partial class FilesViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        public ObservableCollection<SelectableFileItem> Files { get; set; }

        private bool _isAllSelected;
        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (_isAllSelected != value)
                {
                    _isAllSelected = value;

                    foreach (var file in Files)
                    {
                        file.PropertyChanged -= OnFileSelectionChanged;
                        file.IsSelected = _isAllSelected;
                        file.PropertyChanged += OnFileSelectionChanged;
                    }

                    OnPropertyChanged();
                    (DeleteFileCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private bool _showDetails = true;
        public bool ShowDetails
        {
            get => _showDetails;
            set
            {
                if (_showDetails != value)
                {
                    _showDetails = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenFileContentCommand { get; }

        public ICommand ToggleDetailsCommand { get; }
        public ICommand FileTappedCommand { get; } 

        public ICommand LoadFilesCommand { get; }
        public ICommand UploadFileCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DownloadFileCommand { get; }

        private SelectableFileItem _lastTappedItem;
        private DateTime _lastTapTime;
        private const int DoubleTapThresholdMs = 500;

        public FilesViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Files = new ObservableCollection<SelectableFileItem>();

            LoadFilesCommand = new Command(async () => await LoadFilesAsync());
            UploadFileCommand = new Command(async () => await UploadFileAsync());
            DeleteFileCommand = new Command(async () => await DeleteSelectedFilesAsync(), () => Files.Any(f => f.IsSelected));
            DownloadFileCommand = new Command(async () => await DownloadSelectedFilesAsync(), () => Files.Any(f => f.IsSelected));
            SortCommand = new Command<string>(async (order) => await SortFilesAsync(order));
            FilterCommand = new Command<string>(async (filter) => await FilterFilesAsync(filter));
            LogoutCommand = new Command(async () => await LogoutAsync());
            ToggleDetailsCommand = new Command(() => ShowDetails = !ShowDetails);
            OpenFileContentCommand = new Command<SelectableFileItem>(async (file) => await OpenFileContentAsync(file));
            FileTappedCommand = new Command<SelectableFileItem>(async (item) => await OnFileTappedAsync(item));
        }

        private async Task LoadFilesAsync(string sortBy = "filename", string order = "asc", string filter = "all")
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var filesFromApi = await _apiService.GetFilesAsync(token, sortBy, order, filter);

            Files.Clear();
            foreach (var fileDto in filesFromApi)
            {
                var selectableFile = new SelectableFileItem(fileDto);
                selectableFile.PropertyChanged += OnFileSelectionChanged;
                Files.Add(selectableFile);
            }

            UpdateSelectAllState();
        }

        private async Task OnFileTappedAsync(SelectableFileItem tappedItem)
        {
            if (tappedItem == null) return;

            var currentTime = DateTime.Now;
            var timeSinceLastTap = currentTime - _lastTapTime;

            if (_lastTappedItem == tappedItem && timeSinceLastTap.TotalMilliseconds < DoubleTapThresholdMs)
            {
                _lastTappedItem = null;
                _lastTapTime = DateTime.MinValue;

                await OpenFileContentAsync(tappedItem);
            }
            else
            {
                tappedItem.IsSelected = !tappedItem.IsSelected;

                _lastTappedItem = tappedItem;
                _lastTapTime = currentTime;
            }
        }

        private async Task OpenFileContentAsync(SelectableFileItem fileItem)
        {
            if (fileItem == null) return;

            var extension = Path.GetExtension(fileItem.FileName).ToLower();
            if (extension != ".c" && extension != ".jpg")
            {
                return;
            }

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var (fileContents, contentType, fileName) = await _apiService.GetFileContentAsync(token, fileItem.FileId);

            if (fileContents != null)
            {
                var viewModel = new FileContentViewModel { FileName = fileName };

                if (contentType.Contains("text/plain"))
                {
                    viewModel.TextContent = System.Text.Encoding.UTF8.GetString(fileContents);
                    viewModel.IsTextVisible = true;
                }
                else if (contentType.Contains("image/jpeg"))
                {
                    viewModel.ImageContent = ImageSource.FromStream(() => new MemoryStream(fileContents));
                    viewModel.IsImageVisible = true;
                }

                await Application.Current.MainPage.Navigation.PushAsync(new Views.FileContentPage(viewModel));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Could not load file content.", "OK");
            }
        }

        private async Task DownloadSelectedFilesAsync()
        {
            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            if (!selectedFiles.Any()) return;

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            int successCount = 0;
            foreach (var file in selectedFiles)
            {
                var (fileContents, fileName) = await _apiService.DownloadFileAsync(token, file.FileId);

                if (fileContents != null && !string.IsNullOrEmpty(fileName))
                {
                    string targetPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

                    try
                    {
                        await File.WriteAllBytesAsync(targetPath, fileContents);
                        successCount++;
                        Debug.WriteLine($"File saved to: {targetPath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error saving file {fileName}: {ex.Message}");
                    }
                }
            }

            if (successCount > 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    $"{successCount} file(s) downloaded successfully to the application's cache directory.",
                    "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Could not download the selected file(s).",
                    "OK");
            }
        }

        private async Task DeleteSelectedFilesAsync()
        {
            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            if (!selectedFiles.Any()) return;

            bool confirmed = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {selectedFiles.Count} file(s)?",
                "Yes", "No");

            if (!confirmed) return;

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            foreach (var file in selectedFiles)
            {
                var success = await _apiService.DeleteFileAsync(token, file.FileId);
                if (success)
                {
                    file.PropertyChanged -= OnFileSelectionChanged;
                    Files.Remove(file);
                }
            }
        }

        private void OnFileSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableFileItem.IsSelected))
            {
                UpdateSelectAllState();
                (DeleteFileCommand as Command)?.ChangeCanExecute();
                (DownloadFileCommand as Command)?.ChangeCanExecute();
            }
        }

        private void UpdateSelectAllState()
        {
            var allSelected = Files.Any() && Files.All(f => f.IsSelected);
            if (_isAllSelected != allSelected)
            {
                _isAllSelected = allSelected;
                OnPropertyChanged(nameof(IsAllSelected));
            }
        }

        private async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("auth_token");
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        private async Task UploadFileAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var result = await FilePicker.PickAsync();
            if (result != null)
            {
                var success = await _apiService.UploadFileAsync(token, result.FullPath);
                if (success)
                {
                    await LoadFilesAsync();
                }
            }
        }

        private async Task SortFilesAsync(string order)
        {
            await LoadFilesAsync(sortBy: "filename", order: order);
        }

        private async Task FilterFilesAsync(string filter)
        {
            await LoadFilesAsync(filter: filter);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
