using AppUser.ViewModels;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace AppUser.Pages
{
    public partial class AudioPlayerPage : ContentPage
    {
        private readonly AudioPlayerViewModel _vm;
        private static readonly HttpClient _httpClient = new();
        private bool _isLoadingSource;
        private string _currentUrl = string.Empty;
        private bool _completionReviewTriggered;
        private bool _isDragging;

        public AudioPlayerPage(AudioPlayerViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Subscribe to events FIRST
            _vm.PlayRequested += OnPlayRequested;
            _vm.PauseRequested += OnPauseRequested;
            _vm.SeekRequested += OnSeekRequested;
            _vm.SourceChanged += OnSourceChanged;

            // Then load and play audio if available
            _ = LoadAndPlayAudioAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            if (_vm.GoBackCommand.CanExecute(null))
            {
                _vm.GoBackCommand.Execute(null);
            }

            return true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.PlayRequested -= OnPlayRequested;
            _vm.PauseRequested -= OnPauseRequested;
            _vm.SeekRequested -= OnSeekRequested;
            _vm.SourceChanged -= OnSourceChanged;
            
            try { MediaElement.Stop(); } catch { /* ignore */ }
        }

        /// <summary>Load audio from URL to local cache and play in-app.</summary>
        private async Task LoadAndPlayAudioAsync()
        {
            if (_isLoadingSource) return;
            var url = _vm.AudioUrl;
            if (string.IsNullOrWhiteSpace(url)) 
            {
                System.Diagnostics.Debug.WriteLine("[AudioPlayer] AudioUrl is empty, cannot play.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Loading audio from: {url}");
            _currentUrl = url;

            try
            {
                _isLoadingSource = true;
                _completionReviewTriggered = false;
                FallbackAudioContainer.IsVisible = false;

                // Download to cache then play local file for better reliability across platforms.
                var bytes = await _httpClient.GetByteArrayAsync(url);
                var ext = Path.GetExtension(new Uri(url).AbsolutePath);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".mp3";
                var fileName = $"poi_audio_{Guid.NewGuid():N}{ext}";
                var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllBytesAsync(localPath, bytes);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        MediaElement.Source = CommunityToolkit.Maui.Views.MediaSource.FromFile(localPath);
                        MediaElement.Play();
                        System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Playing local file: {localPath}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Play() failed: {ex.Message}");
                        ShowFallbackWebPlayer(url);
                    }
                });

                _ = EnsureStartedOrFallbackAsync(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Error loading source: {ex.Message}");
                ShowFallbackWebPlayer(url);
            }
            finally
            {
                _isLoadingSource = false;
            }
        }

        private void OnSourceChanged(object? sender, EventArgs e)
        {
            // When source changes (e.g. language switch), reload
            _completionReviewTriggered = false;
            _ = LoadAndPlayAudioAsync();
        }

        private void OnPlayRequested(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try { MediaElement.Play(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Play error: {ex.Message}");
                }
            });
        }

        private void OnPauseRequested(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try { MediaElement.Pause(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Pause error: {ex.Message}");
                }
            });
        }

        private void OnSeekRequested(object? sender, double position)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    MediaElement.SeekTo(TimeSpan.FromSeconds(position * MediaElement.Duration.TotalSeconds));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Seek error: {ex.Message}");
                }
            });
        }

        private void OnMediaElementPositionChanged(object? sender, MediaPositionChangedEventArgs e)
        {
            // Skip auto-updating progress while user is dragging the slider
            if (!_isDragging)
            {
                _vm.UpdateProgress(e.Position, MediaElement.Duration);
            }

            if (_completionReviewTriggered)
            {
                return;
            }

            var duration = MediaElement.Duration;
            if (duration > TimeSpan.Zero && e.Position >= duration.Subtract(TimeSpan.FromMilliseconds(800)))
            {
                _completionReviewTriggered = true;
                MainThread.BeginInvokeOnMainThread(_vm.PromptReviewAfterCompletion);
            }
        }

        private bool _isAnimating = false;

        private void OnMediaElementStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioPlayer] State changed to: {e.NewState}");
            
            var isPlayingNow = e.NewState == MediaElementState.Playing;
            
            MainThread.BeginInvokeOnMainThread(() => {
                _vm.IsPlaying = isPlayingNow;
                
                // Track Listen automatically when state becomes Playing
                if (isPlayingNow)
                {
                    _vm.CheckAndTrackListen();
                }

                if (isPlayingNow)
                    StartRotationAnimation();
                else
                    StopRotationAnimation();
            });
        }

        private async void StartRotationAnimation()
        {
            if (_isAnimating) return;
            
            _isAnimating = true;
            try
            {
                while (_vm.IsPlaying)
                {
                    await AlbumArtBorder.RotateTo(360, 12000, Easing.Linear);
                    
                    if (!_vm.IsPlaying) break;
                    
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
            AlbumArtBorder.CancelAnimations();
        }

        private async void OnMediaElementMediaFailed(object? sender, CommunityToolkit.Maui.Core.Primitives.MediaFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Media FAILED. URL was: {_vm.AudioUrl}");
            ShowFallbackWebPlayer(_vm.AudioUrl);
        }

        private async Task EnsureStartedOrFallbackAsync(string url)
        {
            await Task.Delay(3500);
            if (!_vm.IsPlaying && url == _currentUrl)
            {
                ShowFallbackWebPlayer(url);
            }
        }

        private void ShowFallbackWebPlayer(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var safeUrl = Uri.EscapeDataString(url);
                var html = $"""
                    <html>
                      <body style="margin:0;background:#101828;color:white;font-family:Segoe UI;">
                        <audio controls autoplay style="width:100%;">
                          <source src="{safeUrl}" type="audio/mpeg" />
                        </audio>
                      </body>
                    </html>
                    """;
                FallbackAudioWebView.Source = new HtmlWebViewSource { Html = html };
                FallbackAudioContainer.IsVisible = true;
            });
        }

        // === Slider drag-to-seek handlers ===

        private void OnSliderDragStarted(object? sender, EventArgs e)
        {
            _isDragging = true;
        }

        private void OnSliderDragCompleted(object? sender, EventArgs e)
        {
            _isDragging = false;
            var position = ProgressSlider.Value;
            _vm.SeekToPosition(position);
        }
    }
}
