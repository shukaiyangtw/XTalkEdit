/// -----------------------------------------------------------------------------------------------
/// <summary>
///     腳本編輯頁面
/// </summary>
/// <remarks>
///     這個頁面用來編輯 app.Scene，它用一個 ListBox 列出 app.Scene.Frames，並且在面板編輯目前所選擇
///     的畫面。當 FrameList 中畫面被選為 CurFrame 的時候，呼叫 SetCurFrame() 將它的內容載入到頁面上
///     的控制項，當使用者要離開或點及其他頁的時候，呼叫 SaveCurFrame() 儲存畫面上的更動。
/// </remarks>
/// <history>
///     2022/4/22 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace XTalkEdit
{
    public partial class ScenePage : Page
    {
        /// 目前編輯中的畫面:
        public XTFrame CurFrame = null;

        /// 暫存畫面上的選項:
        private XTFrame.PathOption[] m_option = new XTFrame.PathOption[4];

        /// 畫面上的文字內容是否被更動過:
        private Boolean m_curFrameModified = false;
        private Boolean m_curFrameLabelModified = false;
        public Boolean IsModified
        {
            get
            {   return m_curFrameModified;   }
        }

        /// 因為初始化控制項的時候也會喚起 changed 與 checked events，避免重複動作:
        private Boolean m_pageLoaded = false;

        #region 頁面的初始化與退回。
        public ScenePage()
        {
            InitializeComponent();
            m_option[0] = new XTFrame.PathOption();  m_option[0].Value = "1";
            m_option[1] = new XTFrame.PathOption();  m_option[1].Value = "2";
            m_option[2] = new XTFrame.PathOption();  m_option[2].Value = "3";
            m_option[3] = new XTFrame.PathOption();  m_option[3].Value = "4";
        }

        /// <summary>
        ///  把 app.Scene.Frames 繫結到畫面上，並開始編輯第一個畫面。
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            Debug.WriteLine("ScenePage.Page_Loaded()");

            FrameList.ItemsSource = app.Scene.Frames;
            m_pageLoaded = true;

            if (app.Scene.Frames.Count > 0)
            {
                SetCurFrame(app.Scene.Frames[0]);
                FrameList.SelectedIndex = 0;
            }
            else
            {   SetCurFrameNull();  }

            TestFrameButton_Click(null, null);
        }

        /// <summary>
        ///  儲存 app.Scene 並且返回 ProjectPage。
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            Debug.WriteLine("ScenePage.BackButton_Click()");

            if ((CurFrame != null) && (m_curFrameModified == true))
            {   SaveCurFrame();  }

            if (app.Scene.isModified)
            {
                try {  app.Scene.SaveXml();  }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  return;  }
            }

            /// 清理目前已載入的 app.Scene 並且返回 ProjectPage:
            if (this.NavigationService.CanGoBack)
            {
                app.Scene = null;
                Debug.WriteLine("ScenePage.NavigationService.GoBack()");
                this.NavigationService.GoBack();
            }
        }

        /// <summary>
        ///  快速鍵實作，不知道為什麼無法捕捉到 Ctrl 組合鍵。
        /// </summary>
        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.PageUp)
            {
                PrevNextButton_Click(PrevFrameButton, null);
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                PrevNextButton_Click(NextFrameButton, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                SearchFrameButton_Click(SearchFrameButton, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                TestFrameButton_Click(TestFrameButton, null);
                e.Handled = true;
            }
         /* else if ((e.Key == Key.C) && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                CopyFrameButton_Click(CopyFrameButton, null);
                e.Handled = true;
            }
            else if ((e.Key == Key.V) && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                PasteFrameButton_Click(PasteFrameButton, null);
                e.Handled = true;
            }
            else if ((e.Key == Key.S) && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                PlayButton_Click(PlayButton, null);
                e.Handled = true;
            } */
        }

        /// <summary>
        ///  顯示關於畫面標籤的說明訊息。
        /// </summary>
        private void LabelHelp_Click(object sender, RoutedEventArgs e)
        {   MessageBox.Show(Properties.Messages.helpFrameLabel);  }
        #endregion

        #region 載入 CurFrame 的資料到頁面。
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  指定並載入 CurFrame 的資料到頁面的 Button 和 TextBox 上。
        /// </summary>
        private void SetCurFrame(XTFrame frame)
        {
            if (CurFrame != null)
            {
                /// 儲存現有的資料:
                if (m_curFrameModified == true) {  SaveCurFrame();  }
                if (frame.Equals(CurFrame)) {  return;  }
            }

         /* Debug.WriteLine(String.Format("ScenePage.SetCurFrame({0})", frame.ID)); */
            CurFrame = frame;

            /// 啟用共通的控制項:
            LabelTextBox.IsEnabled = true;
            CoverPageButton.IsEnabled = true;
            CharDialogButton.IsEnabled = true;
            FullScrTextButton.IsEnabled = true;
            CoverButton.IsEnabled = true;
            SoundButton.IsEnabled = true;
            TestFrameButton.IsEnabled = true;

            /// 設定 TextBox 以及各類型共通控制向的內容:
            LabelTextBox.Text = frame.Label;
            DialogTextBox.Text = frame.Text;

            if (String.IsNullOrEmpty(frame.Background) == false)
            {   CoverButton.Content = frame.Background.Replace('_', '-');   }
            else {  CoverButton.Content = Properties.Resources.None;  }

            DarkenCheckBox.IsChecked = frame.BgIsDarken;
            BlurCheckBox.IsChecked = frame.BgIsBlur;

            if (frame.Sound != null)
            {   SoundButton.Content = frame.Sound.ID.Replace('_', '-');  }
            else {  SoundButton.Content = Properties.Resources.None;  }

            /// 根據 FrameType 切換 radio buttons 的狀態
            if (frame.Type == XTFrame.FrameType.CoverPage)
            {   CoverPageButton.IsChecked = true;  }
            else if (frame.Type == XTFrame.FrameType.Dialog)
            {   CharDialogButton.IsChecked = true;  }
            else if (frame.Type == XTFrame.FrameType.FullDialog)
            {   FullScrTextButton.IsChecked = true;  }

            /// 只有 Dialog 類型的 frame 可以選擇角色圖片:
            if (frame.Type == XTFrame.FrameType.Dialog)
            {
                SpeakerTextBox.Text = frame.Speaker;

                if (frame.Left != null)
                {   Left1Button.Content = frame.Left.ID.Replace('_', '-');  }
                else {  Left1Button.Content = Properties.Resources.None;  }

                if (frame.Left2 != null)
                {   Left2Button.Content = frame.Left2.ID.Replace('_', '-');  }
                else {  Left2Button.Content = Properties.Resources.None;  }

                if (frame.Center != null)
                {   CenterButton.Content = frame.Center.ID.Replace('_', '-');  }
                else {  CenterButton.Content = Properties.Resources.None;  }

                if (frame.Right != null)
                {   Right1Button.Content = frame.Right.ID.Replace('_', '-');  }
                else {  Right1Button.Content = Properties.Resources.None;  }

                if (frame.Right2 != null)
                {   Right2Button.Content = frame.Right2.ID.Replace('_', '-');  }
                else {  Right2Button.Content = Properties.Resources.None;  }
            }
            else
            {
                SpeakerTextBox.Text = String.Empty;
                Left1Button.Content = Properties.Resources.None;
                Left2Button.Content = Properties.Resources.None;
                CenterButton.Content = Properties.Resources.None;
                Right1Button.Content = Properties.Resources.None;
                Right2Button.Content = Properties.Resources.None;
            }

            /// 複製路徑到 m_option 陣列當中，只有 CoverPage 類型不可以有路徑選項:
            if ((frame.Type == XTFrame.FrameType.CoverPage) || (frame.Options.Count == 0))
            {
                NextButton.IsChecked = true;
                foreach (XTFrame.PathOption option in m_option) {  option.Reset();  }
            }
            else
            {
                TextOptButton.IsChecked = true;

                int i = 0;
                foreach (XTFrame.PathOption option in frame.Options)
                {   m_option[i].Copy(option);  ++i;  }

                while (i < 4)
                {   m_option[i].Reset();  ++i;  }
            }

            /// 根據 FrameType 切換 radio buttons 的狀態:
            SetCtrlsByFrameType(CurFrame.Type);

            /// 設定路徑選項 TextBox 與 Button 上的文字:
            NextTargetButton.Content = frame.Next.ToString();

            Opt1TextBox.Text = m_option[0].Text;
            Opt1ValTextBox.Text = m_option[0].Value;
            Opt1TargetButton.Content = m_option[0].ToString();

            Opt2TextBox.Text = m_option[1].Text;
            Opt2ValTextBox.Text = m_option[1].Value;
            Opt2TargetButton.Content = m_option[1].ToString();

            Opt3TextBox.Text = m_option[2].Text;
            Opt3ValTextBox.Text = m_option[2].Value;
            Opt3TargetButton.Content = m_option[2].ToString();

            Opt4TextBox.Text = m_option[3].Text;
            Opt4ValTextBox.Text = m_option[3].Value;
            Opt4TargetButton.Content = m_option[3].ToString();

            /// 清除 modified 旗標:
            m_curFrameModified = false;
            m_curFrameLabelModified = false;
        }
        #endregion

        #region 切換控制項的禁用的狀態.
        /// <summary>
        ///  根據 FrameType 切換控制項的禁用的狀態。
        /// </summary>
        private void SetCtrlsByFrameType(XTFrame.FrameType frameType)
        {
            if (frameType == XTFrame.FrameType.CoverPage)
            {
             /* Debug.WriteLine("ScenePage.SetCtrlsByFrameType( CoverPage )"); */
                SpeakerTextBox.IsEnabled = false;
                DialogTextBox.IsEnabled = false;

                Left1Button.IsEnabled = false;
                Left2Button.IsEnabled = false;
                CenterButton.IsEnabled = false;
                Right1Button.IsEnabled = false;
                Right2Button.IsEnabled = false;

                if (NextButton.IsChecked == false)
                {   NextButton.IsChecked = true;  }
                else {  NextButton_Checked(null, null);  }
               
                NextButton.IsEnabled = true;
                TextOptButton.IsEnabled = false;
            }
            else if (frameType == XTFrame.FrameType.Dialog)
            {
             /* Debug.WriteLine("ScenePage.SetCtrlsByFrameType( Dialog )"); */
                SpeakerTextBox.IsEnabled = true;
                DialogTextBox.IsEnabled = true;

                Left1Button.IsEnabled = true;
                Left2Button.IsEnabled = true;
                CenterButton.IsEnabled = true;
                Right1Button.IsEnabled = true;
                Right2Button.IsEnabled = true;

                NextButton.IsEnabled = true;
                TextOptButton.IsEnabled = true;

            }
            else if (frameType == XTFrame.FrameType.FullDialog)
            {
             /* Debug.WriteLine("ScenePage.SetCtrlsByFrameType( FullDialog )"); */
                SpeakerTextBox.IsEnabled = false;
                DialogTextBox.IsEnabled = true;

                Left1Button.IsEnabled = false;
                Left2Button.IsEnabled = false;
                CenterButton.IsEnabled = false;
                Right1Button.IsEnabled = false;
                Right2Button.IsEnabled = false;

                NextButton.IsEnabled = true;
                TextOptButton.IsEnabled = true;
            }
        }

        /// <summary>
        ///  利用 radio buttons 改變 CurGrame 的 FrameType。
        /// </summary>
        private void CoverPageButton_Checked(object sender, RoutedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
         /* Debug.WriteLine("ScenePage.CoverPageButton_Checked()"); */

            if (CurFrame != null) {  CurFrame.Type = XTFrame.FrameType.CoverPage;  }
            SetCtrlsByFrameType(XTFrame.FrameType.CoverPage);
            m_curFrameModified = true;
        }

        private void CharDialogButton_Checked(object sender, RoutedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
         /* Debug.WriteLine("ScenePage.CharDialogButton_Checked()"); */

            if (CurFrame != null) {  CurFrame.Type = XTFrame.FrameType.Dialog;  }
            SetCtrlsByFrameType(XTFrame.FrameType.Dialog);
            m_curFrameModified = true;
        }

        private void FullScrTextButton_Checked(object sender, RoutedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
         /* Debug.WriteLine("ScenePage.FullScrTextButton_Checked()"); */

            if (CurFrame != null) {  CurFrame.Type = XTFrame.FrameType.FullDialog;  }
            SetCtrlsByFrameType(XTFrame.FrameType.FullDialog);
            m_curFrameModified = true;
        }

        /// <summary>
        ///  根據 LinkType 切換控制項的禁用的狀態。
        /// </summary>
        private void NextButton_Checked(object sender, RoutedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
         /* Debug.WriteLine("ScenePage.NextButton_Checked()"); */
            NextTargetButton.IsEnabled = true;

            Opt1TextBox.IsEnabled = false;
            Opt1ValTextBox.IsEnabled = false;
            Opt1TargetButton.IsEnabled = false;

            Opt2TextBox.IsEnabled = false;
            Opt2ValTextBox.IsEnabled = false;
            Opt2TargetButton.IsEnabled = false;

            Opt3TextBox.IsEnabled = false;
            Opt3ValTextBox.IsEnabled = false;
            Opt3TargetButton.IsEnabled = false;

            Opt4TextBox.IsEnabled = false;
            Opt4ValTextBox.IsEnabled = false;
            Opt4TargetButton.IsEnabled = false;

            m_curFrameModified = true;
        }

        private void TextOptButton_Checked(object sender, RoutedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
         /* Debug.WriteLine("ScenePage.TextOptButton_Checked()"); */
            NextTargetButton.IsEnabled = false;

            Opt1TextBox.IsEnabled = true;
            Opt1ValTextBox.IsEnabled = true;
            Opt1TargetButton.IsEnabled = true;

            Opt2TextBox.IsEnabled = true;
            Opt2ValTextBox.IsEnabled = true;
            Opt2TargetButton.IsEnabled = true;

            Opt3TextBox.IsEnabled = true;
            Opt3ValTextBox.IsEnabled = true;
            Opt3TargetButton.IsEnabled = true;

            Opt4TextBox.IsEnabled = true;
            Opt4ValTextBox.IsEnabled = true;
            Opt4TargetButton.IsEnabled = true;

            m_curFrameModified = true;
        }

        private void OnLabelTextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
            m_curFrameLabelModified = true;
            m_curFrameModified = true;
        }

        private void OnOptTextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
            m_curFrameModified = true;
        }

        private void DarkenBlurCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (m_pageLoaded == false) {  return;  }
            m_curFrameModified = true;
        }

        /// <summary>
        ///  沒有 CurFrame 所以禁用所有的控制項。
        /// </summary>
        private void SetCurFrameNull()
        {
            /// 儲存現有的資料:
            if (CurFrame != null)
            {   if (m_curFrameModified == true) {  SaveCurFrame();  }  }
            Debug.WriteLine("ScenePage.SetCurFrameNull()");
            CurFrame = null;

            /// 禁用所有的控制項:
            LabelTextBox.IsEnabled = false;
            CoverPageButton.IsEnabled = false;
            CharDialogButton.IsEnabled = false;
            FullScrTextButton.IsEnabled = false;

            NextButton.IsEnabled = false;
            NextTargetButton.IsEnabled = false;

            TextOptButton.IsEnabled = false;
            Opt1ValTextBox.Text = String.Empty;
            Opt1TextBox.IsEnabled = false;
            Opt1ValTextBox.Text = "1";
            Opt1ValTextBox.IsEnabled = false;
            Opt1TargetButton.Content = Properties.Resources.NextFrame;
            Opt1TargetButton.IsEnabled = false;

            Opt2ValTextBox.Text = String.Empty;
            Opt2TextBox.IsEnabled = false;
            Opt2ValTextBox.Text = "2";
            Opt2ValTextBox.IsEnabled = false;
            Opt2TargetButton.Content = Properties.Resources.NextFrame;
            Opt2TargetButton.IsEnabled = false;

            Opt3ValTextBox.Text = String.Empty;
            Opt3TextBox.IsEnabled = false;
            Opt3ValTextBox.Text = "3";
            Opt3ValTextBox.IsEnabled = false;
            Opt3TargetButton.Content = Properties.Resources.NextFrame;
            Opt3TargetButton.IsEnabled = false;

            Opt4ValTextBox.Text = String.Empty;
            Opt4TextBox.IsEnabled = false;
            Opt4ValTextBox.Text = "4";
            Opt4ValTextBox.IsEnabled = false;
            Opt4TargetButton.Content = Properties.Resources.NextFrame;
            Opt4TargetButton.IsEnabled = false;

            CoverButton.Content = Properties.Resources.None;
            CoverButton.IsEnabled = false;

            SoundButton.Content = Properties.Resources.None;
            SoundButton.IsEnabled = false;

            Left1Button.Content = Properties.Resources.None;
            Left1Button.IsEnabled = false;

            Left2Button.Content = Properties.Resources.None;
            Left2Button.IsEnabled = false;

            CenterButton.Content = Properties.Resources.None;
            CenterButton.IsEnabled = false;

            Right1Button.Content = Properties.Resources.None;
            Right1Button.IsEnabled = false;

            Right2Button.Content = Properties.Resources.None;
            Right2Button.IsEnabled = false;
            TestFrameButton.IsEnabled = false;

            SpeakerTextBox.Text = String.Empty;
            SpeakerTextBox.IsEnabled = false;
            DialogTextBox.Text = String.Empty;
            DialogTextBox.IsEnabled = false;

            /// 清除 modified 旗標:
            m_curFrameModified = false;
            m_curFrameLabelModified = false;
        }
        #endregion

        #region 選擇、上一頁、下一頁。
        /// ///////////////////////////////////////////////////////////////////////////////////////
        private void FrameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            XTFrame frame = FrameList.SelectedItem as XTFrame;
            if (frame != null) {  SetCurFrame(frame);  }
            TestFrameButton_Click(null, null);
        }

        /// <summary>
        ///  根據 CurFrame.Num 切換上一畫面或下一畫面，並改變 FrameList 的選取範圍。
        /// </summary>
        private void PrevNextButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            int index = 0;

            if (CurFrame != null)
            {
                index = CurFrame.Num;
                if (m_curFrameModified == true) {  SaveCurFrame();  }
            }

            if (sender.Equals(PrevFrameButton))
            {
                if (index > 0) {  index = index - 1; }
            }
            else if (sender.Equals(NextFrameButton))
            {
                if (index < (app.Scene.Frames.Count - 1))
                {   index = index + 1;  }
            }

            if (index < (app.Scene.Frames.Count))
            {
                SetCurFrame(app.Scene.Frames[index]);
                FrameList.SelectedIndex = index;
            }
            else {  SetCurFrameNull();  }
        }
        #endregion

        #region 將頁面控制項的內容寫回 CurFrame。
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  將頁面控制項的內容寫回 CurFrame 當中:
        /// </summary>
        public void SaveCurFrame()
        {
            App app = Application.Current as App;

            if (CurFrame == null)
            {
                m_curFrameLabelModified = false;
                m_curFrameModified = false;
                return;
            }

            Debug.WriteLine("ScenePage.SaveCurFrame()");

            /// 如果是標籤更動，必須同步地修改其他畫面連接到它的路徑選項:
            if (m_curFrameLabelModified)
            {
                String newLabel = String.Empty;
                if (String.IsNullOrWhiteSpace(LabelTextBox.Text) == false)
                { newLabel = LabelTextBox.Text; }

                foreach (XTFrame frame in app.Scene.Frames)
                {
                    if (frame.Next.Type == XTFrame.LinkType.Label)
                    {
                        if (frame.Next.Goto.Equals(CurFrame.Label))
                        { frame.Next.Goto = newLabel; }
                    }

                    foreach (XTFrame.PathOption option in frame.Options)
                    {
                        if (option.Type == XTFrame.LinkType.Label)
                        {
                            if (option.Goto.Equals(CurFrame.Label))
                            { option.Goto = newLabel; }
                        }
                    }
                }

                CurFrame.Label = newLabel;
            }

            /// 儲存對話文字內容，背景與角色的更動則是在 button click 的時候就儲存了:
            if (DarkenCheckBox.IsChecked == true) {  CurFrame.BgIsDarken = true;  }
            else {  CurFrame.BgIsDarken = false;  }
            if (BlurCheckBox.IsChecked == true) {  CurFrame.BgIsBlur = true;  }
            else {  CurFrame.BgIsBlur = false;  }
            CurFrame.Speaker = SpeakerTextBox.Text;
            CurFrame.Text = DialogTextBox.Text;

            /// 將非空白的路徑選項儲存到 CurFrame.Options 當中:
            CurFrame.Options.Clear();
            if (TextOptButton.IsChecked == true)
            {
                m_option[0].Text = Opt1TextBox.Text;
                m_option[0].Value = Opt1ValTextBox.Text;

                m_option[1].Text = Opt2TextBox.Text;
                m_option[1].Value = Opt2ValTextBox.Text;

                m_option[2].Text = Opt3TextBox.Text;
                m_option[2].Value = Opt3ValTextBox.Text;

                m_option[3].Text = Opt4TextBox.Text;
                m_option[3].Value = Opt4ValTextBox.Text;

                for (int i=0; i<4; ++i)
                {
                    if (String.IsNullOrEmpty(m_option[i].Text) == false)
                    {
                        XTFrame.PathOption option = new XTFrame.PathOption();
                        option.Copy(m_option[i]);
                        CurFrame.Options.Add(option);
                    }
                }

                /// 如果有選項但是標籤是空白的，產生一個預設的標籤:
                if (String.IsNullOrEmpty(CurFrame.Label))
                {   CurFrame.Label = String.Format("options_{0}", CurFrame.Num);  }
            }

            m_curFrameModified = false;
            app.Scene.isModified = true;
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  利用 PathOptionDialog 編輯路徑選項。
        /// </summary>
        private void NextTargetButton_Click(object sender, RoutedEventArgs e)
        {
            PathOptionDialog dialog = new PathOptionDialog();
            if (CurFrame != null)
            {
                dialog.Type = CurFrame.Next.Type;
                dialog.Target = CurFrame.Next.Target;
                dialog.Goto = CurFrame.Next.Goto;
                dialog.PackOpts = CurFrame.Next.PackOpts;
            }

            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Next.Type = dialog.Type;
                CurFrame.Next.Target = dialog.Target;
                CurFrame.Next.Goto = dialog.Goto;
                CurFrame.Next.PackOpts = dialog.PackOpts;
                NextTargetButton.Content = CurFrame.Next.ToString();

                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        private void Opt1TargetButton_Click(object sender, RoutedEventArgs e)
        {
            PathOptionDialog dialog = new PathOptionDialog();
            dialog.Type = m_option[0].Type;
            dialog.Target = m_option[0].Target;
            dialog.Goto = m_option[0].Goto;
            dialog.PackOpts = m_option[0].PackOpts;

            if (dialog.ShowDialog() == false) {  return;  }

            m_option[0].Type = dialog.Type;
            m_option[0].Target = dialog.Target;
            m_option[0].Goto = dialog.Goto;
            m_option[0].PackOpts = dialog.PackOpts;
            Opt1TargetButton.Content = m_option[0].ToString();
            m_curFrameModified = true;
        }

        private void Opt2TargetButton_Click(object sender, RoutedEventArgs e)
        {
            PathOptionDialog dialog = new PathOptionDialog();
            dialog.Type = m_option[1].Type;
            dialog.Target = m_option[1].Target;
            dialog.Goto = m_option[1].Goto;
            dialog.PackOpts = m_option[1].PackOpts;

            if (dialog.ShowDialog() == false) {  return;  }

            m_option[1].Type = dialog.Type;
            m_option[1].Target = dialog.Target;
            m_option[1].Goto = dialog.Goto;
            m_option[1].PackOpts = dialog.PackOpts;
            Opt2TargetButton.Content = m_option[1].ToString();
            m_curFrameModified = true;
        }

        private void Opt3TargetButton_Click(object sender, RoutedEventArgs e)
        {
            PathOptionDialog dialog = new PathOptionDialog();
            dialog.Type = m_option[2].Type;
            dialog.Target = m_option[2].Target;
            dialog.Goto = m_option[2].Goto;
            dialog.PackOpts = m_option[2].PackOpts;

            if (dialog.ShowDialog() == false) {  return;  }

            m_option[2].Type = dialog.Type;
            m_option[2].Target = dialog.Target;
            m_option[2].Goto = dialog.Goto;
            m_option[2].PackOpts = dialog.PackOpts;
            Opt3TargetButton.Content = m_option[2].ToString();
            m_curFrameModified = true;
        }

        private void Opt4TargetButton_Click(object sender, RoutedEventArgs e)
        {
            PathOptionDialog dialog = new PathOptionDialog();
            dialog.Type = m_option[3].Type;
            dialog.Target = m_option[3].Target;
            dialog.Goto = m_option[3].Goto;
            dialog.PackOpts = m_option[3].PackOpts;

            if (dialog.ShowDialog() == false) {  return;  }

            m_option[3].Type = dialog.Type;
            m_option[3].Target = dialog.Target;
            m_option[3].Goto = dialog.Goto;
            m_option[3].PackOpts = dialog.PackOpts;
            Opt4TargetButton.Content = m_option[3].ToString();
            m_curFrameModified = true;
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  利用 BackgroundDialog 選擇背景圖片。
        /// </summary>
        private void CoverButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundDialog dialog = new BackgroundDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Background = dialog.FileName;
                if (String.IsNullOrEmpty(dialog.FileName))
                {   CoverButton.Content = Properties.Resources.None;  }
                else {  CoverButton.Content = dialog.FileName.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        /// <summary>
        ///  利用 SoundDialog 選擇過場音效。
        /// </summary>
        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            SoundDialog dialog = new SoundDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Sound = dialog.SelectedItem;
                if (dialog.SelectedItem == null)
                {   SoundButton.Content = Properties.Resources.None;  }
                else {  SoundButton.Content = CurFrame.Sound.ID.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        /// <summary>
        ///  利用 SpriteDialog 選擇角色畫片。
        /// </summary>
        private void Left1Button_Click(object sender, RoutedEventArgs e)
        {
            SpriteDialog dialog = new SpriteDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Left = dialog.SelectedItem;
                if (dialog.SelectedItem == null)
                {   Left1Button.Content = Properties.Resources.None;  }
                else {  Left1Button.Content = CurFrame.Left.ID.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        private void Left2Button_Click(object sender, RoutedEventArgs e)
        {
            SpriteDialog dialog = new SpriteDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Left2 = dialog.SelectedItem;
                if (dialog.SelectedItem == null)
                {   Left2Button.Content = Properties.Resources.None;  }
                else {  Left2Button.Content = CurFrame.Left2.ID.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            SpriteDialog dialog = new SpriteDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Center = dialog.SelectedItem;
                if (dialog.SelectedItem == null)
                {   CenterButton.Content = Properties.Resources.None;  }
                else {  CenterButton.Content = CurFrame.Center.ID.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        private void Right1Button_Click(object sender, RoutedEventArgs e)
        {
            SpriteDialog dialog = new SpriteDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Right = dialog.SelectedItem;
                if (dialog.SelectedItem == null)
                {   Right1Button.Content = Properties.Resources.None;  }
                else {  Right1Button.Content = CurFrame.Right.ID.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }

        private void Right2Button_Click(object sender, RoutedEventArgs e)
        {
            SpriteDialog dialog = new SpriteDialog();
            if (dialog.ShowDialog() == false) {  return;  }

            if (CurFrame != null)
            {
                CurFrame.Right2 = dialog.SelectedItem;
                if (dialog.SelectedItem == null)
                {   Right2Button.Content = Properties.Resources.None;  }
                else {  Right2Button.Content = CurFrame.Right2.ID.Replace('_', '-');  }
                App app = Application.Current as App;
                app.Scene.isModified = true;
            }
        }
        #endregion

        #region 新增、複製、貼上。
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  產生一個新 XTFrame 物件，並且加入 app.Scene.Frames 與 FrameList 與當中。
        /// </summary>
        private void AddFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if ((CurFrame != null) && (m_curFrameModified == true))
            {   SaveCurFrame();  }

            /// 新增的畫面預設採用最後一個畫面的背景:
            App app = Application.Current as App;
            String background = String.Empty;
            if (app.Scene.Frames.Count > 0)
            {   background = app.Scene.Frames[app.Scene.Frames.Count - 1].Background;  }

            /// 產生一個新的 XTFrame 物件，並加入 app.Scene.Frames 當中:
            XTFrame frame = new XTFrame();
            frame.Num = FrameList.Items.Count;
            frame.Background = background;
            app.Scene.Frames.Add(frame);
            app.Scene.isModified = true;
            FrameList.SelectedItem = frame;
            FrameList.ScrollIntoView(frame);
        }

        /// <summary>
        ///  在 CurFrame 所在位置複製一個 XTFrame 物件並且插入 app.Scene.Frames。
        /// </summary>
        private void DuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            if (app.Scene.Frames.Count == 0) {  return;  }

            XTFrame frame = FrameList.SelectedItem as XTFrame;
            if (frame == null) {  frame = app.Scene.Frames[app.Scene.Frames.Count - 1];  }
            else if (m_curFrameModified == true) {  SaveCurFrame();  }

            /// 複製目前選中的 frame:
            XTFrame newFrame = frame.Duplicate();
            newFrame.Num = frame.Num + 1;

            /// 將複製的新畫面插入在舊 frame 之後：
            app.Scene.Frames.Insert(newFrame.Num, newFrame);
            for (int i = (newFrame.Num+1); i< app.Scene.Frames.Count; ++i)
            {   app.Scene.Frames[i].Num = i;  }
            app.Scene.isModified = true;

            /// 選擇新複製的畫面:
            FrameList.SelectedItem = newFrame;
            FrameList.ScrollIntoView(newFrame);
        }

        /// <summary>
        ///  刪除目前選擇的所有畫面。
        /// </summary>
        private void DeleteFrameButton_Click(object sender, RoutedEventArgs e)
        {
            List<XTFrame> toBeDeleted = new List<XTFrame>();
            foreach (XTFrame frame in FrameList.SelectedItems) {  toBeDeleted.Add(frame);  }
            if (toBeDeleted.Count == 0) {  return;  }

            if (MessageBox.Show(Properties.Messages.warnRUSureToDelete,
                    Properties.Resources.BtnDelete, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No) {  return;  }

            /// 從 app.Scene.Frames 移除目前選擇的所有 XTFrame 物件:
            App app = Application.Current as App;
            int firstIndex = app.Scene.Frames.Count;
            foreach (XTFrame frame in toBeDeleted)
            {
                if (frame.Num < firstIndex) {  firstIndex = frame.Num;  }
                app.Scene.Frames.Remove(frame);
            }

            app.Scene.isModified = true;

            /// 並且更新所有畫面的 frame num:
            int i = firstIndex;
            while (i < app.Scene.Frames.Count)
            {   app.Scene.Frames[i].Num = i;  ++i;  }
        }

        /// <summary>
        ///  複製目前選擇的畫面到專案的剪貼簿。
        /// </summary>
        private void CopyFrameButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            app.Project.Copied.Clear();

            foreach (XTFrame frame in FrameList.SelectedItems)
            {
                XTFrame newFrame = frame.Duplicate();
                app.Project.Copied.Add(newFrame);
            }

            if (app.Project.Copied.Count > 0)
            {
                String msg = String.Format(Properties.Messages.msgFramesCopied, app.Project.Copied.Count);
                MessageBox.Show(msg);
            }
        }

        /// <summary>
        ///  將 app.Project.Copied 中的項目插入到目前的位置。
        /// </summary>
        private void PasteFrameButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            if (app.Project.Copied.Count == 0) {  return; }

            if ((CurFrame != null) && (m_curFrameModified == true))
            {   SaveCurFrame();  }

            int index = FrameList.SelectedIndex;
            if (index == -1) {  index = FrameList.Items.Count;  }

            /// 插入到選定的位置:
            int i = index;
            foreach (XTFrame frame in app.Project.Copied)
            {
                XTFrame newFrame = frame.Duplicate();
                newFrame.Num = i;
                app.Scene.Frames.Insert(i, newFrame);
                ++i;
            }

            app.Scene.isModified = true;

            /// 更新後續畫面的 frame num:
            while (i < app.Scene.Frames.Count)
            {
                app.Scene.Frames[i].Num = i;
                ++i;
            }
        }
        #endregion

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  利用 HtmlExporter 的功能產生無互動功能、單畫面的 HTML 內容。
        /// </summary>
        private void TestFrameButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;

            /// 如果目前沒有選擇任何畫面，則產生空文件:
            if (CurFrame == null)
            {   FrameView.NavigateToString(HtmlExporter.BlankHtml());  return;  }

            /// 先儲存畫面的更動:
            if (m_curFrameModified == true) {  SaveCurFrame();  }

            /// 產生單畫面的 HTML 內容字串:
            HtmlExporter html = new HtmlExporter(CurFrame);
            String htmlStr = html.ToString();

            String pathName = Path.Combine(app.Project.Dir, "preview.htm");
            File.WriteAllText(pathName, htmlStr, Encoding.UTF8);
            FrameView.Navigate("file:///" + pathName.Replace('\\', '/'));
        }

        /// <summary>
        ///  產生 .js 與 .html 檔案以後利用 System.Diagnostics.Process.Start(url) 開啟瀏覽器。
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            Debug.WriteLine("ScenePage.PlayButton_Click():");

            /// 先儲存場景的更動:
            if ((CurFrame != null) && (m_curFrameModified == true))
            {   SaveCurFrame();  }

            if (app.Scene.isModified)
            {
                try {  app.Scene.SaveXml();  }
                catch (Exception ex)
                {   MessageBox.Show(ex.Message);  return;  }
            }

            /// 檢查檔案版本，並且視需要重新產生 .js 與 .html 檔案:
            String xtalkPathName = Path.Combine(app.Project.Dir, app.Scene.Name + ".xtalk");
            String jsonPathName = Path.Combine(app.Project.Dir, app.Scene.Name + ".js");
            String htmlPathName = Path.Combine(app.Project.Dir, app.Scene.Name + ".html");
            if (App.IsFileNewerThan(xtalkPathName, jsonPathName))
            {
                JsonExporter json = new JsonExporter(app.Scene);
                String str = json.ToString();
                File.WriteAllText(jsonPathName, str, Encoding.UTF8);

                HtmlExporter html = new HtmlExporter(app.Scene);
                str = html.ToString();
                File.WriteAllText(htmlPathName, str, Encoding.UTF8);
            }

            /// 以外部瀏覽器開啟 (name).html 網頁:
            String fileUrl = "file:///" +  htmlPathName.Replace('\\', '/');
            if (FrameList.SelectedIndex != -1)
            {   fileUrl += ("?xf=" + FrameList.SelectedIndex.ToString());  }
            Debug.WriteLine(fileUrl);

            try
            {
                ProcessStartInfo info = new ProcessStartInfo("iexplore.exe", "\"" + fileUrl + "\"");
                Process.Start(info);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        ///  搜尋包含指定台詞的畫面。
        /// </summary>
        private void SearchFrameButton_Click(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            SearchDialog dialog = new SearchDialog();
            int index = FrameList.SelectedIndex + 1;
            if (index < app.Scene.Frames.Count) {   dialog.Index = index;  }
            else {  dialog.Index = 0;  }

            /// 開啟搜尋對話方塊:
            if (dialog.ShowDialog() == false) {  return;  }

            /// 沒有找到任何結果，顯示訊息:
            if (dialog.Index == -1)
            {
                MessageBox.Show(Properties.Messages.msgNoMoreFrames, Properties.Resources.BtnSearch);
                return;
            }

            /// 讓 FrameList 選擇並捲動到搜尋到的畫面項目:
            XTFrame frame = app.Scene.Frames[dialog.Index];
            FrameList.SelectedItem = frame;
            FrameList.ScrollIntoView(frame);
        }
    }
}
