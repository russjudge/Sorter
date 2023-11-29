using RussJudge.WPFListSorter;
using System.Windows;

namespace Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<KeyValuePair<string, string>> theList =
            [
                new KeyValuePair<string, string>("TestKey1", "Test data for row 1"),
                new KeyValuePair<string, string>("TestKey2", "A Brown Fox"),
                new KeyValuePair<string, string>("TestKey3", "Was discovered underneath"),
                new KeyValuePair<string, string>("TestKey4", "the tree"),
                new KeyValuePair<string, string>("TestKey5", "in the east"),
            ];
            DataContext = theList;
        }

        private void OnSearch(object sender, RoutedEventArgs e)
        {
            var matchRow = Sorter.Find(TheListView, SearchText.Text, false);
            if (matchRow is KeyValuePair<string, string> matched)
            {
                MessageBox.Show("Matched on " + matched.Key + ": " + matched.Value);
                TheListView.ScrollIntoView(matched);
            }
            else
            {
                MessageBox.Show(SearchText.Text + " was not found.");
            }
        }
    }
}