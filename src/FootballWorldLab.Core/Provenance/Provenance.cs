using System;

namespace FootballWorldLab.Core.Provenance
{
    /// <summary>
    /// Represents the origin or source of data in the simulation.
    /// </summary>
    public enum ProvenanceSource
    {
        /// <summary>
        /// Data is synthetic/procedurally generated.
        /// </summary>
        Synthetic,
        
        /// <summary>
        /// Data is based on real-world information.
        /// </summary>
        RealWorld,
        
        /// <summary>
        /// Data was created or modified by user input.
        /// </summary>
        UserGenerated,
        
        /// <summary>
        /// Data was estimated or calculated from other data.
        /// </summary>
        Derived,
        
        /// <summary>
        /// Data origin is unknown or unspecified.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Tracks the provenance information for an entity.
    /// </summary>
    public readonly record struct ProvenanceInfo
    {
        public ProvenanceSource Source { get; }
        public string? Description { get; }
        public DateTime RecordedAt { get; }
        public string? RecordedBy { get; }

        public ProvenanceInfo(ProvenanceSource source, string? description = null, 
                              DateTime? recordedAt = null, string? recordedBy = null)
        {
            Source = source;
            Description = description;
            RecordedAt = recordedAt ?? DateTime.UtcNow;
            RecordedBy = recordedBy;
        }

        public ProvenanceInfo WithSource(ProvenanceSource source) => 
            new ProvenanceInfo(source, Description, RecordedAt, RecordedBy);

        public ProvenanceInfo WithDescription(string description) => 
            new ProvenanceInfo(Source, description, RecordedAt, RecordedBy);

        public ProvenanceInfo Recorded(string? recordedBy = null) => 
            new ProvenanceInfo(Source, Description, DateTime.UtcNow, recordedBy ?? RecordedBy);
    }
}
