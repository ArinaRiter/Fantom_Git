using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CT_SliceManager : MonoBehaviour
{
    public string baseFolderPath = "Assets/CT_Slices/"; // Базовая папка с КТ-срезами
    public Renderer planeRenderer; // Рендер плоскости
    public Transform slicePlane; // Объект плоскости среза
    public Transform modelTarget; // Родитель (Model Target)
    public Slider sliceSlider; // Ползунок для переключения срезов
    public Text axisText; // UI-текст для отображения текущей оси

    public enum Axis { X, Y, Z }
    private Axis currentAxis = Axis.Z;

    private Dictionary<Axis, List<Texture2D>> sliceTextures = new Dictionary<Axis, List<Texture2D>>();
    [SerializeField] private int currentSliceIndex = 0;
    private bool isAnimating = false;
    private Vector3 initialLocalPosition;

    void Start()
    {
        LoadSlices(Axis.X, "X_Axis");
        LoadSlices(Axis.Y, "Y_Axis");
        LoadSlices(Axis.Z, "Z_Axis");

        initialLocalPosition = slicePlane.localPosition; // Запоминаем начальное положение
        UpdateTexture();
        UpdateUI();
    }

    private void Update()
    {
        UpdateTexture();
        UpdatePlanePosition();
    }

    void LoadSlices(Axis axis, string folderName)
    {
        string folderPath = Path.Combine(baseFolderPath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"Папка {folderPath} не найдена!");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "image_*.png");

        // Сортировка по числовому значению
        System.Array.Sort(files, (a, b) =>
        {
            int numA = int.Parse(Path.GetFileNameWithoutExtension(a).Split('_')[1]);
            int numB = int.Parse(Path.GetFileNameWithoutExtension(b).Split('_')[1]);
            return numA.CompareTo(numB);
        });

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
            UpdatePlanePosition();
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

        while (elapsedTime < duration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / duration);
            mat.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mat.mainTexture = newTexture;

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

    void UpdatePlanePosition()
    {
        float step = 0.002f; // Расстояние между срезами

        Vector3 offset = Vector3.zero;
        if (currentAxis == Axis.X) offset = new Vector3(step * currentSliceIndex, 0, 0);
        else if (currentAxis == Axis.Y) offset = new Vector3(0, step * currentSliceIndex, 0);
        else if (currentAxis == Axis.Z) offset = new Vector3(0, 0, step * currentSliceIndex);

        slicePlane.localPosition = initialLocalPosition + offset;
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
        UpdatePlanePosition();
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
