using UnityEngine;

public class ToolIconProvider : MonoBehaviour
{
    public static ToolIconProvider Instance { get; private set; }

    [Tooltip("Assign sprites for each ToolType by index. Index 0 => ToolType.Saw, 1 => Axe, 2 => Hammer")]
    public Sprite[] icons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        EnsureSize();
    }

    private void EnsureSize()
    {
        int n = System.Enum.GetNames(typeof(ToolType)).Length;
        if (icons == null || icons.Length != n)
        {
            var newArr = new Sprite[n];
            if (icons != null)
            {
                for (int i = 0; i < Mathf.Min(icons.Length, n); i++) newArr[i] = icons[i];
            }
            icons = newArr;
        }
    }

    public Sprite GetIcon(ToolType t)
    {
        EnsureSize();
        var idx = (int)t;
        if (idx < 0 || idx >= icons.Length) return null;
        return icons[idx];
    }
}