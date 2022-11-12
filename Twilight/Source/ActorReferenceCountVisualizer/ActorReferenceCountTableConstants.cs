using System;
using Twilight.Source.Main;

namespace Twilight.Source.ActorReferenceCountVisualizer
{
    public class ActorReferenceCountTableConstants
    {
        public static readonly UInt32 ActorReferenceTableBaseGc = 0x804224B8;
        public static readonly UInt32 ActorReferenceTableBaseWii_1_0 = 0x804AEC34;
        public static readonly UInt32 ActorReferenceTableBaseWii_1_2 = 0x8049623C;
        public static readonly Int32 ActorReferenceCountTableMaxEntries = 128;
        public static readonly Int32 ActorSlotStructSize = typeof(ActorReferenceCountTableSlot).StructLayoutAttribute.Size;

        public static UInt32 GetActorReferenceTableSize()
        {
            return MainViewModel.GetInstance().IsWii ? ActorReferenceTableBaseWii_1_0 : ActorReferenceTableBaseGc;
        }
    }
}
