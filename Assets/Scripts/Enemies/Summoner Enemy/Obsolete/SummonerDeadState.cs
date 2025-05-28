using UnityEngine;
public class SummonerDeadState : BaseState<SummonerState>
    {
        private SummonerAI manager; 
        private SummonerData summonerData; 
        private SummonerState nextState;
        
        public SummonerDeadState(SummonerAI manager, SummonerData data)
            : base(SummonerState.Dead)
        {
            this.manager = manager;
            this.summonerData = data;
            nextState = SummonerState.Dead;
        }

        public override void EnterState()
        {
            Debug.Log("Summoner entró en estado: DEAD");
            summonerData.isDead = true;
            manager.StopAgent();
            summonerData.currentDeathTime = summonerData.deathDuration;
            if (summonerData.animator != null)
            {
                summonerData.animator.ResetTrigger("Summon");
                summonerData.modelRoot.SetActive(false);
                summonerData.deathVFX.SetActive(true);
            }
            nextState = SummonerState.Dead;
        }

        public override void UpdateState()
        {
            if (!summonerData.shouldDestroyOnDeath) return;

            summonerData.currentDeathTime -= Time.deltaTime;

            if (summonerData.currentDeathTime <= 0f)
            {
                Debug.Log("Destruyendo Summoner muerto.");
                manager.NotifyEnemyDestroyed();
                GameObject.Destroy(manager.gameObject);
            }
        }


        public override void ExitState() { }

        public override SummonerState GetNextState()
        {
            return nextState;
        }

        public override void OnTriggerEnter(Collider other) { }
        public override void OnTriggerStay(Collider other) { }
        public override void OnTriggerExit(Collider other) { }
    }
