/// -----------------------------------------------------------------------------------------------
/// <summary>
///     資源檔案
/// </summary>
/// <remarks>
///     有設定名稱的資源檔案。
/// </remarks>
/// <history>
///     2021/7/25 by Shu-Kai Yang (skyang@nycu.edu.tw)
/// </history>
/// -----------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;

namespace XTalkEdit
{
    public class XTNamedFileItem
    {
        public String ID {  get;  set;  }
        public String FileName {  get;  set;  }
        public Boolean IsLooped {  get; set;  }

        /// <summary>
        ///  根據 ID 將 item 安插於 list 中的適當位置，以維持 ID 由小到大排列。
        /// </summary>
        static public void InsertByID(ObservableCollection<XTNamedFileItem> list, XTNamedFileItem newItem)
        {
            int i = 0;

            foreach (XTNamedFileItem item in list)
            {
                if (String.Compare(newItem.ID, item.ID) < 0)
                {   list.Insert(i, newItem);  return;  }

                ++i;
            }

            list.Add(newItem);
        }
    }
}
