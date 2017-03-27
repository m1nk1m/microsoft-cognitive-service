using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Emotion;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CognitiveService
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CameraCaptureUI captureUi = new CameraCaptureUI();
        StorageFile photo;
        private IRandomAccessStream imageStream;

        const string APIKEY = "d7aaad7f84f44baa99b780c3b8d85fc5";
        EmotionServiceClient emotionServiceClient = new EmotionServiceClient(APIKEY);
        private Emotion[] emotionResult;

        public MainPage()
        {
            this.InitializeComponent();
            captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUi.PhotoSettings.CroppedAspectRatio = new Size(200, 200);
        }

        private async void takePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                photo = await captureUi.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (photo == null)
                {
                    return;
                }
                else
                {
                    imageStream = await photo.OpenAsync(FileAccessMode.Read);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);
                    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    SoftwareBitmap softwareBitmapBRG8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                    await bitmapSource.SetBitmapAsync(softwareBitmapBRG8);

                    image.Source = bitmapSource;
                }
            }
            catch
            {
                output.Text = "Error Taking the Picture";
            }
        }

        private async void getEmotion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                emotionResult = await emotionServiceClient.RecognizeAsync(imageStream.AsStream());
                if (emotionResult != null)
                {
                    EmotionScores scores = emotionResult[0].Scores;
                    output.Text = "Your emotions are \n" +
                        "Happiness: " + scores.Happiness + "\n" +
                        "Sadness: " + scores.Sadness + "\n" +
                        "Fear: " + scores.Fear + "\n" +
                        "Neutral: " + scores.Neutral + "\n";
                }
            }
            catch
            {
                output.Text = "Error returning emotion";
            }
        }
    }
}
