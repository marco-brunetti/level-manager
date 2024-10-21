using UnityEngine;
using UnityEngine.Serialization;

namespace Layouts
{
    [CreateAssetMenu(fileName = "LayoutData", menuName = "ScriptableObjects/LayoutData", order = 1)]
    public class LayoutDataObject : ScriptableObject
    {
        [FormerlySerializedAs("layoutPrefabs")] public LevelLayout[] LayoutPrefabs;
        public LevelItem[] ItemPrefabs;

        [Header("Style0: Original blue cursed silence layout")]
        public Material WallMat1;

        public Material LowerWallMat1;
        public Material FloorMat1;
        public Material CeilingMat1;
        public Material WindowDecorMat1;

        [Header("Style1: Green and wood walls")]
        public Material WallMat2;

        public Material LowerWallMat2;
        public Material FloorMat2;
        public Material CeilingMat2;
        public Material WindowDecorMat2;

        [Header("Style2: Old wooden house level")]
        public Material WallMat3;

        public Material LowerWallMat3;
        public Material FloorMat3;
        public Material CeilingMat3;
        public Material WindowDecorMat3;

        [Header("Style3: Old concrete wall level")]
        public Material WallMat4;

        public Material LowerWallMat4;
        public Material FloorMat4;
        public Material CeilingMat4;
        public Material WindowDecorMat4;

        [Header("Style4: Fabric white asylum walls")]
        public Material WallMat5;

        public Material LowerWallMat5;
        public Material FloorMat5;
        public Material CeilingMat5;
        public Material WindowDecorMat5;
    }

    public enum LayoutItemSize
    {
        Small,
        Medium,
        Large
    }

// style0: reserved for main level and related
// style1: modern house
// style2: old house
// style3: abandoned structure
// style4: psychiatric ward

    public enum LayoutType
    {
        None,

        MainLevelStyle0,
        StraightHallwayStyle1,
        StraightHallwayStyle2,
        StraightHallwayStyle3,
        StraightHallwayStyle4,

        THallwayStyle1,
        THallwayStyle2,
        THallwayStyle3,
        THallwayStyle4,

        LeftLHallwayStyle1,
        LeftLHallwayStyle2,
        LeftLHallwayStyle3,

        RightLHallwayStyle1,
        RightLHallwayStyle2,
        RightLHallwayStyle3,

        SmallOfficeStyle1,
        BedroomStyle2,
        BathroomStyle3,
        ShedStyle3,
        TinyCellStyle4,
        FoodStackStyle1,
        TinyHouseVintageStyle2,
        PaintingRoomStyle1,
        PlayingRoomStyle1,
        PlayingRoomStyle2,
        
        Void
    }
}