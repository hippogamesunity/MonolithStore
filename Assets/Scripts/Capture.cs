using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UI;

public class Capture : MonoBehaviour
{
    public GameObject[] Pages;
    public Image Screenshot;
    public Text Collection;
    public Text Title;
    public Text Description;
    public Text MaterialTitle;
    public Text MaterialDescription;
    public Text TrayTitle;
    public Text TrayDescription;
    public Image[] PropertyIcons;
    public Text[] Properties;
    public string ItemFilter;
    public Item[] Items;
    public List<ItemMeta> ItemMetas;
    
    public IEnumerator Start()
    {
        foreach (var item in Items)
        {
            if (ItemFilter != "" && !item.Id.Contains(ItemFilter)) continue;

            CreateMeta(item);

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

            Collection.text = item.Collection.ToUpper();
            Title.text = item.Title;
            Description.text = item.Description;
            Description.fontSize = item.DescriptionTextSize;
            
            for (var j = 0; j < item.Properties.Length; j++)
            {
                PropertyIcons[j].gameObject.SetActive(true);
                PropertyIcons[j].sprite = item.Properties[j].Icon;
                Properties[j].text = item.Properties[j].Text;
            }

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 1);

            // 2

            if (item.Screenshots.Length == 1) continue;

            Pages[0].SetActive(false);
            Pages[1].SetActive(true);
            SetScreenshot(item, 1);
            MaterialTitle.text = item.MaterialTitle;
            MaterialDescription.text = item.MaterialDescription;
            MaterialDescription.fontSize = item.MaterialDescriptionTextSize;

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 2);

            // 3

            if (item.Screenshots.Length == 2) continue;

            Pages[1].SetActive(false);
            Pages[2].SetActive(true);
            SetScreenshot(item, 2);
            TrayTitle.text = item.TrayTitle;
            TrayDescription.text = item.TrayDescription;

            yield return new WaitForEndOfFrame();
            CreateScreenshot(item.Id, 3);

            // 4

            if (item.Screenshots.Length == 3) continue;

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
        Screenshot.transform.localPosition = item.Screenshots[index].Offset;
        Screenshot.transform.localScale = item.Screenshots[index].Scale * Vector3.one;
        Screenshot.transform.localRotation = Quaternion.Euler(0, 0, item.Screenshots[index].Rotation);
    }

    private void CreateMeta(Item item)
    {
        if (item.MetaTemplateId == "") return;

        var path = $"Output/{item.Id}/Meta.txt";

        Directory.CreateDirectory("Output");
        Directory.CreateDirectory($"Output/{item.Id}");

        var meta = ItemMetas.Single(i => i.Id == item.MetaTemplateId);

        File.WriteAllText(path, meta.Title.Replace("%TITLE%", item.Title) + "\r\n\r\n" + meta.Description.Replace("%TITLE%", item.Title).Replace("%COLLECTION%", item.Collection.ToUpper()).Replace(" SERIES", ""));
    }

    private void CreateScreenshot(string folder, int index)
    {
        var path = $"Output/{folder}/{index}.png";

        Directory.CreateDirectory("Output");
        Directory.CreateDirectory($"Output/{folder}");

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
    public string Collection;
    public string Description;
    public int DescriptionTextSize = 120;
    public string MaterialTitle = "Композитный камень";
    public string MaterialDescription = "Гипс, белый цемент, мраморная мука, минеральные наполнители, армирующая фибра";
    public int MaterialDescriptionTextSize = 80;
    public string TrayTitle = "Технический горшок и поддон";
    public string TrayDescription = "В комплекте. Кашпо не намокнет, а растение не засохнет.";
    public string MetaTemplateId;
    public ItemProperty[] Properties;
    public ItemScreenshot[] Screenshots;
}

[Serializable]
public class ItemScreenshot
{
    public Sprite Sprite;
    public Vector2 Offset = Vector2.zero;
    public float Scale = 1;
    public float Rotation;
}

[Serializable]
public class ItemProperty
{
    [TextArea(1, 2)]
    public string Text;
    public Sprite Icon;
}

[Serializable]
public class ItemMeta
{
    public string Id;
    public string Title;
    [TextArea(1, 20)]
    public string Description;
}