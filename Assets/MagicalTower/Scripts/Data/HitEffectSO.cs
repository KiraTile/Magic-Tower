using UnityEngine;

public abstract class HitEffectSO : ScriptableObject
{
    public abstract HitEffectData ToRuntimeEffect();
}
