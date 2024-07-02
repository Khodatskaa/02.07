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
        private List<Book> books;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnLoadBooks_Click(object sender, RoutedEventArgs e)
        {
            txtBookContent.Text = "Loading... Please wait.";
            try
            {
                books = await LoadPopularBooksAsync();
                lstBooks.ItemsSource = books;
                lstBooks.DisplayMemberPath = "Title";
                txtBookContent.Text = "Select a book to view its content.";
            }
            catch (Exception ex)
            {
                txtBookContent.Text = $"Error: {ex.Message}";
            }
        }

        private async void lstBooks_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstBooks.SelectedItem is Book selectedBook)
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

        private async Task<List<Book>> LoadPopularBooksAsync()
        {
            string url = "https://gutenberg.org/browse/scores/top";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return ParsePopularBooks(responseBody);
            }
        }

        private List<Book> ParsePopularBooks(string html)
        {
            var books = new List<Book>();
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//ol/li/a");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (books.Count >= 100) break;

                    string title = node.InnerText;
                    string href = node.GetAttributeValue("href", string.Empty);

                    if (!string.IsNullOrEmpty(href) && href.StartsWith("/"))
                    {
                        string bookUrl = "https://www.gutenberg.org" + href;
                        books.Add(new Book { Title = title, Url = bookUrl });
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
        }
    }
}