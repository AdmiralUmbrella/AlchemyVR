using UnityEngine;
using UnityEngine.AI;

public class SummonerChaseState : BaseState<SummonerState>
    {
        private SummonerAI manager; 
        private SummonerData summonerData; 
        private float pathUpdateTimer; 
        private SummonerState nextState;
        
        public SummonerChaseState(SummonerAI manager, SummonerData data)
            : base(SummonerState.Chase)
        {
            this.manager = manager;
            this.summonerData = data;
            nextState = SummonerState.Chase;
        }

        public override void EnterState()
        {
            Debug.Log("Summoner entró en estado: CHASE");
            manager.ResumeAgent();
            pathUpdateTimer = 0f;
            if (summonerData.animator != null)
            {
                summonerData.animator.SetBool("IsMoving", true);
            }
            UpdateChasePath();
            nextState = SummonerState.Chase;
        }

        public override void UpdateState()
        {
            if (summonerData.targetTransform == null || summonerData.isDead)
            {
                nextState = SummonerState.Idle;
                return;
            }

            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0f)
            {
                pathUpdateTimer = summonerData.pathUpdateInterval;
                UpdateChasePath();
            }

            float dist = Vector3.Distance(manager.transform.position, summonerData.targetTransform.position);
            if (dist > summonerData.stopChaseDistance)
            {
                nextState = SummonerState.Patrol;
                return;
            }
            if (dist <= summonerData.summonRange)
            {
                nextState = SummonerState.Summon;
                return;
            }
            nextState = SummonerState.Chase;
        }

        public override void ExitState()
        {
            Debug.Log("Summoner saliendo de estado: CHASE");
            if (summonerData.animator != null)
            {
                summonerData.animator.SetBool("IsMoving", false);
            }
        }

        public override SummonerState GetNextState()
        {
            return nextState;
        }

        public override void OnTriggerEnter(Collider other) { }
        public override void OnTriggerStay(Collider other) { }
        public override void OnTriggerExit(Collider other) { }

        private void UpdateChasePath()
        {
            NavMeshAgent navAgent = manager.GetComponent<NavMeshAgent>();
            if (navAgent != null && summonerData.targetTransform != null)
            {
                navAgent.SetDestination(summonerData.targetTransform.position);
            }
        }
    }
