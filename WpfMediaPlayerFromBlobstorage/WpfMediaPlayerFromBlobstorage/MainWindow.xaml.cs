using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace WpfMediaPlayerFromBlobstorage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string blobStorageConnectionString =
            "DefaultEndpointsProtocol=http;AccountName=YOUR_ACCOUNT_HERE;AccountKey=YOUR_KEY_HERE";
        private Uri uploadedBlobUri=null;


        public MainWindow()
        {
            InitializeComponent();
            this.controls.IsEnabled = false;
        }

        private async void BtnUpload_OnClick(object sender, RoutedEventArgs e)
        {
            this.controls.IsEnabled = false;
            OpenFileDialog fd = new OpenFileDialog();
            fd.InitialDirectory=@"c:\";
            var result = fd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
                    container.CreateIfNotExists();
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");
                    container.SetPermissions(
                        new BlobContainerPermissions
                        {
                            PublicAccess =
                                BlobContainerPublicAccessType.Blob
                        }
                    );

                    using (var fileStream = File.OpenRead(fd.FileName))
                    {
                        await blockBlob.UploadFromStreamAsync(fileStream);
                        uploadedBlobUri = blockBlob.Uri;
                        this.controls.IsEnabled = true;
                        MessageBox.Show("File uploaded ok");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Ooops : " + exception.Message);
                }
            }


           
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            player.Source = uploadedBlobUri;
            player.Play();
            timelineSlider.Value = 0;
        }

        private void BtnPause_OnClick(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            player.Stop();
            timelineSlider.Value = 0;
        }

        private void Element_MediaOpened(object sender, EventArgs e)
        {
            timelineSlider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void Element_MediaEnded(object sender, EventArgs e)
        {
            player.Stop();
            timelineSlider.Value = 0;
        }


        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            int sliderValue = (int)timelineSlider.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            player.Position = ts;
        }
    }
}
