using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ItemGrid : MonoBehaviour
{
    private Canvas canvas;
    
    public const float TileSize = 50;

    private Vector2 positionOnGrid;
    private Vector2Int tileGridPosition;

    private Item[,] inventoryItemSlot;

    [HideInInspector] public List<Item> items = new();

    private RectTransform rectTransform;

    [SerializeField] private int gridSizeWidth = 10;
    [SerializeField] private int gridSizeHeight = 10;

    public int GridSizeWidth => gridSizeWidth;
    public int GridSizeHeight => gridSizeHeight;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        Init(gridSizeWidth, gridSizeHeight);
    }

    private void Init(int width, int height)
    {
        inventoryItemSlot = new Item[width, height];
        var size = new Vector2(width * TileSize, height * TileSize);
        rectTransform.sizeDelta = size;
        /*var slot = transform.Find("Slot");
        slot.gameObject.SetActive(false);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var singleSlot = Instantiate(slot, transform);
                singleSlot.gameObject.SetActive(true);
            }
        }
        GetComponent<GridLayoutGroup>().cellSize = new Vector2(TileSize, TileSize);
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height) * TileSize;
        GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;*/
    }

    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        var position = rectTransform.position;
        positionOnGrid.x = mousePosition.x - position.x;
        positionOnGrid.y = position.y - mousePosition.y;

        var scaleFactor = canvas.scaleFactor;
        tileGridPosition.x = (int) ((positionOnGrid.x / TileSize) / scaleFactor);
        tileGridPosition.y = (int) ((positionOnGrid.y / TileSize) / scaleFactor);

        return tileGridPosition;
    }

    public bool PlaceItem(Item item, int posX, int posY, ref Item overlapItem)
    {
        if (!BoundryCheck(posX, posY, item.Width, item.Height))
            return false;

        if (!OverlapCheck(posX, posY, item.Width, item.Height, ref overlapItem))
        {
            overlapItem = null;
            return false;
        }

        if (overlapItem != null)
        {
            RemoveGridReference(overlapItem);
        }

        PlaceItem(item, posX, posY);
        return true;
    }

    public void PlaceItem(Item item, int posX, int posY)
    {
        var itemRectTransform = item.GetComponent<RectTransform>();
        itemRectTransform.SetParent(rectTransform);
        for (int x = 0; x < item.Width; x++)
        {
            for (int y = 0; y < item.Height; y++)
            {
                inventoryItemSlot[posX + x, posY + y] = item;
            }
        }

        item.onGridPositionX = posX;
        item.onGridPositionY = posY;

        var position = GetPositionOnGrid(item, posX, posY);
        itemRectTransform.localPosition = position;
        
        items.Add(item);
    }

    public Vector2 GetPositionOnGrid(Item item, int posX, int posY)
    {
        var position = new Vector2();
        position.x = posX * TileSize + TileSize * item.Width / 2;
        position.y = -(posY * TileSize + TileSize * item.Height / 2);
        return position;
    }

    private bool OverlapCheck(int posX, int posY, int width, int height, ref Item overlapItem)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (inventoryItemSlot[posX + x, posY + y] != null)
                {
                    if (overlapItem == null)
                        overlapItem = inventoryItemSlot[posX + x, posY + y];
                    else
                    {
                        if (overlapItem != inventoryItemSlot[posX + x, posY + y])
                            return false;
                    }
                }
            }
        }

        return true;
    }

    public Item PickUpItem(int x, int y)
    {
        var returnedItem = inventoryItemSlot[x, y];
        if (returnedItem == null) return null;
        RemoveGridReference(returnedItem);

        items.Remove(returnedItem);

        inventoryItemSlot[x, y] = null;
        return returnedItem;
    }

    private void RemoveGridReference(Item item)
    {
        for (int ix = 0; ix < item.Width; ix++)
        {
            for (int iy = 0; iy < item.Height; iy++)
            {
                inventoryItemSlot[item.onGridPositionX + ix, item.onGridPositionY + iy] = null;
            }
        }
    }

    bool PositionCheck(int posX, int posY)
    {
        return posX >= 0 && posY >= 0 && posX < gridSizeWidth && posY < gridSizeHeight;
    }

    public bool BoundryCheck(int posX, int posY, int width, int height)
    {
        return PositionCheck(posX, posY) && PositionCheck(posX + width - 1, posY + height - 1);
    }

    public Item GetItem(int x, int y)
    {
        return inventoryItemSlot[x, y];
    }

    public Vector2Int? FindSpaceForObject(Item itemToInsert)
    {
        var height = gridSizeHeight - itemToInsert.Height + 1;
        var width = gridSizeWidth - itemToInsert.Width + 1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (CheckAvailableSpace(x, y, itemToInsert.Width, itemToInsert.Height))
                    return new Vector2Int(x, y);
            }
        }

        return null;
    }

    private bool CheckAvailableSpace(int posX, int posY, int width, int height)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (inventoryItemSlot[posX + x, posY + y] != null)
                 return false;

        return true;
    }
}