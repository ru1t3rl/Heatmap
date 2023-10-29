using Ru1t3rl.StateRecorder.Recorders;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace Ru1t3rl.StateRecorder.Editor
{
    [CustomEditor(typeof(BaseDataRecorder))]
    public class BaseDatRecorderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);


            using var hLayout = new EditorGUILayout.HorizontalScope();
            var recorder = target as BaseDataRecorder;

            GUI.enabled = Application.isPlaying && recorder.State != RecorderState.Playing;
            if (GUILayout.Button("Play"))
            {
                recorder.Play();
            }

            GUI.enabled = Application.isPlaying && recorder.State == RecorderState.Playing;
            if (GUILayout.Button("Pause"))
            {
                recorder.Pause();
            }

            GUI.enabled = Application.isPlaying && recorder.State != RecorderState.Idle;
            if (GUILayout.Button("Stop"))
            {
                recorder.Stop();
            }

            hLayout.Dispose();

            GUI.enabled = Application.isPlaying && recorder.State != RecorderState.Playing;
            if (GUILayout.Button("Save"))
            {
                recorder.Save();
            }

            GUI.enabled = true;
        }
    }
}