using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Layouts
{
    public class LayoutManager : MonoBehaviour
    {
        [SerializeField] private TextAsset mapJson;
        [FormerlySerializedAs("layoutData")] [SerializeField] private LayoutDataObject layoutDataObject;
        [SerializeField] private ItemManager itemManager;

        private int currentIndex;
        private LayoutMap savedMap;
        private Dictionary<LayoutType, LevelLayout> prefabs = new();
        private List<LayoutData> loadedMap = new();
        private LevelLayout mainLevel;
        private HashSet<LevelLayout> layoutPool = new();
        private Queue<LevelLayout> deactivateQueue = new();

        public static EventHandler<string> LayoutStyleChanged;

        public static LayoutManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            //if (TryInstantiateLayout(LayoutType.MainLevelStyle0, out var main)) mainLevel = main;

            foreach (var layoutData in savedMap.Layouts.Where(x => x.enable))
            {
                if(mainLevel && layoutData.type == LayoutType.MainLevelStyle0) continue;

                if(TryInstantiateLayout(layoutData.type, out var layout)) loadedMap.Add(layoutData);
            }
        }

        private void Start()
        {
            var currentMapLayout = loadedMap[currentIndex];
            ActivateLayout(null, currentMapLayout.type, Vector3.zero, Quaternion.Euler(Vector3.zero));
        }

        private bool TryInstantiateLayout(LayoutType type, out LevelLayout layout)
        {
            if (type == LayoutType.MainLevelStyle0 && mainLevel)
            {
                layout = mainLevel;
                return true;
            }
            
            var prefab = layoutDataObject.LayoutPrefabs.FirstOrDefault(x => x != null && x.Type == type);

            if(prefab)
            {
                layout = Instantiate(prefab);
                layout.gameObject.SetActive(false);
                layoutPool.Add(layout);
                return true;
            }

            Debug.Log($"Level type {type} not found.");
            layout = null;
            return false;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void ActivateLayout(LevelLayout previousLayout, LayoutType nextType, Vector3 position, Quaternion rotation)
        {
            if (currentIndex >= loadedMap.Count)
            {
                Debug.Log("End of map.");
                return;
            }

            if (GetLayout(nextType, out var layout))
            {
                if (previousLayout) layout.transform.parent = previousLayout.transform;
                layout.transform.SetLocalPositionAndRotation(position, rotation);
                layout.transform.parent = null;
                layout.gameObject.SetActive(true);

                var i = currentIndex;
                var mapLayout = loadedMap[i];
                var isEndOfZone = i < (loadedMap.Count - 1) && mapLayout.zone != loadedMap[i + 1].zone;
                SetCurrentStyle(layout.Type);
                layout.Setup(i, mapLayout.nextTypes, isEndOfZone, previousLayout?.NextLayoutOffsets, decorators: null);
                if (i == 0) layout.EntranceDoorEnabled(true);
                layout.ItemList = mapLayout.items;
                itemManager.FillItems(layout);
                if (layout.HasDoors()) currentIndex++;
            }
        }



        private bool GetLayout(LayoutType type, out LevelLayout layout)
        {
            if (type == LayoutType.MainLevelStyle0 && mainLevel)
            {
                layout = mainLevel;
                return true;
            }

            layout = layoutPool.FirstOrDefault(x => x != null && x.Type == type && !x.gameObject.activeInHierarchy);

            if (layout) return true;

            Debug.Log($"Level type {type} not found.");
            return false;
        }

        public IEnumerator DeactivateLevelLayouts()
        {
            while (deactivateQueue.Count > 0)
            {
                var layout = deactivateQueue.Dequeue();
                layout.gameObject.SetActive(false);
                ActivateZoneEntranceDoor(layout.MapIndex + 1);
                itemManager.RemoveFrom(layout);
                layout.MapIndex = -1;
                yield return null;
            }

            MarkForDeactivation();
        }

        private void ActivateZoneEntranceDoor(int index)
        {
            if (deactivateQueue.Count == 0 && index < loadedMap.Count)
            {
                layoutPool.FirstOrDefault(x => x.MapIndex == index && x.gameObject.activeInHierarchy)
                    ?.EntranceDoorEnabled(true);
            }
        }

        private void MarkForDeactivation()
        {
            foreach (var layout in layoutPool.Where(x=>x.MapIndex <= currentIndex))
            {
                deactivateQueue.Enqueue(layout);
            }
        }

        private void SetCurrentStyle(LayoutType type)
        {
            string style;
            
            switch (type)
            {
                case LayoutType.MainLevelStyle0:
                    style = "style_0";
                    break;
                case LayoutType.StraightHallwayStyle1:
                case LayoutType.THallwayStyle1:
                case LayoutType.LeftLHallwayStyle1:
                case LayoutType.RightLHallwayStyle1:
                case LayoutType.SmallOfficeStyle1:
                case LayoutType.FoodStackStyle1:
                case LayoutType.PaintingRoomStyle1:
                case LayoutType.PlayingRoomStyle1:
                    style = "style_1";
                    break;
                case LayoutType.StraightHallwayStyle2:
                case LayoutType.THallwayStyle2:
                case LayoutType.LeftLHallwayStyle2:
                case LayoutType.RightLHallwayStyle2:
                case LayoutType.BedroomStyle2:
                case LayoutType.TinyHouseVintageStyle2:
                case LayoutType.PlayingRoomStyle2:
                    style = "style_2";
                    break;
                case LayoutType.StraightHallwayStyle3:
                case LayoutType.THallwayStyle3:
                case LayoutType.LeftLHallwayStyle3:
                case LayoutType.RightLHallwayStyle3:
                case LayoutType.BathroomStyle3:
                case LayoutType.ShedStyle3:
                    style = "style_3";
                    break;
                case LayoutType.StraightHallwayStyle4:
                case LayoutType.THallwayStyle4:
                case LayoutType.TinyCellStyle4:
                    style = "style_4";
                    break;
                default:
                    style = "style_5";
                    break;
            }
        }
    }
}