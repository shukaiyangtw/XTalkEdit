/// -----------------------------------------------------------------------------------------------
/// <summary>
///     腳本專案
/// </summary>
/// <remarks>
///     腳本專案(project)包括若干個場景腳本(scene scripts)和檔案資產(assets)，其中場景腳本是一序列畫面(frame)
///     的陣列，而畫面中的影像和聲音只能使用這裡的資產檔案。
/// </remarks>
/// <history>
///     2024/9/27 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;

namespace XTalkEdit
{
    public class XTProject
    {
        /// <summary>
        ///  專案的名稱與目錄。
        /// </summary>
        private String m_name;
        private String m_dir;
        public String Name {  get {  return m_name;  }  }
        public String Dir {  get {  return m_dir;  }  }

        /// 原始圖檔尺寸(通常等於背景圖檔尺寸):
        public Int32 PxWidth = 960;
        public Int32 PxHeight = 480;

        /// 每一幕的腳本:
        public ObservableCollection<XTScene> Scenes = new ObservableCollection<XTScene>();

        /// 資源檔案(ID與檔名):
        public ObservableCollection<String> Backgrounds = new ObservableCollection<String>();
        public ObservableCollection<XTNamedFileItem> Sprites = new ObservableCollection<XTNamedFileItem>();
        public ObservableCollection<XTNamedFileItem> Sounds = new ObservableCollection<XTNamedFileItem>();

        /// 跨場景剪貼畫面用的剪貼簿:
        public List<XTFrame> Copied = new List<XTFrame>();

        /// 這個專案是否被更動過:
        public Boolean isModified = false;

