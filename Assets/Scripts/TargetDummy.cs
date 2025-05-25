using UnityEngine;
using DG.Tweening;

public class TargetDummy : MonoBehaviour
{
    [Header("Settings")]
    public Transform rotatingPart;              // The transform that rotates (e.g., head or body)
    public float rotationAmount = 15f;          // Degrees to rotate left/right
    public float rotationDuration = 0.1f;       // Time per twist
    public int shakeCount = 3;                  // How many shakes (back and forth)

    [Tooltip("Layer name for player bullets (must match Unity layer name exactly)")]
    public string playerBulletLayerName = "PlayerBullet";

    private int playerBulletLayer;
    private Quaternion originalRotation;

    void Start()
    {
        if (rotatingPart == null)
        {
            rotatingPart = transform;
        }

        originalRotation = rotatingPart.localRotation;

        playerBulletLayer = LayerMask.NameToLayer(playerBulletLayerName);
        if (playerBulletLayer == -1)
        {
            Debug.LogError($"TargetDummy: Layer '{playerBulletLayerName}' not found. Please add it in Unity's Tags & Layers manager.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == playerBulletLayer)
        {
            Shake();
            PlayerCombat playerCombat = FindAnyObjectByType<PlayerCombat>();
            if (playerCombat == null)
            {
                Debug.LogWarning("TargetDummy: PlayerCombat component not found in the scene.");
                return;
            }
            playerCombat.ApplyKickback();
            playerCombat.PickUpBullet();
        }
    }

    void Shake()
    {
        if (rotatingPart == null) return;

        // Stop any existing tweens and reset to original rotation
        rotatingPart.DOKill();
        rotatingPart.localRotation = originalRotation;

        // Create a shake tween using a Sequence
        Sequence shakeSeq = DOTween.Sequence();

        for (int i = 0; i < shakeCount; i++)
        {
            shakeSeq.Append(rotatingPart.DOLocalRotate(new Vector3(rotatingPart.eulerAngles.x + rotationAmount, 0, 0), rotationDuration).SetEase(Ease.OutSine));
            shakeSeq.Append(rotatingPart.DOLocalRotate(new Vector3(rotatingPart.eulerAngles.x + -rotationAmount, 0, 0), rotationDuration).SetEase(Ease.OutSine));
        }

        // Return to original rotation
        shakeSeq.Append(rotatingPart.DOLocalRotateQuaternion(originalRotation, rotationDuration).SetEase(Ease.InOutSine));
    }
}
