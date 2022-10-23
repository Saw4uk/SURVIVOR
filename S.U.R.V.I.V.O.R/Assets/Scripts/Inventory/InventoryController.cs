using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class InventoryController : MonoBehaviour
{
    [HideInInspector] private ItemGrid selectedItemGrid;
    public ItemGrid SelectedItemGrid
    {
        get => selectedItemGrid;
        set
        {
            selectedItemGrid = value;
            inventoryHighlight.SetParent(value);
        }
    }

    private Item selectedItem;
    private Item overlapItem;
    private RectTransform rectTransform;

    [SerializeField] private List<ItemData> items;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform canvasTransform;

    [SerializeField] private ItemListManager listManager;

    private InventoryHighlight inventoryHighlight;
    private Item itemToHighlight;
    private Vector2Int? previousPosition;

    private void Awake()
    {
        inventoryHighlight = GetComponent<InventoryHighlight>();
    }

    private void Update()
    {
        if (selectedItem != null)
            rectTransform.position = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CreateRandomItem();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            InsertRandomItem(); 
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }

        if (selectedItemGrid == null)
        {
            inventoryHighlight.Show(false);
            return;
        }

        HandleHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            var tileGridPosition = GetTileGridPosition();

            if (selectedItem == null)
            {
                PickUpItem(tileGridPosition);
            }
            else
            {
                PlaceItem(tileGridPosition);
            }
        }
    }

    private void PickUpItem(Vector2Int tileGridPosition)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        if (selectedItem != null)
            rectTransform = selectedItem.GetComponent<RectTransform>();
    }

    private void PlaceItem(Vector2Int tileGridPosition)
    {
        var complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y,
            ref overlapItem);
        if (complete)
        {
            selectedItem = null;
            if (overlapItem != null)
            {
                selectedItem = overlapItem;
                overlapItem = null;
                rectTransform = selectedItem.GetComponent<RectTransform>();
                rectTransform.SetAsLastSibling();
            }
        }
    }

    private void RotateItem()
    {
        if(selectedItem == null) return;
        selectedItem.Rotated();
    }

    private void HandleHighlight()
    {
        var positionOnGrid = GetTileGridPosition();
        if(previousPosition == positionOnGrid) return;
        if (selectedItem == null)
        {
            itemToHighlight = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);
            if (itemToHighlight != null)
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(itemToHighlight);
                inventoryHighlight.SetPosition(selectedItemGrid, itemToHighlight);
            }
            else
            {
                inventoryHighlight.Show(false);
            }
        }
        else
        {
            inventoryHighlight.Show(selectedItemGrid.BoundryCheck(positionOnGrid.x, positionOnGrid.y,
                selectedItem.Width, selectedItem.Height));
            inventoryHighlight.SetSize(selectedItem);
            inventoryHighlight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
        }
    }

    private Vector2Int GetTileGridPosition()
    {
        var mousePosition = Input.mousePosition;
        if (selectedItem != null)
        {
            mousePosition.x -= (selectedItem.Width - 1) * ItemGrid.TileSize / 2;
            mousePosition.y += (selectedItem.Height - 1) * ItemGrid.TileSize / 2;
        }

        return selectedItemGrid.GetTileGridPosition(mousePosition);
    }
    
    private void CreateRandomItem()
    {
        if(selectedItem != null)
            return;
        var inventoryItem = Instantiate(itemPrefab).GetComponent<Item>();
        inventoryItem.transform.SetParent(canvasTransform);
        var selectedItemID = Random.Range(0, items.Count);
        inventoryItem.Set(items[selectedItemID]);
        // listManager.currentItemList.Add(inventoryItem);
        // listManager.PopulateList(listManager.currentItemList);
        
    }
    
    private void InsertRandomItem()
    {
        if(selectedItem != null)
            return;
        CreateRandomItem();
        var itemToInsert = selectedItem;
        selectedItem = null;
        listManager.currentItemList.Add(itemToInsert);
        listManager.PopulateList(listManager.currentItemList);
        
    }

    private void InsertItem(Item itemToInsert)
    {
        var positionOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);
        if(positionOnGrid == null)
        {
            Destroy(itemToInsert.gameObject);
            return;
        }
        selectedItemGrid.PlaceItem(itemToInsert, positionOnGrid.Value.x, positionOnGrid.Value.y);
    }

    public Item GetSelectedItem()
    {
        return selectedItem;
    }

    public void ResetSelectedItem()
    {
        Destroy(selectedItem);
    }
}