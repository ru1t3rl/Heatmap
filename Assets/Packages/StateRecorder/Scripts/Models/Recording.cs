using System;
using System.Collections.Generic;

namespace Ru1t3rl.StateRecorder
{
    [Serializable]
    public class Recording
    {
        public DateTime startTimestamp { get; private set; }
        public List<DataPoint<Object>> dataPoints { get; private set; } = new();

        public Recording()
        {
            startTimestamp = DateTime.Now;
        }

        public void AddDataPoint(DataPoint<Object> dataPoint)
        {
            dataPoints.Add(dataPoint);
        }

        public void Clear()
        {
            dataPoints.Clear();
        }
    }
}