using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;

public class Photographer : MonoBehaviour
{
    public float Pitch { get; private set; }// 上下旋转
    public float Yaw { get; private set; }// 左右旋转
    public float mouseSensitivity = 5;// 鼠标灵敏度
    public float cameraRotatingSpeed = 80;// 摇杆旋转速度
    public float cameraYSpeed = 5;// 遥感移动速度
    public AnimationCurve _armLengthCurve;// 相机距离曲线
    public float _scrollSpeed = 2f; // 控制滚轮滚动对距离的影响
    public float scrollMinDistance = -10f;// 镜头拉近最远距离
    public float scrollMaxDistance = -3f;// 镜头拉近最近距离
    public float cameraOffset = 1.2f; // 由于角色位置在脚步，所以需要偏移量把相机目标锁定到头部

    private Transform player;
    private Transform mainCamera;
    private float scrollInput = 0f; // 用于存储鼠标滚轮的输入

    private void Awake()
    {
        mainCamera = transform.GetChild(0);
    }

    public void InitCamera(Transform target)
    {
        player = target;
        transform.position = target.position;
    }

    void Update()
    {
        UpdateRotation();
        UpdatePosition();
        UpdateArmLength();
    }

    private void UpdateRotation()
    {
        Yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        Yaw += Input.GetAxis("CameraRateX") * cameraRotatingSpeed * Time.deltaTime;
        Pitch += Input.GetAxis("Mouse Y") * mouseSensitivity;
        Pitch += Input.GetAxis("CameraRateY") * cameraRotatingSpeed * Time.deltaTime;
        Pitch = Mathf.Clamp(Pitch, -90, 90);

        transform.rotation = Quaternion.Euler(Pitch, Yaw, 0);
    }

    private void UpdatePosition()
    {
        Vector3 position = new Vector3(player.position.x, player.position.y + cameraOffset, player.position.z);
        float newY = Mathf.Lerp(transform.position.y, position.y, Time.deltaTime * cameraYSpeed);
        transform.position = new Vector3(position.x, newY, position.z);
    }

    private void UpdateArmLength()
    {
        scrollInput += Input.GetAxis("Mouse ScrollWheel") * _scrollSpeed;
        scrollInput = Math.Clamp(scrollInput, -5, 10 + scrollMaxDistance);
        mainCamera.localPosition = new Vector3(0, 0, Math.Clamp(_armLengthCurve.Evaluate(Pitch) * -5 + scrollInput, scrollMinDistance, scrollMaxDistance));
    }
}
