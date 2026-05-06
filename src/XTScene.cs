/// -----------------------------------------------------------------------------------------------
/// <summary>
///     腳本檔案
/// </summary>
/// <remarks>
///     一個場景腳本單純地僅包含一個 XTFrame 的陣列，而每個 XTFrame 利用專案的檔案資產指定畫面背景以及出現的角色。
/// </remarks>
/// <history>
///     2024/9/27 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace XTalkEdit
{
    public class XTScene
    {
        /// <summary>
        ///  這個場景的名稱，也就是將會生成 .js 和 .htm 的檔名。
        /// </summary>
        private String m_name;
        private XTProject m_proj;
        public String Name {  get {  return m_name;  }  }
        public XTProject Project {  get {  return m_proj;  }  }

        /// <summary>
        ///  這個場景的所有畫面。
        /// </summary>
        public ObservableCollection<XTFrame> Frames = new ObservableCollection<XTFrame>();

        /// 這個場景是否被更動過:
        public Boolean isModified = false;

        /// <summary>
        ///  m_proj 與 m_name 在建構式內指定以後就不能再更動。
        /// </summary>
        public XTScene(XTProject proj, String name)
        {
            m_name = name;
            m_proj = proj;
        }

        /// <summary>
        ///  改變 m_name 並且同時修改相關的檔名。
        /// </summary>
        public void Rename(String newName)
        {
            String newFileName = newName + ".xtalk";
            String newPathName = Path.Combine(m_proj.Dir, newFileName);
            if (File.Exists(newPathName) == true)
            {   throw new ArgumentException(Properties.Messages.warnFileAlreadyExists);  }

            /// 更改檔案名稱:
            String oldFileName = m_name + ".xtalk";
            String oldPathName = Path.Combine(m_proj.Dir, oldFileName);
            File.Move(oldPathName, newPathName);

            oldFileName = m_name + ".js";
            oldPathName = Path.Combine(m_proj.Dir, oldFileName);
            if (File.Exists(oldPathName)) {  File.Delete(oldPathName);  }

            oldFileName = m_name + ".html";
            oldPathName = Path.Combine(m_proj.Dir, oldFileName);
            if (File.Exists(oldPathName)) {  File.Delete(oldPathName);  }

            /// 允許更名:
            m_name = newName;
            m_proj.isModified = true;
         /* isModified = true; */
        }

        #region 讀寫 (m_name).xtalk 檔案。
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  載入 m_proj.Dir/(m_name).xtalk
        /// </summary>
        public void LoadXml()
        {
            String fileName = m_name + ".xtalk";
            String pathName = Path.Combine(m_proj.Dir, fileName);
            if (File.Exists(pathName) == false)
            {   throw new FileNotFoundException(fileName + " not found!");  }
            Debug.WriteLine(String.Format("XTScene.LoadXml({0})", pathName));

            /// 載入 xtalk.proj 檔案並且 parse 為 XML 文件:
            XmlDocument doc = new XmlDocument();
            doc.Load(pathName);

            /// 清除舊內容:
            Frames.Clear();
            int frameNum = 0;

            /// 解析 XML 樹狀內容，在 .xtalk 中只會有一種元素就是 <frame>:
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = node as XmlElement;
                    XTFrame frame = new XTFrame();
                    frame.Num = frameNum;

                    if (element.HasAttribute("label"))
                    {
                        frame.Label = element.Attributes["label"].Value;
                     /* Debug.WriteLine(String.Format(" frame: {0}", frame.Label)); */
                    }
                 /* else
                    {   Debug.WriteLine(String.Format(" frame: {0}", frameNum));  } */

                    foreach (XmlNode child in element.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            XmlElement ele = child as XmlElement;
                            if (ele.Name.Equals("cover"))
                            {
                                /* cover src="背景圖檔案名稱" /> */
                                frame.Type = XTFrame.FrameType.CoverPage;
                                frame.Background = ele.Attributes["src"].Value;
                            }
                            else if (ele.Name.Equals("background"))
                            {
                                /* <background src="背景圖檔案名稱" /> */
                                frame.Background = ele.Attributes["src"].Value;
                            }
                            else if (ele.Name.Equals("darken"))
                            {
                                /* <darken scene="true" /> */
                                if (ele.Attributes["scene"].Value.Equals("true"))
                                {   frame.BgIsDarken = true;  }
                            }
                            else if (ele.Name.Equals("blur"))
                            {
                                /* <blur scene="true" /> */
                                if (ele.Attributes["scene"].Value.Equals("true"))
                                {   frame.BgIsBlur = true;  }
                            }
                            else if (ele.Name.Equals("audio"))
                            {
                                /* <audio id="到達此頁時要撥放的音效" /> */
                                frame.Sound = m_proj.GetSoundByID(ele.Attributes["id"].Value);
                            }
                            else if (ele.Name.Equals("left"))
                            {
                                /* <left id="左一角色畫片名稱" /> */
                                frame.Left = m_proj.GetSpriteByID(ele.Attributes["id"].Value);
                            }
                            else if (ele.Name.Equals("left2"))
                            {
                                /* <left2 id="左二角色畫片名稱" /> */
                                frame.Left2 = m_proj.GetSpriteByID(ele.Attributes["id"].Value);
                            }
                            else if (ele.Name.Equals("center"))
                            {
                                /* <center id="中央角色畫片名稱" /> */
                                frame.Center = m_proj.GetSpriteByID(ele.Attributes["id"].Value);
                            }
                            else if (ele.Name.Equals("right"))
                            {
                                /* <right id="右一角色畫片名稱" /> */
                                frame.Right = m_proj.GetSpriteByID(ele.Attributes["id"].Value);
                            }
                            else if (ele.Name.Equals("right2"))
                            {
                                /* <right2 id="右二角色畫片名稱" /> */
                                frame.Right2 = m_proj.GetSpriteByID(ele.Attributes["id"].Value);
                            }
                            else if (ele.Name.Equals("speaker"))
                            {
                                /* <speaker>選擇性的說話人名</speaker> */
                                frame.Speaker = ele.InnerXml;
                            }
                            else if (ele.Name.Equals("text"))
                            {
                                /* <text>說話內容</text> */
                                frame.Type = XTFrame.FrameType.Dialog;
                                frame.Text = ele.InnerXml;
                            }
                            else if (ele.Name.Equals("full_text"))
                            {
                                /* <full_text>文字內容</full_text> */
                                frame.Type = XTFrame.FrameType.FullDialog;
                                frame.Text = ele.InnerXml;
                            }
                            else if (ele.Name.Equals("option"))
                            {
                                XTFrame.PathOption option = new XTFrame.PathOption();
                                LoadXmlPathOption(ele, option);
                                frame.Options.Add(option);
                            }
                            else if (ele.Name.Equals("next"))
                            {   LoadXmlPathOption(ele, frame.Next);  }
                        }
                    }

                    Frames.Add(frame);
                    ++frameNum;
                }
            }

            isModified = false;
        }

        /* <option value="1">
              <text>選項內容</text>
              <label id="要前往的標籤" />
              <url target="空白 or _top or _blank" href="網址或網頁" packopt="true or false" />
           </option> */
        private void LoadXmlPathOption(XmlElement element, XTFrame.PathOption option)
        {
            if (element.HasAttribute("value"))
            {   option.Value = element.Attributes["value"].Value;  }

            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    XmlElement ele = child as XmlElement;
                    if (ele.Name.Equals("text"))
                    {   option.Text = ele.InnerXml;  }
                    else if (ele.Name.Equals("label"))
                    {
                        option.Type = XTFrame.LinkType.Label;
                        option.Goto = ele.Attributes["id"].Value;
                    }
                    else if (ele.Name.Equals("url"))
                    {
                        option.Type = XTFrame.LinkType.Url;
                        option.Goto = ele.Attributes["href"].Value;

                        if (ele.HasAttribute("target"))
                        {
                            String s = ele.Attributes["target"].Value;
                            if (s.Equals("_top")) {  option.Target = XTFrame.LinkTarget.Top;  }
                            else if (s.Equals("_blank")) {  option.Target = XTFrame.LinkTarget.Blank;  }
                        }

                        if (ele.HasAttribute("packopt"))
                        {
                            String s = ele.Attributes["packopt"].Value;
                            if (s.Equals("false")) {  option.PackOpts = false;  }
                            else {  option.PackOpts = true;  }
                        }
                    }
                }
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  儲存 m_proj.Dir/(m_name).xtalk
        /// </summary>
        public void SaveXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(declaration);

            XmlElement root = doc.CreateElement("frames");
            doc.AppendChild(root);

            foreach (XTFrame frame in Frames)
            {
                XmlElement element = doc.CreateElement("frame");
                if (String.IsNullOrEmpty(frame.Label) == false)
                {   element.SetAttribute("label", frame.Label);  }

                /// 儲存背景圖片與過場音效設定:
                if (frame.Type == XTFrame.FrameType.CoverPage)
                {
                    XmlElement child = doc.CreateElement("cover");
                    child.SetAttribute("src", frame.Background);
                    element.AppendChild(child);
                }
                else if (String.IsNullOrEmpty(frame.Background) == false)
                {
                    XmlElement child = doc.CreateElement("background");
                    child.SetAttribute("src", frame.Background);
                    element.AppendChild(child);
                }

                if (frame.BgIsDarken)
                {
                    XmlElement child = doc.CreateElement("darken");
                    child.SetAttribute("scene", "true");
                    element.AppendChild(child);
                }

                if (frame.BgIsBlur)
                {
                    XmlElement child = doc.CreateElement("blur");
                    child.SetAttribute("scene", "true");
                    element.AppendChild(child);
                }

                if (frame.Sound != null)
                {
                    XmlElement child = doc.CreateElement("audio");
                    child.SetAttribute("id", frame.Sound.ID);
                    element.AppendChild(child);
                }

                /// 儲存半幅對話框設定
                if (frame.Type == XTFrame.FrameType.Dialog)
                {
                    XmlElement child = doc.CreateElement("text");
                    if (String.IsNullOrEmpty(frame.Text) == false)
                    {   child.InnerXml = frame.Text;  }
                    element.AppendChild(child);

                    if (String.IsNullOrEmpty(frame.Speaker) == false)
                    {
                        child = doc.CreateElement("speaker");
                        child.InnerXml = frame.Speaker;
                        element.AppendChild(child);
                    }

                    if (frame.Left != null)
                    {
                        child = doc.CreateElement("left");
                        child.SetAttribute("id", frame.Left.ID);
                        element.AppendChild(child);
                    }

                    if (frame.Left2 != null)
                    {
                        child = doc.CreateElement("left2");
                        child.SetAttribute("id", frame.Left2.ID);
                        element.AppendChild(child);
                    }

                    if (frame.Center != null)
                    {
                        child = doc.CreateElement("center");
                        child.SetAttribute("id", frame.Center.ID);
                        element.AppendChild(child);
                    }

                    if (frame.Right != null)
                    {
                        child = doc.CreateElement("right");
                        child.SetAttribute("id", frame.Right.ID);
                        element.AppendChild(child);
                    }

                    if (frame.Right2 != null)
                    {
                        child = doc.CreateElement("right2");
                        child.SetAttribute("id", frame.Right2.ID);
                        element.AppendChild(child);
                    }
                }
                /// 儲存全幅對話框設定
                else if (frame.Type == XTFrame.FrameType.FullDialog)
                {
                    XmlElement child = doc.CreateElement("full_text");
                    if (String.IsNullOrEmpty(frame.Text) == false)
                    {   child.InnerXml = frame.Text;  }
                    element.AppendChild(child);
                }

                /// 儲存 option 或 next 的路徑選項:
                if ((frame.Options.Count == 0) || (frame.Type == XTFrame.FrameType.CoverPage))
                {
                    XmlElement child = doc.CreateElement("next");
                    SaveXmlPathOption(doc, child, frame.Next);
                    element.AppendChild(child);
                }
                else
                {
                    foreach (XTFrame.PathOption option in frame.Options)
                    {
                        XmlElement child = doc.CreateElement("option");
                        SaveXmlPathOption(doc, child, option);
                        element.AppendChild(child);
                    }
                }

                root.AppendChild(element);
            }

            /// 將文件寫入 (m_name).xtalk 檔案當中:
            String fileName = m_name + ".xtalk";
            String scenePathName = Path.Combine(m_proj.Dir, fileName);
            Debug.WriteLine(String.Format("XTScene.SaveXml({0})", scenePathName));

            using (XmlTextWriter writer = new XmlTextWriter(scenePathName, null))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            isModified = false;
        }

        /* <option value="1">
             <text>選項內容</text>
             <label id="要前往的標籤" />
             <url target="空白 or _top or _blank" href="網址或網頁" packopt="true or false" />
           </option> */
        private void SaveXmlPathOption(XmlDocument doc, XmlElement element, XTFrame.PathOption option)
        {
            if (String.IsNullOrEmpty(option.Value) == false)
            {   element.SetAttribute("value", option.Value);   }

            if (String.IsNullOrEmpty(option.Text) == false)
            {
                XmlElement child = doc.CreateElement("text");
                child.InnerXml = option.Text;
                element.AppendChild(child);
            }

            if (option.Type == XTFrame.LinkType.Label)
            {
                XmlElement child = doc.CreateElement("label");
                child.SetAttribute("id", option.Goto);
                element.AppendChild(child);
            }
            else if (option.Type == XTFrame.LinkType.Url)
            {
                XmlElement child = doc.CreateElement("url");
                if (option.Target == XTFrame.LinkTarget.Top)
                {   child.SetAttribute("target", "_top");  }
                else if (option.Target == XTFrame.LinkTarget.Blank)
                {   child.SetAttribute("target", "_blank");  }

                child.SetAttribute("href", option.Goto);

                if (option.PackOpts == true)
                {   child.SetAttribute("packopt", "true");  }
                else {  child.SetAttribute("packopt", "false");  }

                element.AppendChild(child);
            }
        }
        #endregion
    }
}
