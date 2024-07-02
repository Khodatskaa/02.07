using HtmlAgilityPack;
using System.IO;
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

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string firstName = txtAuthorFirstName.Text.Trim();
            string lastName = txtAuthorLastName.Text.Trim();
            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            {
                txtBookContent.Text = "Searching... Please wait.";
                try
                {
                    books = await SearchBooksByAuthorAsync(firstName, lastName);
                    lstBooks.ItemsSource = books;
                    txtBookContent.Text = "Select a book to view its content or click 'Download All Books'.";
                }
                catch (Exception ex)
                {
                    txtBookContent.Text = $"Error: {ex.Message}";
                }
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

        private async void btnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            if (books != null && books.Count > 0)
            {
                txtBookContent.Text = "Downloading all books... Please wait.";
                try
                {
                    await DownloadAllBooksAsync(books);
                    txtBookContent.Text = "All books downloaded successfully.";
                }
                catch (Exception ex)
                {
                    txtBookContent.Text = $"Error: {ex.Message}";
                }
            }
        }

        private async Task<List<Book>> SearchBooksByAuthorAsync(string firstName, string lastName)
        {
            string query = $"{firstName} {lastName}".Trim();
            string url = $"https://www.gutenberg.org/ebooks/search/?query={Uri.EscapeDataString(query)}&submit_search=Go%21";
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

        private async Task DownloadAllBooksAsync(List<Book> books)
        {
            string downloadFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GutenbergBooks");
            Directory.CreateDirectory(downloadFolder);

            using (HttpClient client = new HttpClient())
            {
                foreach (var book in books)
                {
                    HttpResponseMessage response = await client.GetAsync(book.Url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    string fileName = $"{book.Title.Replace(" ", "_").Replace(":", "")}.txt";
                    string filePath = System.IO.Path.Combine(downloadFolder, fileName);

                    await File.WriteAllTextAsync(filePath, responseBody);
                }
            }
        }

        public class Book
        {
            public required string Title { get; set; }
            public required string Url { get; set; }
        }
    }
}