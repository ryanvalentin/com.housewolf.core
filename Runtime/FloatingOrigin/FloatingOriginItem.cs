using UnityEngine;

/// <summary>
/// Attach to any child object that needs to be moved with the origin.
/// </summary>
public class FloatingOriginItem : MonoBehaviour
{
    public static FloatingOriginItem CreateFloatingGameObject(string name)
    {
        var go = new GameObject(name);
        var floatingItem = go.AddComponent<FloatingOriginItem>();
        floatingItem._registerTransform = go.transform;

        return floatingItem;
    }

    [SerializeField]
    private FloatingOriginUtility.ResetOriginEvent _onResetOrigin;

    [SerializeField]
    private Transform _registerTransform;

    [SerializeField]
    private Rigidbody _registerRigidbody;

    [SerializeField]
    private ParticleSystem _registerParticleSystem;

    public Transform RegisteredTransform
    {
        get => _registerTransform;
        set => _registerTransform = value;
    }

    private void OnEnable()
    {
        if (!TryGetOriginHandler(out FloatingOriginUtility originHandler))
            return;

        if (_onResetOrigin != default)
            originHandler.Register(HandleResetOrigin);

        if (_registerTransform && !_registerTransform.parent)
            originHandler.Register(_registerTransform);

        if (_registerRigidbody)
            originHandler.Register(_registerRigidbody);

        if (_registerParticleSystem)
            originHandler.Register(_registerParticleSystem);
    }

    private void OnDisable()
    {
        if (!TryGetOriginHandler(out FloatingOriginUtility originHandler))
            return;

        if (_onResetOrigin != default)
            originHandler.Unregister(HandleResetOrigin);

        if (_registerTransform)
            originHandler.Unregister(_registerTransform);

        if (_registerRigidbody)
            originHandler.Unregister(_registerRigidbody);

        if (_registerParticleSystem)
            originHandler.Unregister(_registerParticleSystem);
    }

    private bool TryGetOriginHandler(out FloatingOriginUtility originHandler)
    {
        if (FloatingOriginUtility.Instance)
        {
            originHandler = FloatingOriginUtility.Instance;
            return true;
        }

        originHandler = null;
        return false;
    }

    private void HandleResetOrigin(Vector3 currentPosition)
    {
        _onResetOrigin.Invoke(currentPosition);
    }
}
