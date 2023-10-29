using System.Collections;
using UnityEngine;

namespace Ru1t3rl.StateRecorder.Recorders
{
    public abstract class IntervalRecorder : BaseDataRecorder
    {
        [SerializeField] private float interval = .5f;
        private Coroutine recordRoutine = null;

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