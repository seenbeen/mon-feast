using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyManager : MonoBehaviour {
    class CandyScriptIndex
    {
        public int row, column;
        public CandyScriptIndex(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
    };

    // Generation-Related Fields
    [SerializeField]
    private GameObject candyPrefab = null;

    [SerializeField]
    private Vector2 offsetUnits = new Vector2(); // Origin of the grid system, normally just (0, 0)

    [SerializeField]
    private int cols = 8;
    [SerializeField]
    private int initRows = 4;
    [SerializeField]
    private float risePeriod = 10.0f;
    [SerializeField]
    private int riseRate = 4;


    // Candy-Related Fields

    // Poly-Piece Generation
    private PolyPieceGenerator ppGen = new PolyPieceGenerator();
    public float mainRadius = 1.0f;

    private float new_rise_period;


    // we're gunna use a weighted probability decay based on the length of the 
    // total chain that would be formed :)

    // let's say that you have a 10% decay for
    // each increase in length - this effectively means 0.9^L is
    // the probability that a chain of length L would form shrinks
    // exponentially with L relative to the other colours
    public float chainLengthProbabilityDecay = 0.5f;

    private List<List<CandyScript>> candy_grid = new List<List<CandyScript>>();
    private Dictionary<CandyScript, CandyScriptIndex> candy_indexer = new Dictionary<CandyScript, CandyScriptIndex>();
    private List<int> parity_map = new List<int>();

    private float spawn_counter = 0.0f;
    private int next_row_parity = 0;

    private bool currently_frozen = false;



    // Use this for initialization
    void Start() {
        ppGen.numberOfSides = 6;
        ppGen.debugRender = false;
        ppGen.mainRadius = this.mainRadius;
        ppGen.generateCollider = true;
        ppGen.colliderIsTrigger = false;

        new_rise_period = risePeriod;
        spawn_counter = risePeriod * (riseRate - 1);
        for (int j = initRows - 1; j >= 0; --j)
        {
            GenerateRow(j);
        }
    }

    public CandyScript.Colour GenerateSpawnColour()
    {
        Dictionary<CandyScript.Colour, float> matrix = new Dictionary<CandyScript.Colour, float>
        {
            { CandyScript.Colour.RED, 0 },
            { CandyScript.Colour.GREEN, 0 },
            { CandyScript.Colour.BLUE, 0 }
        };
        int ctr = 0;
        foreach (List<CandyScript> row in candy_grid)
        {
            foreach (CandyScript c in row)
            {
                if (c != null)
                {
                    ++matrix[c.colour];
                    ++ctr;
                }
                if (ctr >= 15)
                {
                    break;
                }
            }
            if (ctr >= 15)
            {
                break;
            }
        }

        return CalculateGeneratedColor(matrix);
    }

    public Vector2 GetGridPosition(float row, float col, int parity)
    {
        float vShift = PolyPieceGenerator.CalculateVStackDistance(6, mainRadius);
        const float OVERLAP_DIST = 0.0f;
        float lattice_offset = mainRadius - 0.5f;

        float x_translate = (col * 2) * (mainRadius - OVERLAP_DIST);
        float odd_row_offset = parity * mainRadius;
        return new Vector3(offsetUnits.x, offsetUnits.y) +
                           new Vector3(lattice_offset + x_translate - odd_row_offset,
                                       (0.5f + row) * vShift, transform.position.z);
    }

    public Vector2 GetVerticalVelocityByTimePeriod(float period)
    {
        float actualRadius = PolyPieceGenerator.CalculateVStackDistance(6, mainRadius);
        return new Vector2(0.0f, actualRadius / period);
    }

    public float GetHighestHeight()
    {
        float vStack = PolyPieceGenerator.CalculateVStackDistance(6, mainRadius);
        float aRad = PolyPieceGenerator.CalculateActualRadius(6, mainRadius);
        foreach (List<CandyScript> l in candy_grid)
        {
            foreach (CandyScript c in l)
            {
                if (c != null)
                {
                    return (c.gameObject.transform.position.y + aRad) / vStack;
                }
            }
        }
        return 0;
    }

    public void SetRisePeriod(float period)
    {
        new_rise_period = period;
    }

    public void Freeze()
    {
        if (!currently_frozen)
        {
            foreach (List<CandyScript> l in candy_grid)
            {
                foreach (CandyScript c in l)
                {
                    if (c != null)
                    {
                        c.Freeze();
                    }
                }
            }
            currently_frozen = true;
        }
        else
        {
            Debug.LogWarning("Freezing frozen generator.");
        }
    }

    public void Unfreeze()
    {
        if (currently_frozen)
        {
            foreach (List<CandyScript> l in candy_grid)
            {
                foreach (CandyScript c in l)
                {
                    if (c != null)
                    {
                        c.Unfreeze();
                    }
                }
            }
            currently_frozen = false;
        }
        else
        {
            Debug.LogWarning("Unfreezing unfrozen generator.");
        }
    }

    // Update is called once per frame
    void Update() {
        CleanUpDead();

        if (currently_frozen)
        {
            return;
        }

        spawn_counter += Time.deltaTime;
        if (spawn_counter >= risePeriod * riseRate)
        {
            spawn_counter -= risePeriod * riseRate;
            for (int j = 0; j < riseRate; ++j)
            {
                GenerateRow(-j);
            }
            risePeriod = new_rise_period;
            Vector2 new_vel = GetVerticalVelocityByTimePeriod(risePeriod * riseRate);
            foreach (List<CandyScript> l in candy_grid)
            {
                foreach (CandyScript c in l)
                {
                    if (c != null)
                    {
                        c.GetRB().velocity = new_vel;
                    }
                }
            }
        }
    }

    void CleanUpDead()
    {
        for (int i = candy_grid.Count - 1; i >= 0; --i)
        {
            for (int j = candy_grid[i].Count - 1; j >= 0; --j)
            {
                if (candy_grid[i][j] != null && candy_grid[i][j].isDead)
                {
                    candy_grid[i][j].SpawnGhost();
                    candy_indexer.Remove(candy_grid[i][j]);
                    Destroy(candy_grid[i][j].gameObject);
                    candy_grid[i][j] = null;
                }
            }
        }
        while (candy_grid.Count > 0 && candy_grid[0].FindIndex(c => c != null) == -1)
        {
            parity_map.RemoveAt(0);
            candy_grid.RemoveAt(0);
            foreach (KeyValuePair<CandyScript, CandyScriptIndex> kvp in candy_indexer)
            {
                --kvp.Value.row;
            }
        }
    }

    void GenerateRow(int row)
    {
        List<CandyScript> next_row = new List<CandyScript>();
        candy_grid.Add(next_row);
        int row_index = candy_grid.Count - 1;
        parity_map.Add(next_row_parity);
        for (int i = 0; i < next_row_parity + cols; ++i)
        {
            GameObject candy = GameObject.Instantiate(candyPrefab);
            CandyScript script = candy.GetComponent<CandyScript>();
            script.manager = this;
            candy.transform.position = GetGridPosition(row, i, next_row_parity);
            script.GetRB().velocity = GetVerticalVelocityByTimePeriod(risePeriod * riseRate);

            next_row.Add(script);
            candy_indexer.Add(script, new CandyScriptIndex(row_index, i));

            List<CandyScript> neighbours = FetchNeighbours(script);
            script.colour = GenerateColour(script, neighbours);
            ppGen.Generate(candy);
        }
        next_row_parity = (1 + next_row_parity) % 2;
    }

    CandyScript.Colour GenerateColour(CandyScript script, List<CandyScript> neighbours)
    {
        Dictionary<CandyScript.Colour, float> matrix = new Dictionary<CandyScript.Colour, float>();
        List<CandyScript.Colour> colours = new List<CandyScript.Colour>{
                CandyScript.Colour.RED,
                CandyScript.Colour.BLUE,
                CandyScript.Colour.GREEN
            };

        foreach (CandyScript.Colour colour in colours)
        {
            List<CandyScript> matching = new List<CandyScript>();
            foreach (CandyScript n in neighbours)
            {
                if (n.colour == colour)
                {
                    matching.Add(n);
                }
            }
            List<CandyScript> res = BFS(matching, c => c.colour == colour);
            matrix[colour] = Mathf.Pow(chainLengthProbabilityDecay, res.Count);
        }
        return CalculateGeneratedColor(matrix);
    }

    public List<CandyScript> BFS(List<CandyScript> initial_queue, System.Func<CandyScript, bool> pred = null, int layers = -1)
    {
        HashSet<CandyScript> visited = new HashSet<CandyScript>(initial_queue);
        Queue<CandyScript> queue = new Queue<CandyScript>(initial_queue);
        while (queue.Count > 0 && layers != 0)
        {
            int layerSize = queue.Count;
            for (int i = 0; i < layerSize; ++i)
            {
                CandyScript cur = queue.Dequeue();
                List<CandyScript> neighbours = FetchNeighbours(cur);
                foreach (CandyScript n in neighbours)
                {
                    if (!visited.Contains(n) && (pred == null || pred(n)))
                    {
                        visited.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }
            layers -= 1;
        }
        foreach (CandyScript c in initial_queue)
        {
            visited.Remove(c);
        }
        return new List<CandyScript>(visited);
    }

    public Vector2Int ConvertToGridIndices(Vector2Int index)
    {
        return new Vector2Int(index.y, index.x);
    }

    CandyScript SafeGridFetch(int row, int col)
    {
        if (row < 0 || row >= candy_grid.Count || col < 0 || col >= candy_grid[row].Count)
        {
            return null;
        }

        return candy_grid[row][col];
    }

    List<CandyScript> FetchNeighbours(CandyScript script)
    {
        CandyScriptIndex id = candy_indexer[script];
        int x = id.column;
        int y = id.row;
        int p = parity_map[y];
        List<Vector2Int> neighbour_indices = new List<Vector2Int>();
        List<CandyScript> neighbours = new List<CandyScript>();
        if (p == 1)
        {
            if (id.column != 0)
            {
                neighbour_indices.Add(new Vector2Int(x - 1, y));
                neighbour_indices.Add(new Vector2Int(x - 1, y + 1));
                neighbour_indices.Add(new Vector2Int(x - 1, y - 1));
            }
            if (id.column != cols)
            {
                neighbour_indices.Add(new Vector2Int(x + 1, y));
                neighbour_indices.Add(new Vector2Int(x, y + 1));
                neighbour_indices.Add(new Vector2Int(x, y - 1));
            }
        } else
        {
            neighbour_indices.Add(new Vector2Int(x + 1, y + 1));
            neighbour_indices.Add(new Vector2Int(x + 1, y - 1));
            neighbour_indices.Add(new Vector2Int(x, y + 1));
            neighbour_indices.Add(new Vector2Int(x, y - 1));
            if (id.column != 0)
            {
                neighbour_indices.Add(new Vector2Int(x - 1, y));
            }
            if (id.column != cols - 1)
            {
                neighbour_indices.Add(new Vector2Int(x + 1, y));
            }
        }
        foreach (Vector2Int ni in neighbour_indices)
        {
            Vector2Int gc = ConvertToGridIndices(ni);
            CandyScript neighbour = SafeGridFetch(gc.x, gc.y);
            if (neighbour != null)
            {
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }

    CandyScript.Colour CalculateGeneratedColor(Dictionary<CandyScript.Colour, float> matrix)
    {
        float total_range = 0; // this is the range to pull a number out of
        List<float> start_range = new List<float>(); // where this colour range begins
        List<float> end_range = new List<float>(); // where this colour range ends
        List<CandyScript.Colour> assoc_cols = new List<CandyScript.Colour>(); // the associated colours matching the probabilities

        foreach (KeyValuePair<CandyScript.Colour, float> pair in matrix)
        {
            if (pair.Value == -1)
            {
                continue;
            }
            start_range.Add(total_range);
            total_range += pair.Value;
            end_range.Add(total_range);
            assoc_cols.Add(pair.Key);
        }
        // Now we do a random draw!
        float winner = Random.Range(0, total_range);
        // find our winner
        for (int i = 0; i < assoc_cols.Count; ++i)
        {
            if (start_range[i] <= winner && winner < end_range[i])
            {
                return assoc_cols[i];
            }
        }
        return CandyScript.Colour.BLUE; // should never happen but compiler complains if no return value
    }

    // a candy is stable if either:
    // - it's on the bottom row
    // - it has two blocks supporting it
    // - it is of odd parity and on an edge, with 1 block supporting it
    bool IsStable(CandyScript c)
    {
        CandyScriptIndex id = candy_indexer[c];
        return (id.row == candy_grid.Count - 1)
            ||
            (parity_map[id.row] == 0 && // even parity
            (SafeGridFetch(id.row + 1, id.column) != null && SafeGridFetch(id.row + 1, id.column + 1) != null)) // has 2 under it
            ||
            (parity_map[id.row] == 1 && // odd parity
            (SafeGridFetch(id.row + 1, id.column) != null && SafeGridFetch(id.row + 1, id.column - 1) != null) || // has 2 under it
            ((id.column == 0 || id.column == cols) && // is an edge piece
                     (SafeGridFetch(id.row + 1, id.column) != null || SafeGridFetch(id.row + 1, id.column - 1) != null))
                ); // one supporting piece 

    }

    KeyValuePair<Vector2Int, Vector2Int> FindStablizingMove()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int i = candy_grid.Count - 2; i >= 0; --i)
        {
            for (int j = candy_grid[i].Count - 1; j >= 0; --j)
            {
                if (candy_grid[i][j] == null)
                {
                    continue;
                }
                if (parity_map[i] == 0)
                {
                    if (SafeGridFetch(i + 1, j) == null)
                    {
                        candidates.Add(new Vector2Int(i + 1, j));
                    }
                    if (SafeGridFetch(i + 1, j + 1) == null)
                    {
                        candidates.Add(new Vector2Int(i + 1, j + 1));
                    }
                } else
                {
                    if (j == 0 && SafeGridFetch(i + 1, j) == null)
                    {
                        candidates.Add(new Vector2Int(i + 1, j));
                    } else if (j == cols && SafeGridFetch(i + 1, j - 1) == null)
                    {
                        candidates.Add(new Vector2Int(i + 1, j - 1));
                    } else if (j != 0 && j != cols)
                    {
                        if (SafeGridFetch(i + 1, j) == null)
                        {
                            candidates.Add(new Vector2Int(i + 1, j));
                        }
                        if (SafeGridFetch(i + 1, j - 1) == null)
                        {
                            candidates.Add(new Vector2Int(i + 1, j - 1));
                        }
                    }
                }

                if (candidates.Count > 0)
                {
                    return new KeyValuePair<Vector2Int, Vector2Int>(new Vector2Int(i, j), candidates[Random.Range(0, candidates.Count)]);
                }
            }
        }
        return new KeyValuePair<Vector2Int, Vector2Int>( new Vector2Int(-1, -1), new Vector2Int(-1, -1) );
    }

    // returns null if the system is considered "stable"
    // on the other hand, if settling is necessary, returns the 
    // offending script after making one move towards a stable state
    public CandyScript SettleStep()
    {
        KeyValuePair<Vector2Int, Vector2Int> move = FindStablizingMove();
        if (move.Key.x == -1)
        {
            return null;
        }

        candy_grid[move.Value.x][move.Value.y] = candy_grid[move.Key.x][move.Key.y];
        candy_grid[move.Key.x][move.Key.y] = null;
        
        CandyScript script = candy_grid[move.Value.x][move.Value.y];
        
        float dx = new int[] { -1, 1 }[move.Value.y - move.Key.y + parity_map[move.Key.x]] * mainRadius;
        float dy = PolyPieceGenerator.CalculateVStackDistance(6, mainRadius);
        script.transform.position += new Vector3(dx, -dy, 0);
        
        CandyScriptIndex id = candy_indexer[script];
        id.row = move.Value.x;
        id.column = move.Value.y;
        
        return script;
    }
}
