using UnityEngine;
using Cinemachine;

public class CameraRotation : MonoBehaviour
{
    public CinemachineVirtualCamera lockCamera;
    public CinemachineTargetGroup cinemachineTargetGroup;
    public Transform player;
    public Transform target;
    public float angle;
    public float horizontalAngle;

    private CinemachineOrbitalTransposer orbitalTransposer;

    void Start()
    {
        orbitalTransposer = lockCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }
    void Update()
    {
        target = cinemachineTargetGroup.m_Targets.Length > 1 ? cinemachineTargetGroup.m_Targets[1].target : null;
        if (target != null)
        {
            Vector3 directionToTarget = target.position - player.position;
            Vector3 directionToCamera = player.position - lockCamera.transform.position;
            angle = Vector3.SignedAngle(directionToCamera, directionToTarget, Vector3.up);
            horizontalAngle = Vector2.SignedAngle(new Vector2(directionToCamera.x, directionToCamera.z), new Vector2(directionToTarget.x, directionToTarget.z));
            if (Mathf.Abs(horizontalAngle) > 5f)
                orbitalTransposer.m_XAxis.Value += angle * Time.deltaTime;
        }

    }
}
