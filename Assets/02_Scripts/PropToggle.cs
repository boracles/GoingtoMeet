using UnityEngine;

public class PropToggle : MonoBehaviour
{
    [Tooltip("손에 붙일 고기 오브젝트 (meat)")]
    public GameObject meat;

    public void MeatOn()
    {
        if (meat) meat.SetActive(true);
    }

    public void MeatOff()
    {
        if (meat) meat.SetActive(false);
    }
}
