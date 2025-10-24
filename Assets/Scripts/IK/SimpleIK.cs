using UnityEngine;

/// <summary>
///Ik solver and interpolation between FK/IK 
/// </summary>
[ExecuteInEditMode]
public class SimpleIK : MonoBehaviour
{
    public Transform[] _bones;
    public Transform _target;
    public Transform _rotationTarget;
    public Vector3 _offset;
    public float _ShoulderOffset;
    [Range(0f, 1f)]
    public float _interpolationSwitch = 0;

    Quaternion[] _FKRotations = new Quaternion[4];
    Quaternion[] _IKRotations = new Quaternion[4];
    Quaternion[] _actualRot = new Quaternion[4];
    float[] _lengths = new float[2];
    float _totalLength;

    void Start()
    {
        SetUpLength();
    }

    void LateUpdate()
    {
        SetUpLength();

        SolveIK();
        SolveFK();
        Interpolation(_interpolationSwitch);
    }

    void SetUpLength()
    {
        if (_lengths[0] != 0 || _lengths[1] != 0 || _totalLength != 0)
            return;

        _lengths[0] = Vector3.Distance(_bones[0].position, _bones[1].position);
        _lengths[1] = Vector3.Distance(_bones[1].position, _bones[2].position);
        _totalLength = _lengths[0] + _lengths[1];
    }

    void SolveFK()
    {
        if (_interpolationSwitch != 0)
            return;

        _FKRotations[0] = _bones[0].rotation;
        _FKRotations[1] = Quaternion.identity;
        _FKRotations[2] = _bones[1].rotation;
        _FKRotations[3] = Quaternion.identity;
    }

    void SolveIK()
    {
        if (_bones[0] == null || _bones[1] == null || _bones[2] == null || _target == null || _rotationTarget == null)
            return;

        Vector3 shoulderToTarget = _target.position - _bones[0].position;
        float targetDistance = Mathf.Min(shoulderToTarget.magnitude, _totalLength - 0.001f);
        Vector3 shoulderToElbowTarget = (_rotationTarget.position - _bones[0].position).normalized;
        Vector3 axis = Vector3.Cross(shoulderToTarget, shoulderToElbowTarget).normalized;
        if (axis.sqrMagnitude < 0.001f)
            axis = Vector3.up; // fallback security

        //Cosinus law
        float cosAngle0 = Mathf.Clamp((_lengths[0] * _lengths[0] + targetDistance * targetDistance - _lengths[1] * _lengths[1]) / (2 * _lengths[0] * targetDistance), -1.0f, 1.0f);
        float shoulderAngle = Mathf.Acos(cosAngle0) * Mathf.Rad2Deg;

        //Shoulder rot
        Quaternion shoulderToTargetRotation = Quaternion.LookRotation(shoulderToTarget, axis) * Quaternion.Euler(_offset);

        //Shoulder rot target
        Quaternion shoulderRotWorld = Quaternion.AngleAxis(shoulderAngle, axis);
        Quaternion shoulderRotLocal = Quaternion.AngleAxis(_ShoulderOffset, Vector3.up);

        //Elbow rot
        Vector3 elbowToHandWorld = _target.position - _bones[1].position;
        Quaternion elbowToHandLocal = Quaternion.AngleAxis(_ShoulderOffset, Vector3.up);

        _IKRotations[0] = shoulderRotWorld * shoulderToTargetRotation;
        _IKRotations[1] = shoulderRotLocal;
        _IKRotations[2] = Quaternion.LookRotation(elbowToHandWorld, axis) * Quaternion.Euler(_offset);
        _IKRotations[3] = elbowToHandLocal;
    }

    void Interpolation(float time)
    {
        _actualRot[0] = Quaternion.Lerp(_FKRotations[0], _IKRotations[0], time);
        _actualRot[1] = Quaternion.Lerp(_FKRotations[1], _IKRotations[1], time);
        _actualRot[2] = Quaternion.Lerp(_FKRotations[2], _IKRotations[2], time);
        _actualRot[3] = Quaternion.Lerp(_FKRotations[3], _IKRotations[3], time);

        _bones[0].rotation = _actualRot[0];
        _bones[0].localRotation *= _actualRot[1];
        _bones[1].rotation = _actualRot[2];
        _bones[1].localRotation *= _actualRot[3];
    }
}