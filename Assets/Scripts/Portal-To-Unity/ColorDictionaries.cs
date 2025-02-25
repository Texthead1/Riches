using System.Collections.Generic;
using UnityEngine;

namespace PortalToUnity
{
    public readonly struct ElementColors
    {
        public Color32 Color { get; }

        public ElementColors(byte defaultR, byte defaultG, byte defaultB)
        {
            Color = new Color32(defaultR, defaultG, defaultB, 255);
        }
    }

    public class PortalElements
    {
        public static Dictionary<Element, ElementColors> colors = new Dictionary<Element, ElementColors>();

        static PortalElements()
        {
            colors.Add(Element.Earth,
            new ElementColors(0x7F, 0x19, 0x00));

            colors.Add(Element.Water,
            new ElementColors(0x00, 0x44, 0xFF));

            colors.Add(Element.Air,
            new ElementColors(0x58, 0xC2, 0xDC));

            colors.Add(Element.Fire,
            new ElementColors(0xFF, 0x00, 0x00));

            colors.Add(Element.Life,
            new ElementColors(0x00, 0xFF, 0x00));

            colors.Add(Element.Undead,
            new ElementColors(0x15, 0x12, 0x32));

            colors.Add(Element.Magic,
            new ElementColors(0x74, 0x00, 0x9B));

            colors.Add(Element.Tech,
            new ElementColors(0xAC, 0x6A, 0x00));

            colors.Add(Element.Light,
            new ElementColors(0xF2, 0xF2, 0x58));

            colors.Add(Element.Dark,
            new ElementColors(0x06, 0x00, 0x16));

            colors.Add(Element.Kaos,
            new ElementColors(0x06, 0x00, 0x16));
        }
    }
}