<<<<<<< ours
=======
using System.Collections.Immutable;
>>>>>>> theirs
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Rules
{
    /// <summary>
    /// Represents an effect that can be applied to the world state.
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// Applies the effect to the given world state, returning a new state.
        /// </summary>
        WorldState Apply(WorldState state);
    }

    /// <summary>
    /// Represents a rule that can produce effects based on the current state and events.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Evaluates the rule against the current state and recent events, producing zero or more effects.
        /// </summary>
        ImmutableArray<IEffect> Evaluate(WorldState state, ImmutableArray<BaseEvent> recentEvents);
    }
<<<<<<< ours
}
=======
}
>>>>>>> theirs
