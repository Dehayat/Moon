using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Character selectedCharacter = null;
    private Character hoveredCharacter = null;

    private void Update()
    {
        if (WorldData.instance.isActionPaused)
        {
            return;
        }
        if (selectedCharacter == null && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100f, WorldData.instance.charactersLayer);
            if (rayHit.collider != null)
            {
                var character = rayHit.collider.GetComponent<Character>();
                if (character != null && character.CanSelect())
                {
                    character.Select();
                    selectedCharacter = character;
                    selectedCharacter.actionUsed += SelectedCharacterAction;
                    selectedCharacter.requestDeselect += DeslectCurrentCharacter;
                }
            }
        }
        if (selectedCharacter != null && Input.GetMouseButtonDown(1))
        {
            DeslectCurrentCharacter();
        }
        if (hoveredCharacter == null)
        {
            RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100f, WorldData.instance.charactersLayer);
            if (rayHit.collider != null)
            {
                var character = rayHit.collider.GetComponent<Character>();
                if (character != null)
                {
                    hoveredCharacter = character;
                    hoveredCharacter.GetComponent<CharacterUI>().ShowStats();
                }
            }
        }
        else
        {
            bool leaveHover = false;
            RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100f, WorldData.instance.charactersLayer);
            if (rayHit.collider != null)
            {
                var character = rayHit.collider.GetComponent<Character>();
                if (character == null && hoveredCharacter != character)
                {
                    leaveHover = true;
                    hoveredCharacter = character;
                }
            }
            else
            {
                leaveHover = true;
            }
            if (leaveHover)
            {
                hoveredCharacter.GetComponent<CharacterUI>().HideStats();
                hoveredCharacter = null;
            }
        }
    }

    private void SelectedCharacterAction()
    {
        DeslectCurrentCharacter();
    }

    private void DeslectCurrentCharacter()
    {
        selectedCharacter.actionUsed -= SelectedCharacterAction;
        selectedCharacter.requestDeselect -= DeslectCurrentCharacter;
        selectedCharacter.Deselect();
        selectedCharacter = null;
    }
}
