using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;

namespace FootballWorldLab.Core.Events
{
    /// <summary>
    /// Base class for all immutable events in the simulation.
    /// </summary>
    public abstract record BaseEvent
    {
        public StableId EventId { get; }
        public long Tick { get; }
        public ProvenanceInfo Provenance { get; }

        protected BaseEvent(StableId eventId, long tick, ProvenanceInfo provenance)
        {
            EventId = eventId;
            Tick = tick;
            Provenance = provenance;
        }
    }
}