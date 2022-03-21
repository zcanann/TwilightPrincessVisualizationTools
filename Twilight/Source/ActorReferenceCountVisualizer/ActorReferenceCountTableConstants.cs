using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twilight.Source.ActorReferenceCountVisualizer
{
    public class ActorReferenceCountTableConstants
    {
        public static readonly UInt32 ActorReferenceTableBase = 0x804224B8;
        public static readonly Int32 ActorReferenceCountTableMaxEntries = 128;
        public static readonly Int32 ActorSlotStructSize = typeof(ActorReferenceCountTableSlot).StructLayoutAttribute.Size;
    }
}
