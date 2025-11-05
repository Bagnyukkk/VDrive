using System.ComponentModel;
using System.Runtime.CompilerServices;
using VDriveDesktop.Models;

namespace VDriveDesktop.ViewModels
{
    public class SelectableFileItem : INotifyPropertyChanged
    {
        public FileDetailDto FileDetail { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public SelectableFileItem(FileDetailDto fileDetail)
        {
            FileDetail = fileDetail;
        }

        public string FileName => FileDetail.FileName;
        public DateTime CreationDate => FileDetail.CreationDate;
        public DateTime ModificationDate => FileDetail.ModificationDate;
        public string UploaderName => FileDetail.UploaderName;
        public string LastEditorName => FileDetail.LastEditorName;
        public Guid FileId => FileDetail.FileId;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
