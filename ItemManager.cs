using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Layouts
{
	public class ItemManager : MonoBehaviour
	{
		[SerializeField] private Transform itemPoolParent;
		[FormerlySerializedAs("layoutData")] [SerializeField] private LayoutDataObject layoutDataObject;
		private System.Random _random = new();
		private LevelItem[] prefabResources;
		private Dictionary<int, LevelItem> prefabs = new();
		private Dictionary<int, LevelItem> itemPool = new();
		private Dictionary<int, LevelItem> wallItemPool = new();
		private Dictionary<int, LevelItem> ceilingItemPool = new();
		private Dictionary<int, LevelItem> floorItemPool = new();

		private void Awake()
		{
			itemPoolParent.gameObject.SetActive(false);

			foreach (var prefab in layoutDataObject.ItemPrefabs)
			{
				prefabs.Add(prefab.Id, prefab);
			}
		}

		//In the map generator, make sure the ids are compatible with this layout shape
		public void FillItems(LevelLayout layout)
		{
			if (layout.ItemList == null) return;

			layout.GetAnchors(out var smallAnchors, out var mediumAnchors, out var largeAnchors);
			
			foreach(var item in layout.ItemList.ToList().Select(itemReference =>GetItem(itemReference, layout.ItemList)).Where(item => item && item.Size == LayoutItemSize.Large))
			{
				if (largeAnchors.Count <= 0 || !TryAddItem(item, new() { largeAnchors }))
				{
					break;
				}
			}
			
			foreach(var item in layout.ItemList.ToList().Select(itemReference =>GetItem(itemReference, layout.ItemList)).Where(item => item && item.Size == LayoutItemSize.Medium))
			{
				if (largeAnchors.Count <= 0 || !TryAddItem(item, new() { largeAnchors, mediumAnchors }))
				{
					break;
				}
			}
			
			foreach(var item in layout.ItemList.ToList().Select(itemReference =>GetItem(itemReference, layout.ItemList)).Where(item => item && item.Size == LayoutItemSize.Small))
			{
				if (largeAnchors.Count <= 0 || !TryAddItem(item, new() { largeAnchors, mediumAnchors, smallAnchors }))
				{
					break;
				}
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private LevelItem GetItem(LayoutItem itemReference, List<LayoutItem> itemList)
		{
			itemList.Remove(itemReference);
			
			if (itemPool.TryGetValue(itemReference.id, out var item))
			{
				//Makes sure the item is free for use
				if(item.transform.parent == itemPoolParent)
				{
					return item;
				}
				else
				{
					Debug.Log($"Item with id: {itemReference.id} being used.");
					return null;
				}
			}

			if(prefabs.TryGetValue(itemReference.id, out var prefab))
			{
				var instance = Instantiate(prefab, itemPoolParent);
				instance.transform.parent = itemPoolParent;
				instance.gameObject.SetActive(false);
				return instance;
			}

			Debug.Log($"Item with id: {itemReference.id} not found.");
			return null;
		}

		private bool TryAddItem(LevelItem item, List<List<Transform>> anchorLists)
		{
			List<Transform> anchors = new();

			foreach (var anchorList in anchorLists)
			{
				anchors.AddRange(anchorList);
			}

			Transform selectedAnchor;
			
			do
			{
				var anchor = anchors[_random.Next(0, anchors.Count)];
				anchors.Remove(anchor);
				selectedAnchor = anchor.childCount > 0 ? anchor : null;
;
			} while (selectedAnchor == null && anchors.Count > 0);
			
			if(!selectedAnchor) return false;

			item.IsUsed = true;
			item.transform.parent = selectedAnchor;
			item.transform.SetLocalPositionAndRotation(item.Position, Quaternion.Euler(item.Rotation));
			item.transform.localScale = item.Scale;
			item.gameObject.SetActive(true);

			return true;
		}

		public void RemoveFrom(LevelLayout layout)
		{
			layout.GetAnchors(out List<Transform> wallAnchors, out List<Transform> ceilingAnchors, out List<Transform> floorAnchors);

			foreach (var layoutAnchor in wallAnchors)
			{
				if(layoutAnchor.childCount >  0) RemoveItemFrom(layoutAnchor);
			}

			foreach (var layoutAnchor in ceilingAnchors)
			{
				if (layoutAnchor.childCount > 0) RemoveItemFrom(layoutAnchor);
			}

			foreach (var layoutAnchor in floorAnchors)
			{
				if (layoutAnchor.childCount > 0) RemoveItemFrom(layoutAnchor);
			}
		}

		private void RemoveItemFrom(Transform layoutAnchor) 
		{
			foreach(Transform child in layoutAnchor)
			{
				if(child.TryGetComponent(out LevelItem item))
				{
					item.gameObject.SetActive(false);
					item.transform.parent = itemPoolParent;
					item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
					item.IsUsed = false;
				}
			}
		}
	}
}