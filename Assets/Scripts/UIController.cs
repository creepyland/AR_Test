using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button addButton;
    [SerializeField] private Button templateButton;
    [SerializeField] private RectTransform addMenu;

    public static UIController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetupButtons(string[] modelUrls, ArController obj)
    {
        char[] separators = new char[] { '\\', '/' };
        foreach (var url in modelUrls)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Button b = Instantiate(templateButton);
                TextMeshProUGUI buttonText = b.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                {
                    string[] broken = url.Split(separators);
                    if (broken.Length > 0)
                    {
                        buttonText.text = broken[broken.Length - 1];
                    }
                }

                b.transform.parent = templateButton.transform.parent;
                b.transform.localScale = Vector3.one;
                b.onClick.AddListener(delegate () { obj.LoadAndSpawnGLTFModel(url); });
                b.gameObject.SetActive(true);
            }
        }
    }

    public void ToggleMenuVisibility()
    {
        addMenu.gameObject.SetActive(!addMenu.gameObject.activeSelf);
    }
}
