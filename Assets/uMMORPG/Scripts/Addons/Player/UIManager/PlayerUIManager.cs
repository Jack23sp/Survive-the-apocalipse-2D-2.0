using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public NavMeshAgent2D agent2D;

    public void Awake()
    {
        agent2D = GetComponent<NavMeshAgent2D>();
    }

    public float GetSpeed()
    {
        return agent2D.speed;
    }
}
