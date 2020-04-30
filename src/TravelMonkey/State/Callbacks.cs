using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TravelMonkey.State
{
    public class Callbacks
    {
        public static Callbacks Instance { get; } = new Callbacks();

        public Func<MemoryStream, Task> Detect { get; set; }
        public MemoryStream streamImage = new MemoryStream();


    }
}
