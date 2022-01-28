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
            spriteRenderer.enabled = false;
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

    [Header("Move Animation")]
    public float nodeHopDuration = 0.4f;
    public float nodeHopePauseDuration = 0.1f;

    [Header("Attack Animation")]
    public float fadeAwayDuration = 0.5f;
    public float fadeStayDuration = 0.05f;
    public float attackEffectDuration = 0.3f;
    public float attackStayDuration = 0.05f;
    public float fadeBackDuration = 0.5f;
    public ParticleSystem getHitEffect;


    [Header("Debug Info")]
    public int currentHealth = 0;
    public bool isWolf = false;
    public bool isAlive = true;

    private Node currentNode = null;
    private bool canAction = false;
    private SpriteRenderer spriteRenderer;

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
        //WorldData.instance.cycleManager.RemoveCharacter(this);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = Health;
    }

    private void Start()
    {
        FinalizeGoToNode(startNode);
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
                        StartCoroutine(AttackSequence(character));
                    }
                }
            }
        }
    }

    public void Remove()
    {
        currentNode.ExitNode(this);
        isRemoved = true;
        gameObject.SetActive(false);
    }

    IEnumerator AttackSequence(Character character)
    {
        WorldData.instance.isActionPaused = true;
        Deselect();
        UseAction();

        float timer = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = spriteRenderer.color;
        endColor.a = 0f;
        while (timer < fadeAwayDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / fadeAwayDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        spriteRenderer.color = endColor;
        yield return new WaitForSeconds(fadeStayDuration);
        AttackCharacter(character);
        yield return new WaitForSeconds(attackEffectDuration);
        yield return new WaitForSeconds(attackStayDuration);
        startColor.a = 0f;
        endColor.a = 1f;
        timer = 0f;
        while (timer < fadeBackDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / fadeBackDuration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        spriteRenderer.color = endColor;


        WorldData.instance.isActionPaused = false;
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
            //Debug.LogError(this + " can't find target human");
        }

        //Debug.Log(gameObject + " wants to wolfAttack " + humanTarget);
    }

    private bool attackedByWolf = false;
    internal bool isRemoved;

    private void AttackByWolf(int damage)
    {
        attackedByWolf = true;
        Damage(damage);
        if (!isAlive)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void UseAction()
    {
        canAction = false;
        actionUsed?.Invoke();
    }

    private void AttackCharacter(Character character)
    {
        character.getHitEffect.Play();
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
                    if (node.IsEmpty() && nodesInRange.Contains(node))
                    {
                        StartCoroutine(GoToNodeSequence(node));
                    }
                }
            }
        }
    }
    IEnumerator GoToNodeSequence(Node node)
    {
        WorldData.instance.isActionPaused = true;
        Deselect();
        UseAction();
        List<Node> movePath = WorldData.instance.map.GetPathToNode(currentNode, node);

        for (int i = 0; i < movePath.Count - 1; i++)
        {
            Vector3 startPosition = movePath[i].transform.position;
            Vector3 endPosition = movePath[i + 1].transform.position;
            float timer = 0f;
            while (timer < nodeHopDuration)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, timer / nodeHopDuration);
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            transform.position = endPosition;
            yield return new WaitForSeconds(nodeHopePauseDuration);
        }

        FinalizeGoToNode(node);
        WorldData.instance.isActionPaused = false;
    }

    private void FinalizeGoToNode(Node node)
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
                    if (nodesInRange[i].IsEmpty())
                    {
                        Gizmos.DrawWireSphere(nodesInRange[i].transform.position, 1.1f);
                    }
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
