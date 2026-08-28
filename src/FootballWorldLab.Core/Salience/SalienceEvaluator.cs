using System;
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Salience
{
    public static class SalienceEvaluator
    {
        /// <summary>
        /// Calculates a salience score from 0.0 (routine event) to 1.0 (historic event).
        /// </summary>
        public static double EvaluateMatchSalience(MatchCompletedEvent evt, WorldState state, double homeElo, double awayElo, bool isTitleDecider = false)
        {
            double baseScore = 0.2;

            // Upset calculation: if lower Elo team beats higher Elo team
            double eloDiff = homeElo - awayElo; // positive means home is favored
            bool homeWon = evt.HomeGoals > evt.AwayGoals;
            bool awayWon = evt.AwayGoals > evt.HomeGoals;
            
            double upsetFactor = 0.0;
            if (homeWon && eloDiff < -100)
            {
                upsetFactor = Math.Min(0.5, Math.Abs(eloDiff) / 500.0);
            }
            else if (awayWon && eloDiff > 100)
            {
                upsetFactor = Math.Min(0.5, eloDiff / 500.0);
            }

            // High scoring thriller factor
            int totalGoals = evt.HomeGoals + evt.AwayGoals;
            double goalFactor = Math.Min(0.3, totalGoals * 0.05);

            double titleStakesFactor = isTitleDecider ? 0.3 : 0.0;

            double score = baseScore + upsetFactor + goalFactor + titleStakesFactor;
            return Math.Clamp(score, 0.0, 1.0);
        }

        public static double EvaluateTransferSalience(PlayerTransferredEvent evt, double playerRating, decimal fee)
        {
            double ratingFactor = Math.Max(0.0, (playerRating - 75.0) / 25.0) * 0.4;
            double feeFactor = Math.Min(0.4, (double)fee / 50_000_000.0 * 0.4);
            return Math.Clamp(0.1 + ratingFactor + feeFactor, 0.0, 1.0);
        }
    }
}
