﻿namespace Twilight.Engine.Scanning.Scanners.Pointers.SearchKernels
{
    using Twilight.Engine.Scanning.Scanners.Pointers.Structures;
    using Twilight.Engine.Scanning.Snapshots;
    using System;

    internal class SearchKernelFactory
    {
        public static IVectorSearchKernel GetSearchKernel(Snapshot boundsSnapshot, UInt32 maxOffset, PointerSize pointerSize)
        {
            if (boundsSnapshot.SnapshotRegions.Length < 64)
            {
                // Linear is fast for small region sizes
                return new LinearSearchKernel(boundsSnapshot, maxOffset, pointerSize);
            }
            else
            {
                return new SpanSearchKernel(boundsSnapshot, maxOffset, pointerSize);
            }
        }
    }
    //// End class
}
//// End namespace