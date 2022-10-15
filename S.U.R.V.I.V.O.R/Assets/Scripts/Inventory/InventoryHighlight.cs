using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHighlight : MonoBehaviour
{
    [SerializeField] private RectTransform highlighter;

    public void SetSize(InventoryItem targetItem)
    {
        var size = new Vector2(targetItem.Width * ItemGrid.TileSize,
            targetItem.Height * ItemGrid.TileSize);
        highlighter.sizeDelta = size;
    }

    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem)
    {
        SetPosition(targetGrid, targetItem, targetItem.onGridPositionX, targetItem.onGridPositionY);
    }

    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem, int posX, int posY)
    {
        var position = targetGrid.GetPositionOnGrid(targetItem, posX, posY);
        highlighter.localPosition = position;
    }

    public void Show(bool state)
    {
        highlighter.gameObject.SetActive(state);
    }

    public void SetParent(ItemGrid targetGrid)
    {
        if(targetGrid == null) return;
        highlighter.SetParent(targetGrid.GetComponent<RectTransform>());
    }
}