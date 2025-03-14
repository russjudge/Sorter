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
            List<Item> theList =
            [
                new Item("TestKey1", "Test data for row 1", new DateTime(2025, 1, 20), 4),
                new Item("TestKey2", "A Brown Fox", new DateTime(1967, 11, 7), 99),
                new Item("TestKey4", "the tree", new DateTime(2001, 9, 11), 24),
                new Item("TestKey5", "in the east", DateTime.Now, 5032),
                new Item("TestKey3", "Was discovered underneath", new DateTime(1986, 1, 28), 7),
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