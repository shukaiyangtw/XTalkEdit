/// -----------------------------------------------------------------------------------------------
/// <summary>
///     工作目錄首頁
/// </summary>
/// <remarks>
///     這是應用程式的首頁，選擇(或載入上一次的)工作目錄之後，準備 CSS, Images, JavaScript 範本檔案，然後列出每
///     一個包含有 xtalk.proj 的子目錄，每個子目錄都是一個腳本專案。
/// </remarks>
/// <history>
///     2018/11/22 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;

namespace XTalkEdit
{
    public partial class MainPage : Page
    {
        /// <summary>
        ///  從 app.WorkDir 找到的所有專案子目錄。
        /// </summary>
        private ObservableCollection<DirectoryInfo> m_projects;

        public MainPage()
        {
            InitializeComponent();

            /// 指定 ProjectList 顯示 m_projects 中的項目:
            m_projects = new ObservableCollection<DirectoryInfo>();
            ProjectList.ItemsSource = m_projects;

            /// 載入說明檔案 Introduction.xaml 到 FlowDocumentScrollViewer 當中:
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.Name.Equals("zh-TW"))
            {
                FlowDocument doc = Application.LoadComponent(new Uri("/Introduction.zh-TW.xaml", UriKind.RelativeOrAbsolute)) as FlowDocument;
                IntroDocViewer.Document = doc;
            }
            else
            {
                FlowDocument doc = Application.LoadComponent(new Uri("/Introduction.xaml", UriKind.RelativeOrAbsolute)) as FlowDocument;
                IntroDocViewer.Document = doc;
            }
        }

        /// <summary>
        ///  檢查目前的工作目錄是否為一個有效的路徑。
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage.Page_Loaded():");

