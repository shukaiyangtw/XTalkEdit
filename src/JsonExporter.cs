/// -----------------------------------------------------------------------------------------------
/// <summary>
///     XTFrame 轉 JSON
/// </summary>
/// <remarks>
///     在 XTScene 中最主要的資料就是 Frames 陣列，這個類別將它轉換成 JSON 物件的陣列。
/// </remarks>
/// <history>
///     2022/4/22 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace XTalkEdit
{
    class JsonExporter
    {
        private XTScene m_scene = null;

        public JsonExporter(XTScene scene)
        {   m_scene = scene;  }

        /// <summary>
        ///  將 m_scene.Frames 轉換為 JSON 物件的陣列。
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var xt_width = ");
            sb.Append(m_scene.Project.PxWidth);
            sb.Append(";\nvar xt_height = ");
            sb.Append(m_scene.Project.PxHeight);
            sb.Append(";\nvar xt_baseline = 0;\n");
            sb.Append("var xt_optionConfirm = false;\n\n");

            sb.Append("var xt_frames =\n[\n");
            int i = 0, lastFrameIndex = m_scene.Frames.Count - 1;
            foreach (XTFrame frame in m_scene.Frames)
            {
                sb.Append(FrameToJson(frame));
                if (i == lastFrameIndex) {  sb.Append("\n];\n");  }
                else {  sb.Append(",\n");  }
                ++i;
            }

            return sb.ToString();
        }

        static public string CRLFtoBR(string str)
        {
            string s = str.Replace("\r", String.Empty);
            return s.Replace("\n", "<br />");
        }

        /// <summary>
        ///  將單一個 XTFrame 轉為一段 JSON 字串。
        /// </summary>
        static public String FrameToJson(XTFrame frame)
        {
            StringBuilder sb = new StringBuilder("{\n");
            if (String.IsNullOrEmpty(frame.Label) == false)
            {
                sb.Append("    label: \"");
                sb.Append(frame.Label);
                sb.Append("\",\n");
            }

            if (String.IsNullOrEmpty(frame.Background) == false)
            {
                if (frame.Type == XTFrame.FrameType.CoverPage)
                {   sb.Append("    cover: \"");   }
                else {   sb.Append("    background: \"");   }
                sb.Append(frame.Background);
                sb.Append("\",\n");
            }

            if (frame.BgIsDarken == true)
            {   sb.Append("    darken: \"true\",\n");  }

            if (frame.BgIsBlur == true)
            {   sb.Append("    blur: \"true\",\n");  }

            if (frame.Sound != null)
            {
                sb.Append("    sound: \"xt_");
                sb.Append(frame.Sound.ID);
                sb.Append("\",\n");
            }

            if (frame.Type == XTFrame.FrameType.Dialog)
            {
                if (frame.Left != null)
                {
                    sb.Append("    left: \"xt_");
                    sb.Append(frame.Left.ID);
                    sb.Append("\",\n");
                }

                if (frame.Left2 != null)
                {
                    sb.Append("    left2: \"xt_");
                    sb.Append(frame.Left2.ID);
                    sb.Append("\",\n");
                }

                if (frame.Center != null)
                {
                    sb.Append("    center: \"xt_");
                    sb.Append(frame.Center.ID);
                    sb.Append("\",\n");
                }

                if (frame.Right != null)
                {
                    sb.Append("    right: \"xt_");
                    sb.Append(frame.Right.ID);
                    sb.Append("\",\n");
                }

                if (frame.Right2 != null)
                {
                    sb.Append("    right2: \"xt_");
                    sb.Append(frame.Right2.ID);
                    sb.Append("\",\n");
                }

                if (String.IsNullOrEmpty(frame.Speaker) == false)
                {
                    sb.Append("    speaker: \"");
                    sb.Append(frame.Speaker);
                    sb.Append("\",\n");
                }

                if (String.IsNullOrEmpty(frame.Text) == false)
                {
                    sb.Append("    text: \"");
                    sb.Append(CRLFtoBR(frame.Text).Replace("\"", "\\\""));
                    sb.Append("\",\n");
                }
            }
            else if (frame.Type == XTFrame.FrameType.FullDialog)
            {
                sb.Append("    full_text: \"");
                sb.Append(CRLFtoBR(frame.Text).Replace("\"", "\\\""));
                sb.Append("\",\n");
            }

            if (frame.Options.Count > 0)
            {
                sb.Append("    options: \n    [\n");
                foreach (XTFrame.PathOption option in frame.Options)
                {
                    sb.Append("        {\n");
                    sb.Append("            text: \"");
                    sb.Append(option.Text.Replace('"', '\''));
                    sb.Append("\",\n");

                    if (String.IsNullOrEmpty(option.Value) == false)
                    {
                        sb.Append("            value: \"");
                        sb.Append(option.Value);
                        sb.Append("\",\n");
                    }

                    if (option.Type == XTFrame.LinkType.Label)
                    {
                        sb.Append("            label: \"");
                        sb.Append(option.Goto);
                        sb.Append("\"\n");
                    }
                    else if (option.Type == XTFrame.LinkType.Url)
                    {
                        sb.Append("            url: \"");
                        sb.Append(option.Goto);
                        sb.Append("\",\n");

                        if (option.Target == XTFrame.LinkTarget.Top)
                        {   sb.Append("            target: \"_top\",\n");  }
                        else if (option.Target == XTFrame.LinkTarget.Blank)
                        {   sb.Append("            target: \"_blank\",\n");  }

                        if (option.PackOpts == true)
                        {   sb.Append("            packopt: \"true\"\n");  }
                        else {   sb.Append("            packopt: \"false\"\n");  }
                    }

                    sb.Append("        },\n");
                }
                sb.Append("    ]\n");
            }
            else
            {
                sb.Append("    next: \n    {\n");
                if (frame.Next.Type == XTFrame.LinkType.Label)
                {
                    sb.Append("        label: \"");
                    sb.Append(frame.Next.Goto);
                    sb.Append("\"\n");
                }
                else if (frame.Next.Type == XTFrame.LinkType.Url)
                {
                    sb.Append("        url: \"");
                    sb.Append(frame.Next.Goto);
                    sb.Append("\",\n");

                    if (frame.Next.Target == XTFrame.LinkTarget.Top)
                    {   sb.Append("        target: \"_top\",\n");  }
                    else if (frame.Next.Target == XTFrame.LinkTarget.Blank)
                    {   sb.Append("        target: \"_blank\",\n");  }

                    if (frame.Next.PackOpts == true)
                    {   sb.Append("        packopt: \"true\"\n");  }
                    else {   sb.Append("        packopt: \"false\"\n");  }
                }

                sb.Append("    }\n");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
