﻿namespace Twilight.Engine.Scanning.Scanners.Pointers.Structures
{
    using Twilight.Engine.Common;
    using System;

    public static class PointerSizeExtensions
    {
        public static Int32 ToSize(this PointerSize pointerSize)
        {
            switch (pointerSize)
            {
                case PointerSize.Byte4:
                    return 4;
                case PointerSize.Byte8:
                    return 8;
                default:
                    throw new ArgumentException("Unknown pointer size");
            }
        }

        public static ScannableType ToDataType(this PointerSize pointerSize)
        {
            switch (pointerSize)
            {
                case PointerSize.Byte4:
                    return ScannableType.UInt32;
                case PointerSize.Byte8:
                    return ScannableType.UInt64;
                default:
                    throw new ArgumentException("Unknown pointer size");
            }
        }
    }

    /// <summary>
    /// An enum for possible pointer sizes.
    /// </summary>
    public enum PointerSize
    {
        Byte4,
        Byte8,
    }
    //// End class
}
//// End namespace