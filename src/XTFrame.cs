/// -----------------------------------------------------------------------------------------------
/// <summary>
///     腳本畫面
/// </summary>
/// <remarks>
///     這個結構體記載了腳本裡每個畫面的組成元素，包括封面、背景、角色、文字，以及點擊後要前往的位置。
/// </remarks>
/// <history>
///     2022/4/25 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace XTalkEdit
{
    public class XTFrame : INotifyPropertyChanged
    {
        /// <summary>
        ///  畫面的類型，指定那些資料欄位是有效的。
        /// </summary>
        public enum FrameType
        {
            CoverPage  = 0,
            Dialog     = 1,
            FullDialog = 2
        };

        /// <summary>
        ///  這些列舉類型用來指示 next 按鈕或 option 點擊後要前往的位置類型。
        /// </summary>
        public enum LinkType
        {   NextFrame = 0, Label = 1, Url = 2  };

        public enum LinkTarget
        {   Self = 0, Top = 1, Blank = 2  };

        /// <summary>
        ///  這個結構體用來紀錄 next 按鈕或 option 點擊後要前往的位置。
        /// </summary>
        public class PathOption
        {
            /// 如果是 option，它顯示的文字與代表數值:
            public String Text = String.Empty;
            public String Value = String.Empty;

            /// 點擊後要前往的位置類型:
            public LinkType Type = LinkType.NextFrame;
            public LinkTarget Target = LinkTarget.Self;
            public String Goto = String.Empty;
            public Boolean PackOpts = true;

            public void Reset()
            {
                Text = String.Empty;
                Value = String.Empty;
                Type = LinkType.NextFrame;
                Target = LinkTarget.Self;
                Goto = String.Empty;
                PackOpts = true;
            }

            public void Copy(PathOption option)
            {
                Text = option.Text;
                Value = option.Value;
                Type = option.Type;
                Target = option.Target;
                Goto = option.Goto;
                PackOpts = option.PackOpts;
            }

            #region PathOption.ToString()
            /// <summary>
            ///  組織一個顯示用的字串。
            /// </summary>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (Type == LinkType.NextFrame)
                {
                    sb.Append(Properties.Resources.NextFrame);
                }
                else if (Type == LinkType.Label)
                {
                    sb.Append(Properties.Resources.Label);
                    sb.Append(": ");
                    sb.Append(Goto);
                }
                else if (Type == LinkType.Url)
                {
                    sb.Append(Properties.Resources.WebPage);
                    sb.Append(": ");

                    if (Goto.StartsWith("http:") || Goto.StartsWith("https:"))
                    {
                        Uri uri = new Uri(Goto);
                        sb.Append(uri.AbsolutePath);
                    }
                    else
                    {   sb.Append(Goto);  }

                    if (Target == LinkTarget.Top)
                    {
                        sb.Append(", ");
                        sb.Append(Properties.Resources.HrefTop);
                    }
                    else if (Target == LinkTarget.Blank)
                    {
                        sb.Append(", ");
                        sb.Append(Properties.Resources.HrefBlank);
                    }

                    if (PackOpts == true)
                    {
                        sb.Append(", ");
                        sb.Append(Properties.Resources.PackOpts);
                    }
                }

                return sb.ToString().Replace('_', '-');
            }
            #endregion
        }

        /// <summary>
        ///  這個畫面的標籤(選擇性)。
        /// </summary>
        private String m_label = String.Empty;
        public String Label
        {
            get
            {   return m_label;  }
            set
            {   m_label = value; OnPropertyChanged("ID"); }
        }

        /// 畫面編號:
        private int m_num = 0;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Num
        {
            get
            {   return m_num;  }
            set
            {   m_num = value; OnPropertyChanged("ID"); }
        }

        /// 畫面顯示用:
        public String ID
        {
            get
            {
                if (String.IsNullOrEmpty(m_label))
                {   return String.Format("frame {0}", m_num.ToString());  }

                return m_label;
            }
        }

        /// <summary>
        /// 畫面的背景、音效、角色，文字。
        /// </summary>
        public FrameType Type = FrameType.Dialog;
        public String Background = String.Empty;
        public Boolean BgIsDarken = false;
        public Boolean BgIsBlur = false;

        public XTNamedFileItem Sound = null;

        public XTNamedFileItem Center = null;
        public XTNamedFileItem Left2 = null;
        public XTNamedFileItem Right2 = null;
        public XTNamedFileItem Left = null;
        public XTNamedFileItem Right = null;

        public String Speaker = String.Empty;
        public String Text = String.Empty;

        /// <summary>
        ///  選項以及點擊後要前往的位置。
        /// </summary>
        public ObservableCollection<PathOption> Options = new ObservableCollection<PathOption>();
        public PathOption Next = new PathOption();

        /// <summary>
        ///  複製一個一模一樣的 XTFrame 物件。
        /// </summary>
        public XTFrame Duplicate()
        {
            XTFrame frame = new XTFrame();
            frame.Type = this.Type;
            frame.Background = this.Background;
            frame.BgIsDarken = this.BgIsDarken;
            frame.BgIsBlur = this.BgIsBlur;
            frame.Sound = this.Sound;

            frame.Center = this.Center;
            frame.Left2 = this.Left2;
            frame.Right2 = this.Right2;
            frame.Left = this.Left;
            frame.Right = this.Right;

            frame.Speaker = this.Speaker;
            frame.Text = this.Text;

            frame.Next.Copy(this.Next);

            foreach (PathOption option in Options)
            {
                PathOption newOpt = new PathOption();
                newOpt.Copy(option);
                frame.Options.Add(newOpt);
            }

            return frame;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {   handler(this, new PropertyChangedEventArgs(name));  }
        }
    }
}
