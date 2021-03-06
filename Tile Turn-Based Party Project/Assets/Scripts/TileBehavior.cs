﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class TileBehavior : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {
    #region Selection Variables
    static List<GameObject> highlightedTiles = new List<GameObject>();
    static List<GameObject> moveableTiles = new List<GameObject>();
    public static GameObject selectedUnit;
    public static GameObject selectedTile;
    protected static string selectionState;
    #endregion

    #region UI Variables    
    public static float tileDim;
    public static Button attackButton;
    public static Button abilityButton;
    public static Button useRollButton;
    public static GameObject diceAttack;
    #endregion

    #region Instance Variables
    bool highlighted;
    public GameObject myUnit;
    public int movementCost = 1;
    public int xPosition;
    public int yPosition;
    public string tileType;
    public int playerside;
    public bool unitAttacked;

    [SerializeField]
    GameObject tileHighlighter;
    Animator tileHighlighterAnimator;
    public float playerOpacity;
    public float enemyOpacity;

    float stepDuration = 0.1f;
    #endregion

    #region Initialization
    public void Awake() {
        tileHighlighter.transform.position = transform.position;
        tileHighlighterAnimator = tileHighlighter.GetComponent<Animator>();
        setHighlightOpacity(playerOpacity);
    }
    #endregion

    #region Opacity
    private void setHighlightOpacity(float opacity) {
        Color c = tileHighlighter.GetComponent<Renderer>().material.color;
        c.a = opacity;
        tileHighlighter.GetComponent<Renderer>().material.color = c;
    }
    #endregion

    #region Unit Functions
    public void PlaceUnit(GameObject unit) {
        unit.GetComponent<Character>().SetAnimVar();
        myUnit = unit;
        myUnit.transform.position = transform.position - new Vector3(0, 0, 0);
        myUnit.GetComponent<Character>().RecalculateDepth();
        myUnit.GetComponent<Character>().SetOccupiedTile(gameObject);
    }

    public bool HasUnit() {
        return myUnit != null;
    }

    public GameObject GetUnit() {
        return myUnit;
    }

    public void ClearUnit() {
        myUnit = null;
    }
    #endregion

    #region Variable Functions
    public static string GetSelectionState() {
        return selectionState;
    }

    public static void SetSelectionState(string s) {
        selectionState = s;
    }

    public int GetXPosition() {
        return xPosition;
    }

    public int GetYPosition() {
        return yPosition;
    }

    public void SetSelectedTile(GameObject unit) {
        selectedTile = unit;
    }
    #endregion

    #region Highlight Functions
    public void HighlightCanMove(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", true);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", enemySelect);
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity);
        }
        else {
            setHighlightOpacity(playerOpacity);
        }
        highlighted = true;
    }

    public void HighlightCanMoveCovered(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", true);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", enemySelect);
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity / 2f);
        }
        else {
            setHighlightOpacity(playerOpacity / 2f);
        }
        highlighted = true;
    }

    public void HighlightCanAttack(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", true);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        highlighted = true;
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity + 0.1f);
        }
        else {
            setHighlightOpacity(playerOpacity + 0.1f);
        }
    }

    public void HighlightCanAttackEmpty(bool enemySelect = false) {
        tileHighlighterAnimator.SetBool("canAttack", true);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        setHighlightOpacity(playerOpacity / 2f);
        highlighted = true;
        if (enemySelect) {
            setHighlightOpacity(enemyOpacity / 2f);
        }
        else {
            setHighlightOpacity(playerOpacity / 2f);
        }
    }

    public void HighlightCanSpawn() {
        tileHighlighterAnimator.SetBool("canAttack", true);
        tileHighlighterAnimator.SetBool("canMove", true);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        highlighted = true;
        setHighlightOpacity(playerOpacity);
    }

    public void HighlightSelected() {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", true);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        setHighlightOpacity(playerOpacity);
    }

    public void Dehighlight() {
        tileHighlighterAnimator.SetBool("canAttack", false);
        tileHighlighterAnimator.SetBool("canMove", false);
        tileHighlighterAnimator.SetBool("selected", false);
        tileHighlighterAnimator.SetBool("enemySelected", false);
        highlighted = false;
        setHighlightOpacity(playerOpacity);
    }
    #endregion

    #region Highlight Valid Tiles Functions
    void HighlightMoveableTiles(int moveEnergy) {
        // Don't do anything if you've run out of energy.
        if (moveEnergy < 0 || tileType == "wall" || tileType == "nexus") {
            return;
        }

        //Otherwise, hightlight yourself...
        if (myUnit == null) {
            HighlightCanMove();
        }
        else if (!myUnit.Equals(selectedUnit)) {
            HighlightCanMoveCovered();
        }
        highlightedTiles.Add(gameObject);

        //...and all adjacent tiles (if they don't contain enemy units).

        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f);
            if (hit.collider != null) {
                TileBehavior otherTile = hit.transform.GetComponent<TileBehavior>();
                if (otherTile.myUnit == null || otherTile.myUnit.GetComponent<Character>().player == selectedUnit.GetComponent<Character>().player) {
                    hit.transform.GetComponent<TileBehavior>().HighlightMoveableTiles(moveEnergy - movementCost);
                }
            }
        }
    }

    void HighlightAttackableTiles(GameObject unit, bool enemySelect = false) {
        List<int[,]> attackRanges = unit.GetComponent<Character>().GetAttackRange();
        float tileSize = GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        foreach (int[,] attackRange in attackRanges) {
            for (int i = 0; i < attackRange.GetLength(0); i++) {
                Vector3 xOffSet = new Vector3(tileSize, 0.0f, 0.0f) * attackRange[i, 0];
                Vector3 yOffSet = new Vector3(0.0f, tileSize, 0.0f) * attackRange[i, 1];
                Vector2 tilePosition = transform.position + xOffSet + yOffSet;
                Collider2D hit = Physics2D.OverlapPoint(tilePosition);

                // If there exists a tile in that direction...
                if (hit != null) {
                    // Highlight that tile.
                    highlightedTiles.Add(hit.gameObject);

                    // If that tile has a unit...
                    if (hit.gameObject.GetComponent<TileBehavior>().HasUnit()) {
                        // And the unit belongs to the enemy team...
                        GameObject hitUnit = hit.gameObject.GetComponent<TileBehavior>().GetUnit();
                        if (hitUnit.GetComponent<Character>().GetPlayer() != selectedUnit.GetComponent<Character>().GetPlayer()) {
                            // Stop. Go no further in this direction.
                            hit.gameObject.GetComponent<TileBehavior>().HighlightCanAttack(enemySelect);
                            break;

                        }
                        // And the unit belongs to the player team...
                        else {
                            // Keep going.
                            hit.gameObject.GetComponent<TileBehavior>().HighlightCanAttackEmpty(enemySelect);
                        }
                    }
                    // If that tile is a wall...
                    else if (hit.gameObject.GetComponent<TileBehavior>().tileType == "wall") {
                        // Stop. Do not pass Go. Do not collect 200 dollars.
                        break;
                    }
                    else {
                        // Keep going.
                        hit.gameObject.GetComponent<TileBehavior>().HighlightCanAttackEmpty(enemySelect);
                    }
                }
            }
        }
    }

    void HighlightRange(int moveEnergy, int attackRange) {
        HighlightRangeMovement(moveEnergy);
        foreach (GameObject tileObject in moveableTiles) {
            TileBehavior tile = tileObject.GetComponent<TileBehavior>();
            tile.HighlightRangeAttack(attackRange);
        }
    }

    void HighlightRangeMovement(int moveEnergy) {
        // Don't do anything if you've run out of energy.
        if (moveEnergy < 0 || tileType == "wall" || tileType == "nexus") {
            return;
        }

        //Otherwise, hightlight yourself...
        if (myUnit == null || myUnit.Equals(selectedUnit)) {
            HighlightCanMove();
            moveableTiles.Add(gameObject);
        }
        else {
            HighlightCanMoveCovered();
        }
        highlightedTiles.Add(gameObject);

        //...and all adjacent tiles (if they don't contain enemy units).

        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f);
            if (hit.collider != null) {
                TileBehavior otherTile = hit.transform.GetComponent<TileBehavior>();
                if (otherTile.myUnit == null || otherTile.myUnit.GetComponent<Character>().player == selectedUnit.GetComponent<Character>().player) {
                    hit.transform.GetComponent<TileBehavior>().HighlightRangeMovement(moveEnergy - movementCost);
                }
            }
        }
    }

    void HighlightRangeAttack(int attackRange) {
        if (attackRange < 0) {
            return;
        }

        if (!moveableTiles.Contains(gameObject)) {
            HighlightCanAttack();
            highlightedTiles.Add(gameObject);
        }

        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f);
            if (hit.collider != null) {
                TileBehavior otherTile = hit.transform.GetComponent<TileBehavior>();
                if (!moveableTiles.Contains(otherTile.gameObject) || !highlightedTiles.Contains(otherTile.gameObject)) {
                    hit.transform.GetComponent<TileBehavior>().HighlightRangeAttack(attackRange - 1);
                }
            }
        }
    }

    void HighlightSummonableTiles() {
        if (highlightedTiles.Contains(gameObject)) {
            return;
        }
        if (tileType != "nexus" && myUnit == null) {
            //Change to something else once we have the code/art
            HighlightCanMove();
        }
        highlightedTiles.Add(gameObject);
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f);
            if (hit.collider != null) {
                TileBehavior otherTile = hit.transform.GetComponent<TileBehavior>();
                if (otherTile.tileType == "nexus" && tileType == "nexus" || tileType == "nexus") {
                    hit.transform.GetComponent<TileBehavior>().HighlightSummonableTiles();
                }
            }
        }
    }
    #endregion

    #region Selection Functions
    public virtual void OnPointerClick(PointerEventData data) {
        //Condition where pointer click fails
        if (GameManager.actionInProcess) {
            return;
        }
        // If nothing is currently selected...
        if (selectionState == null) {
            // and if it was a right click...
            if (data.button == PointerEventData.InputButton.Right) {
                Debug.Log($"This tile is a {tileType}");
            }
            // and if the tile is your Nexus
            else if (tileType == "nexus" && playerside == GameManager.currentPlayer) {
                SelectionStateToSummon();
            }
            // else if this tile has a unit on it...
            else if (myUnit != null) {
                // and the unit's player is equal to to the current player...
                if (GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer()) && myUnit.GetComponent<Character>().GetCanMove() == true) {
                    // select that unit/tile and highlight the tiles that the unit can move to (if it can move).
                    SelectionStateToMove();
                }
            }

            // and this tile does not have a unit on it...
            else {
                // do nothing.
            }
        }
        // If something is currently selected...
        else {
            // and selection state is move...
            if (selectionState.Equals("move")) {
                // and if it was a right click...
                if (data.button == PointerEventData.InputButton.Right) {
                    SelectionStateToNull();
                    return;
                }
                // and the selected character can move onto this tile...
                if (highlighted && myUnit == null) {
                    // move that character onto this tile and dehighlight everything.
                    selectedTile.GetComponent<TileBehavior>().ClearUnit();
                    // attack here
                    StartCoroutine(MoveUnitToThisTile(selectedUnit, selectedTile));
                }

                // and you are the selectedTile...
                else if (selectedTile.Equals(gameObject)) {
                    // and the selected unit can attack...
                    if (selectedUnit.GetComponent<Character>().GetCanAttack() == true) {
                        SelectionStateToAttack();
                    }
                    // and the selected unit can't attack...
                    else {
                        SelectionStateToNull();
                    }
                }
                // and the selected character cannot move onto this tile...
                else {
                    // Dehighlight everything.
                    SelectionStateToNull();
                }
            }
            // and selection state is attack...
            else if (selectionState.Equals("attack")) {
                // and the selected character can attack there...
                if (highlighted && myUnit != null && myUnit.GetComponent<Character>().GetPlayer() != selectedUnit.GetComponent<Character>().GetPlayer()) {
                    // (Attack), and deselect everything.

                    //ADD CODE FOR ATTACK
                    unitAttacked = true;
                    selectedUnit.GetComponent<Character>().SetCanMove(false);
                    selectedUnit.GetComponent<Character>().SetCanAttack(false);
                    SelectionStateToNull();

                }
                // and if the tile is the enemy's Nexus
                else if (highlighted && tileType == "nexus" && playerside != GameManager.currentPlayer)
                {
                    GameManager gameManager = GameManager.GetSingleton();
                    gameManager.AddNexusObjectivePoints();
                    selectedUnit.GetComponent<Character>().SetCanMove(false);
                    selectedUnit.GetComponent<Character>().SetCanAttack(false);
                    SelectionStateToNull();
                }
                // and you are the selectedTile...
                else if (selectedTile.Equals(gameObject)) {
                    selectedUnit.GetComponent<Character>().SetCanMove(false);
                    selectedUnit.GetComponent<Character>().SetCanAttack(false);
                    SelectionStateToNull();
                }
            }
            // and selection state is summoning...
            else if (selectionState == "summoning") {
                GameManager gameManager = GameManager.GetSingleton();
                // and ifit was a right click...
                if (data.button == PointerEventData.InputButton.Right) {
                    gameManager.ExitSummonPanel();
                    SelectionStateToNull();
                }
                // and if it was a left click...
                else if (highlighted && tileType != "nexus") {
                    gameManager.PlaceCharacterOnTile(GameManager.GetSingleton().boughtUnit, xPosition, yPosition, GameManager.currentPlayer);
                    gameManager.SubtractCost();
                    SelectionStateToNull();
                }
            }
        }
    }

    public virtual void OnPointerDown(PointerEventData data) {

        if (myUnit != null && data.button == PointerEventData.InputButton.Left) {
            if (selectionState == null) {
                if (!GameManager.currentPlayer.Equals(myUnit.GetComponent<Character>().GetPlayer()) || (!myUnit.GetComponent<Character>().GetCanMove() && !myUnit.GetComponent<Character>().GetCanAttack())) {
                    //Highlight
                    SelectionStateToCheckRange();
                }
            }
        }
    }

    public virtual void OnPointerUp(PointerEventData data) {
        // Needs to be changed
        if (selectionState == "checkRange") {
            Unhighlight();
            selectionState = null;
        }
    }
    #endregion

    #region Selection State To Functions
    public void SelectionStateToNull() {
        // Deselect everything else
        Deselect();
    }

    public void SelectionStateToSummon() {
        // Deselect everything else
        Deselect();

        GameManager gameManager = GameManager.GetSingleton();
        gameManager.endButton.gameObject.SetActive(false);

        // Switch selection state to summon
        selectionState = "summoning";

        // Select this tile
        selectedTile = gameObject;
        HighlightSummonableTiles();
    }

    public void SelectionStateToMove() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "move";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightSelected();

        // Open the Character UI
        GameManager.GetSingleton().ShowCharacterUI(selectedUnit);

        // Highlight moveable tiles
        if (selectedUnit.GetComponent<Character>().GetCanMove()) {
            HighlightMoveableTiles(selectedUnit.GetComponent<Character>().GetMovement());
        }
    }

    public void SelectionStateToAttack() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "attack";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightSelected();

        // Open the Character UI
        GameManager.GetSingleton().ShowCharacterUI(selectedUnit);

        //Highlight attackable tiles
        selectedTile.transform.GetComponent<TileBehavior>().HighlightAttackableTiles(selectedUnit);
    }

    public void SelectionStateToCheckRange() {
        // Deselect everything else
        Deselect();

        // Switch selection state to move
        selectionState = "checkRange";

        // Select this tile and its unit
        selectedUnit = myUnit;
        selectedTile = gameObject;
        HighlightCanMove();

        //Highlight range
        HighlightRange(myUnit.GetComponent<Character>().movement, myUnit.GetComponent<Character>().maxrange);
    }
    #endregion

    #region Deselect
    public static void Deselect() {
        Unhighlight();

        // Reset all selection variables
        selectedUnit = null;
        if (selectedTile != null) {
            selectedTile.GetComponent<TileBehavior>().Dehighlight();
        }
        selectedTile = null;
        selectionState = null;

        //Get rid of all the UI
        GameManager.GetSingleton().ClearUI();
    }

    public static void Unhighlight() {
        // Dehighlight everything
        foreach (GameObject highlightedTile in highlightedTiles) {
            highlightedTile.transform.GetComponent<TileBehavior>().Dehighlight();
        }
        // Clear the list of highlighted tiles
        highlightedTiles.Clear();
        // Clear the list of moveable tiles
        moveableTiles.Clear();
    }
    #endregion

    #region Movement Functions
    IEnumerator MoveUnitToThisTile(GameObject unit, GameObject originalTile) {
        // Action in process!
        GameManager.actionInProcess = true;

        // Deselect everything
        Deselect();
        float tileSize = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        tileDim = tileSize;

        // Calculate the steps you need to take
        int unitPlayer = unit.GetComponent<Character>().player;
        List<string> movementSteps = CalculateMovement(new List<string>(), originalTile, gameObject, unit.GetComponent<Character>().GetMovement(), unitPlayer);

        //Take those steps!
        foreach (string step in movementSteps) {
            if (step.Equals("up")) {
                unit.transform.position += new Vector3(0, tileSize);
            }
            else if (step.Equals("right")) {
                unit.transform.position += new Vector3(tileSize, 0);
            }
            else if (step.Equals("down")) {
                unit.transform.position -= new Vector3(0, tileSize);
            }
            else if (step.Equals("left")) {
                unit.transform.position -= new Vector3(tileSize, 0);
            }
            unit.GetComponent<Character>().RecalculateDepth();
            unit.GetComponent<Character>().StartBounceAnimation();
            yield return new WaitForSeconds(stepDuration);
        }
        PlaceUnit(unit);
        unit.GetComponent<Character>().SetCanMove(false);
        int total = 0;
        total += Mathf.Abs(xPosition - originalTile.GetComponent<TileBehavior>().xPosition) + Mathf.Abs(yPosition - originalTile.GetComponent<TileBehavior>().yPosition);
        unit.GetComponent<TestClass>().distmoved = total;

        // Action over!
        GameManager.actionInProcess = false;
        gameObject.GetComponent<TileBehavior>().SelectionStateToAttack();
        
    }

    // Recursive helper function to calculate the steps to take to get from tile A to tile B
    public static List<string> CalculateMovement(List<string> movement, GameObject currentTile, GameObject goalTile, int moveEnergy, int unitPlayer) {
        // If you're there, return the movement path.
        if (currentTile.Equals(goalTile)) {
            return movement;
        }

        // If you're out of energy, it's an invalid path.
        if (moveEnergy < 0) {
            return null;
        }

        List<List<string>> validPaths = new List<List<string>>();
        // Check for all adjacent tiles:
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (Vector2 direction in directions) {
            RaycastHit2D hit = Physics2D.Raycast(currentTile.transform.position, direction, 1.0f);
            if (hit.collider != null && hit.transform.GetComponent<TileBehavior>().tileType != "wall") {
                GameObject otherTileUnit = hit.transform.GetComponent<TileBehavior>().myUnit;
                if (otherTileUnit == null || otherTileUnit.GetComponent<Character>().player == unitPlayer) {
                    List<string> newMovement = new List<string>(movement.ToArray());
                    if (direction.Equals(Vector2.right)) {
                        newMovement.Add("right");
                    }
                    if (direction.Equals(Vector2.left)) {
                        newMovement.Add("left");
                    }
                    if (direction.Equals(Vector2.up)) {
                        newMovement.Add("up");
                    }
                    if (direction.Equals(Vector2.down)) {
                        newMovement.Add("down");
                    }
                    List<string> path = CalculateMovement(newMovement, hit.collider.gameObject, goalTile, moveEnergy - currentTile.GetComponent<TileBehavior>().movementCost, unitPlayer);
                    if (path != null) {
                        validPaths.Add(path);
                    }
                }
            }
        }

        // Return the shortest valid path
        if (validPaths.Count != 0) {
            List<string> shortestList = validPaths[0];
            foreach (List<string> path in validPaths) {
                if (path.Count < shortestList.Count) {
                    shortestList = path;
                }
            }
            return shortestList;
        }

        // If there are no valid paths from this point, return null
        return null;
    }
    #endregion
}
