/// -----------------------------------------------------------------------------------------------
/// <summary>
///     專案輸出
/// </summary>
/// <remarks>
///     這個類別會檢查每個場景的 .js/.html 檔案與 .xtalk 檔案的日期，決定那些檔案必須重新產製。
/// </remarks>
/// <history>
///     2018/12/28 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Windows;

namespace XTalkEdit
{
    class XTProjectExporter
    {
        private XTProject m_proj = null;

        /// <summary>
        ///  這個類別會自己捕捉所有的 exception 並且將錯誤訊息存放在這裡。
        /// </summary>
        String m_error = String.Empty;
        public String ErrorMessage
        {   get {  return m_error; } }

        /// <summary>
        ///  匯出後的網頁以這個檔案為首頁。
        /// </summary>
        static public readonly String HomepageName = "exported.htm";

        public XTProjectExporter(XTProject project)
        {   m_proj = project;  }

        /// <summary>
        ///  這個函式逐一檢查各場景的檔案版本，並視需要重新產生它們。
        /// </summary>
        public Boolean Run()
        {
            App app = Application.Current as App;

            foreach (XTScene scene in m_proj.Scenes)
            {
                String xtalkPathName = Path.Combine(m_proj.Dir, scene.Name + ".xtalk");
                String jsonPathName = Path.Combine(m_proj.Dir, scene.Name + ".js");
                String htmlPathName = Path.Combine(m_proj.Dir, scene.Name + ".html");

                if (App.IsFileNewerThan(xtalkPathName, jsonPathName))
                {
                    JsonExporter json = new JsonExporter(scene);
                    String str = json.ToString();

                    try {  File.WriteAllText(jsonPathName, str, Encoding.UTF8);  }
                    catch (Exception ex) {  m_error = ex.Message;  return false;  }

                    HtmlExporter html = new HtmlExporter(scene);
                    str = html.ToString();

                    try {  File.WriteAllText(htmlPathName, str, Encoding.UTF8);  }
                    catch (Exception ex) {  m_error = ex.Message;  return false;  }
                }
            }

            /// 檢查 xtalk.proj 和 HomepageName 檔案的日期:
            String projPathName = Path.Combine(m_proj.Dir, "xtalk.proj");
            String outputPathName = Path.Combine(m_proj.Dir, "exported.htm");
            if (App.IsFileNewerThan(projPathName, outputPathName))
            {
                /// 產生 toc.htm:
                StringBuilder sb = new StringBuilder("<!doctype html>\n");
                sb.Append("<html>\n<head>\n");
                sb.Append("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n");
                sb.Append("    <style> a {  font-size:x-large;  text-decoration:none;  }</style>\n");
                sb.Append("</head>\n<body>\n<ul>\n");
                foreach (XTScene scene in m_proj.Scenes)
                {
                    sb.Append("    <li><a href=\"");
                    sb.Append(scene.Name);
                    sb.Append(".html\" target=\"preview\">");
                    sb.Append(scene.Name);
                    sb.Append("</a>\n");
                }
                sb.Append("</ul>\n</body>\n</html>");

                String htmlPathName = Path.Combine(m_proj.Dir, "toc.htm");
                try {  File.WriteAllText(htmlPathName, sb.ToString(), Encoding.UTF8);  }
                catch (Exception ex) {  m_error = ex.Message;  return false;  }

                /// 其實 exported.htm 只是個 frameset 檔案:
                sb = new StringBuilder("<!doctype html>\n");
                sb.Append("<html>\n<frameset cols=200,*>\n");
                sb.Append("    <frame name=\"toc\" src=\"toc.htm\" />\n");
 
                if (m_proj.Scenes.Count > 0)
                {
                    sb.Append("    <frame name=\"preview\" src=\"");
                    sb.Append(m_proj.Scenes[0].Name);
                    sb.Append(".html\" />\n");
                }
                else
                {   sb.Append("    <frame name=\"preview\" src=\"unavailable.htm\" />\n");  }
                sb.Append("</frameset>\n</html>");

                try {  File.WriteAllText(outputPathName, sb.ToString(), Encoding.UTF8);  }
                catch (Exception ex) {  m_error = ex.Message;  return false;  }

                /// 如果場景數量為零，則必須產生 unavailable.htm 檔案:
                if (m_proj.Scenes.Count == 0)
                {
                    htmlPathName = Path.Combine(m_proj.Dir, "unavailable.htm");
                    if (File.Exists(htmlPathName) == false)
                    {
                        try {  File.WriteAllText(htmlPathName, HtmlExporter.BlankHtml(), Encoding.UTF8);  }
                        catch (Exception ex) {  m_error = ex.Message;  return false;  }
                    }
                }
            }

            return true;
        }
    }
}
