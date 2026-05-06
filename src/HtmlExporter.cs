/// -----------------------------------------------------------------------------------------------
/// <summary>
///     HTML 內容輸出
/// </summary>
/// <remarks>
///     這個類別會以固定範本產生 HTML 內容，視建構式可以是為整個 scene 產生完整的互動網頁，也可以只是單頁內容。
/// </remarks>
/// <history>
///     2019/1/1 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System.Text;
using System.Windows;
using System.Collections.Generic;

namespace XTalkEdit
{
    class HtmlExporter
    {
        private XTScene m_scene = null;
        private XTFrame m_frame = null;

        /// 先收集一下，在 m_scene 或 m_frame 當中用到那些 sprite:
        private List<XTNamedFileItem> m_sprites = new List<XTNamedFileItem>();
        private List<XTNamedFileItem> m_sounds = new List<XTNamedFileItem>();

        /// <summary>
        ///  視建構式決定產生完整的互動網頁或是單頁的預覽。
        /// </summary>
        public HtmlExporter(XTScene scene)
        {
            m_scene = scene;
            foreach (XTFrame frame in scene.Frames)
            {   AddFrameItems(frame);  }
        }

        public HtmlExporter(XTFrame frame)
        {
            m_frame = frame;
            AddFrameItems(frame);
        }

