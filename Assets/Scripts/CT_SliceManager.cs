using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CT_SliceManager : MonoBehaviour
{
    public string baseFolderPath = "Assets/CT_Slices/"; // Базовая папка
    public Renderer planeRenderer;  // Рендерер плоскости
    public Slider sliceSlider; // Ползунок для перелистывания срезов
    public Text axisText; // UI для отображения текущей оси

    public enum Axis { X, Y, Z }    // Выбор оси
    private Axis currentAxis = Axis.Z;

    private Dictionary<Axis, List<Texture2D>> sliceTextures = new Dictionary<Axis, List<Texture2D>>();
    private int currentSliceIndex = 0;
    private bool isAnimating = false; // Флаг для плавного перехода

    void Start()
    {
        LoadSlices(Axis.X, "X");
        LoadSlices(Axis.Y, "Y");
        LoadSlices(Axis.Z, "Z");

        UpdateTexture(); // Установим начальную текстуру
        UpdateUI(); // Обновляем UI
    }

    void LoadSlices(Axis axis, string folderName)
    {
        string folderPath = Path.Combine(baseFolderPath, folderName);
        string[] files = Directory.GetFiles(folderPath, "*.png");

        List<Texture2D> textures = new List<Texture2D>();
        foreach (string file in files)
        {
            Texture2D tex = LoadTexture(file);
            if (tex != null) textures.Add(tex);
        }

        sliceTextures[axis] = textures;

        if (axis == currentAxis && sliceSlider != null)
        {
            sliceSlider.maxValue = textures.Count - 1;
            sliceSlider.value = 0;
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
            return tex;
        return null;
    }

    public void ChangeSlice(float index)
    {
        int newIndex = Mathf.Clamp((int)index, 0, sliceTextures[currentAxis].Count - 1);
        if (newIndex != currentSliceIndex)
        {
            currentSliceIndex = newIndex;
            StartCoroutine(FadeTexture(sliceTextures[currentAxis][currentSliceIndex]));
        }
    }

    IEnumerator FadeTexture(Texture2D newTexture)
    {
        if (isAnimating) yield break;
        isAnimating = true;

        Material mat = planeRenderer.material;
        float duration = 0.3f;
        float elapsedTime = 0f;
        Color color = mat.color;

        // Исчезновение
        while (elapsedTime < duration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / duration);
            mat.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Смена текстуры
        mat.mainTexture = newTexture;

        // Появление
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            color.a = Mathf.Lerp(0, 1, elapsedTime / duration);
            mat.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isAnimating = false;
    }

    public void SetAxis(string axis)
    {
        if (axis == "X") currentAxis = Axis.X;
        else if (axis == "Y") currentAxis = Axis.Y;
        else if (axis == "Z") currentAxis = Axis.Z;

        currentSliceIndex = 0;
        sliceSlider.maxValue = sliceTextures[currentAxis].Count - 1;
        sliceSlider.value = 0;
        UpdateTexture();
        UpdateUI();
    }

    void UpdateTexture()
    {
        if (sliceTextures.ContainsKey(currentAxis) && sliceTextures[currentAxis].Count > 0)
        {
            planeRenderer.material.mainTexture = sliceTextures[currentAxis][currentSliceIndex];
        }
    }

    void UpdateUI()
    {
        if (axisText != null)
            axisText.text = "Ось: " + currentAxis.ToString();
    }
}

