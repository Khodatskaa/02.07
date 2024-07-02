using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _02._07
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnSearchImages_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = txtImageSearchQuery.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                MessageBox.Show("Please enter a search query.");
                return;
            }

            imgSearchResults.Items.Clear();

            try
            {
                List<string> imageUrls = await SearchImagesAsync(searchQuery);

                foreach (var url in imageUrls)
                {
                    imgSearchResults.Items.Add(new BitmapImage(new Uri(url)));
                }

                ScrollViewer scrollViewer = GetScrollViewer(imgSearchResults);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToTop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching images: {ex.Message}");
            }
        }

        private async Task<List<string>> SearchImagesAsync(string query)
        {
            string url = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}&tbm=isch";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                return ExtractImageUrls(responseBody);
            }
        }

        private List<string> ExtractImageUrls(string html)
        {
            List<string> imageUrls = new List<string>();
            
            string pattern = @"<img[^>]*src\s*=\s*['""]([^'""]+)['""]";

            MatchCollection matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                string imageUrl = match.Groups[1].Value;
                imageUrls.Add(imageUrl);
            }

            return imageUrls;
        }

        private ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is ScrollViewer scrollViewer)
                {
                    return scrollViewer;
                }

                ScrollViewer childScrollViewer = GetScrollViewer(child);
                if (childScrollViewer != null)
                {
                    return childScrollViewer;
                }
            }

            return null;
        }
    }
}
