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
        EnterDeslectState();
    }
    public void DisableAction()
    {
        canAction = false;
        EnterDeslectState();
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
            //spriteRenderer.enabled = false;
            isAlive = false;
            currentHealth = 0;
            spriteRenderer.sprite = deadSprite;
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
    public string characterName = "Name";

    public UnityEngine.UI.Text nameText;

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

    [Header("Wolf Attack Animation")]
    public ParticleSystem getHitByWolfEffect;

    [Header("Other Animation")]
    public Animator targetAnim;
    public Animator canActAnim;
    public ParticleSystem bleedEffect;
    public Material selectedMaterial;

    [Header("Other stuff")]

    public SpriteRenderer spriteRenderer;
    public Sprite deadSprite;


    [Header("Debug Info")]
    public int currentHealth = 0;
    public bool isWolf = false;
    public bool isAlive = true;

    private Node currentNode = null;
    private bool canAction = false;

    private enum SelectState
    {
        deselected,
        selected
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
    private Material defaultMaterial;
    private void Awake()
    {
        defaultMaterial = spriteRenderer.material;
        currentHealth = Health;
    }

    private void Start()
    {
        nameText.text = characterName;
        FinalizeGoToNode(startNode);
    }
    private List<Node> nodesInRange;
    private List<Character> charactersInRange;
    private void Update()
    {
        if (WorldData.instance.isActionPaused)
        {
            return;
        }
        if (selectState == SelectState.selected)
        {

            if (!TryAttack())
            {
                TryMove();
            }
        }
    }

    private bool TryAttack()
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
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void WolfAttackEffect()
    {
        getHitByWolfEffect.Play();
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
        UseAction();
        Deselect();

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
        WorldData.instance.audioSource.PlayOneShot(WorldData.instance.attackSound, 0.5f);
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
        EventSystem.instance.QueueEvent("CharAtk");
    }

    public void ResetInfo()
    {
        attackedByWolf = false;
        bleedEffect.Stop();
    }

    public Character ChooseWolfAttack()
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

        return humanTarget;

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
            bleedEffect.Stop();
            //spriteRenderer.enabled = false;
        }
        else
        {
            bleedEffect.Play();
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnHighLightAttack();
            EnterSelectState();
        }
    }

    private void EnterSelectState()
    {
        selectState = SelectState.selected;
        HighLightMove();
        HighLightAttack();
    }

    private void HighLightMove()
    {
        nodesInRange = WorldData.instance.map.GetNodesInRange(currentNode, Range);
        if (nodesInRange != null)
        {
            for (int i = 0; i < nodesInRange.Count; i++)
            {
                if (nodesInRange[i].IsEmpty())
                {
                    nodesInRange[i].Focus();
                }
            }
        }
    }
    private void ExitSelectState()
    {
        UnHighLightMove();
        UnHighLightAttack();
    }
    private void UnHighLightMove()
    {
        if (nodesInRange != null)
        {
            for (int i = 0; i < nodesInRange.Count; i++)
            {
                if (nodesInRange[i].IsEmpty())
                {
                    nodesInRange[i].UnFocus();
                }
            }
        }
    }
    private void HighLightAttack()
    {
        charactersInRange = WorldData.instance.map.GetCharactersInRange(currentNode, Range);
        if (charactersInRange != null)
        {
            for (int i = 0; i < charactersInRange.Count; i++)
            {
                charactersInRange[i].Focus();
            }
        }
    }
    private void UnHighLightAttack()
    {
        if (charactersInRange != null)
        {
            for (int i = 0; i < charactersInRange.Count; i++)
            {
                charactersInRange[i].UnFocus();
            }
        }
    }

    private void TryChangeToAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExitSelectState();
            HighLightAttack();
        }
    }

    public void Select()
    {
        ExitDeslectState();
        EnterSelectState();
    }

    private void EnterDeslectState()
    {
        spriteRenderer.material = defaultMaterial;
        if (canAction)
        {
            canActAnim.Play("active");
        }
        else
        {
            canActAnim.Play("default");
        }
        selectState = SelectState.deselected;
    }
    private void ExitDeslectState()
    {
        canActAnim.Play("default");
        spriteRenderer.material = selectedMaterial;
    }

    public void Deselect()
    {
        if (selectState == SelectState.selected)
        {
            ExitSelectState();
        }
        EnterDeslectState();
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
        UseAction();
        Deselect();
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


    public void Focus()
    {
        targetAnim.Play("focus");
    }
    public void UnFocus()
    {
        targetAnim.Play("default");
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
        if (selectState == SelectState.selected)
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
        if (selectState == SelectState.selected)
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
