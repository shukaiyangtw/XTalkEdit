/// -----------------------------------------------------------------------------------------------
/// <summary>
///     腳本專案頁面
/// </summary>
/// <remarks>
///     這個頁面解讀 xtalk.proj，並把當中的場景腳本與檔案資產列在 ListBox 上。
/// </remarks>
/// <history>
///     2024/10/12 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XTalkEdit
{
    public partial class ProjectPage : Page
    {
        /// 用來播放音效檔案的物件:
        private MediaPlayer m_mediaPlayer = new MediaPlayer();

        public ProjectPage()
        {
            InitializeComponent();
            m_mediaPlayer.MediaEnded += delegate {  m_mediaPlayer.Close();  };
        }

        #region Page load and back events.
        /// <summary>
        ///  把 app.Project 的內容繫結到頁面上。
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            WidthTextBox.Text = app.Project.PxWidth.ToString();
            HeightTextBox.Text = app.Project.PxHeight.ToString();
            app.Project.isModified = false;

            /// 指定 ObservableCollection 和 ListBox 之間的資料繫結:
            SceneList.ItemsSource = app.Project.Scenes;
            BackgroundList.ItemsSource = app.Project.Backgrounds;
            SoundList.ItemsSource = app.Project.Sounds;
            SpriteList.ItemsSource = app.Project.Sprites;
        }

        /// <summary>
        ///  當 TextBox 變動的時候，把數值寫回 PxWidth 和 PxHeight，但是避開零值。
        /// </summary>
        private void Width_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value = 0;

            if (Int32.TryParse(WidthTextBox.Text, out value))
            {
                if (value > 0)
                {
                    App app = Application.Current as App;
                    app.Project.PxWidth = value;
                    app.Project.isModified = true;
                 /* Debug.WriteLine(String.Format("ProjectPage.Width_TextChanged({0})", WidthTextBox.Text)); */
                }
            }
        }

        private void Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value = 0;

            if (Int32.TryParse(HeightTextBox.Text, out value))
            {
                if (value > 0)
                {
                    App app = Application.Current as App;
                    app.Project.PxHeight = value;
                    app.Project.isModified = true;
                 /* Debug.WriteLine(String.Format("ProjectPage.Height_TextChanged({0})", HeightTextBox.Text)); */
                }
            }
        }

        /// <summary>
        ///  儲存 xtalk.proj 以後返回 MainPage。
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            Debug.WriteLine("ProjectPage.BackButton_Click()");

            /// 返回 MainPage 之前檢查場景和專案檔案是否需要儲存:
            try
            {
                foreach (XTScene scene in app.Project.Scenes)
                {   if (scene.isModified == true) {  scene.SaveXml();   }  }

                if (app.Project.isModified == true) {  app.Project.SaveXml();  }
            }
            catch (Exception ex)
            {   MessageBox.Show(ex.Message);  return;  }

            /// 清理目前已載入的 app.Project 並且返回 MainPage:
            if (this.NavigationService.CanGoBack)
            {
                app.Project.Scenes.Clear();
                app.Project = null;

                Debug.WriteLine("ProjectPage.NavigationService.GoBack()");
                this.NavigationService.GoBack();
            }
        }

        /// <summary>
        ///  顯示關於檔案資產的說明訊息。
        /// </summary>
        private void AssetFileHelp_Click(object sender, RoutedEventArgs e)
        {   MessageBox.Show(Properties.Messages.helpSelectWorkDirectory);  }
        #endregion

        #region Scenes.
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  建立新的場景腳本檔案 (scene_name).xtalk。
        /// </summary>
        private void NewSceneButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxDialog dialog = new TextBoxDialog();
            dialog.Title = Properties.Resources.BtnNew;
            dialog.Message = Properties.Messages.msgNewSceneName;
            dialog.EmptyTextAllowed = false;

            if (dialog.ShowDialog() == true)
            {
                App app = Application.Current as App;
                String xtalkFileName = dialog.Text + ".xtalk";
                String xtalkPathName = Path.Combine(app.Project.Dir, xtalkFileName);
                Debug.WriteLine(String.Format("ProjectPage.NewSceneButton_Click({0}", xtalkFileName));

                if (File.Exists(xtalkPathName) == true)
                {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists);  return;  }

                /// 將 Assets/empty.xtalk 複製為 xtalkPathName:
                FileInfo exeFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                String assetsPath = Path.Combine(exeFileInfo.Directory.FullName, "Assets");
                DirectoryInfo assetDir = new DirectoryInfo(assetsPath);
                String assetFileName = Path.Combine(assetDir.FullName, "empty.xtalk");
                Debug.WriteLine(String.Format("copy {0} to {1}", assetFileName, xtalkPathName));

                try
                {   File.Copy(assetFileName, xtalkPathName);  }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  return;  }

                /// 加入到 scene list:
                XTScene scene = new XTScene(app.Project, dialog.Text);
                app.Project.Scenes.Add(scene);
             /* app.Project.isModified = true; */
                app.Project.SaveXml();
            }
        }

        /// <summary>
        ///  導覽到 ScenePage 去編輯指定的腳本檔案。
        /// </summary>
        private void EditSceneButton_Click(object sender, RoutedEventArgs e)
        {
            if (SceneList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            app.Scene = SceneList.SelectedItem as XTScene;

            /* sprite 更名可能會造成 scene 在編輯前就被更動，所以要先檢查並儲存: */
            if (app.Scene.isModified)
            {
                try {  app.Scene.SaveXml();  }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  return;  }
            }

            Debug.WriteLine(String.Format("ProjectPage.EditSceneButton_Click({0})", app.Scene.Name));
            NavigationService.Navigate(new ScenePage());
        }

        private void SceneList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   EditSceneButton_Click(null, null);  }

        /// <summary>
        ///  將 .xtalk 檔案改名。
        /// </summary>
        private void RenameSceneButton_Click(object sender, RoutedEventArgs e)
        {
            if (SceneList.SelectedItem == null) {  return;  }
            XTScene scene = SceneList.SelectedItem as XTScene;

            TextBoxDialog dialog = new TextBoxDialog();
            dialog.Title = Properties.Resources.BtnRename;
            dialog.Message = Properties.Messages.msgRenameScene;
            dialog.EmptyTextAllowed = false;
            dialog.Text = scene.Name;

            if (dialog.ShowDialog() == true)
            {
                Debug.WriteLine(String.Format("ProjectPage.RenameSceneButton_Click({0} to {1})", scene.Name, dialog.Text));

                try
                {   scene.Rename(dialog.Text);  }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  return;  }

                SceneList.Items.Refresh();
            }
        }

        /// <summary>
        ///  刪除 .xtalk 檔案並將它從 Scenes 當中移除。
        /// </summary>
        private void DelSceneButton_Click(object sender, RoutedEventArgs e)
        {
            if (SceneList.SelectedItem == null) {  return;  }
            if (MessageBox.Show(Properties.Messages.warnRUSureToDelete,
                    Properties.Resources.BtnDelete, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No) {  return;  }

            XTScene scene = SceneList.SelectedItem as XTScene;
            Debug.WriteLine(String.Format("ProjectPage.DelSceneButton_Click({0})", scene.Name));

            App app = Application.Current as App;
            String fileName = scene.Name + ".xtalk";
            String pathName = Path.Combine(app.Project.Dir, fileName);

            try
            {   File.Delete(pathName);  }
            catch (Exception ex)
            {   MessageBox.Show(ex.Message);  return;  }

            fileName = scene.Name + ".js";
            pathName = Path.Combine(app.Project.Dir, fileName);
            if (File.Exists(pathName)) {  File.Delete(pathName);  }

            fileName = scene.Name + ".html";
            pathName = Path.Combine(app.Project.Dir, fileName);
            if (File.Exists(pathName)) {  File.Delete(pathName);  }

            app.Project.Scenes.Remove(scene);
            SceneList.Items.Refresh();
         /* app.Project.isModified = true; */
            app.Project.SaveXml();
        }
        #endregion

        #region Backgrounds.
        /// ///////////////////////////////////////////////////////////////////////////////////////
        private void BackgroundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackgroundList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            String fileName = BackgroundList.SelectedItem as String;
            String pathName = fileName;

            if (fileName.StartsWith("../") == true)
            {   pathName = app.WorkDir + fileName.Substring(2).Replace('/', '\\');  }
            else
            {   pathName = Path.Combine(app.Project.Dir, fileName);  }

            BitmapImage img = App.LoadImageFile(pathName);
            if (img != null) {   PreviewImage.Source = img;  }
        }

        /// <summary>
        ///  使用 FileDialog 選擇背景圖片。
        /// </summary>
        private void NewBgButton_Click(object sender, RoutedEventArgs e)
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
            Debug.WriteLine(String.Format("ProjectPage.NewBgButton_Click({0})", srcPathName));

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
            else if (destPathName.Equals(srcPathName) == false)
            {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists);  return;  }

            /// 載入這張圖片到 PreviewImage，並且量測尺寸:
            BitmapImage bitmap = new BitmapImage(new Uri(destPathName));
            if (app.Project.PxWidth == 0)
            {   WidthTextBox.Text = bitmap.PixelWidth.ToString();  }
            if (app.Project.PxHeight == 0)
            {   HeightTextBox.Text = bitmap.PixelHeight.ToString();  }
            bitmap = null;

            bitmap = App.LoadImageFile(destPathName);
            if (bitmap != null) {   PreviewImage.Source = bitmap;  }

            /// 將 fileName 插入 Backgrounds 中的適當位置:
            int pos = -1, i = 0, compared = 0;
            foreach (String item in app.Project.Backgrounds)
            {
                compared = String.Compare(fileName, item);
                if (compared == 0) {  return; }
                if (compared < 0) {  pos = i;  break;  }
                ++i;
            }

            if (pos == -1) {  app.Project.Backgrounds.Add(fileName);   }
            else {  app.Project.Backgrounds.Insert(pos, fileName);  }
            app.Project.isModified = true;
        }

        /// <summary>
        ///  輸入新的背景圖檔案名稱。
        /// </summary>
        private void RenameBgButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackgroundList.SelectedItem == null) {  return;  }
            String fileName = BackgroundList.SelectedItem as String;

            TextBoxDialog dialog = new TextBoxDialog();
            dialog.Title = Properties.Resources.BtnRename;
            dialog.Message = Properties.Messages.msgRenameScene;
            dialog.EmptyTextAllowed = false;
            dialog.Text = fileName;

            if (dialog.ShowDialog() == true)
            {
                String newFileName = dialog.Text;
                if (newFileName.Equals(fileName)) {  return;  }
                Debug.WriteLine(String.Format("ProjectPage.RenameBgButton_Click({0} to {1})", fileName, newFileName));

                /// 檢查專案目錄內是否已經有同名的檔案，如果是就不允許更名:
                App app = Application.Current as App;
                String newPathName = Path.Combine(app.Project.Dir, newFileName);
                if (File.Exists(newPathName))
                {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists); return;  }

                /// 變更檔案名稱:
                String oldPathName = Path.Combine(app.Project.Dir, fileName);

                try
                {   File.Move(oldPathName, newPathName);  }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  return;  }

                /// 將 newFileName 重新插入 Backgrounds 中的適當位置:
                int index = BackgroundList.SelectedIndex;
                app.Project.Backgrounds.RemoveAt(index);

                int pos = -1, i = 0;
                foreach (String item in app.Project.Backgrounds)
                {
                    if (String.Compare(newFileName, item) < 0) {  pos = i;  break;  }
                    ++i;
                }

                if (pos == -1) {  app.Project.Backgrounds.Add(newFileName);  }
                else {  app.Project.Backgrounds.Insert(pos, newFileName);  }
                app.Project.isModified = true;

                /// 檢查場景內有沒有使用到這個背景圖的畫面，若有則一併更名:
                foreach (XTScene scene in app.Project.Scenes)
                {
                    foreach (XTFrame frame in scene.Frames)
                    {
                        if (fileName.Equals(frame.Background))
                        {
                            frame.Background = newFileName;
                            scene.isModified = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        ///   刪除背景圖檔。
        /// </summary>
        private void DelBgButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackgroundList.SelectedItem == null) { return; }
            if (MessageBox.Show(Properties.Messages.warnRUSureToDelete,
                    Properties.Resources.BtnDelete, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No) {  return;  }

            String fileName = BackgroundList.SelectedItem as String;
            int index = BackgroundList.SelectedIndex;

            /// 從 Backgrounds 當中移除選定的項目:
            App app = Application.Current as App;
            app.Project.Backgrounds.RemoveAt(index);
            app.Project.isModified = true;
            BackgroundList.Items.Refresh();


            /// 檢查場景內有沒有使用到這個背景圖的畫面，若有則一併移除:
            foreach (XTScene scene in app.Project.Scenes)
            {
                foreach (XTFrame frame in scene.Frames)
                {
                    if (fileName.Equals(frame.Background))
                    {
                        frame.Background = String.Empty;
                        scene.isModified = true;
                    }
                }
            }

            if (fileName.IndexOf('/') == -1)
            {
                /// 如果非相對路徑的共用檔案，則刪除檔案:
                String pathName = Path.Combine(app.Project.Dir, fileName);

                try
                { File.Delete(pathName); }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }
            }
        }
        #endregion

        #region Sprites.
        /// ///////////////////////////////////////////////////////////////////////////////////////
        private void SpriteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpriteList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            XTNamedFileItem item = SpriteList.SelectedItem as XTNamedFileItem;
            String pathName = item.FileName;

            if (item.FileName.StartsWith("../") == true)
            {   pathName = app.WorkDir + item.FileName.Substring(2).Replace('/', '\\');  }
            else
            {   pathName = Path.Combine(app.Project.Dir, item.FileName);  }

            BitmapImage img = App.LoadImageFile(pathName);
            if (img != null) {   PreviewImage.Source = img;  }
        }

        /// <summary>
        ///  使用 FileDialog 選擇角色圖片。
        /// </summary>
        private void NewSpriteButton_Click(object sender, RoutedEventArgs e)
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
            Debug.WriteLine(String.Format("ProjectPage.NewSpriteButton_Click({0})", srcPathName));

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

            String destPathName = Path.Combine(app.Project.Dir, fileName);
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
            else if (srcPathName.Equals(destPathName) == false)
            {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists);  return;  }

            /// 在預覽視窗顯示新圖片:
            BitmapImage img = App.LoadImageFile(destPathName);
            if (img != null) {   PreviewImage.Source = img;  }

            /// 產生一個新的 XTNamedFileItem 並且插入到 Sprites 當中:
            XTNamedFileItem newItem = new XTNamedFileItem();
            newItem.ID = newID;
            newItem.FileName = fileName;
            XTNamedFileItem.InsertByID(app.Project.Sprites, newItem);
            app.Project.isModified = true;
        }

        /// <summary>
        ///  更改目前選定 Sprite 的 ID。
        /// </summary>
        private void RenameSpriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            XTNamedFileItem selectedItem = SpriteList.SelectedItem as XTNamedFileItem;
            String newID = selectedItem.ID;
            Boolean IDisUnique = true;

            do
            {
                /// 開啟 TextBoxDialog 為這個新項目指定一個唯一的新 ID:
                TextBoxDialog dlg = new TextBoxDialog();
                dlg.Title = Properties.Resources.BtnRename;
                dlg.Message = Properties.Messages.msgNewID;
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

            /// 將被選擇的項目之 ID 指定為 newID，並且重新插入 SpriteList:
            Debug.WriteLine(String.Format("ProjectPage.RenameSpriteButton_Click({0} to {1})", selectedItem.ID, newID));
            app.Project.Sprites.Remove(selectedItem);
            selectedItem.ID = newID;
            XTNamedFileItem.InsertByID(app.Project.Sprites, selectedItem);
            app.Project.isModified = true;

            /// 檢查場景中的 Scenes 有沒有用到此 ID 者:
            foreach (XTScene scene in app.Project.Scenes)
            {
                foreach (XTFrame frame in scene.Frames)
                {
                    if (selectedItem.Equals(frame.Left))
                    {   scene.isModified = true;  }
                    else if (selectedItem.Equals(frame.Left2))
                    {   scene.isModified = true;  }
                    else if (selectedItem.Equals(frame.Center))
                    {   scene.isModified = true;  }
                    else if (selectedItem.Equals(frame.Right))
                    {   scene.isModified = true;  }
                    else if (selectedItem.Equals(frame.Right2))
                    {   scene.isModified = true;  }

                    if (scene.isModified == true) {  break;  }
                }
            }
        }

        /// <summary>
        ///  刪除選定的 Sprite 項目。
        /// </summary>
        private void DelSpriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteList.SelectedItem == null) {  return;  }
            if (MessageBox.Show(Properties.Messages.warnRUSureToDelete,
                    Properties.Resources.BtnDelete, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No) {  return;  }

            /// 先從 Sprites 當中移除選定的項目:
            App app = Application.Current as App;
            XTNamedFileItem selectedItem = SpriteList.SelectedItem as XTNamedFileItem;
            app.Project.Sprites.Remove(selectedItem);
            app.Project.isModified = true;
            SpriteList.Items.Refresh();

            /// 檢查場景中的 Scenes 有沒有用到此項目者:
            foreach (XTScene scene in app.Project.Scenes)
            {
                foreach (XTFrame frame in scene.Frames)
                {
                    if (selectedItem.Equals(frame.Left))
                    {   frame.Left = null;  scene.isModified = true;  }

                    if (selectedItem.Equals(frame.Left2))
                    {   frame.Left2 = null;  scene.isModified = true;  }

                    if (selectedItem.Equals(frame.Center))
                    {   frame.Center = null;  scene.isModified = true;  }

                    if (selectedItem.Equals(frame.Right))
                    {   frame.Right = null;  scene.isModified = true;  }

                    if (selectedItem.Equals(frame.Right2))
                    {   frame.Right2 = null;  scene.isModified = true;  }
                }
            }

            if (selectedItem.FileName.IndexOf('/') == -1)
            {
                /// 檢查是否可以安全地刪除這個檔案:
                foreach (XTNamedFileItem item in app.Project.Sprites)
                {
                    if (item.FileName.Equals(selectedItem.FileName))
                    {   return;  }
                }

                /// 如果非相對路徑的共用檔案，而且沒有其他 sprite 使用這個圖檔，則刪除檔案:
                String pathName = Path.Combine(app.Project.Dir, selectedItem.FileName);
                try { File.Delete(pathName); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        #endregion

        #region Sounds.
        /// ///////////////////////////////////////////////////////////////////////////////////////
        private void SoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoundList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            XTNamedFileItem item = SoundList.SelectedItem as XTNamedFileItem;
            String pathName = item.FileName;

            if (item.FileName.StartsWith("../") == true)
            {   pathName = app.WorkDir + item.FileName.Substring(2).Replace('/', '\\');  }
            else
            {   pathName = Path.Combine(app.Project.Dir, item.FileName);  }

            Uri uri = new Uri(pathName);
            m_mediaPlayer.Open(uri);
            m_mediaPlayer.Play();
        }

        /// <summary>
        ///  使用 FileDialog 選擇音效檔案。
        /// </summary>
        private void NewSoundButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "MP3 Audio files (*.mp3)|*.mp3|Ogg Vorbis files (*.ogg)|*.ogg";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;

            /// 開啟檔案對話盒，選取音效檔案 srcPathName:
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel) {  return;  }

            App app = Application.Current as App;
            String srcPathName = dialog.FileName;
            String fileName = Path.GetFileName(srcPathName);
            Debug.WriteLine(String.Format("ProjectPage.NewSoundButton_Click({0})", srcPathName));

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
                foreach (XTNamedFileItem item in app.Project.Sounds)
                {
                    if (newID.Equals(item.ID))
                    {   IDisUnique = false;  break;  }
                }

            } while (IDisUnique == false);

            String destPathName = Path.Combine(app.Project.Dir, fileName);
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
            else if (srcPathName.Equals(destPathName) == false)
            {   MessageBox.Show(Properties.Messages.warnFileAlreadyExists);  return;  }

            /// 產生一個新的 XTNamedFileItem 並且插入到 Sounds 當中:
            XTNamedFileItem newItem = new XTNamedFileItem();
            newItem.ID = newID;
            newItem.FileName = fileName;
            XTNamedFileItem.InsertByID(app.Project.Sounds, newItem);
            app.Project.isModified = true;
        }

        /// <summary>
        ///  更改目前選定的 Sound 的 ID。
        /// </summary>
        private void RenameSoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundList.SelectedItem == null) {  return;  }

            App app = Application.Current as App;
            XTNamedFileItem selectedItem = SoundList.SelectedItem as XTNamedFileItem;
            String newID = selectedItem.ID;
            Boolean IDisUnique = true;

            do
            {
                /// 開啟 TextBoxDialog 為這個新項目指定一個唯一的新 ID:
                TextBoxDialog dlg = new TextBoxDialog();
                dlg.Title = Properties.Resources.BtnRename;
                dlg.Message = Properties.Messages.msgNewID;
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

            /// 將被選擇的項目之 ID 指定為 newID，並且重新插入 SoundList:
            Debug.WriteLine(String.Format("ProjectPage.RenameSoundButton_Click({0} to {1})", selectedItem.ID, newID));
            app.Project.Sounds.Remove(selectedItem);
            selectedItem.ID = newID;
            XTNamedFileItem.InsertByID(app.Project.Sounds, selectedItem);
            app.Project.isModified = true;

            /// 檢查場景中的 Scenes 有沒有用到此 ID 者:
            foreach (XTScene scene in app.Project.Scenes)
            {
                foreach (XTFrame frame in scene.Frames)
                {
                    if (selectedItem.Equals(frame.Sound)) {  scene.isModified = true;  }
                    if (scene.isModified == true) {  break;  }
                }
            }
        }

        /// <summary>
        ///  刪除選定的音效項目。 
        /// </summary>
        private void DelSoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundList.SelectedItem == null) {  return;  }
            if (MessageBox.Show(Properties.Messages.warnRUSureToDelete,
                    Properties.Resources.BtnDelete, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No) {  return;  }

            /// 先從 Sprites 當中移除選定的項目:
            App app = Application.Current as App;
            XTNamedFileItem selectedItem = SoundList.SelectedItem as XTNamedFileItem;
            app.Project.Sounds.Remove(selectedItem);
            app.Project.isModified = true;
            SoundList.Items.Refresh();

            /// 檢查場景中的 Scenes 有沒有用到此項目者:
            foreach (XTScene scene in app.Project.Scenes)
            {
                foreach (XTFrame frame in scene.Frames)
                {
                    if (selectedItem.Equals(frame.Sound))
                    {   frame.Sound = null;  scene.isModified = true;  }
                }
            }

            if (selectedItem.FileName.IndexOf('/') == -1)
            {
                /// 檢查是否可以安全地刪除這個檔案:
                foreach (XTNamedFileItem item in app.Project.Sounds)
                {
                    if (item.FileName.Equals(selectedItem.FileName))
                    {   return;  }
                }

                /// 如果非相對路徑的共用檔案，而且沒有其他 sound 使用這個音檔，於是刪除之:
                String pathName = Path.Combine(app.Project.Dir, selectedItem.FileName);
                try { File.Delete(pathName); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        #endregion

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  檢查所有 .js 和 .html 檔案的時間，並將需要更新的檔案加以重製。
        /// </summary>
        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            Debug.WriteLine("ProjectPage.OutputButton_Click():");

            /// 將所有尚未儲存的 XML 檔案存檔，以便檢查時間戳記:
            try
            {
                foreach (XTScene scene in app.Project.Scenes)
                {   if (scene.isModified == true) {  scene.SaveXml();   }  }

                if (app.Project.isModified == true) {  app.Project.SaveXml();  }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
                return;
            }

            /// 匯出檔案，如果匯出成功則開啟 exported.htm:
            XTProjectExporter exporter = new XTProjectExporter(app.Project);
            if (exporter.Run() == false)
            {
                Debug.WriteLine(exporter.ErrorMessage);
                MessageBox.Show(exporter.ErrorMessage);
                return;
            }

            String location = app.WorkDir.Replace('\\', '/');
            String fileUrl = String.Format("file:///{0}/{1}/{2}", location, app.Project.Name, XTProjectExporter.HomepageName);
            Debug.WriteLine(fileUrl);

            try {   Process.Start(fileUrl);  }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
    }
}
