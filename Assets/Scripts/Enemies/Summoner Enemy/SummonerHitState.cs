using UnityEngine;
using UnityEngine.AI;

public class SummonerHitState : BaseState<SummonerState>
    {
        private SummonerAI manager; 
        private SummonerData summonerData; 
        private SummonerState nextState;
        
        public SummonerHitState(SummonerAI manager, SummonerData data)
            : base(SummonerState.Hit)
        {
            this.manager = manager;
            this.summonerData = data;
            nextState = SummonerState.Hit;
        }

        public override void EnterState()
        {
            Debug.Log("Summoner entró en estado: HIT");
            summonerData.isStunned = true;
            summonerData.currentStunTime = summonerData.stunDuration;
            manager.StopAgent();
            if (summonerData.animator != null)
            {
                summonerData.animator.ResetTrigger("Summon");
                summonerData.animator.SetBool("IsMoving", false);
                summonerData.animator.SetTrigger("Hit");
            }
            nextState = SummonerState.Hit;
        }

        public override void UpdateState()
        {
            summonerData.currentStunTime -= Time.deltaTime;

            if (summonerData.currentStunTime <= 0f)
            {
                bool inRange = manager.CheckForTargetsInRange(summonerData.detectionRange);
                nextState = inRange ? SummonerState.Chase : SummonerState.Idle;
            }
        }

        public override void ExitState()
        {
            Debug.Log("Summoner saliendo de estado: HIT");
            summonerData.isStunned = false;
        }

        public override SummonerState GetNextState()
        {
            return nextState;
        }

        public override void OnTriggerEnter(Collider other) { }
        public override void OnTriggerStay(Collider other) { }
        public override void OnTriggerExit(Collider other) { }
        
    }
