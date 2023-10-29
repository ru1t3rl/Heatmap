using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Ru1t3rl.StateRecorder
{
    [CreateAssetMenu(menuName = "Data Recorder/Recording Settings", fileName = "RecordingSettings")]
    public class Recording : ScriptableObject
    {
        public DateTime startTimestamp { get; private set; }
        public List<DataPoint<Object>> dataPoints { get; private set; } = new();

        [SerializeField] private string path;
        public string Path => path;

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