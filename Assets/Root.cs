using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public enum WaterState {
    Idle,
    Filling,
    Draining,
}
public class Node {
    public Vector2 relPos;
    public float size = 1;
    public float growAngleRads;
    public Vector2 worldPos;
    public GameObject gameObject;
    public bool alive = true;
    public float waterLevel = 0;
    public WaterState waterState = WaterState.Idle;

    public Node parent = null;
    public List<Node> children = new List<Node>();
    public GameObject aimObject;

    public Node(Vector2 relPos, float size) {
        this.relPos = relPos;
        this.size = size;
        growAngleRads = Mathf.Atan2(this.relPos.normalized.y, this.relPos.normalized.x);
    }

    public Node AddChild(Node node) {
        children.Add(node);
        node.parent = this;
        node.worldPos = worldPos + node.relPos;
        return node;
    }

}


public class Root : MonoBehaviour {
    [SerializeField] private UI ui;
    [SerializeField] private GameObject branchPrefab;
    [SerializeField] private GameObject selectionCircle;
    [SerializeField] private Items items;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color aliveEndColor;
    [SerializeField] private Color aimColor;
    [SerializeField] private ProgressBar timerBar;
    [SerializeField] private Tree tree;
    
    private Node root;
    private float timer = 0;
    private bool paused = true;

    private Vector2 mousePos;
    private Node hoveredNode = null;
    private bool mouseDown = false;
    private float hoveredNodeDistSq = 999;

    private const float maxAngle = 45;
    private const float waterSpeed = 10f;

    private Coroutine showTreeCoroutine;

    private float orgOrhtoSize;
    private Vector3 orgCamPos;
    
    void Awake() {
        var cam = Camera.main;
        orgCamPos = cam.transform.position;
        orgOrhtoSize = cam.orthographicSize;
    }

    public void StartGame() {
        paused = false;
    }

    public void RestartGame() {
        timerBar.gameObject.SetActive(true);
        Camera.main.transform.position = orgCamPos;
        Camera.main.orthographicSize = orgOrhtoSize;
        ClearNode(root);
        items.GenerateItems();
        timer = 0;
        hoveredNode = null;
        mouseDown = false;
        selectionCircle.gameObject.SetActive(false);
        root = new Node(new Vector2(0,0), 1);
        //root.relPos = new Vector2(0, -1);
        root.growAngleRads = Mathf.Atan2(-1, 0);
        root.AddChild(new Node(new Vector2(0, -1), 1));
    }

    public void ShowTree() {
        timerBar.gameObject.SetActive(false);
        showTreeCoroutine = StartCoroutine(ShowTreeCo());
    }

    private IEnumerator ShowTreeCo() {
        var camPos = Camera.main.transform.position;
        while (camPos.y < 11.5f) {
            camPos.y += 11.5f * Time.deltaTime;
            Camera.main.transform.position = camPos;
            yield return new WaitForEndOfFrame();
        }

        
    }

