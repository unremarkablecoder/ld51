using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Items : MonoBehaviour {
    [SerializeField] private Item splitPrefab;
    [SerializeField] private Item rockPrefab;
    private List<Item> items = new List<Item>();
    


    public void GenerateItems() {
        foreach (var item in items) {
            Destroy(item.gameObject);
        }
        items.Clear();
        
        Vector2 fieldSize = new Vector2(40, 16);
        for (int i = 0; i < 10; ++i) {
            var obj = Instantiate<Item>(splitPrefab);
            obj.transform.position = new Vector3(Random.Range(-fieldSize.x / 2, fieldSize.x / 2), Random.Range(-1, -fieldSize.y));
            obj.transform.rotation = quaternion.Euler(0, 0, Random.Range(0, 360));
            items.Add(obj);
        }
        for (int i = 0; i < 10; ++i) {
            var obj = Instantiate<Item>(rockPrefab);
            obj.transform.position = new Vector3(Random.Range(-fieldSize.x / 2, fieldSize.x / 2), Random.Range(-2, -fieldSize.y));
            obj.transform.rotation = quaternion.Euler(0, 0, Random.Range(0, 360));
            items.Add(obj);
        } 
    }

    public Item CheckForItem(Vector2 pos) {
        foreach (var item in items) {
            float radSq = item.radius * item.radius;
            if ((item.transform.position - new Vector3(pos.x, pos.y)).sqrMagnitude < radSq) {
                return item;
            }
        }

        return null;
    }

    public void DestroyItem(Item item) {
        items.Remove(item);
        Destroy(item.gameObject);
    }
}
