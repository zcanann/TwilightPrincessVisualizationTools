
namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.DataStructures;
    using Twilight.Engine.Memory;

    public class ActorSlotsTableDataView : INotifyPropertyChanged
    {
        public ActorSlotsTableDataView(ActorSlotsTableData actorSlotsTableData)
        {
            this.ActorSlotsTableData = actorSlotsTableData;
        }

        public ActorSlotsTableData ActorSlotsTableData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public FullyObservableCollection<RawActorSlotsTableEntry> RawActorSlots
        {
            get
            {
                return this.ActorSlotsTableData.rawActorSlots;
            }

            set
            {
                this.ActorSlotsTableData.rawActorSlots = value;
            }
        }

        public void RefreshAllProperties()
        {
            this.RaisePropertyChanged(nameof(this.RawActorSlots));
        }

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
//// End namespace
