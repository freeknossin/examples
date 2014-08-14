using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Compression;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CompressionExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SaveFile(StorageFile saveFile)
        {
            using (var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var compressor = new Compressor(stream))
            using (var writer = new DataWriter(compressor))
            {
                string stringToWrite = "Compression String";
                writer.WriteInt32(stringToWrite.Length);
                writer.WriteString(stringToWrite);
                await writer.StoreAsync();
            }
        }

        public async Task<string> LoadFile(StorageFile compressedFile)
        {
            using (var readFile = await compressedFile.OpenSequentialReadAsync())
            using (var decompressor = new Decompressor(readFile))
            using (var memoryStream = new InMemoryRandomAccessStream())
            {
                var resultWrite = await RandomAccessStream.CopyAsync(decompressor, memoryStream);
                memoryStream.Seek(0);
                using (var reader = new DataReader(memoryStream.GetInputStreamAt(0)))
                {
                    var readResult = await reader.LoadAsync((uint)memoryStream.Size);
                    var length = reader.ReadInt32();
                    var text = reader.ReadString((uint)length);
                    return text;
                }
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                "TemporaryCompressionFile.dat", 
                CreationCollisionOption.ReplaceExisting);
            try
            {
                SaveFile(tempFile);
            }
            catch (Exception saveException)
            {
                ResulTextBlock.Text = "Compression failed: " + saveException.Message;
            }

            try
            {
                var result = await LoadFile(tempFile);
                ResulTextBlock.Text = result;
            }
            catch (Exception loadException)
            {
                ResulTextBlock.Text = "Decompression failed: " + loadException.Message;
            }
        }
    }
}
