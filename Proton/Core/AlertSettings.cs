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
        public string typeId;

        public Alert alert;

        public bool enabledInt = true;

        public bool Enabled
        {
            get => enabledInt && (avgT < Context.settings.executionTimeLimit || counter < 30);
            set
            {
                enabledInt = value;
                if (!value)
                {
                    UpdateAlert(removeReadout: true);
                }
            }
        }

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
                if (!Enabled)
                    return false;
                if (counter < 15)
                    return true;
                float elapsedSeconds = ((float)stopwatch.ElapsedTicks / Stopwatch.Frequency);
                if (avgT > 2.5f)
                    return elapsedSeconds > Math.Min(30f * (avgT - 1.5f), 60);
                if (elapsedSeconds <= Context.settings.minInterval)
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
            if (counter > 30 && avgT > Context.settings.executionTimeLimit)
            {
                enabledInt = false;
                UpdateAlert();
            }
            stopwatch.Restart();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeId, "typeId");
            Scribe_Values.Look(ref avgT, "avgT", 0.05f);
            Scribe_Values.Look(ref enabledInt, "enabled2", true);
        }

        public void UpdateAlert(bool removeReadout = true)
        {
            if (Enabled)
                return;
            if (alert != null)
            {
                if (removeReadout)
                    Context.readoutInstance.activeAlerts.Remove(alert);
                alert.cachedActive = false;
                return;
            }
            for (int i = 0; i < Context.alerts.Length; i++)
            {
                if (Context.alertSettingsByIndex[i] == this && Context.alerts[i] != null)
                {
                    Alert alert = Context.alerts[i];
                    Context.alertToSettings[alert] = this;
                    if (removeReadout)
                        Context.readoutInstance.activeAlerts.Remove(alert);
                    alert.cachedActive = false;
                }
            }

        }
    }
}