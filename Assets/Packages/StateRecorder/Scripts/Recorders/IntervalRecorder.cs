using System.Collections;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Ru1t3rl.StateRecorder.Recorders
{
    public abstract class IntervalRecorder : BaseDataRecorder
    {
        [SerializeField] private bool recordOnAwake = true;
        [SerializeField] private float interval = .5f;
        private Coroutine recordRoutine = null;

        protected override void Awake()
        {
            base.Awake();
            if (!recordOnAwake) return;
            Play();
        }

        public override void Play()
        {
            base.Play();
            recordRoutine ??= StartCoroutine(Record());
        }

        private IEnumerator Record()
        {
            do
            {
                OnInterval();
                yield return new WaitForSeconds(interval);
                while (State == RecorderState.Paused)
                {
                    yield return new WaitForSeconds(.25f);
                }
            } while (State != RecorderState.Idle);

            recordRoutine = null;
        }

        protected virtual void OnInterval()
        {
            RecordData();
        }
    }
}