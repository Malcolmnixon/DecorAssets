using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject Camera;

    public float RotateSpeed = 90f;
    public float WalkSpeed = 5f;

	[Header("Mouse")]
	public float viewAngleUpper = 50.0f;
	public float viewAngleLower = 80.0f;
	public float lookSensitivity = 3.0f;

    private Rigidbody _playerBody;
	private bool m_cursorIsLocked = true;

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
        _neck += -Input.GetAxis("Mouse Y") * lookSensitivity;

        // Mouse X rotates player
        _rotate += Input.GetAxis("Mouse X") * lookSensitivity;

		InternalLockUpdate();
		if (!m_cursorIsLocked) {
			return;
		}
#endif

        // For android, detect back button to quit
#if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

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
#endif
    }

    private void FixedUpdate()
    {
        // Rotate player Left/Right
        var rotation = new Vector3(0, RotateSpeed * _rotate * Time.deltaTime, 0) * lookSensitivity;
        if (rotation != Vector3.zero) {
            _playerBody.MoveRotation(_playerBody.rotation * Quaternion.Euler(rotation));
        }

        _playerBody.MovePosition(
            transform.position + transform.TransformVector(
                WalkSpeed * _strafe * Time.deltaTime,
                0,
                WalkSpeed * _walk * Time.deltaTime));

        // Rotate Neck Up/Down
        Camera.transform.Rotate(_neck, 0, 0);

        /* Clamp Neck */
        float cameraXRot = Camera.transform.localEulerAngles.x;
        float clampedXRot;
        if (cameraXRot > 180) {
            clampedXRot = Mathf.Clamp(cameraXRot, 360-viewAngleUpper, 360);
        } else {
            clampedXRot = Mathf.Clamp(cameraXRot, 0, viewAngleLower);
        }
        Camera.transform.localEulerAngles = new Vector3(clampedXRot, 0, 0);
    }

	//controls the locking and unlocking of the mouse
	private void InternalLockUpdate()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			m_cursorIsLocked = false;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			m_cursorIsLocked = true;
		}

		if (m_cursorIsLocked)
		{
			UnlockCursor();
		}
		else if (!m_cursorIsLocked)
		{
			LockCursor();
		}
	}

	private void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
