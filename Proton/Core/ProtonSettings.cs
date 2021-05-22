using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using Verse;

namespace Proton
{
    public class AlertSettings : IExposable
    {
        public bool enabled = true;

        public string typeId;

        public float AverageExecutionTime
        {
            get => avgT;
        }

        public float TimeSinceLastExecution
        {
            get => stopwatch?.ElapsedMilliseconds ?? -1f;
        }

        public bool ShouldUpdate
        {
            get
            {
                if (counter < 15)
                    return true;
                float elapsedSeconds = ((float)stopwatch.ElapsedTicks / Stopwatch.Frequency);
                if (avgT > 10.0f && counter > 30)
                    return avgT < 15f && elapsedSeconds > 120;
                if (avgT > 2.5f)
                    return elapsedSeconds > Math.Min(30f * (avgT - 1.5f), 60);
                if (elapsedSeconds <= 3.5f)
                    return false;
                if (elapsedSeconds >= 25f)
                    return true;
                return 10f * avgT <= elapsedSeconds / 4.0f;
            }
        }

        private int counter = 0;

        private float avgT = 0f;

        private Stopwatch stopwatch = new Stopwatch();

        public void UpdatePerformanceMetrics(float t)
        {
            avgT = avgT * 0.9f + 0.1f * t;
            counter++;
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            stopwatch.Restart();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeId, "typeId");
            Scribe_Values.Look(ref avgT, "avgT", 0.05f);
            Scribe_Values.Look(ref enabled, "enabled2", true);
        }
    }

    public class ProtonSettings : IExposable
    {
        public void ExposeData()
        {
            List<AlertSettings> settings = Context.alertsSettings?.ToList() ?? new List<AlertSettings>();
            Scribe_Collections.Look(ref settings, "settings", LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.Saving && settings != null)
            {
                foreach (var s in settings)
                {
                    Context.typeIdToSettings[s.typeId] = s;
                }
            }
        }
    }
}
