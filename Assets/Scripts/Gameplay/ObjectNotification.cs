using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNotification : MonoBehaviour
{
    [SerializeField] private GameObject NotificationIcon = null;

    [SerializeField] private float MovmentDistance = 0.0f;
    [SerializeField] private float MovmentTime = 0.0f;
    [SerializeField] private float MovmentRotation = 0.0f;

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

    public void DisplayNotification(bool active)
    {
        NotificationIcon.SetActive(active);
    }
}
