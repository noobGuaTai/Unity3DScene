using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectEnemy : MonoBehaviour
{
    public float radius = 20f;  // 扇形的半径
    public float angle = 60f;  // 扇形的角度
    public LayerMask enemyLayer;
    public bool onDrawSphere = false;

    private Transform tf;
    private PlayerInputController playerInputController;
    private Dictionary<string, GameObject> enemies = new Dictionary<string, GameObject>();
    private int ENEMYLAYER = 6;
    void Start()
    {
        tf = transform;
        playerInputController = GetComponent<PlayerInputController>();
    }


    void Update()
    {
        DetectEnemies();
        CalculateNearestDetectEnemy();
        DetectEnemiesOnAttack();
        UpdateLockTarget();
    }

    /// <summary>
    /// 相机检测朝向内的敌人，用于锁定视角
    /// </summary>
    void DetectEnemies()
    {
        Vector3 camPosition = playerInputController.followedCamera.transform.position;
        Vector3 camForward = playerInputController.followedCamera.transform.forward;

        Collider[] targetsInRadius = Physics.OverlapSphere(camPosition, radius, enemyLayer);

        foreach (Collider target in targetsInRadius)
        {
            Vector3 directionToTarget = (target.transform.position - camPosition).normalized;
            float angleToTarget = Vector3.Angle(camForward, directionToTarget);
            if (angleToTarget < angle / 2 && !enemies.ContainsKey(target.name))
                enemies.Add(target.name, target.gameObject);
            else if (angleToTarget >= angle / 2 && enemies.ContainsKey(target.name))
                enemies.Remove(target.name);
        }

        // 如果敌人不在检测范围内
        List<string> enemiesToRemove = new List<string>();
        foreach (var enemy in enemies)
        {
            if (enemy.Value == null)
            {
                enemiesToRemove.Add(enemy.Key);
                continue;
            }
            if (!targetsInRadius.Contains(enemy.Value.GetComponent<Collider>()))
                enemiesToRemove.Add(enemy.Key);
        }
        foreach (var enemy in enemiesToRemove)
            enemies.Remove(enemy);
        playerInputController.visibleEnemy = enemies.Values.ToList();
    }

    /// <summary>
    /// 计算相机检测到的敌人中，最近的敌人
    /// </summary>
    void CalculateNearestDetectEnemy()
    {
        GameObject nearestEnemy;
        if (enemies.Count > 0)
        {
            nearestEnemy = enemies.Values.ToList()[0];
            float nearestDistance = Vector3.Distance(tf.position, nearestEnemy.transform.position);
            foreach (GameObject enemy in enemies.Values)
            {
                float distanceToEnemy = Vector3.Distance(tf.position, enemy.transform.position);
                if (distanceToEnemy < nearestDistance)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distanceToEnemy;
                }
            }
        }
        else
            nearestEnemy = null;
        playerInputController.nearestDetectEnemy = nearestEnemy;
    }

    /// <summary>
    /// 锁定目标丢失后，重新选择锁定目标
    /// </summary>
    void UpdateLockTarget()
    {
        if (playerInputController.cameraTargetGroup.m_Targets.Length > 1 && playerInputController.cameraTargetGroup.m_Targets[1].target == null)
        {
            if (playerInputController.nearestDetectEnemy != null)
            {
                playerInputController.cameraTargetGroup.m_Targets[1].target = playerInputController.nearestDetectEnemy.transform;
            }
            else
            {
                playerInputController.UnLockCamera();
            }
        }
    }

    /// <summary>
    /// 攻击时，检测最近的敌人，用于近战吸附
    /// </summary>
    void DetectEnemiesOnAttack()
    {
        if (playerInputController.nearestSurroundEnemy == null)
        {
            float detectionRadius = 5.0f;
            Vector3 playerPosition = transform.position;

            Collider[] hitColliders = Physics.OverlapSphere(playerPosition, detectionRadius, enemyLayer);
            foreach (var hitCollider in hitColliders)
            {
                playerInputController.nearestSurroundEnemy = hitCollider.gameObject;
            }
        }

    }


    void OnDrawGizmos()
    {
        if (tf == null || !onDrawSphere)
            return;
        Gizmos.color = Color.green;
        DrawCone(playerInputController.followedCamera.transform.position, playerInputController.followedCamera.transform.forward, radius, angle);
    }

    void DrawCone(Vector3 position, Vector3 direction, float radius, float angle)
    {
        int segmentCount = 30;
        float halfAngle = angle / 2;

        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segmentCount);
            Vector3 segmentDirection = Quaternion.Euler(0, currentAngle, 0) * direction;
            Vector3 pointOnCircle = position + segmentDirection * radius;

            if (i > 0)
            {
                Gizmos.DrawLine(position, pointOnCircle);
            }
        }
    }
}
