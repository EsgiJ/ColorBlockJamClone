using System;
using UnityEngine;

namespace ColorBlockJamClone.Data
{
    /// <summary>
    /// Maps the BlockColor to actual color display colors and materials
    /// </summary>
    [CreateAssetMenu(fileName = "ColorPalette_", menuName = "ColorBlockJamClone/Color Palette")]
    public class ColorPaletteSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public BlockColor color;
            public Color displayColor;
            public Material material;
        }

        [SerializeField] Entry[] _entries;

        public Color GetColor(BlockColor c)
        {
            foreach(var e in _entries)
            {
                if(e.color == c)
                    return e.displayColor;
            }        

            Debug.LogWarning($"[ColorPalette] No entry for {c}");
            return Color.magenta;
        }

        public Material GetMaterial(BlockColor c)
        {
            foreach (var e in _entries)
            {
                if (e.color == c) 
                    return e.material;
                
            }

            Debug.LogWarning($"[ColorPalette] No material for {c}");
            return null;
        }
    } 
}
