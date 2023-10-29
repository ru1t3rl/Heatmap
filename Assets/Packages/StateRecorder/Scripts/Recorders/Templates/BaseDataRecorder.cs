using System.Threading.Tasks;
using Ru1t3rl.StateRecorder.Utilities;
using UnityEngine;

namespace Ru1t3rl.StateRecorder.Recorders
{
    public abstract class BaseDataRecorder : MonoBehaviour
    {
        [SerializeField] protected Recording recording;
        [SerializeField] protected bool recordOnAwake = true;

        protected DataWriter writer;

        private RecorderState state = RecorderState.Idle;
        public RecorderState State => state;

        protected virtual void Awake()
        {
            writer = new DataWriter(recording.Path);

            if (!recordOnAwake) return;
            Play();
            
            Debug.Log("StartEd");
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

        protected abstract Task RecordData();
    }
}