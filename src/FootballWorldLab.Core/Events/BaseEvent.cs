using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;

namespace FootballWorldLab.Core.Events
{
    /// <summary>
    /// Base class for all immutable, versioned events in the simulation.
    /// </summary>
    public abstract record BaseEvent
    {
        public StableId EventId { get; }
        public long Tick { get; }
        public ProvenanceInfo Provenance { get; }
        public int Version { get; }

        protected BaseEvent(StableId eventId, long tick, ProvenanceInfo provenance, int version = 1)
        {
            EventId = eventId;
            Tick = tick;
            Provenance = provenance;
            Version = version;
        }
    }
}
