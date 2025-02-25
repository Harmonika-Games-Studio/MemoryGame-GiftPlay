using Harmonika.Tools;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storage : MonoBehaviour
{
    [Header("Config")]
    [HideInInspector] public StorageItemConfig[] itemsConfig;

    [Header("References")]
    [SerializeField] private GameObject _storageItemPrefab;
    [SerializeField] private Transform _storageParent;

    private List<StorageItem> _itemsList = new List<StorageItem>();

    #region Propeties
    public List<StorageItem> ItemsList
    {
        get => _itemsList;
    }
    public int InventoryCount
    {
        get
        {
            int totalValue = 0;
            foreach (var item in _itemsList)
            {
                totalValue += item.Quantity;
            }
            return totalValue;
        }
    }
    #endregion

    public void Setup()
    {
        SetupStorage();
    }

    public string GetSpecificPrize(int i)
    {
        var item = ItemsList[i];
        item.Quantity -= 1;
        return item.ItemName;
    }

    public string GetRandomPrize(List<string> restrictions = null)
    {
        if (InventoryCount <= 0)
            return null;

        List<int> CandidateIndexes = new();

        int validInventoryCount = 0;
        for (int i = 0; i < ItemsList.Count; i++)
        {
            if (restrictions == null || !restrictions.Contains(ItemsList[i].ItemName))
            {
                CandidateIndexes.Add(i);
                validInventoryCount += ItemsList[i].Quantity;
            }
        }

        int randomNumber = Random.Range(0, validInventoryCount);

        int cumulativeValue = 0;
        for (int i = 0; i < CandidateIndexes.Count; i++)
        {
            cumulativeValue += ItemsList[CandidateIndexes[i]].Quantity;
            if (randomNumber < cumulativeValue)
            {
                return GetSpecificPrize(i);
            }
        }

        return null;
    }

    public string GetScorePrize(int score, List<string> restrictions = null)
    {
        List<int> candidateIndexes = new List<int>();
        int maxPrizeValue = int.MinValue;

        for (int i = 0; i < _itemsList.Count; i++)
        {
            if (_itemsList[i].Quantity > 0)
            {
                int prizeValue = _itemsList[i].PrizeScore;

                if (prizeValue <= score && (restrictions == null || !restrictions.Contains(ItemsList[i].ItemName)))
                {
                    if (prizeValue > maxPrizeValue)
                    {
                        maxPrizeValue = prizeValue;
                        candidateIndexes.Clear();
                        candidateIndexes.Add(i);
                    }
                    else if (prizeValue == maxPrizeValue)
                    {
                        candidateIndexes.Add(i);
                    }
                }
            }
        }

        if (candidateIndexes.Count == 0)
        {
            Debug.Log("TODO: Programar nova tela de vitória sem prêmio.");
            return null;
        }
        else if (candidateIndexes.Count == 1)
            return GetSpecificPrize(candidateIndexes[0]);
        else
        {
            List<string> restrictionNames = new List<string>();

            for (int i = 0; i < ItemsList.Count; i++)
            {
                if (!restrictions.Contains(ItemsList[i].ItemName))
                {
                    restrictionNames.Add(ItemsList[i].ItemName);
                }
            }

            return GetRandomPrize(restrictionNames);
        }
    }

    private void SetupStorage()
    {
        foreach (StorageItem element in _itemsList)
        {
            Destroy(element.gameObject);
        }

        _itemsList.Clear();

        float totalValue = 0;

        // Instancia e configura os itens de estoque
        foreach (StorageItemConfig itemConfig in itemsConfig)
        {
            GameObject newItem = Instantiate(_storageItemPrefab, _storageParent);
            StorageItem itemScript = newItem.GetComponent<StorageItem>();

            itemScript.Initialize(itemConfig._itemName, PlayerPrefs.GetInt(itemConfig._itemName, itemConfig._initialValue), itemConfig._prizeScore);

            _itemsList.Add(itemScript);

            if (itemScript.Quantity > 0)
            {
                totalValue += itemScript.Quantity;
            }
        }
    }
}
