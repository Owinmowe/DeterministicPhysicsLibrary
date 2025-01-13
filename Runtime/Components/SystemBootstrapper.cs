using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace DeterministicPhysicsLibrary.Unity
{
    internal static class SystemBootstrapper
    {
        private static PlayerLoopSystem DSSystemUpdateSimulation;
        private static PlayerLoopSystem DMSystemUpdateSimulation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Initialize() 
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            
            DSPhysicsSystem.Initialize();
            DSSystemUpdateSimulation = new PlayerLoopSystem()
            {
                type = typeof(DSPhysicsSystem),
                updateDelegate = DSPhysicsSystem.UpdateSimulation,
                subSystemList = null
            };
            InsertSystem<FixedUpdate>(ref playerLoop, DSSystemUpdateSimulation, 0);

            DMPhysicsSystem.Initialize();
            DMSystemUpdateSimulation = new PlayerLoopSystem()
            {
                type = typeof(DMPhysicsSystem),
                updateDelegate = DMPhysicsSystem.UpdateSimulation,
                subSystemList = null
            };
            InsertSystem<FixedUpdate>(ref playerLoop, DMSystemUpdateSimulation, 0);

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index) 
        {
           if (loop.type != typeof(T)) return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);

            var playerLoopSystemList = new List<PlayerLoopSystem>();

            if (loop.subSystemList != null) playerLoopSystemList.AddRange(loop.subSystemList);
            playerLoopSystemList.Insert(index, systemToInsert);

            loop.subSystemList = playerLoopSystemList.ToArray();
            return true;
        }

        private static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index) 
        {
            if (loop.subSystemList == null) 
                return false;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (!InsertSystem<T>(ref loop.subSystemList[i], in systemToInsert, index))
                    continue;

                return true;
            }

            return false;
        }
    }
}