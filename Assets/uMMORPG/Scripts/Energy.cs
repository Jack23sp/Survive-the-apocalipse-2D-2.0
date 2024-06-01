// for health, mana, etc.
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public abstract partial class Energy : NetworkBehaviour
{
    // current value
    // set & get: keep between min and max
    [SyncVar(hook = (nameof(CheckHealth)))] int _current = 0;
    [HideInInspector] public int prevCurrent;
    public int current
    {
        get { return Mathf.Min(_current, max); }
        set
        {
            bool emptyBefore = _current == 0.0f;
            _current = Mathf.Clamp(value, 0, max);
            if (_current == 0.0f && !emptyBefore) onEmpty.Invoke();
        }
    }

    public void CheckHealth(int oldValue, int newValue)
    {
        prevCurrent = oldValue;
        PlayerCallback callback = gameObject.GetComponent<PlayerCallback>();
        if (callback)
        {
            callback.CheckConsume();
        }
    }
    public void Test() { }

    // maximum value (may depend on buffs, items, etc.)
    public abstract int max { get; }

    // recovery rate (may depend on buffs, items etc.)
    public abstract int recoveryRate { get; }

    // don't recover while dead. all energy scripts need to check Health.
    public Health health;

    // spawn with full energy? important for monsters, etc.
    public bool spawnFull = true;

    [Header("Events")]
    public UnityEvent onEmpty = new UnityEvent();

    public float timeToRegenerate = 20.0f;

    public float currentTimer = 20.0f;

    public float perc = 0.0f;
    public float percBoost = 0.0f;
    public float percAbility = 0.0f;

    public override void OnStartServer()
    {
        // set full energy on start if needed
        if (spawnFull) current = max;
        PlayerAbility ability = gameObject.GetComponent<PlayerAbility>();

        if (ability)
        {
            for (int i = 0; i < ability.networkAbilities.Count; i++)
            {
                ability.ManageAbility(ability.networkAbilities[i]);
            }
        }
        // recovery every second
        InvokeRepeating(nameof(Recover), currentTimer, currentTimer);
    }

    // get percentage
    public float Percent() =>
        (current != 0 && max != 0) ? (float)current / (float)max : 0;

    // recover once a second
    [Server]
    public void Recover()
    {
        if (enabled && health.current > 0)
            current += recoveryRate;
    }

    public void SetTimerPercent(float percent, bool sign, int type)
    {
        CancelInvoke(nameof(Recover));

        if (type == 1)
        {
            if (sign)
            {
                percBoost = percent;
            }
            else
            {
                percBoost = 0;
            }
        }
        else
        {
            percAbility = percent;
        }

        perc = percBoost + percAbility;

        if (sign)
        {
            currentTimer = (timeToRegenerate - ((timeToRegenerate / 100) * perc));
            if (currentTimer < 1.0f) currentTimer = 1.0f;
            InvokeRepeating(nameof(Recover), currentTimer, currentTimer);
        }
        else
        {
            currentTimer = (timeToRegenerate + ((timeToRegenerate / 100) * perc));
            if (currentTimer > timeToRegenerate) currentTimer = timeToRegenerate;
            InvokeRepeating(nameof(Recover), currentTimer, currentTimer);
        }
    }
}
