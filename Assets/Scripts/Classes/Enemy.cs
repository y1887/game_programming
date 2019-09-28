using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTAI;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    Root aiRoot = BT.Root();
    PlayerController player;
    Transform playerTransform;
    Transform enemyTransform;
    AIDestinationSetter destination;

    private void OnEnable()
    {
        aiRoot.OpenBranch(
                BT.Sequence().OpenBranch(
                    BT.Call(StopFollow)
                 ),
                BT.While(InAttackRange).OpenBranch(
                    BT.Call(Follow)
                )
        );
    }

    private void Follow()
    {
        destination.enabled = true;
        Debug.Log("in");
    }

    private void StopFollow()
    {
        destination.enabled = false;
    }

    private bool InAttackRange()
    {
        bool isSuccess = ((playerTransform.position - enemyTransform.position).magnitude <= 7.5f ? true : false);
        return isSuccess;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerTransform = player.gameObject.transform.Find("Player");
        enemyTransform = this.transform.Find("Enemy");
        destination = enemyTransform.gameObject.GetComponent<AIDestinationSetter>();
        destination.target = playerTransform;
    }

    private void Update()
    {
        aiRoot.Tick();
    }

    public Root GetAIRoot()
    {
        return aiRoot;
    }
}
