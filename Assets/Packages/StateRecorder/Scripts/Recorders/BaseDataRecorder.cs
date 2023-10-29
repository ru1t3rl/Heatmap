using Ru1t3rl.StateRecorder.Utilities;
using UnityEngine;

namespace Ru1t3rl.StateRecorder.Recorders
{
    public abstract class BaseDataRecorder : MonoBehaviour
    {
        [SerializeField] protected string path;

        protected DataWriter writer;
        protected Recording recording;

        private RecorderState state = RecorderState.Idle;
        public RecorderState State => state;

        protected virtual void Awake()
        {
            writer = new DataWriter(path);
            recording = new Recording();
        }
        
        public virtual void Play()
        {
            state = RecorderState.Playing;
        }

        public virtual void Pause()
        {
            state = RecorderState.Paused;
        }

        public virtual void Stop()
        {
            state = RecorderState.Idle;
        }

        public virtual void Save()
        {
            writer.OverwriteFileContent(recording);
        }

        protected abstract void RecordData();
    }
}