using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // 隐藏鼠标指针
        Cursor.visible = false;

        // 锁定鼠标指针到屏幕中央
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 可根据需要在运行时控制鼠标指针的状态
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 解除鼠标锁定，并使其可见
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // 再次隐藏并锁定鼠标
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
