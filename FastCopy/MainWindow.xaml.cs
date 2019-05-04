using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastCopy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private List<string> fromDirs = new List<string>();
        private Dictionary<string, string> fromHash = new Dictionary<string, string>();
        private Dictionary<string, string> toHash = new Dictionary<string, string>();
        private List<string> toDirs = new List<string>();
        private List<string> diffFiles = new List<string>();
        private int fromDirLen = 0;
        private int toDirLen = 0;
        private string fromDir;
        private string toDir;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    From.Text = fbd.SelectedPath;
                    fromDir = fbd.SelectedPath;
                    fromDirLen = GetDirFileCount(fbd.SelectedPath);
                    DirSearch(fbd.SelectedPath, fbd.SelectedPath);
                    State.Text += $"Enumerated {fromHash.Count} files.\n";
                }
            }
        }
        private static int GetDirFileCount(string myBaseDirectory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(myBaseDirectory);
            return dirInfo.EnumerateDirectories()
                   .AsParallel()
                   .SelectMany(di => di.EnumerateFiles("*.*", SearchOption.AllDirectories))
                   .Count();
        }
        private void DirSearch(string sDir, string parentDir, bool from = true)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    var relPath = f.Replace(parentDir, "");
                    var fileInfo = new FileInfo(f);
                    var length = fileInfo.Length;
                    var lastWriteTime = fileInfo.LastWriteTime;
                    if (from) fromHash.Add(relPath, $"{length}{lastWriteTime}");
                    else toHash.Add(relPath, $"{length}{lastWriteTime}");
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    DirSearch(d, parentDir, from);
                }
            }
            catch (Exception ex)
            {
                State.Text += ex.ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    To.Text = fbd.SelectedPath;
                    toDir = fbd.SelectedPath;
                    DirSearch(fbd.SelectedPath, fbd.SelectedPath, false);
                    State.Text += $"Enumerated {toHash.Count} files.\n";
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            diffFiles.Clear();
            foreach (var item in fromHash)
            {
                string hash;
                var hasKey = toHash.TryGetValue(item.Key, out hash);
                if (!hasKey)
                {
                    //State.Text += $"Diff: {item.Key} Reason: FileExist\n";
                    diffFiles.Add(item.Key);
                }
                else if (item.Value != hash)
                {
                    //State.Text += $"Diff: {item.Key} Reason: HashMismatch\n";
                    diffFiles.Add(item.Key);
                }
            }
            State.Text += $"Compare done. Has {diffFiles.Count} diff files.\n";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += doCopy;
            worker.ProgressChanged += onProgress;
            worker.RunWorkerAsync();
        }
        private void onProgress(object sender, ProgressChangedEventArgs e)
        {
            Copying.Text = (String)e.UserState;
            Progress.Value = e.ProgressPercentage;
        }
        private void doCopy(object sender, DoWorkEventArgs e)
        {
            var from = fromDir;
            var to = toDir;
            var progress = 0;
            foreach (var item in diffFiles)
            {
                progress++;
                (sender as BackgroundWorker).ReportProgress(progress / diffFiles.Count * 100, from + item);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(to + item));
                File.Copy(from + item, to + item, true);
                //System.Threading.Thread.Sleep(100);
            }
        }
    }
}
