/// -----------------------------------------------------------------------------------------------
/// <summary>
///     路徑選項輸入對話方塊
/// </summary>
/// <remarks>
///     這個視窗對應 XTFrame.PathOption 的部分資料成員，提供視覺化的編輯頁面。
/// </remarks>
/// <history>
///     2018/12/26 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;

namespace XTalkEdit
{
    public partial class PathOptionDialog : Window
    {
        /// <summary>
        ///  對應XTFrame.PathOption 的部分資料成員。
        /// </summary>
        public XTFrame.LinkType Type = XTFrame.LinkType.NextFrame;
        public XTFrame.LinkTarget Target = XTFrame.LinkTarget.Self;
        public String Goto = String.Empty;
        public Boolean PackOpts = true;

        public PathOptionDialog()
        {   InitializeComponent();  }

        /// <summary>
        ///  把資料成員的內容抄錄到畫面上的控制項。
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /// 要放在 ComboBox 的其他標籤選項:
            App app = Application.Current as App;
            List<String> labels = new List<String>();
            foreach (XTFrame frame in app.Scene.Frames)
            {
                if (String.IsNullOrEmpty(frame.Label) == false)
                {   labels.Add(frame.Label);  }
            }

            LabelComboBox.ItemsSource = labels;

            /// 根據 Type 設定控制項的狀態:
            if (Type == XTFrame.LinkType.NextFrame)
            {
                NextFrameOptButton.IsChecked = true;
            }
            else if (Type == XTFrame.LinkType.Label)
            {
                LabelOptButton.IsChecked = true;
                LabelComboBox.Text = Goto;
            }
            else if (Type == XTFrame.LinkType.Url)
            {
                WebPageOptButton.IsChecked = true;
                UrlTextBox.Text = Goto;
                PackOptsCheckBox.IsChecked = PackOpts;

                if (Target == XTFrame.LinkTarget.Self)
                {   HrefSelfButton.IsChecked = true;  }
                else if (Target == XTFrame.LinkTarget.Top)
                {   HrefTopButton.IsChecked = true;  }
                else if (Target == XTFrame.LinkTarget.Blank)
                {   HrefBlankButton.IsChecked = true;  }
            }
        }

        /// <summary>
        ///  焦點移動到輸入框的時候自動勾選相對應的 radio button。
        /// </summary>
        private void LabelComboBox_GotFocus(object sender, RoutedEventArgs e)
        {   LabelOptButton.IsChecked = true;  }

        private void UrlTextBox_GotFocus(object sender, RoutedEventArgs e)
        {   WebPageOptButton.IsChecked = true;  }

        /// <summary>
        ///  把畫面上的控制項內容抄錄回資料成員。
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (NextFrameOptButton.IsChecked == true)
            {
                Type = XTFrame.LinkType.NextFrame;
            }
            else if (LabelOptButton.IsChecked == true)
            {
                if (String.IsNullOrEmpty(LabelComboBox.Text))
                {
                    MessageBox.Show(Properties.Messages.warnBlankNotAllowed);
                    return;
                }

                Type = XTFrame.LinkType.Label;
                Goto = LabelComboBox.Text;
            }
            else if (WebPageOptButton.IsChecked == true)
            {
                if (String.IsNullOrEmpty(UrlTextBox.Text))
                {
                    MessageBox.Show(Properties.Messages.warnBlankNotAllowed);
                    return;
                }

                Type = XTFrame.LinkType.Url;
                Goto = UrlTextBox.Text;

                if (HrefSelfButton.IsChecked == true) {  Target = XTFrame.LinkTarget.Self;  }
                else if (HrefTopButton.IsChecked == true) {  Target = XTFrame.LinkTarget.Top;  }
                else if (HrefBlankButton.IsChecked == true) {  Target = XTFrame.LinkTarget.Blank;  }

                if (PackOptsCheckBox.IsChecked == true) {  PackOpts = true;  }
                else {  PackOpts = false;  }
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {   DialogResult = false;  }
    }
}
