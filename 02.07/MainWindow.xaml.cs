using HtmlAgilityPack;
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
            string searchText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                txtBookContent.Text = "Searching... Please wait.";
                try
                {
                    var results = await SearchBooksAsync(searchText);
                    lstResults.ItemsSource = results;
                    txtBookContent.Text = "Select a book to view its content.";
                }
                catch (Exception ex)
                {
                    txtBookContent.Text = $"Error: {ex.Message}";
                }
            }
        }

        private async void lstResults_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstResults.SelectedItem is Book selectedBook)
            {
                txtBookContent.Text = "Loading... Please wait.";
                try
                {
                    string bookContent = await DownloadBookTextAsync(selectedBook.Url);
                    txtBookContent.Text = bookContent;
                }
                catch (Exception ex)
                {
                    txtBookContent.Text = $"Error: {ex.Message}";
                }
            }
        }

        private async Task<List<Book>> SearchBooksAsync(string query)
        {
            string url = $"https://www.gutenberg.org/ebooks/search/?query={Uri.EscapeDataString(query)}";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return ParseSearchResults(responseBody);
            }
        }

        private List<Book> ParseSearchResults(string html)
        {
            var books = new List<Book>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='booklink']");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string title = node.SelectSingleNode(".//span[@class='title']").InnerText.Trim();
                    string href = node.SelectSingleNode(".//a").GetAttributeValue("href", string.Empty);
                    string coverUrl = node.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                    if (!string.IsNullOrEmpty(href) && href.StartsWith("/"))
                    {
                        string bookUrl = "https://www.gutenberg.org" + href;
                        coverUrl = !string.IsNullOrEmpty(coverUrl) ? "https://www.gutenberg.org" + coverUrl : null;
                        books.Add(new Book { Title = title, Url = bookUrl, CoverUrl = coverUrl });
                    }
                }
            }

            return books;
        }

        private async Task<string> DownloadBookTextAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        public class Book
        {
            public string? Title { get; set; }
            public string? Url { get; set; }
            public string? CoverUrl { get; set; }
        }
    }
}