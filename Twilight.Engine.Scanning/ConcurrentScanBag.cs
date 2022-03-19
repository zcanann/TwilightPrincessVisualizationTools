﻿namespace Twilight.Engine.Scanning
{
    using Twilight.Engine.Common.Extensions;
    using Twilight.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class ConcurrentScanBag : ConcurrentBag<IList<SnapshotRegion>>, IEnumerable<SnapshotRegion>
    {
        IEnumerator<SnapshotRegion> IEnumerable<SnapshotRegion>.GetEnumerator()
        {
            foreach (IList<SnapshotRegion> list in this)
            {
                foreach (SnapshotRegion item in list)
                {
                    yield return item;
                }
            }
        }
    }
    //// End class
}
//// End namespace
