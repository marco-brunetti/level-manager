using System;
using System.Collections.Generic;
using UnityEngine;

namespace Layouts
{
    public class LevelItem : MonoBehaviour
    {
        [field: SerializeField] public int Id { get; private set; } = 0;
        [field: SerializeField] public bool Enable { get; private set; } = true;
        [field: SerializeField] public List<LayoutType> LayoutCompatibility { get; private set; }
        [field: SerializeField] public LayoutItemSize Size { get; private set; }
        [field: SerializeField] public Vector3 Position { get; private set; }
        [field: SerializeField] public Vector3 Rotation { get; private set; }
        [field: SerializeField] public bool RandomRotX { get; private set; } = false;
        [field: SerializeField] public bool RandomRotY { get; private set; } = false;
        [field: SerializeField] public bool RandomRotZ { get; private set; } = false;

        [SerializeField] private Vector3 scale;

        [NonSerialized] public bool IsUsed;

        public Vector3 Scale => scale == Vector3.zero ? transform.localScale : scale;
    }
}