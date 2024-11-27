using UnityEngine;

[CreateAssetMenu(fileName = "ElementConfig", menuName = "ScriptableObjects/ElementConfig")]
public class ElementConfig : ScriptableObject
{
    // Danh sách các nguyên tố cơ bản
    public Element[] elements;

    // Danh sách các sự kết hợp của nguyên tố
    public ElementCombination[] combinations;

    // Các phương thức để lấy nguyên tố theo tên
    public Element GetElementByName(string name)
    {
        foreach (var element in elements)
        {
            if (element.elementName == name)
            {
                return element;
            }
        }
        return null; // Không tìm thấy
    }

    // Các phương thức để lấy sự kết hợp theo tên
    public ElementCombination GetCombinationByName(string name)
    {
        foreach (var combination in combinations)
        {
            if (combination.combinationName == name)
            {
                return combination;
            }
        }
        return null; // Không tìm thấy
    }
}

[System.Serializable]
public class Element
{
    public string elementName;
    public Color elementColor; // Màu sắc của nguyên tố
    public string description; // Mô tả về nguyên tố
}

[System.Serializable]
public class ElementCombination
{
    public string combinationName;
    public string firstElement;
    public string secondElement;
    public string description; // Mô tả về sự kết hợp
}

[System.Serializable]
public class ElementRelation
{
    public string elementName;
    public string immuneTo;
    public string resistantTo;
    public string vulnerableTo;
}