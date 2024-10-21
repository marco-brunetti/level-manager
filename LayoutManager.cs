using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Game.General;

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

        private GameControllerV2 gameController;

        public static LayoutManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            savedMap = JsonConvert.DeserializeObject<LayoutMap>(mapJson.ToString());

            foreach (var prefab in layoutDataObject.LayoutPrefabs.Where(x => x != null && !prefabs.ContainsKey(x.Type)))
            {
                prefabs.Add(prefab.Type, prefab);
            }

            if (GetLayout(LayoutType.MainLevelStyle0, out var layout))
            {
                mainLevel = layout;
                mainLevel.gameObject.SetActive(false);
            }

            foreach (var layoutData in savedMap.Layouts.Where(x => x.enable))
            {
                loadedMap.Add(layoutData);
            }

            gameController = GameControllerV2.Instance;
        }

        private void Start()
        {
            var currentMapLayout = loadedMap[currentIndex];
            ActivateLayout(null, currentMapLayout.type, Vector3.zero, Quaternion.Euler(Vector3.zero));
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

            layout = layoutPool.FirstOrDefault(x =>
                x != null && x.Type == type && !x.gameObject.activeInHierarchy);

            if (layout) return true;
            
            if (prefabs.TryGetValue(type, out var prefab))
            {
                layout = Instantiate(prefab);
                layoutPool.Add(layout);
                return true;
            }

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
            CurrentLayoutStyle gameStyle;
            
            switch (type)
            {
                case LayoutType.MainLevelStyle0:
                    gameStyle = CurrentLayoutStyle.Style0;
                    break;
                case LayoutType.StraightHallwayStyle1:
                case LayoutType.THallwayStyle1:
                case LayoutType.LeftLHallwayStyle1:
                case LayoutType.RightLHallwayStyle1:
                case LayoutType.SmallOfficeStyle1:
                case LayoutType.FoodStackStyle1:
                case LayoutType.PaintingRoomStyle1:
                case LayoutType.PlayingRoomStyle1:
                    gameStyle = CurrentLayoutStyle.Style1;
                    break;
                case LayoutType.StraightHallwayStyle2:
                case LayoutType.THallwayStyle2:
                case LayoutType.LeftLHallwayStyle2:
                case LayoutType.RightLHallwayStyle2:
                case LayoutType.BedroomStyle2:
                case LayoutType.TinyHouseVintageStyle2:
                case LayoutType.PlayingRoomStyle2:
                    gameStyle = CurrentLayoutStyle.Style2;
                    break;
                case LayoutType.StraightHallwayStyle3:
                case LayoutType.THallwayStyle3:
                case LayoutType.LeftLHallwayStyle3:
                case LayoutType.RightLHallwayStyle3:
                case LayoutType.BathroomStyle3:
                case LayoutType.ShedStyle3:
                    gameStyle = CurrentLayoutStyle.Style3;
                    break;
                case LayoutType.StraightHallwayStyle4:
                case LayoutType.THallwayStyle4:
                case LayoutType.TinyCellStyle4:
                    gameStyle = CurrentLayoutStyle.Style4;
                    break;
                default:
                    gameStyle = CurrentLayoutStyle.Style0;
                    break;
            }

            gameController.SetCurrentStyle(gameStyle);

        }
    }
}