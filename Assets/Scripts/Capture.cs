using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Capture : MonoBehaviour
{
    public GameObject[] Pages;
    public Image Screenshot;
    public Text Collection;
    public Text Title;
    public Text Description;
    public Text TrayInfo;
    public Image[] PropertyIcons;
    public Text[] Properties;
    public Item[] Items;
    public string ItemFilter;

    public IEnumerator Start()
    {
        foreach (var item in Items)
        {
            if (ItemFilter != "" && !item.Id.Contains(ItemFilter)) continue;

            foreach (var page in Pages)
            {
                page.SetActive(false);
            }

            // 1

            Pages[0].SetActive(true);
            SetScreenshot(item, 0);

            foreach (var icon in PropertyIcons)
            {
                icon.gameObject.SetActive(false);
            }

            Title.text = item.Title;
            Description.text = item.Description;
            Collection.text = item.Collection;

            for (var j = 0; j < item.Properties.Length; j++)
            {
                PropertyIcons[j].gameObject.SetActive(true);
                PropertyIcons[j].sprite = item.Properties[j].Icon;
                Properties[j].text = item.Properties[j].Text;
            }

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 1);

            // 2

            Pages[0].SetActive(false);
            Pages[1].SetActive(true);
            SetScreenshot(item, 1);

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 2);

            // 3

            Pages[1].SetActive(false);
            Pages[2].SetActive(true);
            SetScreenshot(item, 2);
            TrayInfo.text = item.TrayInfo;

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 3);

            // 4

            Pages[2].SetActive(false);
            Pages[3].SetActive(true);
            SetScreenshot(item, 3);

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 4);

            Screenshot.material.SetFloat("_Sensitivity", 0.03f);
        }
    }

    private void SetScreenshot(Item item, int index)
    {
        Screenshot.sprite = item.Screenshots[index].Sprite;
        Screenshot.material.SetFloat("_Sensitivity", item.Screenshots[index].YellowSensitivity);
        Screenshot.transform.localPosition = item.Screenshots[index].Offset;
        Screenshot.transform.localScale = item.Screenshots[index].Scale * Vector3.one;
        Screenshot.transform.localRotation = Quaternion.Euler(0, 0, item.Screenshots[index].Rotation);
    }

    private void CreateScreenshot(string folder, int index)
    {
        var path = $"Screenshots/{folder}/{index}.png";

        Directory.CreateDirectory("Screenshots");
        Directory.CreateDirectory($"Screenshots/{folder}");

        var screenTexture = ScreenCapture.CaptureScreenshotAsTexture();
        var jpegBytes = screenTexture.EncodeToJPG(80);
        
        File.WriteAllBytes(path, jpegBytes);
        Debug.Log($"Screenshot saved: {path}");
    }
}

[Serializable]
public class Item
{
    public string Id;
    public string Title;
    public string Description;
    public string Collection;
    public string TrayInfo;
    public ItemProperty[] Properties;
    public ItemScreenshot[] Screenshots;
}

[Serializable]
public class ItemScreenshot
{
    public Sprite Sprite;
    public float YellowSensitivity = 0.3f;
    public Vector2 Offset = Vector2.zero;
    public float Scale = 1;
    public float Rotation;
}

[Serializable]
public class ItemProperty
{
    public string Text;
    public Sprite Icon;
}