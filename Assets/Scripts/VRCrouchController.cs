using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class VRCrouchController : MonoBehaviour
{
    public XROrigin xrOrigin; // ��Inspector�з������XR Origin����
    public CharacterController characterController; // ��ѡ�����ʹ��CharacterController
    public float crouchHeightRatio = 0.6f; // �¶�ʱ�߶������ı������ɸ���ģ�͵���
    public float heightChangeSmoothing = 0.1f; // �߶ȱ仯ƽ������

    private float originalCameraYOffset;
    private float originalCharacterHeight;
    private float currentHeightVelocity; // ����ƽ���߶ȱ仯

    void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = GetComponent<XROrigin>();
        }

        // ��¼��ʼ��Camera Y��ƫ��
        originalCameraYOffset = xrOrigin.CameraFloorOffsetObject.transform.localPosition.y;

        // ���ʹ����CharacterController����¼��ʼ�߶Ȳ��������ĵ�
        if (characterController != null)
        {
            originalCharacterHeight = characterController.height;
            // ��ʼ����CharacterController�����ĵ�͸߶ȣ�ȷ����ײ�����ȷ
            characterController.center = new Vector3(0, originalCharacterHeight / 2, 0);
        }
    }

    void Update()
    {
        // ��ȡͷ���豸�ڸ��ٿռ��еľֲ�λ��
        Vector3 headLocalPosition;
        if (GetHMDLocalPosition(out headLocalPosition))
        {
            // ����ͷ���豸����ڳ�ʼλ�õ��¶���
            // ͷ���豸���µľֲ�λ�ƣ�Y����٣������û������¶�
            float crouchAmount = Mathf.Max(0, originalCameraYOffset - headLocalPosition.y);

            // Ӧ���¶���������CameraOffset��Y��λ��
            float newCameraYOffset = originalCameraYOffset - crouchAmount;
            // ƽ�����ɵ��µĸ߶�
            float smoothedYOffset = Mathf.SmoothDamp(xrOrigin.CameraFloorOffsetObject.transform.localPosition.y, newCameraYOffset, ref currentHeightVelocity, heightChangeSmoothing);

            Vector3 newCameraPos = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
            newCameraPos.y = smoothedYOffset;
            xrOrigin.CameraFloorOffsetObject.transform.localPosition = newCameraPos;

            // ���ʹ����CharacterController��ͬ��������߶Ⱥ����ĵ�
            if (characterController != null)
            {
                float newCharacterHeight = originalCharacterHeight - crouchAmount;
                // ȷ���߶Ȳ�Ϊ��
                newCharacterHeight = Mathf.Max(newCharacterHeight, 0.1f);
                characterController.height = newCharacterHeight;
                // �������ĵ㣬ȷ����ײ�������¶���ȷ����
                characterController.center = new Vector3(0, newCharacterHeight / 2, 0);
            }
        }
    }

    private bool GetHMDLocalPosition(out Vector3 position)
    {
        // ʹ��Unity XR Input Devicet��ȡHMD��λ��
        InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (hmd.isValid && hmd.TryGetFeatureValue(CommonUsages.devicePosition, out position))
        {
            return true;
        }
        position = Vector3.zero;
        return false;
    }
}