using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNotification : MonoBehaviour
{
    [SerializeField] private GameObject NotificationIcon;

    [SerializeField] private float MovmentDistance;
    [SerializeField] private float MovmentTime;
    [SerializeField] private float MovmentRotation;

    private float m_notificationDelta;

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
