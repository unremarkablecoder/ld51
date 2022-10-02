using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
    Split,
    Rock,
    Toxin,
}

public class Item : MonoBehaviour {
    public ItemType itemType;
    public float radius = 0.5f;
}
