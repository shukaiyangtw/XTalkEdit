/// -----------------------------------------------------------------------------------------------
/// <summary>
///     主要導覽視窗
/// </summary>
/// <remarks>
///     這個衍生自 NavigationWindow 用來導覽 MainPage, ProjectPage, 以及 ScenePage，並提供關閉時存檔的功能。
/// </remarks>
/// <history>
///     2018/11/19 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using XTalkEdit.Properties;

namespace XTalkEdit
{
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            App app = Application.Current as App;
            InitializeComponent();

            /// 初始化了 XAML 上的元件以後，載入先前儲存的視窗尺寸與位置。
            if (Settings.Default.WindowPos != null)
            {
                this.Left = Settings.Default.WindowPos.X;
                this.Top = Settings.Default.WindowPos.Y;
            }

            if (Settings.Default.WindowSize != null)
            {
                this.Width = Settings.Default.WindowSize.Width;
                this.Height = Settings.Default.WindowSize.Height;
            }

            /// 載入上次的工作目錄:
            if (string.IsNullOrEmpty(Settings.Default.WorkDir) == false)
            {   app.WorkDir = Settings.Default.WorkDir;  }
        }

        /// <summary>
        ///  在視窗要關閉之前，把更動過的專案內容存檔，並且記錄視窗尺寸與位置。
        /// </summary>
        private void NavigationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App app = Application.Current as App;
            String curPageType = NavigationService.Content.GetType().ToString();
            Debug.WriteLine(String.Format("NavigationWindow_Closing(): {0}", curPageType));

            /// 儲存目前的專案、包括所有已更動的場景:
            if (app.Project != null)
            {
                /// 以防萬一，儲存目前編輯中的畫面:
                if (curPageType.Equals("XTalkEdit.ScenePage"))
                {
                    ScenePage page = (ScenePage)NavigationService.Content;
                    if (page.IsModified) {  page.SaveCurFrame();  }
                }

                /// 儲存所有更動過的 XML 檔案:
                try
                { 
                    foreach (XTScene scene in app.Project.Scenes)
                    {   if (scene.isModified == true) {  scene.SaveXml();   }  }
                    if (app.Project.isModified == true) {  app.Project.SaveXml();  }
                }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  }
            }

            /// 紀錄工作目錄:
            if (string.IsNullOrEmpty(app.WorkDir) == false)
            {   Settings.Default.WorkDir = app.WorkDir;  }

            /// 紀錄視窗尺寸:
            if (this.WindowState == WindowState.Normal)
            {
                Settings.Default.WindowPos = new System.Drawing.Point((int)this.Left, (int)this.Top);
                Settings.Default.WindowSize = new System.Drawing.Size((int)this.Width, (int)this.Height);
            }
            else
            {
                Settings.Default.WindowPos = new System.Drawing.Point((int)RestoreBounds.Left, (int)RestoreBounds.Top);
                Settings.Default.WindowSize = new System.Drawing.Size((int)RestoreBounds.Size.Width, (int)RestoreBounds.Size.Height);
            }

            Settings.Default.Save();
        }
    }
}