    private void ClearNode(Node node) {
        if (node == null) {
            return;
        }
        if (node.gameObject) {
            Destroy(node.gameObject);
        }

        if (node.aimObject) {
            Destroy(node.aimObject);
        }

        foreach (var child in node.children) {
            ClearNode(child);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        RestartGame();
    }

    
    bool HandleNodeInput(Node node, Vector2 parentPos) {
        bool canStillGrow = false;
        var pos = parentPos + node.relPos;
        Debug.DrawLine(parentPos, pos);
        node.worldPos = pos;
        if (node.children.Count == 0 && node.alive) {
            canStillGrow = true;
            Vector2 growDir = new Vector2(Mathf.Cos(node.growAngleRads), Mathf.Sin(node.growAngleRads));
            //Debug.DrawLine(pos, pos + growDir, Color.gray);
            float distSq = (mousePos - pos).sqrMagnitude;
            if (!mouseDown && distSq < 1.5f && distSq < hoveredNodeDistSq) {
                hoveredNode = node;
                hoveredNodeDistSq = distSq;
            }
        }

        foreach (var child in node.children) {
            canStillGrow |= HandleNodeInput(child, pos);
        }

        return canStillGrow;
    }

    void Grow(Node node) {
        if (node.children.Count == 0 && node.alive) {
            Vector2 growDir = new Vector2(Mathf.Cos(node.growAngleRads), Mathf.Sin(node.growAngleRads));

            if (!Physics2D.Linecast(node.worldPos + growDir * 0.5f, node.worldPos + growDir)
                && node.worldPos.y + growDir.y < 0 && node.worldPos.y + growDir.y > -16 &&
                node.worldPos.x + growDir.x > -21 && node.worldPos.x + growDir.x < 21) {
                Item hitItem = items.CheckForItem(node.worldPos + growDir);
                if (!hitItem) {
                    hitItem = items.CheckForItem(node.worldPos + growDir * 0.5f);
                }
                if (hitItem) {
                    if (hitItem.itemType == ItemType.Split) {
                        var splitter = node.AddChild(new Node(growDir, 1));

                        //split
                        float angleRads = splitter.growAngleRads;
                        float splitRads = 20 * Mathf.Deg2Rad;
                        Vector2 a = new Vector2(Mathf.Cos(angleRads - splitRads), Mathf.Sin(angleRads - splitRads));
                        Vector2 b = new Vector2(Mathf.Cos(angleRads + splitRads), Mathf.Sin(angleRads + splitRads));
                        {
                            var newChild = splitter.AddChild(new Node(a, 1));
                            newChild.growAngleRads = splitter.growAngleRads - splitRads;
                        }
                        {
                            var newChild = splitter.AddChild(new Node(b, 1));
                            newChild.growAngleRads = splitter.growAngleRads + splitRads;
                        }
                        items.DestroyItem(hitItem);
                    } else if (hitItem.itemType == ItemType.Rock) {
                        node.alive = false;
                    }
                }
                else {
                    float prevAngleRads = Mathf.Atan2(node.relPos.normalized.y, node.relPos.normalized.x);
                    float relAngle = node.growAngleRads - prevAngleRads;
                    var newChild = node.AddChild(new Node(growDir, 1));
                    newChild.growAngleRads = node.growAngleRads + relAngle;
                }
            }
            else {
                node.alive = false;
            }

        }
        else {
            foreach (var child in node.children) {
                Grow(child);
            }
        }
    }
    

    int UpdateBranches(Node node, Vector2 parentPos) {
        int numChildrenRecursive = 1;
        var pos = parentPos + node.relPos;
        node.worldPos = pos;
        if (node.gameObject == null && node.parent != null) {
            node.gameObject = Instantiate(branchPrefab);
            node.gameObject.transform.position = node.worldPos;
            var dir = node.relPos.normalized;
            node.gameObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        bool fillChildren = false;
        if (node.waterState == WaterState.Filling) {
            node.waterLevel += waterSpeed * Time.deltaTime;
            if (node.waterLevel >= 1.0f) {
                node.waterState = WaterState.Draining;
                fillChildren = true;

            }
        }

        if (node.waterState == WaterState.Draining) {
            node.waterLevel -= waterSpeed * Time.deltaTime;
            if (node.waterLevel <= 0) {
                node.waterLevel = 0;
                node.waterState = WaterState.Idle;
                if (node.children.Count == 0) {
                    Grow(node);
                }
            }
        }
        
        if (node.gameObject) {
            float scale = 1.0f + node.waterLevel * 0.5f;
            node.gameObject.transform.localScale = Vector3.one * scale;
            if (node.alive && node.children.Count == 0) {
                node.gameObject.GetComponentInChildren<SpriteRenderer>().color = aliveEndColor;
                if (node.aimObject == null) {
                    node.aimObject = Instantiate(branchPrefab);
                    node.aimObject.GetComponentInChildren<SpriteRenderer>().color = aimColor;
                    node.aimObject.GetComponent<Collider2D>().enabled = false;
                }
                node.aimObject.transform.position = node.worldPos;
                node.aimObject.transform.rotation = Quaternion.Euler(0, 0, 180 + node.growAngleRads * Mathf.Rad2Deg);
                
                
            }
            else {
                node.gameObject.GetComponentInChildren<SpriteRenderer>().color = defaultColor;
                if (node.aimObject) {
                    Destroy(node.aimObject);
                    node.aimObject = null;
                }

            }
        }

        foreach (var child in node.children) {
            if (fillChildren) {
                child.waterState = WaterState.Filling;
            }
            numChildrenRecursive += UpdateBranches(child, pos);
        }

        return numChildrenRecursive;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (paused) {
            return;
        }
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDown = Input.GetMouseButton(0);

        if (!mouseDown) {
            hoveredNode = null;
            hoveredNodeDistSq = 999;
        }

        int numBranches = UpdateBranches(root, -root.relPos);
        tree.SetSize(numBranches);

        bool canStillGrow = HandleNodeInput(root, root.relPos);

        
        if (hoveredNode != null) {
            selectionCircle.transform.position = hoveredNode.worldPos;
            selectionCircle.gameObject.SetActive(true);
            Vector2 growDir = new Vector2(Mathf.Cos(hoveredNode.growAngleRads), Mathf.Sin(hoveredNode.growAngleRads));

            Debug.DrawLine(hoveredNode.worldPos, hoveredNode.worldPos + growDir, Color.red);
            if (mouseDown) {
                var newDir = (mousePos - hoveredNode.worldPos).normalized;
                float newDirAngleRads = Mathf.Atan2(newDir.y, newDir.x);
                float prevAngleRads = Mathf.Atan2(hoveredNode.relPos.normalized.y, hoveredNode.relPos.normalized.x);
                float delta = Mathf.DeltaAngle(newDirAngleRads * Mathf.Rad2Deg, prevAngleRads * Mathf.Rad2Deg); 
                
                if (Mathf.Abs(delta) > maxAngle) {
                    newDirAngleRads = prevAngleRads - maxAngle * Mathf.Deg2Rad * Mathf.Sign(delta);
                }

                hoveredNode.growAngleRads = newDirAngleRads;
            }
        }
        else {
            selectionCircle.gameObject.SetActive(false);
        }
        

        timer += Time.deltaTime;
        
        timerBar.SetProgress(timer / 10.0f);
        if (Input.GetKeyDown(KeyCode.Space) || timer >= 10.0f) {
            timer = 0.0f;
            
            //Grow(root);
            root.waterState = WaterState.Filling;
            root.waterLevel = 0;
        }
        
        if (!canStillGrow) {
            //game over
            paused = true;
            ui.ShowGameOver();
        }
    }
}
