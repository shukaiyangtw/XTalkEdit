/// -----------------------------------------------------------------------------------------------
/// <summary>
///     簡單的文字輸入對話方塊
/// </summary>
/// <remarks>
///     這個視窗是個簡單的單行文字輸入方塊。
/// </remarks>
/// <history>
///     2018/11/15 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace XTalkEdit
{
    public partial class TextBoxDialog : Window
    {
        private String m_msg = String.Empty;
        private String m_text = String.Empty;
        private Boolean m_allowEmptyText = true;

        public String Text
        {
            get {  return m_text; }
            set {  m_text = value;  }
        }

        public String Message
        {   set {  m_msg = value;  }  }

        public Boolean EmptyTextAllowed
        {   set {  m_allowEmptyText = value;  }  }

        public TextBoxDialog()
        {   InitializeComponent();  }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(m_msg) == false) {  MessageLabel.Text = m_msg;  }
            if (String.IsNullOrEmpty(m_text) == false) {  InputTextBox.Text = m_text;  }
        }

        /// <summary>
        ///  捕捉 Enter 鍵，相當於按下了 OK 按鈕。
        /// </summary>
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {   OkButton_Click(null, null);  }
        }

        /// <summary>
        ///  把 InputTextBox.Text 抄錄到 m_text 並返回。
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            m_text = InputTextBox.Text.TrimEnd();

            if ((m_allowEmptyText == false) && (String.IsNullOrEmpty(m_text)))
            {
                MessageBox.Show(this, Properties.Messages.warnBlankNotAllowed,
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {   DialogResult = false;  }
    }
}
