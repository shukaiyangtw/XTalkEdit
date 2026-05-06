/// -----------------------------------------------------------------------------------------------
/// <summary>
///     全域物件
/// </summary>
/// <remarks>
///     這個衍生自 Application 的類別用來儲存這個程式的全域物件。
/// </remarks>
/// <history>
///     2018/12/28 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace XTalkEdit
{
    public partial class App : Application
    {
        /// <summary>
        ///  目前的工作目錄。
        /// </summary>
        public String WorkDir = String.Empty;

        /// 目前編輯中的專案:
        public XTProject Project = null;

        /// 目前編輯中的場景:
        public XTScene Scene = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            String[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; ++i)
            {
                if (args[i].Equals("en")) /// 基於測試目的，將整個程式切換成英文版:
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

                    FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
                }
            }
        }

        /* <summary>
        ///  遞迴地刪除指定目錄，效果同 Directory.Delete(dir, true) 但是會先取消檔案的唯讀屬性。
        /// </summary>
        static public void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        } */

        /// <summary>
        ///  要在 Image 當中顯示影像檔案可以簡單地
        ///  Uri uri = new Uri(pathName);
        ///  image.Source = new BitmapImage(uri);
        ///  但如此一來會造成 pathName 所指的檔案被鎖住，而無法更名或刪除，所以先將檔案內容複製到 memory stream
        ///  再建立 BitmapImage 物件並傳回之。
        /// </summary>
        static public BitmapImage LoadImageFile(String pathName)
        {
            Stream fs = null;
            try {  fs = File.Open(pathName, FileMode.Open, FileAccess.Read);  }
            catch (Exception ex) {  Debug.WriteLine(ex.Message);  return null;  }

            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);
            fs.Close();
            ms.Seek(0, SeekOrigin.Begin);

            BitmapImage img = new BitmapImage();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.BeginInit();
            img.StreamSource = ms;
            img.EndInit();
            return img;
        }

        /// <summary>
        ///  檢查檔案是否需要重新產製，pathName1 是原始資料檔案、例如 .xtalk 檔案，pathName2 是生成檔案，
        ///  例如 .js 或 .html 檔案，當 pathName1 的日期較新或 pathName2 根本不存在的時候，傳回 true。
        /// </summary>
        static public Boolean IsFileNewerThan(String pathName1, String pathName2)
        {
            if (File.Exists(pathName2) == false) {  return true; }

            DateTime dt1 = File.GetLastWriteTime(pathName1);

         /* 除非徹底地刪除舊檔案再重建，否則檔案重新覆寫的時候，creation time 仍然不會變，因此檢查檔案版本應
          * 該以 modified time (last-write time) 為準，然而作業系統基於自己的最佳化原則，不一定會在每一次
          * 寫入檔案的時候立即寫入正確的 modified time，此外，複製檔案的時候，產生的新檔案之 creation time
          * 是複製檔案的時間，可是 modified time 卻仍然是舊檔案的最後修改時間，因此會產生 modified time 比
          * creation time 還早的奇怪現象，因此要盡可能正確地比較檔案版本，要取兩者的最新值。 */
            DateTime dt = File.GetCreationTime(pathName1);
            if (DateTime.Compare(dt, dt1) > 0) {  dt1 = dt;  }

            DateTime dt2 = File.GetLastWriteTime(pathName2);
            dt = File.GetCreationTime(pathName2);
            if (DateTime.Compare(dt, dt2) > 0) {  dt2 = dt;  }

            if (DateTime.Compare(dt1, dt2) > 0) {  return true; }
            return false;
        }
    }
}
