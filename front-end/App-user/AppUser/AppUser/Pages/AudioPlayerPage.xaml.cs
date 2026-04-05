using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class AudioPlayerPage : ContentPage
    {
        private readonly AudioPlayerViewModel _vm;

        public AudioPlayerPage(AudioPlayerViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.PlayRequested += OnPlayRequested;
            _vm.PauseRequested += OnPauseRequested;
            _vm.SeekRequested += OnSeekRequested;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.PlayRequested -= OnPlayRequested;
            _vm.PauseRequested -= OnPauseRequested;
            _vm.SeekRequested -= OnSeekRequested;
            MediaElement.Stop();
        }

        private void OnPlayRequested(object? sender, EventArgs e) => MediaElement.Play();
        private void OnPauseRequested(object? sender, EventArgs e) => MediaElement.Pause();
        private void OnSeekRequested(object? sender, double position) => 
            MediaElement.SeekTo(TimeSpan.FromSeconds(position * MediaElement.Duration.TotalSeconds));

        private void OnMediaElementPositionChanged(object? sender, CommunityToolkit.Maui.Core.Primitives.MediaPositionChangedEventArgs e)
        {
            _vm.UpdateProgress(e.Position, MediaElement.Duration);
        }

        private bool _isAnimating = false;

        private void OnMediaElementStateChanged(object? sender, CommunityToolkit.Maui.Core.Primitives.MediaStateChangedEventArgs e)
        {
            var isPlayingNow = e.NewState == CommunityToolkit.Maui.Core.Primitives.MediaElementState.Playing;
            
            // Cập nhật ViewModel trên UI Thread để an toàn
            MainThread.BeginInvokeOnMainThread(() => {
                _vm.IsPlaying = isPlayingNow;
                
                // Điều khiển hiệu ứng xoay đĩa dựa trên trạng thái mới
                if (isPlayingNow)
                    StartRotationAnimation();
                else
                    StopRotationAnimation();
            });
        }

        private async void StartRotationAnimation()
        {
            // Nếu đang trong vòng lặp xoay rồi thì không tạo thêm vòng lặp mới (Tránh Crash)
            if (_isAnimating) return;
            
            _isAnimating = true;
            try
            {
                // Xoay liên tục trong khi nhạc đang phát
                while (_vm.IsPlaying)
                {
                    // Xoay 360 độ trong 12 giây cho cảm giác thư thái
                    await AlbumArtBorder.RotateTo(360, 12000, Easing.Linear);
                    
                    if (!_vm.IsPlaying) break;
                    
                    // Reset góc quay về 0 ngay lập tức để vòng tiếp theo mượt mà
                    AlbumArtBorder.Rotation = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
            }
            finally
            {
                _isAnimating = false;
            }
        }

        private void StopRotationAnimation()
        {
            // Dừng hoạt ảnh đang chạy ngay lập tức
            AlbumArtBorder.CancelAnimations();
        }
    }
}
