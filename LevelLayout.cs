using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Interactables.Behaviours;
using UnityEngine.AI;

namespace Layouts
{
    public class LevelLayout : MonoBehaviour
    {
        [field: SerializeField] public LayoutType Type { get; private set; }
        [field: SerializeField] public List<Vector3> NextLayoutOffsets { get; private set; }
        [field: SerializeField] public List<Vector3> NextLayoutRotations { get; private set; }

        [SerializeField] private GameObject entranceDoor;
        [SerializeField] private List<Behaviour_DoorNew> doors;

        [SerializeField] private Transform[] smallAnchors;
        [SerializeField] private Transform[] mediumAnchors;
        [SerializeField] private Transform[] largeAnchors;

        [NonSerialized] public int MapIndex = -1;
        [NonSerialized] public List<LayoutItem> ItemList = new();
        
        private NavMeshLinkInstance navMeshLink;

        private List<Vector3> initialDoorRotations = new();

        public void Setup(int mapIndex, List<LayoutType> nextLayoutShapes, bool isEndOfZone, List<Vector3> navMeshLinkPoints,
            params LevelItem[] decorators)
        {
            MapIndex = mapIndex;
            
            if(navMeshLinkPoints != null)  SetNavMeshLink(navMeshLinkPoints);
            
            entranceDoor.SetActive(false);
            if(doors.Count > 0) SetDoorActions(nextLayoutShapes, isEndOfZone);
        }

        private void SetNavMeshLink(List<Vector3> previousLayoutLinkPoints) //TODO: JULIO, check if more than one door
        {
            NavMeshLinkData linkData = new NavMeshLinkData
            {
                startPosition = previousLayoutLinkPoints[0] + new Vector3(0.5f, 0, 0),
                endPosition = transform.position + new Vector3(-0.5f, 0, 0),
                bidirectional = true,
                area = NavMesh.AllAreas,
                width = 1f
            };
            navMeshLink = NavMesh.AddLink(linkData);
        }

        public bool HasDoors()
        {
            return doors != null && doors.Count > 0;
        }

        public void GetAnchors(out List<Transform> smallAnchors, out List<Transform> mediumAnchors,
            out List<Transform> largeAnchors)
        {
            smallAnchors = new(this.smallAnchors);
            mediumAnchors = new(this.mediumAnchors);
            largeAnchors = new(this.largeAnchors);
        }

        public void EntranceDoorEnabled(bool enabled)
        {
            entranceDoor.SetActive(enabled);
        }

        private void SetDoorActions(List<LayoutType> nextLayoutShapes, bool isEndOfZone)
        {
            if (doors == null || doors.Count == 0) return;

            for (int i = 0; i < doors.Count; i++)
            {
                initialDoorRotations.Add(doors[i].transform.localEulerAngles);

                if (nextLayoutShapes == null || nextLayoutShapes.Count == 0 || i >= nextLayoutShapes.Count ||
                    (nextLayoutShapes[i] == LayoutType.None))
                {
                    doors[i].SetDoorState(DoorState.Locked);
                    continue;
                }

                if (i < nextLayoutShapes.Count)
                {
                    var nextShape = nextLayoutShapes[i];
                    var offset = NextLayoutOffsets[i];
                    var rotation = Quaternion.Euler(NextLayoutRotations[i]);
                    UnityAction action = () =>
                        LayoutManager.Instance.ActivateLayout(previousLayout: this, nextShape, offset, rotation);

                    //If end of zone, start deactivation process of previous zone
                    if (isEndOfZone && i == nextLayoutShapes.Count - 1)
                    {
                        var manager = LayoutManager.Instance;
                        manager.StartCoroutine(manager.DeactivateLevelLayouts());
                    }

                    doors[i].SetDoorState(DoorState.Closed);
                    doors[i].SetDoorAction(action);
                }
            }
        }

        private void OnDisable()
        {
            NavMesh.RemoveLink(navMeshLink);
            
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].transform.localEulerAngles = initialDoorRotations[i];
            }

            initialDoorRotations.Clear();
        }
    }
}