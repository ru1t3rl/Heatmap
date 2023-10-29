using System;

namespace Ru1t3rl.StateRecorder
{
    [Serializable]
    public class DataPoint<T>
    {
        public DateTime timestamp;
        public T value;
        public EventType? eventType;
    }
}