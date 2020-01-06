using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationHandler : MonoBehaviour
{

    [SerializeField] Button notificationOpenButton;
    [SerializeField] TextMeshProUGUI notificationText;
    [SerializeField] TextMeshProUGUI notificationCountText;
    [SerializeField] Button nextButton;
    [SerializeField] Button previousButton;
    [SerializeField] GameObject notificationPanel;

    List<string> mStoredText;
    int mCurrentPage;

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

    public void ToggleNotification()
    {
        if(notificationPanel.activeSelf)
        {
            notificationPanel.SetActive(false);
            notificationOpenButton.gameObject.SetActive(false);
            mStoredText.Clear();
        }
        else
        {
            notificationPanel.SetActive(true);
            mCurrentPage = 0;
        }
    }

    public void AddNotification(string text)
    {
        mStoredText.Add(text);
    }

    public void AddNotification(string[] text)
    {
        foreach(string s in text)
        {
            mStoredText.Add(s);
        }
    }

    public void AddNotification(ref bool notAlreadySeen, string text)
    {
        if (!notAlreadySeen)
        {
            AddNotification(text);
            notAlreadySeen = true;
        }
    }

    public void AddNotification(ref bool notAlreadySeen, string[] text)
    {
        if (!notAlreadySeen)
        {
            AddNotification(text);
            notAlreadySeen = true;
        }
    }

    public void Back()
    {
        if (mCurrentPage > 0) mCurrentPage--;
    }

    public void Next()
    {
        if (mCurrentPage < mStoredText.Count - 1) mCurrentPage++;
    }

}
