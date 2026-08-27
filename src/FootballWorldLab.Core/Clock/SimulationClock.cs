using System;

namespace FootballWorldLab.Core.Clock
{
    /// <summary>
    /// Discrete deterministic simulation clock managing temporal advancement across days, weeks, seasons, and years.
    /// </summary>
    public sealed class SimulationClock
    {
        public DateTime CurrentDate { get; private set; }
        public int SeasonStartYear { get; private set; }
        public long TotalTicks { get; private set; }

        public int CurrentYear => CurrentDate.Year;
        public int CurrentMonth => CurrentDate.Month;
        public int CurrentDay => CurrentDate.Day;
        public DayOfWeek DayOfWeek => CurrentDate.DayOfWeek;

        public SimulationClock(DateTime startDate)
        {
            CurrentDate = startDate.Date;
            SeasonStartYear = startDate.Year;
            TotalTicks = 0;
        }

        public SimulationClock(int startYear = 2024, int startMonth = 1, int startDay = 1)
            : this(new DateTime(startYear, startMonth, startDay))
        {
        }

        public void StepDay(int days = 1)
        {
            if (days <= 0)
                throw new ArgumentOutOfRangeException(nameof(days), "Days step must be positive.");

            CurrentDate = CurrentDate.AddDays(days);
            TotalTicks += days;
        }

        public void StepWeek(int weeks = 1)
        {
            if (weeks <= 0)
                throw new ArgumentOutOfRangeException(nameof(weeks), "Weeks step must be positive.");

            StepDay(weeks * 7);
        }

        public void AdvanceToNextSeason(int startMonth = 1, int startDay = 1)
        {
            int nextYear = CurrentDate.Year + 1;
            CurrentDate = new DateTime(nextYear, startMonth, startDay);
            SeasonStartYear = nextYear;
            TotalTicks++;
        }

        public override string ToString() => $"{CurrentDate:yyyy-MM-dd} (Tick: {TotalTicks}, Season: {SeasonStartYear})";
    }
}
