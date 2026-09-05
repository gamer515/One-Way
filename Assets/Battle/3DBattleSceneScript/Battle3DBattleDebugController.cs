using UnityEngine;
using UnityEngine.SceneManagement;

public class Battle3DBattleDebugController : MonoBehaviour
{
    public void SetBattleStage1()
    {
        Battle3DBattleStateMachine.BattleIndex = 1;
        ReloadCurrentScene();
    }

    public void SetBattleStage2()
    {
        Battle3DBattleStateMachine.BattleIndex = 2;
        ReloadCurrentScene();
    }

    public void SetBattleStage3()
    {
        Battle3DBattleStateMachine.BattleIndex = 3;
        ReloadCurrentScene();
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
