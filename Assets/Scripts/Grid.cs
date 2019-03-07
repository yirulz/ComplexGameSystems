using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Pieces")]
    public GameObject redPiecePrefab, whitePiecePrefab;
    public Vector3 boardOffset = new Vector3(-4f, 0, -4f);
    public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
    public Piece[,] pieces = new Piece[8, 8];

    [Header("Click & Drag")]
    //For drag and dorp
    private Vector2Int mouseOver; //Grid coordinates the mouse is over
    private Piece selectedPiece; //Piece that has been clicked and dragged

    // Use this for initialization
    void Start()
    {
        GenerateBoard();
    }

    void Update()
    {
        //Update the mouse over information
        MouseOver();
        //If the mouse is pressed
        if(Input.GetMouseButtonDown(0))
        {
            //Try selecting piece
            selectedPiece = SelectedPiece(mouseOver);
        }
        //If there is a selected piece
        if(selectedPiece)
        {
            //Move the piece with mouse
            DragPiece(selectedPiece);
            if(Input.GetMouseButtonUp(0))
            {
                //Move piece to end position
                TryMove(selectedPiece, mouseOver);
                //Let go of the piece
                selectedPiece = null;
            }
        }
    }

    //Converst array coordinates to world position
    Vector3 GetWorldPosition(Vector2Int cell)
    {
        return new Vector3(cell.x, 0, cell.y) + boardOffset + pieceOffset;
    }

    void MovePiece(Piece piece, Vector2Int newCell)
    {
        Vector2Int oldCell = piece.cell;
        //Update array
        pieces[oldCell.x, oldCell.y] = null;
        pieces[newCell.x, newCell.y] = piece;
        //Update data on piece
        piece.oldCell = oldCell;
        piece.cell = newCell;
        //Translate the piece to another location
        piece.transform.localPosition = GetWorldPosition(newCell);
    }

    void GeneratePiece(GameObject prefab, Vector2Int desiredCell)
    {
        //Generate Instance of prefab
        GameObject clone = Instantiate(prefab, transform);
        //Get the piece component
        Piece piece = clone.GetComponent<Piece>();
        //Set the cell data for the first time
        piece.oldCell = desiredCell;
        piece.cell = desiredCell;
        //Reposition clone
        MovePiece(piece, desiredCell);
    }

    void GenerateBoard()
    {
        Vector2Int desiredCell = Vector2Int.zero;
        //Generate White Team
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = y % 2 == 0;
            //Loop through columns
            for (int x = 0; x < 8; x += 2)
            {
                desiredCell.x = oddRow ? x : x + 1;
                desiredCell.y = y;
                //Generate Piece
                GeneratePiece(whitePiecePrefab, desiredCell);
            }
        }
        //Generate Red Team
        for (int y = 5; y < 8; y++)
        {
            bool oddRow = y % 2 == 0;
            //Loop through columns
            for (int x = 0; x < 8; x += 2)
            {
                desiredCell.x = oddRow ? x : x + 1;
                desiredCell.y = y;
                //Generate Piece
                GeneratePiece(redPiecePrefab, desiredCell);
            }
        }
    }

    Piece GetPiece(Vector2Int cell)
    {
        return pieces[cell.x, cell.y];
    }

    //Checks if given coordinates are out of the board range
    bool IsOutOfBounds(Vector2Int cell)
    {
        return cell.x < 0 || cell.x >= 8 || cell.y < 0 || cell.y >= 8;
    }

    Piece SelectedPiece(Vector2Int cell)
    {
        //Check if X and Y is out of bounds
        if (IsOutOfBounds(cell))
        {
            return null;
        }
        //Get the piece at X and Y location
        Piece piece = GetPiece(cell);
        //Check thta it isn't null
        if (piece)
        {
            return piece;
        }

        return null;
    }

    void MouseOver()
    {
        //Perform raycast from mouse position
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //if the ray hits the board
        if (Physics.Raycast(camRay, out hit))
        {
            //Convert mouse coordinates to 2D array coordinates
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            //Default to error (-1)
            mouseOver = new Vector2Int(-1, -1);
        }
    }

    //Drags the selected piece using Raycast location
    void DragPiece(Piece selected)
    {
        //Perform raycast from mouse position
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //Detects mouse ray hit point
        if (Physics.Raycast(camRay, out hit))
        {
            //Updates position of seleted piece to hit point + offset
            selected.transform.position = hit.point + Vector3.up;
        }
    }

    //Checks if selected piece can move to desired cell based on Game Rules
    bool TryMove(Piece selected, Vector2Int desiredCell)
    {
        //Get the selected piece's cell
        Vector2Int startCell = selected.cell;
        
        //Is it not a valid move?
        if(!ValidMove(selected, desiredCell))
        {
            //Move it back to original
            MovePiece(selected, startCell);
            //Exit the function
            return false;
        }

        //Replace end coordinates with our selected piece
        MovePiece(selected, desiredCell);
        //valid move detected
        return true;

    }

    //Check if the start end drag is a valid move
    bool ValidMove(Piece selected, Vector2Int desiredCell)
    {
        //Get direction of movement for some of the next few rules
        Vector2Int direction = selected.cell - desiredCell;

        #region Rule #01 - Is the piece out of bounds?
        if (IsOutOfBounds(desiredCell))
        {
            Debug.Log("<color=red>Invalid - you cannot move outside of the board</color>");
            return false;
        }
        #endregion

        #region Rule #02 - Is the selected cell the same as desired?

        #endregion

        #region Rule #03 - Is the piece at the desired cell not empty?

        #endregion

        #region Rule #04 - Is there any forced moves?

        #endregion

        #region Rule #05 - Is the selecyted cell being dragged two cells over?

        #endregion

        #region Rule #06 - Is the piece not going in a diagonal cell?

        #endregion

        #region Rule #07 - Is the piece moving in the right direction?

        #endregion

        //If all the above rules haven't returned false, it must be a success
        Debug.Log("<color=green>Success - Valid move detected!</color>");
        return true;
    }
}