            /// 檢查目前的工作目錄是否為一個有效的路徑:
            App app = Application.Current as App;
            if ((String.IsNullOrEmpty(app.WorkDir)) || (Directory.Exists(app.WorkDir) == false))
            {   SeletWorkDirButton_Click(null, null);  }
            else {  PrepareWorkDir();  }
        }

        /// <summary>
        ///  對於一個選定的工作目錄，先檢查 CSS, Images, Scripts 檔案是否存在，再找出包含有 xtalk.proj 的子目錄。
        /// </summary>
        private void PrepareWorkDir()
        {
            App app = Application.Current as App;
            FileInfo exeFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            String pathName = Path.Combine(exeFileInfo.Directory.FullName, "Assets");
            DirectoryInfo assetDir = new DirectoryInfo(pathName);
            DirectoryInfo workDir = new DirectoryInfo(app.WorkDir);
            Debug.WriteLine(String.Format("MainPage.PrepareWorkDir({0})", app.WorkDir));

            #region 準備 xtalkshow.css, Images, 以及 Scripts 目錄.
            /// 檢查 xtalkshow.css 是否存在:
            pathName = Path.Combine(app.WorkDir, "xtalkshow.css");
            if (File.Exists(pathName) == false)
            {
                String assetFileName = Path.Combine(assetDir.FullName, "xtalkshow.css");
                Debug.WriteLine(String.Format("copy {0} to {1}", assetFileName, pathName));
                File.Copy(assetFileName, pathName);
            }

            /// 檢查 Images 目錄以及當中的必須檔案是否存在:
            String imagesDir = Path.Combine(app.WorkDir, "Images");
            if (Directory.Exists(imagesDir) == false)
            {
                Debug.WriteLine(String.Format("create directory {0}", imagesDir));
                Directory.CreateDirectory(imagesDir);
            }

            String[] imageFileNames =
            {
                "xtalkframe-full.png", "xtalkframe-half.png", "xtalkspeaker.png", "xtalknext.png",
                "music_on.png", "sound_off.png", "sound_on.png", "sound_off.png"
            };

            foreach (String fileName in imageFileNames)
            {
                pathName = Path.Combine(imagesDir, fileName);
                if (File.Exists(pathName) == false)
                {
                    String assetFileName = Path.Combine(assetDir.FullName, fileName);
                    Debug.WriteLine(String.Format("copy {0} to {1}", assetFileName, pathName));
                    File.Copy(assetFileName, pathName);
                }
            }

            /// 檢查 Scripts 目錄以及當中的必須檔案是否存在:
            String jsDir = Path.Combine(app.WorkDir, "Scripts");
            if (Directory.Exists(jsDir) == false)
            {
                Debug.WriteLine(String.Format("create directory {0}", jsDir));
                Directory.CreateDirectory(jsDir);
            }

            pathName = Path.Combine(jsDir, "xtalkshow-1.0-min.js");
            if (File.Exists(pathName) == false)
            {
                String assetFileName = Path.Combine(assetDir.FullName, "xtalkshow-1.0-min.js");
                Debug.WriteLine(String.Format("copy {0} to {1}", assetFileName, pathName));
                File.Copy(assetFileName, pathName);
            }
            #endregion

            /// 找出 app.WorkDir 中所有包含 xtalk.proj 的子目錄。
            m_projects.Clear();

            String[] directories = Directory.GetDirectories(app.WorkDir);
            foreach (String dir in directories)
            {
                String projPath = Path.Combine(app.WorkDir, dir);
                pathName = Path.Combine(projPath, "xtalk.proj");
                if (File.Exists(pathName))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(projPath);
                    m_projects.Add(dirInfo);
                    Debug.WriteLine(String.Format("  found project #{0}: {1}", m_projects.Count, dirInfo.Name));
                }
            }

            /// 更新 WorkDirLabel 的顯示:
            WorkDirLabel.Text = app.WorkDir;
            NewProjButton.IsEnabled = true;
            EditProjButton.IsEnabled = true;
            DelProjButton.IsEnabled = true;
        }

        /// <summary>
        ///  開啟 FolderBrowserDialog 來選擇新的工作目錄。
        /// </summary>
        private void SeletWorkDirButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                App app = Application.Current as App;
                app.WorkDir = dialog.SelectedPath;
                PrepareWorkDir();
            }
        }

        /// <summary>
        ///  以 MessageBox 顯示說明訊息。
        /// </summary>
        private void SelectWorkDirHelp_Click(object sender, RoutedEventArgs e)
        {   MessageBox.Show(Properties.Messages.helpSelectWorkDirectory);  }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  建立新的腳本專案子目錄。
        /// </summary>
        private void NewProjButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxDialog dialog = new TextBoxDialog();
            dialog.Title = Properties.Resources.BtnNew;
            dialog.Message = Properties.Messages.msgNewProjectName;
            dialog.EmptyTextAllowed = false;

            if (dialog.ShowDialog() == true)
            {
                App app = Application.Current as App;
                String pathName = Path.Combine(app.WorkDir, dialog.Text);
                Debug.WriteLine("MainPage.NewProjButton_Click():");

                /// 建立子目錄:
                if (Directory.Exists(pathName) == false)
                { 
                    try
                    {   Directory.CreateDirectory(pathName);  }
                    catch (Exception ex)
                    {   MessageBox.Show(ex.Message);  return;  }
                }

                /// 複製一個空的 xtalk.proj 檔案到新建立的子目錄:
                DirectoryInfo dirInfo = new DirectoryInfo(pathName);
                String projFileName = Path.Combine(dirInfo.FullName, "xtalk.proj");

                if (File.Exists(projFileName) == false)
                {
                    FileInfo exeFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                    pathName = Path.Combine(exeFileInfo.Directory.FullName, "Assets");
                    DirectoryInfo assetDir = new DirectoryInfo(pathName);
                    String assetFileName = Path.Combine(assetDir.FullName, "xtalk.proj");
                    Debug.WriteLine(String.Format("copy {0} to {1}", assetFileName, projFileName));

                    try
                    {   File.Copy(assetFileName, projFileName);  }
                    catch (Exception ex)
                    {   MessageBox.Show(ex.Message);  return;  }

                    /// 加入到 ProjectList 當中:
                    m_projects.Add(dirInfo);
                }
            }
        }

        /// <summary>
        ///  刪除目前選定的腳本目錄，並且從 ProjectList 移除之。
        /// </summary>
        private void DelProjButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectList.SelectedItem != null)
            {
                if (MessageBox.Show(Properties.Messages.warnRUSureToDelete,
                        Properties.Resources.BtnDelete, MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    DirectoryInfo dir = ProjectList.SelectedItem as DirectoryInfo;
                    m_projects.Remove(dir);
                    Directory.Delete(dir.FullName, true);
                }
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  載入腳本專案並且導覽到 ProjectPage 去編輯指定的腳本專案。
        /// </summary>
        private void EditProjButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectList.SelectedItem == null) { return; }

            /// 從 xtalk.proj 檔案載入目前選定的腳本專案:
            DirectoryInfo dir = ProjectList.SelectedItem as DirectoryInfo;
            XTProject project = new XTProject(dir.Name);
            Debug.WriteLine(String.Format("MainPage.EditProjButton_Click({0}):", project.Dir));

            try
            {
                project.LoadXml();
                foreach (XTScene scene in project.Scenes) {  scene.LoadXml();  }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
                return;
            }

            /// 移動到新的 ProjectPage 頁面:
            App app = Application.Current as App;
            app.Project = project;
            NavigationService.Navigate(new ProjectPage());
        }

        /// <summary>
        ///  同上。
        /// </summary>
        private void ProjectList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   EditProjButton_Click(null, null);  }
    }
}
