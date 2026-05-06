/// -----------------------------------------------------------------------------------------------
/// <summary>
///     簡單的文字搜尋方塊
/// </summary>
/// <remarks>
///     這個視窗是個簡單的單行文字輸入方塊。
/// </remarks>
/// <history>
///     2019/1/9 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace XTalkEdit
{
    public partial class SearchDialog : Window
    {
        private int m_index = -1;
        public int Index
        {
            get {  return m_index;  }
            set {  m_index = value;  }
        }

        static private String m_text = String.Empty;
        static public String Text
        {
            get {  return m_text; }
            set {  m_text = value;  }
        }

        public SearchDialog()
        {   InitializeComponent();  }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {   InputTextBox.Text = m_text;  }

        /// <summary>
        ///  捕捉 Enter 鍵，相當於按下了 OK 按鈕。
        /// </summary>
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {   OkButton_Click(OkButton, null);  }
        }

        /// <summary>
        ///  把 InputTextBox.Text 抄錄到 m_text 並返回。
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            m_text = InputTextBox.Text.TrimEnd();
            if (String.IsNullOrEmpty(m_text))
            {
                MessageBox.Show(this, Properties.Messages.warnBlankNotAllowed,
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (sender.Equals(OkButton)) {  m_index = 0;  }

            /// 從 m_index 位置開始尋找:
            App app = Application.Current as App;
            for (int i=m_index; i<app.Scene.Frames.Count; ++i)
            {
                if (app.Scene.Frames[i].Text.Contains(m_text))
                {
                    m_index = i;
                    DialogResult = true;
                    return;
                }
            }

            m_index = -1;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {   DialogResult = false;  }
    }
}
