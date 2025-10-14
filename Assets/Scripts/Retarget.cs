using UnityEngine;

public class Retarget : MonoBehaviour
{
    public float Distance = 1;
    public Transform TargetAnim;
    public Transform Target;

    private void Start()
    {
        Target.position = TargetAnim.position;
    }

    private void Update()
    {
        if (Vector3.Distance(Target.position, TargetAnim.position) > Distance)
        {
            Target.position = TargetAnim.position;
        }
    }
}
