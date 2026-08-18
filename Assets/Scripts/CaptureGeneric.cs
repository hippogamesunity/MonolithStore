using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CaptureGeneric : MonoBehaviour
{
    public GameObject[] Pages;
    public Image Screenshot;
    public Text Title;
    public Text Description;
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

            Title.text = item.Title;
            Description.text = item.Description;
            Description.fontSize = item.DescriptionTextSize;

            for (var i = 0; i < Pages.Length; i++)
            {
                Pages[i].SetActive(true);
                SetScreenshot(item, i);

                yield return new WaitForEndOfFrame();
                CreateScreenshot(item.Id, i + 1);

                Pages[i].SetActive(false);
            }
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