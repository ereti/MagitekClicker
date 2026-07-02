using System;
using System.Collections.Generic;
using System.Text;

namespace MagitekClicker.Classes
{
    [Serializable]
    public class WhitelistedPlayer
    {
        public string PlayerAlias { get; set; }
        public string PlayerName { get; set; }
        public string PlayerWorld { get; set; }

        public string PlayerDisplay { 
            get {
                if (!string.IsNullOrEmpty(PlayerAlias))
                {
                    return $"{PlayerAlias}";
                }
                else
                {
                    return $"{PlayerName}@{PlayerWorld}";
                }
            } 
        }
    }
}