        private void AddFrameItems(XTFrame frame)
        {
            if (frame.Left != null)
            {
                if (m_sprites.Exists(e => e.Equals(frame.Left)) == false)
                {   m_sprites.Add(frame.Left);   }
            }

            if (frame.Left2 != null)
            {
                if (m_sprites.Exists(e => e.Equals(frame.Left2)) == false)
                {   m_sprites.Add(frame.Left2);   }
            }

            if (frame.Center != null)
            {
                if (m_sprites.Exists(e => e.Equals(frame.Center)) == false)
                {   m_sprites.Add(frame.Center);   }
            }

            if (frame.Right != null)
            {
                if (m_sprites.Exists(e => e.Equals(frame.Right)) == false)
                {   m_sprites.Add(frame.Right);   }
            }

            if (frame.Right2 != null)
            {
                if (m_sprites.Exists(e => e.Equals(frame.Right2)) == false)
                {   m_sprites.Add(frame.Right2);   }
            }

            if (frame.Sound != null)
            {
                if (m_sounds.Exists(e => e.Equals(frame.Sound)) == false)
                {   m_sounds.Add(frame.Sound);   }
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////
        static public string BlankHtml()
        {
            StringBuilder sb = new StringBuilder("<!doctype html>\n");
            sb.Append("<html>\n<head>\n");
            sb.Append("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n");
            sb.Append("</head>\n<body>\n    <h1>");
            sb.Append(Properties.Messages.msgBlankHtml);
            sb.Append("</h1>\n</body>\n</html>");
            return sb.ToString();
        }

        /// <summary>
        ///  逐步產生 HTML 內容並以字串形式傳回之。
        /// </summary>
        public override string ToString()
        {
            App app = Application.Current as App;
            StringBuilder sb = new StringBuilder("<!doctype html>\n");
            sb.Append("<html>\n<head>\n");

            /// WebBrowser 預設使用  IE7 相容性，如果要在裡面顯示單畫面的預覽，必須加上這一行:
            if (m_frame != null)
            {   sb.Append("    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\" />\n");  }

            sb.Append("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n");
            sb.Append("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" >\n");

            if (m_frame != null)
            {
                /// 如果是要在 WebBrowser 裡面顯示單畫面的預覽，產生單畫面的 xt_frames 內容，而不是連結到外部 .js 檔案:
                sb.Append("<script type=\"text/javascript\">\n");
                sb.Append("var xt_width = ");
                sb.Append(app.Project.PxWidth);
                sb.Append(";\nvar xt_height = ");
                sb.Append(app.Project.PxHeight);
                sb.Append(";\n\nvar xt_frames =\n[\n");
                sb.Append(JsonExporter.FrameToJson(m_frame));
                sb.Append("\n];\n");
                sb.Append("</script>\n");
            }
            else
            {
                sb.Append("    <script type=\"text/javascript\" src=\"");
                sb.Append(m_scene.Name);
                sb.Append(".js\"></script>\n");
            }

            /// 嵌入播放 xt_frames 的 JavaScript 引擎以及網頁範本的 CSS 樣式檔案:
            sb.Append("    <script type=\"text/javascript\" src=\"../Scripts/xtalkshow-1.0-min.js\"></script>\n");
            sb.Append("    <link rel=\"stylesheet\" href=\"../xtalkshow.css\" type=\"text/css\" />\n");

            #if DEBUG
            /// 為了測試目的，在網頁標題顯示頁面尺寸:
            sb.Append("<script type=\"text/javascript\">\n");
            sb.Append("    function onSize()\n");
            sb.Append("    {   document.title = document.body.clientWidth + \" x \" + document.body.clientHeight;  }\n");
            sb.Append("</script>\n");
            #endif

            sb.Append("</head>\n");

            #if DEBUG
            sb.Append("<body onload=\"xtOnLoad();\" onresize=\"onSize();\">\n\n");
            #else
            sb.Append("<body onload=\"xtOnLoad();\">\n\n");
            #endif

            /// ///////////////////////////////////////////////////////////////////////////////////
            /// 嵌入隱藏的影像與音效資源。
            foreach (XTNamedFileItem item in m_sprites)
            {
                sb.Append("    <img id=\"xt_");
                sb.Append(item.ID);
                sb.Append("\" class=\"xt_preloaded\" src=\"");
                sb.Append(item.FileName);
                sb.Append("\" alt=\"\" />\n");
            }

            if (m_sprites.Count != 0) {  sb.Append("\n");  }

            foreach (XTNamedFileItem item in m_sounds)
            {
                sb.Append("    <audio id=\"xt_");
                sb.Append(item.ID);
                sb.Append("\" preload class=\"xt_embedded\"");
                if (item.IsLooped) {  sb.Append(" loop");  }
                sb.Append("><source src=\"");
                sb.Append(item.FileName);
                sb.Append("\" type=\"audio/");
                if (item.FileName.EndsWith(".ogg")) {  sb.Append("ogg");  }
                else if (item.FileName.EndsWith(".wav")) {  sb.Append("x-wav");  }
                else {  sb.Append("mpeg");  }
                sb.Append("\" /></audio>\n");
            }

            if (m_sounds.Count != 0) {  sb.Append("\n");  }

            /// ///////////////////////////////////////////////////////////////////////////////////
            /// 由下而上建構 canvas 階層，如果是單畫面、所有的連結都是無效連結。
            sb.Append("    <div class=\"xt_container\">\n");
            sb.Append("        <img id=\"xt_scene\" class=\"xt_slide\" alt=\"\" />\n");
            sb.Append("        <canvas id=\"xt_framebuffer\"></canvas>\n");

            sb.Append("        <div id=\"xt_dialog_full\" style=\"display: none\">\n");
            sb.Append("            <span id=\"xt_dialog_fulltext\"></span>\n");

            if (m_frame == null)
            {
                sb.Append("            <a id=\"xt_option1A\" href=\"javascript:xtOnOptionClicked(1);\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option2A\" href=\"javascript:xtOnOptionClicked(2);\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option3A\" href=\"javascript:xtOnOptionClicked(3);\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option4A\" href=\"javascript:xtOnOptionClicked(4);\" class=\"xt_option\" style=\"display: none\"></a>\n");
            }
            else
            {
                sb.Append("            <a id=\"xt_option1A\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option2A\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option3A\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option4A\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
            }

            sb.Append("        </div>\n");

            sb.Append("        <div id=\"xt_dialog\" style=\"display: none\">\n");
            sb.Append("            <span id=\"xt_dialog_text\"></span>\n");

            if (m_frame == null)
            {
                sb.Append("            <a id=\"xt_option1B\" href=\"javascript:xtOnOptionClicked(1);\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option2B\" href=\"javascript:xtOnOptionClicked(2);\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option3B\" href=\"javascript:xtOnOptionClicked(3);\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option4B\" href=\"javascript:xtOnOptionClicked(4);\" class=\"xt_option\" style=\"display: none\"></a>\n");
            }
            else
            {
                sb.Append("            <a id=\"xt_option1B\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option2B\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option3B\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
                sb.Append("            <a id=\"xt_option4B\" href=\"#\" class=\"xt_option\" style=\"display: none\"></a>\n");
            }

            sb.Append("        </div>\n");

            sb.Append("        <div id=\"xt_speaker\" style=\"display: none\"></div>\n");

            sb.Append("        <a id=\"xt_episode_link\" href=\"");
            if (m_frame == null)
            {   sb.Append("javascript:xtOnNextClicked();");  }
            else {  sb.Append("#");  }
            sb.Append("\" style=\"display: none\"><img id=\"xt_episode\" class=\"xt_slide\" alt=\"\" /></a>\n");

            sb.Append("        <a id=\"xt_next\" href=\"");
            if (m_frame == null)
            {   sb.Append("javascript:xtOnNextClicked();");  }
            else {  sb.Append("#");  }
            sb.Append("\"><img border=\"0\" src=\"../Images/xtalknext.png\" alt=\"Next\" /></a>\n");

            sb.Append("    </div>\n</body>\n</html>");
            return sb.ToString();
        }
    }
}