        /// <summary>
        ///  在建構式當中指定專案名稱與路徑以後就不可以再更動。
        /// </summary>
        public XTProject(String name)
        {
            App app = Application.Current as App;
            m_name = name;
            m_dir = Path.Combine(app.WorkDir, m_name);
            Debug.WriteLine(String.Format("XTProject.XTProject({0})", m_dir));
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// 查詢資產物件。
        public XTNamedFileItem GetSpriteByID(String id)
        {
            foreach (XTNamedFileItem item in Sprites)
            {   if (item.ID.Equals(id)) {  return item;  }  }
            return null;
        }

        public XTNamedFileItem GetSoundByID(String id)
        {
            foreach (XTNamedFileItem item in Sounds)
            {   if (item.ID.Equals(id)) {  return item;  }  }
            return null;
        }

        #region 讀寫 xtalk.proj 檔案。 
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  載入 m_dir/xtalk.proj 檔案。
        /// </summary>
        public void LoadXml()
        {
            String projPathName = Path.Combine(m_dir, "xtalk.proj");
            Debug.WriteLine(String.Format("XTProject.LoadXml({0})", projPathName));
            if (File.Exists(projPathName) == false)
            {   throw new FileNotFoundException("xtalk.proj not found!");  }

            /// 載入 xtalk.proj 檔案並且 parse 為 XML 文件:
            XmlDocument doc = new XmlDocument();
            doc.Load(projPathName);
         /* Debug.WriteLine("XML file is parsed OK!"); */

            /// 清除舊內容:
            Scenes.Clear();
            Sprites.Clear();
            Sounds.Clear();

            /// 解析 XML 樹狀內容:
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = node as XmlElement;
                    if (element.Name.Equals("canvas"))
                    {
                        /* <canvas width="背景圖像素寬度" height="背景圖像素高度" /> */
                        PxWidth = Int32.Parse(element.Attributes["width"].Value);
                        PxHeight = Int32.Parse(element.Attributes["height"].Value);
                        Debug.WriteLine(String.Format("  canvas: {0} x {1}", PxWidth, PxHeight));
                    }
                    else if (element.Name.Equals("scenes"))
                    {
                        /* <scenes>
                              <add>episode01</add>
                              <add>episode02</add>
                              <add>episode03</add>
                              ...
                           </scenes> */
                        foreach (XmlNode child in element.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element)
                            {
                                XmlElement ele = child as XmlElement;
                                XTScene scene = new XTScene(this, ele.InnerText);
                                Debug.WriteLine(String.Format("  scene: {0}", scene.Name));
                                Scenes.Add(scene);
                            }
                        }
                    }
                    else if (element.Name.Equals("backgrounds"))
                    {
                        /* <backgrounds>
                              <add>episode01.jpg</add>
                              <add>scene01.jpg</add>
                              ...
                           </backgrounds> */
                        foreach (XmlNode child in element.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element)
                            {
                                Backgrounds.Add(child.InnerText);
                             /* Debug.WriteLine(String.Format("  background image: {0}", child.InnerText)); */
                            }
                        }
                    }
                    else if (element.Name.Equals("sprites"))
                    {
                        /* <sprites>
                              <add id="hsiao_eating">hsiao01.png</add>
                              <add id="hsiao_waving">hsiao02.png</add>
                              ...
                           </sprites> */
                        foreach (XmlNode child in element.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element)
                            {
                                XmlElement ele = child as XmlElement;
                                XTNamedFileItem item = new XTNamedFileItem();
                                item.ID = ele.Attributes["id"].Value;
                                item.FileName = ele.InnerText;
                             /* Debug.WriteLine(String.Format("  sprite {0} as {1}", item.FileName, item.ID)); */
                                Sprites.Add(item);
                            }
                        }
                    }
                    else if (element.Name.Equals("sounds"))
                    {
                        /* <sounds>
                              <add id="clapping">clap.mp3</add>
                              <add id="oh_shit">oh_shit.mp3</add>
                           </sounds> */
                        foreach (XmlNode child in element.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element)
                            {
                                XmlElement ele = child as XmlElement;
                                XTNamedFileItem item = new XTNamedFileItem();
                                item.ID = ele.Attributes["id"].Value;
                                item.FileName = ele.InnerText;
                                if (ele.HasAttribute("loop") == true) {  item.IsLooped = true;  }
                             /* Debug.WriteLine(String.Format("  sound {0} as {1}", item.FileName, item.ID)); */
                                Sounds.Add(item);
                            }
                        }
                    }
                }
            }

            isModified = false;
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  儲存 m_dir/xtalk.proj 檔案。
        /// </summary>
        public void SaveXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(declaration);

            XmlElement root = doc.CreateElement("project");
            doc.AppendChild(root);

            XmlElement element = doc.CreateElement("canvas");
            element.SetAttribute("width",  PxWidth.ToString());
            element.SetAttribute("height", PxHeight.ToString());
            root.AppendChild(element);

            /* <scenes>
                  <add>episode01</add>
                  <add>episode02</add>
                  <add>episode03</add>
                  ...
               </scenes> */
            if (Scenes.Count > 0)
            {
                element = doc.CreateElement("scenes");
                foreach (XTScene scene in Scenes)
                {
                    XmlElement child = doc.CreateElement("add");
                    child.InnerText = scene.Name;
                    element.AppendChild(child);
                }
                root.AppendChild(element);
            }

            /* <backgrounds>
                  <add>episode01.jpg</add>
                  <add>scene01.jpg</add>
                  ...
               </backgrounds> */
            if (Backgrounds.Count > 0)
            {
                element = doc.CreateElement("backgrounds");
                foreach (String fileName in Backgrounds)
                {
                    XmlElement child = doc.CreateElement("add");
                    child.InnerText = fileName;
                    element.AppendChild(child);
                }
                root.AppendChild(element);
            }

            /* <sprites>
                  <add id="hsiao_eating">hsiao01.png</add>
                  <add id="hsiao_waving">hsiao02.png</add>
                  ...
               </sprites> */
            if (Sprites.Count > 0)
            {
                element = doc.CreateElement("sprites");
                foreach (XTNamedFileItem item in Sprites)
                {
                    XmlElement child = doc.CreateElement("add");
                    child.SetAttribute("id", item.ID);
                    child.InnerText = item.FileName;
                    element.AppendChild(child);
                }
                root.AppendChild(element);
            }

            /* <sounds>
                  <add id="clapping">clap.mp3</add>
                  <add id="oh_shit">oh_shit.mp3</add>
               </sounds> */
            if (Sounds.Count > 0)
            {
                element = doc.CreateElement("sounds");
                foreach (XTNamedFileItem item in Sounds)
                {
                    XmlElement child = doc.CreateElement("add");
                    child.SetAttribute("id", item.ID);
                    child.InnerText = item.FileName;
                    if (item.IsLooped) {  child.SetAttribute("loop", "true");  }
                    element.AppendChild(child);
                }
                root.AppendChild(element);
            }

            /// 將文件寫入 xtalk.proj 檔案當中:
            String projPathName = Path.Combine(m_dir, "xtalk.proj");
            Debug.WriteLine(String.Format("XTProject.SaveXml({0})", projPathName));

            using (XmlTextWriter writer = new XmlTextWriter(projPathName, null))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }

            isModified = false;
        }
        #endregion
    }
}
