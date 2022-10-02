using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public void SetSize(float size) {
        float scale = 0.5f + 0.05f * size;
        transform.localScale = Vector3.one * scale;
    }
}
