using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Platformy2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SelectedPath = System.IO.Directory.GetCurrentDirectory();

        private FileStream? OpenedFile;
        private FileSystemInfo? SelectedFile;

        public MainWindow()
        {
            InitializeComponent();

            UpdateTree();
        }

        private void UpdateTree()
        {
            DirectoryInfo rootDirectory = new System.IO.DirectoryInfo(SelectedPath);

            TreeViewItem root = CreateTreeItem(rootDirectory);

            CreateTree(rootDirectory, root);

            FileTree.Items.Clear();
            FileTree.Items.Add(root);
        }

        private void CreateTree(DirectoryInfo directory, TreeViewItem parent)
        {
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                TreeViewItem item = CreateTreeItem(subdirectory);

                CreateTree(subdirectory, item);
                parent.Items.Add(item);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                TreeViewItem item = CreateTreeItem(file);

                parent.Items.Add(item);
            }
        }

        private TreeViewItem CreateTreeItem(FileSystemInfo systemInfo)
        {
            ContextMenu menu = new ContextMenu();

            if (systemInfo is DirectoryInfo directoryInfo)
            {
                MenuItem create = new MenuItem
                {
                    Header = "Create"
                };

                create.Click += (sender, e) =>
                {
                    CreateFile(directoryInfo);
                    UpdateTree();
                };

                menu.Items.Add(create);
            }

            if (systemInfo is FileInfo fileInfo)
            {
                MenuItem open = new MenuItem
                {
                    Header = "Open"
                };

                open.Click += (sender, e) =>
                {
                    if (OpenedFile != null) OpenedFile.Close();

                    OpenedFile = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    UpdateOpenedFile();
                };

                menu.Items.Add(open);
            }


            MenuItem delete = new MenuItem
            {
                Header = "Delete"
            };

            delete.Click += (sender, e) =>
            {
                DeleteFile(systemInfo);
                UpdateTree();
            };

            menu.Items.Add(delete);


            TreeViewItem item = new TreeViewItem
            {
                Header = systemInfo.Name,
                Tag = systemInfo.Name
            };

            item.ContextMenu = menu;

            item.Selected += (sender, e) =>
            {
                if (item.IsSelected == false) return;
                SelectedFile = systemInfo;
                UpdateSelectedFile();
            };

            return item;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog() { Description = "Select directory to open" };
            DialogResult result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            SelectedPath = dialog.SelectedPath;
            UpdateTree();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateFile(DirectoryInfo directoryInfo)
        {
            try
            {
                CreateDialog createDialog = new CreateDialog();

                bool result = createDialog.ShowDialog() == true;

                if (!result) return;

                if (createDialog.IsDirectory)
                {
                    DirectoryInfo newDirectory = directoryInfo.CreateSubdirectory(createDialog.SelectedName);

                    if (createDialog.IsReadOnly) newDirectory.Attributes |= FileAttributes.ReadOnly;
                    if (createDialog.IsArchive) newDirectory.Attributes |= FileAttributes.Archive;

                    return;
                }

                string path = Path.Combine(directoryInfo.FullName, createDialog.SelectedName);
                FileStream newFile = System.IO.File.Create(path);
                if (newFile == null) return;

                newFile.Close();
                FileInfo fileInfo = new FileInfo(path);

                if (createDialog.IsReadOnly) fileInfo.Attributes |= FileAttributes.ReadOnly;
                if (createDialog.IsArchive) fileInfo.Attributes |= FileAttributes.Archive;
                if (createDialog.IsHidden) fileInfo.Attributes |= FileAttributes.Hidden;
                if (createDialog.IsSystem) fileInfo.Attributes |= FileAttributes.System;

                UpdateTree();
            }
            catch (IOException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFile(FileSystemInfo systemInfo)
        {
            try
            {
                if (systemInfo is FileInfo fileInfo)
                {
                    if ((fileInfo.Attributes & FileAttributes.ReadOnly) != 0) RemoveReadOnly(fileInfo);

                    fileInfo.Delete();
                    return;
                }

                if (systemInfo is DirectoryInfo directoryInfo)
                {
                    foreach (FileSystemInfo info in directoryInfo.GetFileSystemInfos())
                    {
                        DeleteFile(info);
                    }

                    directoryInfo.Delete();
                    return;
                }

                systemInfo.Delete();

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void UpdateOpenedFile()
        {
            if (OpenedFile == null) return;

            string Content = new StreamReader(OpenedFile).ReadToEnd();

            FileContent.Text = Content;
        }

        private void UpdateSelectedFile()
        {
            if (SelectedFile == null) return;

            if (SelectedFile is FileInfo fileInfo)
            {
                Status.Text = GetAttributeString(fileInfo);
                return;
            }

            Status.Text = "";
        }

        private static string GetAttributeString(FileInfo fileInfo)
        {
            string attributes = "";
            attributes += (fileInfo.Attributes & FileAttributes.ReadOnly) != 0 ? "r" : "-";
            attributes += (fileInfo.Attributes & FileAttributes.Archive) != 0 ? "a" : "-";
            attributes += (fileInfo.Attributes & FileAttributes.Hidden) != 0 ? "h" : "-";
            attributes += (fileInfo.Attributes & FileAttributes.System) != 0 ? "s" : "-";

            return attributes;
        }

        private static void RemoveReadOnly(FileInfo fileInfo) => fileInfo.Attributes &= ~FileAttributes.ReadOnly;
    }
}