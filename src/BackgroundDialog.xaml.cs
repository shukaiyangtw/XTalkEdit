/// -----------------------------------------------------------------------------------------------
/// <summary>
///     背景圖片選擇對話框
/// </summary>
/// <remarks>
///     這個視窗包含 ListBox 和 Image 控制項，在 Window_Loaded() 繫結 app.Project.Backgrounds 到 ListBox
///     上，每當使用者在 ListBox 選擇一個檔名項目的時候，就把圖片顯示在 Image 控制項，當使用者按下確定的時候，就把
///     它紀錄在 FileName 資料成員當中。
/// </remarks>
/// <history>
///     2021/7/25 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace XTalkEdit
{
    public partial class BackgroundDialog : Window
    {
        private String m_fileName = String.Empty;
        public String FileName {  get {  return m_fileName;  }  }

        public BackgroundDialog()
        {   InitializeComponent();  }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            BackgroundList.ItemsSource = app.Project.Backgrounds;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackgroundList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            m_fileName = BackgroundList.SelectedItem as String;
            String pathName = m_fileName;

            if (m_fileName.StartsWith("../") == true)
            {   pathName = app.WorkDir + m_fileName.Substring(2).Replace('/', '\\');  }
            else
            {   pathName = Path.Combine(app.Project.Dir, m_fileName);  }

            BitmapImage img = App.LoadImageFile(pathName);
            if (img != null) {   PreviewImage.Source = img;  }
        }

        /// <summary>
        ///  使用 FileDialog 選擇背景圖片。
        /// </summary>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "JPEG files (*.jpg)|*.jpg|PNG files (*.png)|*.png";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;

            /// 開啟檔案對話盒，選取影像檔案 srcPathName:
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel) {  return;  }

            App app = Application.Current as App;
            String srcPathName = dialog.FileName;
            String fileName = Path.GetFileName(srcPathName);
            Debug.WriteLine(String.Format("BackgroundDialog.ImportButton_Click({0})", srcPathName));

            String destPathName = destPathName = Path.Combine(app.Project.Dir, fileName);
            if (File.Exists(destPathName) == false)
            {
                if (srcPathName.StartsWith(app.WorkDir, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    /// 如果是同工作目錄，不同專案目錄的共享檔案，不複製檔案，只產生相對連結:
                    fileName = ".." + srcPathName.Substring(app.WorkDir.Length).Replace('\\', '/');
                    Debug.WriteLine(String.Format("  shared file name:({0})", fileName));
                }
                else
                {
                    /// 複製選定的檔案到專案目錄:
                    try
                    {   File.Copy(srcPathName, destPathName);  }
                    catch (Exception ex)
                    {   MessageBox.Show(ex.Message);  return;  }

                    Debug.WriteLine(String.Format("  added file name:({0})", fileName));
                }
            }
            else if (destPathName.Equals(srcPathName) == false)
            {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists);  return;  }

            /// 在 Image 控制項當中顯示新的背景圖片:
            BitmapImage img = App.LoadImageFile(destPathName);
            if (img != null) {   PreviewImage.Source = img;  }
            m_fileName = fileName;

            /// 將 fileName 插入 Backgrounds 中的適當位置:
            int pos = -1, i = 0, compared = 0;
            foreach (String item in app.Project.Backgrounds)
            {
                compared = String.Compare(m_fileName, item);
                if (compared == 0) {  return; }
                if (compared < 0) {  pos = i;  break;  }
                ++i;
            }

            if (pos == -1) {  app.Project.Backgrounds.Add(m_fileName);   }
            else {  app.Project.Backgrounds.Insert(pos, m_fileName);  }

         /* app.Project.isModified = true; */
            app.Project.SaveXml();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(m_fileName)) {  return;  }
            DialogResult = true;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            m_fileName = String.Empty;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {   DialogResult = false;  }
    }
}
