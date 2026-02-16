using UnityEngine;

public class ActZoneTrigger : MonoBehaviour
{
    public ActSceneManager mgr;
    public ActId targetAct;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CityCat"))   // 고양이 태그가 Player라면
        {
            mgr.SwitchAct(targetAct);
        }
    }
}