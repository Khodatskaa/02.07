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
            if (!string.IsNullOrEmpty(searchQuery) && searchQuery != "Enter search query")
            {
                txtSearchResults.Text = "Searching... Please wait.";

                try
                {
                    string searchResults = await SearchGoogleAsync(searchQuery);
                    txtSearchResults.Text = searchResults;
                }
                catch (Exception ex)
                {
                    txtSearchResults.Text = $"Error: {ex.Message}";
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid search query.");
            }
        }

        private async Task<string> SearchGoogleAsync(string query)
        {
            string url = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        private void txtSearchQuery_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchQuery.Text == "Enter search query")
            {
                txtSearchQuery.Text = "";
                txtSearchQuery.Foreground = System.Windows.Media.Brushes.Black; 
            }
        }

        private void txtSearchQuery_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchQuery.Text))
            {
                txtSearchQuery.Text = "Enter search query";
                txtSearchQuery.Foreground = System.Windows.Media.Brushes.LightGray; 
            }
        }
    }
}