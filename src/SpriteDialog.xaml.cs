/// -----------------------------------------------------------------------------------------------
/// <summary>
///     角色圖片選擇對話框
/// </summary>
/// <remarks>
///     這個視窗包含一個 ListBox 和 Image 控制項，在 Window_Loaded()中繫結 app.Project.Sprites 到 ListBox
///     上，每當使用者在 ListBox 選擇一個 XTNamedFileItem 項目的時候，就把圖片顯示在 Image 控制項，當使用者按下
///     確定的時候，就把它紀錄在 SelectedItem 資料成員當中。
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
    public partial class SpriteDialog : Window
    {
        private XTNamedFileItem m_item = null;
        public XTNamedFileItem SelectedItem
        {   get {  return m_item;  }  }

        public SpriteDialog()
        {   InitializeComponent();  }

        /// <summary>
        ///  把 app.Project.Sprites 的內容繫結到頁面上。
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            SpriteList.ItemsSource = app.Project.Sprites;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpriteList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            m_item = SpriteList.SelectedItem as XTNamedFileItem;
            String pathName = m_item.FileName;

            if (m_item.FileName.StartsWith("../") == true)
            {   pathName = app.WorkDir + m_item.FileName.Substring(2).Replace('/', '\\');  }
            else
            {   pathName = Path.Combine(app.Project.Dir, m_item.FileName);  }

            BitmapImage img = App.LoadImageFile(pathName);
            if (img != null) {   PreviewImage.Source = img;  }
        }

        /// <summary>
        ///  使用 FileDialog 匯入新的角色圖片。
        /// </summary>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "PNG files (*.png)|*.png|JPEG files (*.jpg)|*.jpg";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;

            /// 開啟檔案對話盒，選取影像檔案 srcPathName:
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel) {  return;  }

            App app = Application.Current as App;
            String srcPathName = dialog.FileName;
            String fileName = Path.GetFileName(srcPathName);
            Debug.WriteLine(String.Format("SpriteDialig.ImportButton_Click({0})", srcPathName));

            String newID = Path.GetFileNameWithoutExtension(fileName);
            Boolean IDisUnique = true;

            do
            {
                /// 開啟 TextBoxDialog 為這個新項目指定一個唯一的新 ID:
                TextBoxDialog dlg = new TextBoxDialog();
                dlg.Title = Properties.Resources.BtnNew;
                dlg.Message = Properties.Messages.msgNewIDforFile;
                dlg.EmptyTextAllowed = false;
                dlg.Text = newID;
                if (dlg.ShowDialog() == false) {  return;  }

                /// 檢查輸入的新 ID 是否與他人重複:
                newID = dlg.Text;
                IDisUnique = true;
                foreach (XTNamedFileItem item in app.Project.Sprites)
                {
                    if (newID.Equals(item.ID))
                    {   IDisUnique = false;  break;  }
                }

            } while (IDisUnique == false);

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
                }

                Debug.WriteLine(String.Format("  added file name:({0})", fileName));
            }
            else if (srcPathName.Equals(destPathName) == false)
            {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists);  return;  }

            /// 在預覽視窗顯示新圖片:
            BitmapImage img = App.LoadImageFile(destPathName);
            if (img != null) {   PreviewImage.Source = img;  }

            /// 產生一個新的 XTNamedFileItem 並且插入到 Sprites 當中:
            m_item = new XTNamedFileItem();
            m_item.ID = newID;
            m_item.FileName = fileName;
            XTNamedFileItem.InsertByID(app.Project.Sprites, m_item);

         /* app.Project.isModified = true; */
            app.Project.SaveXml();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_item == null) {  return;  }
            DialogResult = true;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            m_item = null;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {   DialogResult = false;  }
    }
}
