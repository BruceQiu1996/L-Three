using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace ThreeL.Client.Win.ViewModels
{
    public class VoiceChatWindowViewModel : ObservableObject
    {
        private RelationViewModel _current;
        public RelationViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        private bool _started;
        public bool Started
        {
            get => _started;
            set => SetProperty(ref _started, value);
        }

        private string _wattingText;
        public string WattingText
        {
            get => _wattingText;
            set => SetProperty(ref _wattingText, value);
        }

        private string _loadingText;
        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }

        public string VoiceChatKey { get; set; }

        private Task _loadingTask;
        public VoiceChatWindowViewModel()
        {
            _loadingTask = Task.Run(async () =>
            {
                while (true)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (LoadingText?.Length >= 3)
                        {
                            LoadingText = ".";
                        }
                        else
                        {
                            LoadingText = $"{LoadingText}.";
                        }
                    });

                    await Task.Delay(1000);
                }
            });
        }

        ~VoiceChatWindowViewModel()
        {
            _loadingTask.Dispose();
        }
    }
}
