using System;
using System.Threading.Tasks;
using Ru1t3rl.StateRecorder.Utilities.Json;
using UnityEngine;
using Object = System.Object;

namespace Ru1t3rl.StateRecorder.Recorders
{
    public class CollisionRecorder : BaseDataRecorder
    {
        [SerializeField] private bool logOnCollisionEnter = true;
        [SerializeField] private bool logOnCollisionStay = false;

        private async void OnCollisionEnter(Collision other)
        {
            if (State != RecorderState.Playing || !logOnCollisionEnter) return;
            await RecordData();
        }

        private async void OnCollisionStay(Collision other)
        {
            if (State != RecorderState.Playing || !logOnCollisionStay) return;
            await RecordData();
        }

        protected override async Task RecordData()
        {
            DataPoint<Object> transformRecording = new DataPoint<Object>();

            transformRecording.timestamp = DateTime.Now;
            transformRecording.value = new CollisionData
            {
                collisionPoint = transform.position
            };
            transformRecording.eventType = EventType.Collision;
            recording.AddDataPoint(transformRecording);

            await writer.OverwriteFileContent(recording, new Vector3JsonConverter());
        }
    }
}