using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject Camera;

    public float RotateSpeed = 90f;
    public float WalkSpeed = 5f;

    private Rigidbody _playerBody;

    private float _strafe;
    private float _rotate;
    private float _walk;
    private float _neck;
    private int _touchMoveId = -1;
    private Vector2 _touchMoveStart;
    private int _touchLookId = -1;
    private Vector2 _touchLookStart;

    // Start is called before the first frame update
    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        _strafe = 0f;
        _rotate = 0f;
        _walk = 0f;
        _neck = 0f;

        // For PC handle keyboard and mouse look
#if !UNITY_ANDROID
        // Horizontal control strafes player
        _strafe += Input.GetAxis("Horizontal");

        // Vertical control walks player
        _walk += Input.GetAxis("Vertical");

        // Mouse Y raises or lowers neck
        _neck += -Input.GetAxis("Mouse Y") * 3;

        // Mouse X rotates player
        _rotate += Input.GetAxis("Mouse X") * 3;
#endif

        // For android, detect back button to quit
#if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
#endif

        // For touch devices handle two-touch move/look
        for (var i = 0; i < Input.touchCount; ++i)
        {
            var touch = Input.GetTouch(i);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (touch.position.x * 2 < Screen.width)
                    {
                        _touchMoveId = touch.fingerId;
                        _touchMoveStart = touch.position;
                    }
                    else
                    {
                        _touchLookId = touch.fingerId;
                        _touchLookStart = touch.position;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (touch.fingerId == _touchMoveId)
                    {
                        _strafe += (touch.position.x - _touchMoveStart.x) / Screen.width * 3;
                        _walk += (touch.position.y - _touchMoveStart.y) / Screen.height * 3;
                    }
                    else if (touch.fingerId == _touchLookId)
                    {
                        _rotate += (touch.position.x - _touchLookStart.x) / Screen.width * 6;
                        _neck += -(touch.position.y - _touchLookStart.y) / Screen.height * 6;
                    }
                    break;

                case TouchPhase.Ended:
                    if (touch.fingerId == _touchMoveId)
                        _touchMoveId = -1;

                    if (touch.fingerId == _touchLookId)
                        _touchLookId = -1;
                    break;
            }
        }

        _strafe = Mathf.Clamp(_strafe, -1f, 1f);
        _walk = Mathf.Clamp(_walk, -1f, 1f);
        _rotate = Mathf.Clamp(_rotate, -1f, 1f);
        _neck = Mathf.Clamp(_neck, -1f, 1f);
    }

    private void FixedUpdate()
    {
        // Rotate player using hori
        transform.Rotate(
            0,
            RotateSpeed * _rotate * Time.deltaTime, 
            0);

        _playerBody.MovePosition(
            transform.position + transform.TransformVector(
                WalkSpeed * _strafe * Time.deltaTime,
                0,
                WalkSpeed * _walk * Time.deltaTime));

        Camera.transform.Rotate(_neck, 0, 0);
    }
}
