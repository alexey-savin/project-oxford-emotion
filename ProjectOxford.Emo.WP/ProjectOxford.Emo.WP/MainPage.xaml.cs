using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=391641

namespace ProjectOxford.Emo.WP
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string SUBSCRIPTION_KEY = "dce55ae45fbd4a69aeb9b73e60c11313";

        private MediaCapture _mediaCapture;
        private bool _isPreviewing = false;
        private bool _isInitialized;
        private bool _mirroringPreview;
        
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Подготовьте здесь страницу для отображения.

            // TODO: Если приложение содержит несколько страниц, обеспечьте
            // обработку нажатия аппаратной кнопки "Назад", выполнив регистрацию на
            // событие Windows.Phone.UI.Input.HardwareButtons.BackPressed.
            // Если вы используете NavigationHelper, предоставляемый некоторыми шаблонами,
            // данное событие обрабатывается для вас.
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mediaCapture = new MediaCapture();

            await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
            });
        }

        private async void btnStartPreview_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Clear();

            if (_isPreviewing == false)
            {
                _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise270Degrees);
                previewElement.Source = _mediaCapture;
                
                await _mediaCapture.StartPreviewAsync();

                _isPreviewing = true;
            }
            previewElement.Visibility = Visibility.Visible;
        }

        private async void btnTakePhoto_Click(object sender, RoutedEventArgs e)
        {
            btnTakePhoto.IsEnabled = false;
            btnStartPreview.IsEnabled = false;

            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            // rotation ->
            BitmapDecoder dec = await BitmapDecoder.CreateAsync(stream);
            InMemoryRandomAccessStream newStream = new InMemoryRandomAccessStream();
            BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(newStream, dec);
            enc.BitmapTransform.Rotation = BitmapRotation.Clockwise270Degrees;
            await enc.FlushAsync();
            // rotation <-

            newStream.Seek(0); // текущую позицию потока устанавливаем в 0
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(newStream);
            captureImage.Source = bitmap;

            newStream.Seek(0); // еще раз перегоняем позицию потока в 0
            Stream st = newStream.AsStream(); // из InMemoryRandomAccessStream получаем System.IO.Stream
            
            if (_isPreviewing == true)
                await _mediaCapture.StopPreviewAsync();

            _isPreviewing = false;
            previewElement.Visibility = Visibility.Collapsed;

            // Отправляем поток и получаем результат
            progring.IsActive = true;
            Emotion[] emotionResult = null;

            try
            {
                EmotionServiceClient emotionServiceClient =
                        new EmotionServiceClient(SUBSCRIPTION_KEY);
                emotionResult = await emotionServiceClient.RecognizeAsync(st);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine(ex.InnerException.Message);
                }
            }
            progring.IsActive = false;

            // Далее считываем результат (если он есть) в коллекцию emo и активируем кнопочки
            if ((emotionResult != null) && (emotionResult.Length > 0))
            {
                ViewModel.Emotion = emotionResult[0];
                RefreshViewModel();
            }

            btnStartPreview.IsEnabled = true;
            btnTakePhoto.IsEnabled = true;

            /*
            // в манифесте необходимо разрешение на библиотеку изображений
            Windows.Storage.StorageFile photoFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(
                     "lastphoto.jpeg", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            await mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), photoFile);

            IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            captureImage.Source = bitmap;

            photoStream.Seek(0); // еще раз перегоняем позицию потока в 0
            Stream st = photoStream.AsStream(); // из IRandomAccessStream получаем System.IO.Stream
            */
        }

        private void RefreshViewModel()
        {
            ViewModel.RefreshTop3Emotion();
        }

        private EmotionViewModel ViewModel => DataContext as EmotionViewModel;
    }
}
