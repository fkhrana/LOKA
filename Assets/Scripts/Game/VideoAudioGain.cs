using UnityEngine;

public class VideoAudioGain : MonoBehaviour
{
    private float gain = 1f;

    public void SetGain(float value)
    {
        gain = Mathf.Max(1f, value);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] *= gain;
    }
}
