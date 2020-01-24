using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNotification : MonoBehaviour
{
    // Object that is sued to define when a notification is ready
    [SerializeField] private GameObject NotificationIcon = null;

    // How far the item moves up and down
    [SerializeField] private float MovmentDistance = 0.0f;
    // How fast the item moves up and down
    [SerializeField] private float MovmentTime = 0.0f;
    // How fast the item rotates
    [SerializeField] private float MovmentRotation = 0.0f;

    // Current timer for the notification moving
    private float m_notificationDelta = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_notificationDelta = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        m_notificationDelta += MovmentTime * Time.deltaTime;

        NotificationIcon.transform.localPosition = new Vector3(0.0f, Mathf.Cos(m_notificationDelta) * MovmentDistance, 0.0f);
        NotificationIcon.transform.Rotate(Vector3.up, MovmentRotation * Time.deltaTime);
    }

    // Make the notification icon active
    public void DisplayNotification(bool active)
    {
        NotificationIcon.SetActive(active);
    }
}
