using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlastPool : Pool<Blast> { }

public class EffectManager : SingletonMonoBehaviour<EffectManager>
{
    [SerializeField]
    private BlastPool blasts = new BlastPool();

    private void Start()
    {
        blasts.beforeOnInstantiated = (Blast blast) =>
        {
            blast.transform.parent = transform;
        };
    }

    public void Spawn(Vector3 pos)
    {
        blasts.Generate(pos);
    }

    public void Restart()
    {
        blasts.AllToUnused();
    }
}
