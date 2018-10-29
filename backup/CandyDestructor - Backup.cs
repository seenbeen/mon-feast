using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyDestructor : MonoBehaviour {
    [SerializeField]
    private PlayerController player = null;
    [SerializeField]
    private CandyManager candyManager = null;
    [SerializeField]
    private FallingCandyGenerator candyGen = null;

    public float perChainBlockDestructionTime = 0.050f; // 50 mils by default

    public float destruct_timer = 0;
    public int destruct_count = 0;
    public bool freezing = false;
    public List<CandyScript> death_row = new List<CandyScript>();
    public CandyScript.Colour death_colour;

    CandyDestructorFSM fsm;
    
    class CandyDestructorFSM : FiniteStateMachine
    {
        const string IDLE_STATE = "Idle State";
        const string DESTROY_CANDY_STATE = "Destroy Candy State";
        const string SETTLE_CANDY_STATE = "Settling Candy State";

        class CandyDestructorFSMState : FiniteStateMachine.State<CandyDestructorFSM>
        {
            public virtual void DestructTile(CandyScript script, int count) {}
            public override void Update(){}
        }

        class IdleState : CandyDestructorFSMState {}

        class DestroyingCandyState : CandyDestructorFSMState
        {
            float destruct_timer;
            public int destruct_count = 0;
            public CandyScript.Colour death_colour;

            public override void Update()
            {
                List<CandyScript> death_row = GetFSM().death_row;
                CandyDestructor candyDestructor = GetFSM().candyDestructor;

                if (destruct_count == 0 || death_row.Count == 0)
                {
                    GetFSM().TransitionTo(SETTLE_CANDY_STATE);
                    return;
                }

                if (destruct_timer <= 0)
                {
                    --destruct_count;
                    destruct_timer += candyDestructor.perChainBlockDestructionTime;
                    DestructNextTile();
                }
                destruct_timer -= Time.deltaTime;
            }

            void DestructNextTile()
            {
                List<CandyScript> death_row = GetFSM().death_row;
                CandyManager candyManager = GetFSM().candyManager;

                foreach (CandyScript c in death_row)
                {
                    c.isDead = true;
                }
                death_row = candyManager.BFS(death_row, c => c.colour == death_colour, 1);
            }

            public override void DestructTile(CandyScript script, int count)
            {
                List<CandyScript> death_row = GetFSM().death_row;
                Debug.Assert(death_row.Count == 0);
                destruct_count = count;
                destruct_timer = 0;
                death_colour = script.colour;
                death_row.Add(script);
            }
        }

        class SettlingCandyState : CandyDestructorFSMState
        {
            float destruct_timer = 0;

            public override void Update()
            {
                if (destruct_timer <= 0)
                {
                    if (candyManager.SettleStep())
                    {
                        destruct_timer += perChainBlockDestructionTime;
                    }
                    else
                    {
                        freezing = false;
                        player.Unfreeze();
                        candyManager.Unfreeze();
                        candyGen.Unfreeze();
                        death_row.Clear();
                    }
                }
                destruct_timer -= Time.deltaTime;
            }
        }

        CandyDestructor candyDestructor;
        CandyManager candyManager;
        List<CandyScript> death_row;

        public CandyDestructorFSM(CandyDestructor candyDestructor, CandyManager candyManager) 
            : base(IDLE_STATE,
                   new Dictionary<string, BaseState> {
                       { IDLE_STATE, new IdleState() },
                       { DESTROY_CANDY_STATE, new DestroyingCandyState() },
                       { SETTLE_CANDY_STATE, new SettlingCandyState() }
                   })
        {
            this.candyDestructor = candyDestructor;
            this.candyManager = candyManager;
            death_row = new List<CandyScript>();
        }

        public void DestructTile(CandyScript script, int count)
        {
            GetCurrentState<CandyDestructorFSMState>().DestructTile(script, count);
        }
    }
    
    // Use this for initialization
    void Start()
    {
        fsm = new CandyDestructorFSM(this, candyManager);
    }

    // Update is called once per frame
    void Update () {
        if (freezing)
        {
            if (destruct_count == 0 || death_row.Count == 0)
            {
                if (destruct_timer <= 0)
                {
                    if (candyManager.SettleStep())
                    {
                        destruct_timer += perChainBlockDestructionTime;
                    }
                    else
                    {
                        freezing = false;
                        player.Unfreeze();
                        candyManager.Unfreeze();
                        candyGen.Unfreeze();
                        death_row.Clear();
                    }
                }
                destruct_timer -= Time.deltaTime;
            } else
            {
                /*if (destruct_timer <= 0)
                {
                    --destruct_count;
                    destruct_timer += perChainBlockDestructionTime;
                    DestructNextTile();
                }
                destruct_timer -= Time.deltaTime;*/
            }
        }
	}
    
    public void DestructTile(CandyScript script, int count)
    {
        player.Freeze();
        candyManager.Freeze();
        candyGen.Freeze();
        fsm.DestructTile(script, count);
    }
}
