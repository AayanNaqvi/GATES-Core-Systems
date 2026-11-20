using System;
using UnityEngine;

[Serializable]
public enum ElementType
{
    Fire,
    Water,
    Thunder,
    Frost,
    Poison,
    Basic
}

public static class ElementChart
{
    public static float GetMultiplier(ElementType attack, ElementType defense)
    {
        
        //Weaknesses
        
        if (attack == ElementType.Fire && defense == ElementType.Thunder) return 2f;
        if (attack == ElementType.Fire && defense == ElementType.Frost) return 2f;

        if (attack == ElementType.Water && defense == ElementType.Fire) return 2f;
        if (attack == ElementType.Water && defense == ElementType.Poison) return 2f;

        if (attack == ElementType.Thunder && defense == ElementType.Water) return 2f;

        if (attack == ElementType.Poison && defense == ElementType.Thunder) return 2f;

        if (attack == ElementType.Frost && defense == ElementType.Water) return 2f;
        if (attack == ElementType.Frost && defense == ElementType.Poison) return 2f;


        //Resistances

        if (attack == ElementType.Fire && defense == ElementType.Poison) return 0.5f;
        if (attack == ElementType.Fire && defense == ElementType.Water) return 0.5f;
        if (attack == ElementType.Fire && defense == ElementType.Fire) return 0.5f;

        if (attack == ElementType.Water && defense == ElementType.Water) return 0.5f;
        if (attack == ElementType.Water && defense == ElementType.Thunder) return 0.5f;
        if (attack == ElementType.Water && defense == ElementType.Frost) return 0.5f;

        if (attack == ElementType.Thunder && defense == ElementType.Poison) return 0.5f;
        if (attack == ElementType.Thunder && defense == ElementType.Thunder) return 0.5f;
        if (attack == ElementType.Thunder && defense == ElementType.Fire) return 0.5f;

        if (attack == ElementType.Poison && defense == ElementType.Water) return 0.5f;
        if (attack == ElementType.Poison && defense == ElementType.Poison) return 0.5f;

        if (attack == ElementType.Frost && defense == ElementType.Fire) return 0.5f;
        if (attack == ElementType.Frost && defense == ElementType.Frost) return 0.5f;

        return 1f;
    }
}