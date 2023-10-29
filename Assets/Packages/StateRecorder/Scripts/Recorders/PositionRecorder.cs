using System;
using System.Threading.Tasks;
using Object = System.Object;
using Ru1t3rl.StateRecorder.Utilities.Json;

namespace Ru1t3rl.StateRecorder.Recorders
{
    public class PositionRecorder : IntervalRecorder
    {
        protected override async Task RecordData()
        {
            DataPoint<Object> transformRecording = new DataPoint<Object>();

            transformRecording.timestamp = DateTime.Now;
            transformRecording.value = transform.position;
            transformRecording.eventType = EventType.Position;
            recording.AddDataPoint(transformRecording);

            await writer.OverwriteFileContent(recording, new Vector3JsonConverter());
        }
    }
}