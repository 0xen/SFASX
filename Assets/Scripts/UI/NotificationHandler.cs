using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationHandler : MonoBehaviour
{
    // Button used to open the notification panel
    [SerializeField] Button notificationOpenButton = null;
    // Text that makes up the notification box
    [SerializeField] TextMeshProUGUI notificationText = null;
    // How many notification messages are left
    [SerializeField] TextMeshProUGUI notificationCountText = null;
    // Next notification button
    [SerializeField] Button nextButton = null;
    // Previous notification button
    [SerializeField] Button previousButton = null;
    // The notification panel thats visibility is toggled
    [SerializeField] GameObject notificationPanel = null;

    // Stored notifications
    private List<string> mStoredText = null;
    // What page we are currently on
    private int mCurrentPage = 0;

    // Start is called before the first frame update
    void Start()
    {
        mCurrentPage = 0;

        mStoredText = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        int textCount = mStoredText.Count;
        if (textCount > 0)
        {
            notificationOpenButton.gameObject.SetActive(true);
        }
        else
        {
            if (notificationPanel.activeSelf)
            {
                ToggleNotification();
            }
        }
        if (notificationPanel.activeSelf)
        {
            previousButton.gameObject.SetActive(mCurrentPage > 0);
            nextButton.gameObject.SetActive(mCurrentPage < textCount - 1);

            notificationText.text = mStoredText[mCurrentPage];

            notificationCountText.text = (mCurrentPage+1) + " / " + textCount;
        }
    }

    // Toggle the notification panel open and closed
    public void ToggleNotification()
    {
        if(notificationPanel.activeSelf)
        {
            notificationPanel.SetActive(false);
            notificationOpenButton.gameObject.SetActive(false);
            // When we close the panel, clear all notifications
            mStoredText.Clear();
        }
        else
        {
            notificationPanel.SetActive(true);
            mCurrentPage = 0;
        }
    }

    // Add a new notification to the notification panel
    public void AddNotification(string text)
    {
        mStoredText.Add(text);
    }

    // Add several notification pages
    public void AddNotification(string[] text)
    {
        foreach(string s in text)
        {
            mStoredText.Add(s);
        }
    }

    // If the notification has never been displayed before, display it
    public void AddNotification(ref bool notAlreadySeen, string text)
    {
        if (!notAlreadySeen)
        {
            AddNotification(text);
            notAlreadySeen = true;
        }
    }

    // If the notification has never been displayed before, display it
    public void AddNotification(ref bool notAlreadySeen, string[] text)
    {
        if (!notAlreadySeen)
        {
            AddNotification(text);
            notAlreadySeen = true;
        }
    }

    // Go back one page
    public void Back()
    {
        if (mCurrentPage > 0) mCurrentPage--;
    }

    // Go forward one page
    public void Next()
    {
        if (mCurrentPage < mStoredText.Count - 1) mCurrentPage++;
    }

}
