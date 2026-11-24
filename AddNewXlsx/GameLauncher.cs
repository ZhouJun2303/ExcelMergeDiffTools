using ET;
using Hlwd.Framework;
using System;
using UnityEngine;

namespace Hlwd.AOT
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class GameLauncher : BaseMonoBehavior
    {
        public static GameLauncher Instance;

        // 游戏流程状态机
        GameProcedureFSM<GameProcedureState> gameProcedureFsm;

        private void Awake()
        {
            StartupTiming.Mark("aot.game_launcher_awake");
            Instance = this;
            SetLogCallback();
        }

        private void Start()
        {
            StartupTiming.Mark("aot.game_launcher_start");
            this.LogInfo("GameLauncher Start");

            //将当前时间戳保存在本地
            PlayerPrefs.SetString("LastLaunchTime", DateTime.Now.ToString());
            PlayerPrefs.SetString("LastLaunchTime", DateTime.Now.ToString());

            // 游戏流程状态机初始化
            gameProcedureFsm = AotEntity.Fsm.CreateFSM<GameProcedureFSM<GameProcedureState>, GameProcedureState>("GameProcedure");
            var fsm = gameProcedureFsm;
            fsm.AddState(new ProcedureLaunch(GameProcedureState.Launch, fsm));
            fsm.AddState(new ProcedureGetGBABTestParams(GameProcedureState.InitABTest, fsm));
            fsm.AddState(new ProcedureCheckHotfix(GameProcedureState.CheckHotfix, fsm));
            fsm.AddState(new ProcedureDownloadAssets(GameProcedureState.DownloadAssets, fsm));
            fsm.AddState(new ProcedureSyncArchive(GameProcedureState.SyncArchive, fsm));
            fsm.AddState(new ProcedureStartGame(GameProcedureState.StartGame, fsm));
            fsm.AddState(new ProcedureExitGame(GameProcedureState.ExitGame, fsm));
            fsm.StartFsm(GameProcedureState.Launch);
        }

        private void OnDestroy()
        {
            //AotEntity.Fsm.DestroyFsm(gameProcedureFsm);
        }

        public void StartGame()
        {
            gameProcedureFsm?.SetState(GameProcedureState.InitABTest);

        }

        public GameProcedureState CurrentState { get { return gameProcedureFsm.State; } }

        private void SetLogCallback()
        {
            System.AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Debug.LogError(e.ExceptionObject.ToString());
            };
            ETTask.ExceptionHandler += LogError;

            void LogError(Exception err)
            {
                if (err.Data.Contains("StackTrace"))
                {
                    Debug.LogError($"{err.Data["StackTrace"]}\n{err}");
                    return;
                }
                Debug.LogError(str);
            }
        }
    }
}
