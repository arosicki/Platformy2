using System.Text.RegularExpressions;
using System.Windows;

namespace Platformy2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CreateDialog : Window
    {
        public string SelectedName = "";
        public bool IsDirectory = false;
        public bool IsReadOnly = false;
        public bool IsArchive = false;
        public bool IsHidden = false;
        public bool IsSystem = false;

        public CreateDialog()
        {
            InitializeComponent();
        }

        private void ReadState()
        {
            SelectedName = NameBox.Text;
            IsDirectory = DirectoryRadio.IsChecked == true;
            IsReadOnly = ReadOnlyBox.IsChecked == true;
            IsArchive = ArchiveBox.IsChecked == true;
            IsHidden = HiddenBox.IsChecked == true;
            IsSystem = SystemBox.IsChecked == true;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ReadState();
            if (!IsDirectory && !Regex.IsMatch(SelectedName, "^[A-Za-z0-9_`-]{1,8}.txt|php|html$"))
            {
                System.Windows.MessageBox.Show("Invalid file name. Name should contain 1-8 characters, numbers, underscores, tylda or dash. File should have one of txt, php or html extensions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
