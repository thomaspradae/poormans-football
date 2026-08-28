using FootballWorldLab.Core.Ids;

namespace FootballWorldLab.Core.Events
{
    /// <summary>
    /// Represents an effect produced by a simulation event or rule execution.
    /// </summary>
    public sealed record Effect
    {
        public StableId EffectId { get; }
        public StableId SourceEventId { get; }
        public string SourceRuleId { get; }
        public StableId TargetEntityId { get; }
        public string TargetProperty { get; }
        public object? OldValue { get; }
        public object? NewValue { get; }
        public long Tick { get; }

        public Effect(
            StableId effectId,
            StableId sourceEventId,
            string sourceRuleId,
            StableId targetEntityId,
            string targetProperty,
            object? oldValue,
            object? newValue,
            long tick)
        {
            EffectId = effectId;
            SourceEventId = sourceEventId;
            SourceRuleId = sourceRuleId;
            TargetEntityId = targetEntityId;
            TargetProperty = targetProperty;
            OldValue = oldValue;
            NewValue = newValue;
            Tick = tick;
        }
    }
}
