using UnityEngine;

public class MouseAimSource : MonoBehaviour, IAimSource
{
    [SerializeField] private Camera cam;
    [Tooltip("Mouse world position'unu bulmak için kullanýlan zemin Y'si")]
    [SerializeField] private float groundY = 0f;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    public Vector3 GetAimDirection(Vector3 fromPosition)
    {
        if (cam == null) return Vector3.zero;

        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3 dir = mouseWorld - fromPosition;
        dir.y = 0f;

        return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.zero;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreen = UnityEngine.InputSystem.Mouse.current != null
            ? UnityEngine.InputSystem.Mouse.current.position.ReadValue()
            : Vector2.zero;

        Ray ray = cam.ScreenPointToRay(mouseScreen);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));

        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return Vector3.zero;
    }

    // Editor'de mouse'un world position'unu göster — debug için
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || cam == null) return;
        Vector3 mp = GetMouseWorldPosition();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(mp, 0.2f);
    }
}