using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class VRCrouchController : MonoBehaviour
{
    public XROrigin xrOrigin; // 在Inspector中分配你的XR Origin对象
    public CharacterController characterController; // 可选：如果使用CharacterController
    public float crouchHeightRatio = 0.6f; // 下蹲时高度缩减的比例，可根据模型调整
    public float heightChangeSmoothing = 0.1f; // 高度变化平滑参数

    private float originalCameraYOffset;
    private float originalCharacterHeight;
    private float currentHeightVelocity; // 用于平滑高度变化

    void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = GetComponent<XROrigin>();
        }

        // 记录初始的Camera Y轴偏移
        originalCameraYOffset = xrOrigin.CameraFloorOffsetObject.transform.localPosition.y;

        // 如果使用了CharacterController，记录初始高度并调整中心点
        if (characterController != null)
        {
            originalCharacterHeight = characterController.height;
            // 初始设置CharacterController的中心点和高度，确保碰撞检测正确
            characterController.center = new Vector3(0, originalCharacterHeight / 2, 0);
        }
    }

    void Update()
    {
        // 获取头戴设备在跟踪空间中的局部位置
        Vector3 headLocalPosition;
        if (GetHMDLocalPosition(out headLocalPosition))
        {
            // 计算头戴设备相对于初始位置的下蹲量
            // 头戴设备向下的局部位移（Y轴减少）表明用户正在下蹲
            float crouchAmount = Mathf.Max(0, originalCameraYOffset - headLocalPosition.y);

            // 应用下蹲量来调整CameraOffset的Y轴位置
            float newCameraYOffset = originalCameraYOffset - crouchAmount;
            // 平滑过渡到新的高度
            float smoothedYOffset = Mathf.SmoothDamp(xrOrigin.CameraFloorOffsetObject.transform.localPosition.y, newCameraYOffset, ref currentHeightVelocity, heightChangeSmoothing);

            Vector3 newCameraPos = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
            newCameraPos.y = smoothedYOffset;
            xrOrigin.CameraFloorOffsetObject.transform.localPosition = newCameraPos;

            // 如果使用了CharacterController，同步调整其高度和中心点
            if (characterController != null)
            {
                float newCharacterHeight = originalCharacterHeight - crouchAmount;
                // 确保高度不为负
                newCharacterHeight = Mathf.Max(newCharacterHeight, 0.1f);
                characterController.height = newCharacterHeight;
                // 调整中心点，确保碰撞盒随着下蹲正确下移
                characterController.center = new Vector3(0, newCharacterHeight / 2, 0);
            }
        }
    }

    private bool GetHMDLocalPosition(out Vector3 position)
    {
        // 使用Unity XR Input Devicet获取HMD的位置
        InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (hmd.isValid && hmd.TryGetFeatureValue(CommonUsages.devicePosition, out position))
        {
            return true;
        }
        position = Vector3.zero;
        return false;
    }
}