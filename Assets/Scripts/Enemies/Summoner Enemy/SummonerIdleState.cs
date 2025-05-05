using UnityEngine;

public class SummonerIdleState : BaseState<SummonerState>
    {
        private SummonerAI manager;
        private SummonerData summonerData; 
        private float checkTimer; 
        private SummonerState nextState;
        
        public SummonerIdleState(SummonerAI manager, SummonerData data)
            : base(SummonerState.Idle)
        {
            this.manager = manager;
            this.summonerData = data;
            nextState = SummonerState.Idle;
        }

        public override void EnterState()
        {
            Debug.Log("Summoner entró en estado: IDLE");
            manager.StopAgent();
            if (summonerData.animator != null)
            {
                summonerData.animator.SetTrigger("Idle");
            }
            checkTimer = 0f;
            nextState = SummonerState.Idle;
        }

        public override void UpdateState()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = summonerData.idleCheckInterval;

                // ¿Hay torres en rango?
                bool towerFound = manager.CheckForTowerInRange(summonerData.detectionRange);

                if (!towerFound)
                {
                    // No hay torre → destruir enemigos invocados
                    manager.DestroyAllSummonedEnemies();
                }

                // Ahora checamos si hay cualquier objetivo en rango (torres o jugador)
                bool foundAnyTarget = manager.CheckForTargetsInRange(summonerData.detectionRange);
                if (foundAnyTarget)
                {
                    nextState = SummonerState.Chase;
                }
                else if (summonerData.waypoints != null && summonerData.waypoints.Length > 0)
                {
                    nextState = SummonerState.Patrol;
                }
            }
        }


        public override void ExitState()
        {
            Debug.Log("Summoner saliendo de estado: IDLE");
        }

        public override SummonerState GetNextState()
        {
            return nextState;
        }

        public override void OnTriggerEnter(Collider other) { }
        public override void OnTriggerStay(Collider other) { }
        public override void OnTriggerExit(Collider other) { }
    }
