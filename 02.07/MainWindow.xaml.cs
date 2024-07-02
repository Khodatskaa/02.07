using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace _02._07
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.gutenberg.org/files/1524/1524-0.txt";
            txtHamlet.Text = "Downloading... Please wait.";

            try
            {
                string hamletText = await DownloadHamletTextAsync(url);
                txtHamlet.Text = hamletText;
            }
            catch (Exception ex)
            {
                txtHamlet.Text = $"Error: {ex.Message}";
            }
        }

        private async Task<string> DownloadHamletTextAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }
    }
}
