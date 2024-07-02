using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _02._07
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = txtSearchQuery.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                MessageBox.Show("Please enter a search query.");
                return;
            }

            txtSearchResults.Text = "Searching... Please wait.";

            List<Task<string>> searchTasks = new List<Task<string>>();

            if (chkGoogle.IsChecked == true)
                searchTasks.Add(SearchAsync("Google", searchQuery));

            if (chkBing.IsChecked == true)
                searchTasks.Add(SearchAsync("Bing", searchQuery));

            try
            {
                await Task.WhenAll(searchTasks);

                foreach (var result in searchTasks)
                {
                    txtSearchResults.Text += $"{result.Result}{Environment.NewLine}{Environment.NewLine}";
                }
            }
            catch (Exception ex)
            {
                txtSearchResults.Text = $"Error: {ex.Message}";
            }
        }

        private async Task<string> SearchAsync(string engine, string query)
        {
            string url = "";
            if (engine == "Google")
                url = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
            else if (engine == "Bing")
                url = $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return $"{engine} Search Results:{Environment.NewLine}{responseBody}";
            }
        }
    }
}
