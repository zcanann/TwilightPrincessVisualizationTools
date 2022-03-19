﻿namespace Twilight.Engine.Scanning.Snapshots
{
    /// <summary>
    /// Interface for a class which listens for changes in the active snapshot.
    /// </summary>
    public interface ISnapshotObserver
    {
        /// <summary>
        /// Recieves an update of the active snapshot.
        /// </summary>
        /// <param name="snapshot">The active snapshot.</param>
        void Update(Snapshot snapshot);
    }
    //// End interface
}
//// End namespace