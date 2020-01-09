using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSFX : MonoBehaviour
{
    [SerializeField] private string sfxName = "";
    private Button button = null;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        if (button == null) Destroy(this);

        button.onClick.AddListener(() => PlaySFX(sfxName));
    }

    private void PlaySFX(string name)
    {
        SFXController.instance.PlaySFX(name);
    }
}
