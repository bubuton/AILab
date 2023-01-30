using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    A simple tank AI based on behaviour trees.
    */
    public class TankBT : MonoBehaviour
    {
        public int m_PlayerNumber = 1;      // Used to identify which tank belongs to which player.  This is set by this tank's manager.
        public int m_Behaviour = 0;         // Used to select an AI behaviour in the Unity Inspector

        private TankMovement m_Movement;    // Reference to tank's movement script, used by the AI to control movement.
        private TankShooting m_Shooting;    // Reference to tank's shooting script, used by the AI to fire shells.
        private List<GameObject> m_Targets; // List of enemy targets for this tank
        private Root tree;                  // The tank's behaviour tree
        private Blackboard blackboard;      // The tank's behaviour blackboard

        // Initialisation
        private void Awake()
        {
            m_Targets = new List<GameObject>();
        }

        // Start behaviour tree
        private void Start()
        {
            Debug.Log("Initialising AI player " + m_PlayerNumber);
            m_Movement = GetComponent<TankMovement> ();
            m_Shooting = GetComponent<TankShooting> ();

            tree = CreateBehaviourTree();
            blackboard = tree.Blackboard;
            #if UNITY_EDITOR
            Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = tree;
            #endif
            
            tree.Start();
        }

        private Root CreateBehaviourTree()
        {
            // To change a tank's behaviour:
            // - Examine the GameManager object in the inspector.
            // - Open the Tanks list, find the tank's entry.
            // - Enable the NPC checkbox.
            // - Edit the Behaviour field to an integer N corresponding to the behaviour below
            switch (m_Behaviour)
            {
                // N=1
                case 1:
                    return SpinBehaviour(-0.05f, 1f);
                case 2:
                    return FireRandomly();
                case 3:
                    return FireTurnRandom();
                // Default behaviour: turn slowly
                default:
                    return TurnSlowly();
            }
        }

        /**************************************
         * 
         * TANK ACTIONS
         * 
         * Move, turn and fire
         */

        // ACTION: move the tank with a velocity between -1 and 1.
        // -1: fast reverse
        // 0: no change
        // 1: fast forward
        private void Move(float velocity) { 
            m_Movement.AIMove(velocity);
        }
        
        // ACTION: turn the tank with angular velocity between -1 and 1.
        // -1: fast turn left
        // 0: no change
        // 1: fast turn right
        private void Turn(float velocity) { 
            m_Movement.AITurn(velocity);
        }

        // ACTION: fire a shell with a force between 0 and 1
        // 0: minimum force
        // 1: maximum force
        private void Fire(float force) { 
            m_Shooting.AIFire(force);
        }

       

        /**************************************
        * 
        * TANK PERCEPTION
        * 
        */

        private void UpdatePerception()
        {
            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;
            blackboard["targetDistance"] = localPos.magnitude;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
        }

        // Register an enemy target 
        public void AddTarget(GameObject target)
        {
            m_Targets.Add(target);
        }

        // Get the transform for the first target
        private Transform TargetTransform()
        {
            if (m_Targets.Count > 0)
            {
                return m_Targets[0].transform;
            }
            else
            {
                return null;
            }
        }

        /**************************************
         * 
         * BEHAVIOUR TREES
         * 
         */

        // Just turn slowly
        private Root TurnSlowly()
        {
            return new Root(new Action(() => Turn(0.1f)));
        }

        // Constantly spin and fire on the spot 
        private Root SpinBehaviour(float turn, float shoot)
        {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));
        }

        // RandomFire tree
        private Root FireRandomly() {
            return new Root(RandomFire());
        }

        // Tree that random fire and random turn
        private Root FireTurnRandom() {
            return new Root(new Sequence(
                    RandomFire(),
                    RandomTurn()
                    //new Action (() => Debug.Log("hi")),
                    //StopTurning(),
                    //new Wait(2)
                ));
        }

        private Node StopTurning()
        {
            return new Action(() => Turn(0));
        }

        private Node RandomFire()
        {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }
        //random turn method
        private Node RandomTurn() {
            //float velocity = UnityEngine.Random.Range(-1f, 1f);
            //float randomTime = UnityEngine.Random.Range(2, 5);
            //Debug.Log(velocity);
            return new Sequence(
                new Action(() => Turn(UnityEngine.Random.Range(-1f, 1f))),
                new Wait(UnityEngine.Random.Range(2, 5))
                );
        }


        
    }
}