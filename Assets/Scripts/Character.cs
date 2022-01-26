using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Action actionUsed;
    public Action damaged;
    public Action requestDeselect;

    public void EnabelAction()
    {
        canAction = true;
    }
    public void DisableAction()
    {
        canAction = false;
    }
    public bool CanSelect()
    {
        return isAlive && canAction;
    }
    public void Damage(int damageAmount)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            isAlive = false;
            currentHealth = 0;
            if (canAction)
            {
                if (selectState != SelectState.deselected)
                {
                    requestDeselect?.Invoke();
                }
                DisableAction();
            }
        }
        damaged?.Invoke();
    }

    public Node startNode;

    [Header("Stats")]
    public int Range = 3;
    public int Health = 10;
    public int Attack = 3;
    public int WolfAttack = 5;
    public BloodTypeData bloodType;

    private Node currentNode = null;
    private bool canAction = false;
    [Header("Debug Info")]
    public int currentHealth = 0;
    public bool isWolf = false;
    public bool isAlive = true;

    private enum SelectState
    {
        deselected,
        Move,
        Attack
    }
    private SelectState selectState;

    private void OnEnable()
    {
        WorldData.instance.cycleManager.AddCharacter(this);
    }
    private void OnDisable()
    {
        WorldData.instance.cycleManager.RemoveCharacter(this);
    }

    private void Awake()
    {
        currentHealth = Health;
    }

    private void Start()
    {
        GoToNode(startNode);
    }
    private List<Node> nodesInRange;
    private List<Character> charactersInRange;
    private void Update()
    {
        if (selectState == SelectState.Move)
        {
            TryMove();
            TryChangeToAttack();
        }
        else if (selectState == SelectState.Attack)
        {
            TryAttack();
            TryChangeToMove();
        }
    }

    private void TryAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100f, WorldData.instance.charactersLayer);
            if (rayHit.collider != null)
            {
                var character = rayHit.collider.GetComponent<Character>();
                if (character != null)
                {
                    if (charactersInRange.Contains(character))
                    {
                        AttackCharacter(character);
                        Deselect();
                        UseAction();
                    }
                }
            }
        }
    }

    public void ResetInfo()
    {
        attackedByWolf = false;
    }

    public void ChooseWolfAttack()
    {
        var humanTarget = WorldData.instance.map.FindHumanToAttack(currentNode, bloodType.bloodTypePreference);

        if (humanTarget != null)
        {
            humanTarget.AttackByWolf(WolfAttack);
        }
        else
        {
            Debug.LogError(this + " can't find target human");
        }

        //Debug.Log(gameObject + " wants to wolfAttack " + humanTarget);
    }

    private bool attackedByWolf = false;
    private void AttackByWolf(int damage)
    {
        attackedByWolf = true;
        Damage(damage);
    }

    private void UseAction()
    {
        canAction = false;
        actionUsed?.Invoke();
    }

    private void AttackCharacter(Character character)
    {
        character.Damage(Attack);
    }

    private void TryChangeToMove()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            selectState = SelectState.Move;
            nodesInRange = WorldData.instance.map.GetNodesInRange(currentNode, Range);
        }
    }


    private void TryChangeToAttack()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            selectState = SelectState.Attack;
            charactersInRange = WorldData.instance.map.GetCharactersInRange(currentNode, Range);
        }
    }

    public void Select()
    {
        selectState = SelectState.Move;
        nodesInRange = WorldData.instance.map.GetNodesInRange(currentNode, Range);
    }
    public void Deselect()
    {
        selectState = SelectState.deselected;
    }

    private void TryMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100f, WorldData.instance.nodesLayer);
            if (rayHit.collider != null)
            {
                var node = rayHit.collider.GetComponent<Node>();
                if (node != null)
                {
                    if (nodesInRange.Contains(node))
                    {
                        GoToNode(node);
                        Deselect();
                        UseAction();
                    }
                }
            }
        }
    }

    private void GoToNode(Node node)
    {
        if (currentNode != null)
        {
            currentNode.ExitNode(this);
        }
        currentNode = node;
        node.EnterNode(this);
        transform.position = node.transform.position;
        nodesInRange = WorldData.instance.map.GetNodesInRange(currentNode, Range);
    }
    private void OnDrawGizmos()
    {
        if (!isAlive)
        {
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.4f, "console.warnicon", true, Color.yellow);
            return;
        }

        if (attackedByWolf)
        {
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.6f + Vector3.left * 0.4f, "console.erroricon.sml", true, Color.red);
        }

        if (selectState == SelectState.deselected)
        {
            if (canAction)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawIcon(transform.position + Vector3.up * 0.6f + Vector3.right * 0.3f, "lightMeter/orangeLight", true, Color.yellow);
                //Gizmos.DrawWireSphere(transform.position+Vector3.up*0.5f+ Vector3.right * 0.2f, 0.3f);
            }
        }
        if (selectState == SelectState.Move)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.9f);

            if (nodesInRange != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < nodesInRange.Count; i++)
                {
                    Gizmos.DrawWireSphere(nodesInRange[i].transform.position, 1.1f);
                }
            }
        }
        if (selectState == SelectState.Attack)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.9f);

            if (charactersInRange != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < charactersInRange.Count; i++)
                {
                    Gizmos.DrawWireSphere(charactersInRange[i].transform.position, 1f);
                }
            }
        }
    }
}
