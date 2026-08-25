using UnityEngine;

public class FBXRotator : MonoBehaviour
{
    public float PitchSpeed { get; set; }

    void Update()
    {
        if (PitchSpeed != 0f)
        {
            transform.Rotate(Vector3.right * PitchSpeed * Time.deltaTime, Space.Self);
        }
    }
}
