using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagitekClicker.Classes
{
    [Serializable]
    public class AudioFile(string name)
    {
        public string Name { get; set; } = name;
        public string Path { get; set; } = "";
    }
}
