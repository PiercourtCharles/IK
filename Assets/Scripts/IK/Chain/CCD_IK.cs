using UnityEngine;

public class CCD_IK : MonoBehaviour
{
    public Transform target; // Cible � atteindre
    public Transform[] bones; // Bones du bout vers la base
    public int maxIterations = 10; // Nombre max d'it�rations
    public float threshold = 0.001f; // Distance minimale avant d�arr�ter
    public float damping = 0.8f; // Facteur d�adoucissement (0.1 � 1.0)

    void Update()
    {
        SolveIK();
    }

    void SolveIK()
    {
        if (bones == null || bones.Length == 0 || target == null)
            return;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            Transform effector = bones[0];

            // V�rifier si l�effector est proche de la cible
            if (Vector3.Distance(effector.position, target.position) < threshold)
                break;

            // Parcourir les bones du dernier vers le premier
            for (int i = 1; i < bones.Length; i++)
            {
                Transform bone = bones[i];

                // Vecteurs avant/apr�s
                Vector3 toEffector = effector.position - bone.position;
                Vector3 toTarget = target.position - bone.position;

                // V�rifier si les vecteurs sont valides (�viter des erreurs)
                if (toEffector.sqrMagnitude < 0.0001f || toTarget.sqrMagnitude < 0.0001f)
                    continue;

                // Trouver la rotation n�cessaire
                Quaternion rotationNeeded = Quaternion.FromToRotation(toEffector, toTarget);
                bone.rotation = Quaternion.Slerp(Quaternion.identity, rotationNeeded, damping) * bone.rotation;
            }
        }
    }
}
