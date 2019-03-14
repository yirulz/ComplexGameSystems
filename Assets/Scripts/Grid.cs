using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    using ForcedMoves = Dictionary<Piece, List<Vector2Int>>;

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

        private  ForcedMoves forcedMoves = new ForcedMoves();
        // Use this for initialization
        void Start()
        {
            GenerateBoard();
        }

        void Update()
        {
            DetectForcedMoves();
            //Update the mouse over information
            MouseOver();
            //If the mouse is pressed
            if (Input.GetMouseButtonDown(0))
            {
                //Try selecting piece
                selectedPiece = SelectedPiece(mouseOver);
            }
            //If there is a selected piece
            if (selectedPiece)
            {
                //Move the piece with mouse
                DragPiece(selectedPiece);
                if (Input.GetMouseButtonUp(0))
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
            //Is desired cell out of bounds?
            if (IsOutOfBounds(desiredCell))
            {
                //Move it back to original
                MovePiece(selected, startCell);
                Debug.Log("<color=red>Invalid - You cannot move outside of the board.</color>");
                return false;
            }
            //Is it not a valid move?
            if (!ValidMove(selected, desiredCell))
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
            if (selected.cell == desiredCell)
            {
                Debug.Log("<color=red>Invalid - Putting pieces back don't count as a valid move </color>");
                return false;
            }
            #endregion

            #region Rule #03 - Is the piece at the desired cell not empty?
            if (GetPiece(desiredCell))
            {
                Debug.Log("<color=red>Invalid - You can't go on top of other piece</color>");
                return false;
            }
            #endregion

            #region Rule #04 - Is there any forced moves?
            //Is there any forced moves?
            if(HasForcedMoves(selected))
            {
                //If it is not a forced move
                if(!IsforcedMove(selected, desiredCell))
                {
                    Debug.Log("<color=red>Invalid - You have to use forced moves!</color>");
                    return false;
                }
            }
            #endregion

            #region Rule #05 - Is the selecyted cell being dragged two cells over?
            //Is the piece moved two spaces?
            if (direction.magnitude > 2)
            {
                if(forcedMoves.Count == 0)
                {
                    Debug.Log("<color=red>Invalid - You can only move two spaces if there are forced moves</color>");
                    return false;
                }
            }
            #endregion

            #region Rule #06 - Is the piece not going in a diagonal cell?
            //Is the player not moving diagonally?
            if(Mathf.Abs(direction.x) != Mathf.Abs(direction.y))
            {
                Debug.Log("<color=red>Invalid - You have ot be moving diagonally</color>");
                return false;
            }
            #endregion

            #region Rule #07 - Is the piece moving in the right direction?
            //Is the selected piece not a king?
            if(!selectedPiece.isKing)
            {
                //Is the selected piece white?
                if(selectedPiece.isWhite)
                {
                    //Is it moving down?
                    if(direction.y > 0)
                    {
                        Debug.Log("<color=red>Invalid - Can't move a white piece backwards</color>");
                        return false;
                    }
                }
                //Is the selected piece red?
                else
                {
                    //Is it moving up?
                    if(direction.y < 0)
                    {
                        Debug.Log("<color=red>Invalid - Can't move a red piece backwards</color>");
                        return false;
                    }
                }
            }
            #endregion

            //If all the above rules haven't returned false, it must be a success
            Debug.Log("<color=green>Success - Valid move detected!</color>");
            return true;
        }
        //Detects if there is a forced move for a given piece
        void CheckForcedMove(Piece piece)
        {
            //Get cell location for piece
            Vector2Int cell = piece.cell;

            //Loop through adjacent cells of cell
            for (int x = 0; x < 1; x += 2)
            {
                for (int y = 0; y < 1; y += 2)
                {
                    //Create offset cell from index
                    Vector2Int offset = new Vector2Int(x, y);
                    //Creating a new X from piece coordinates using offset
                    Vector2Int desiredCell = cell + offset;

                    #region Check #01 - Correct Direction?

                    //Is the piece not king?
                    if (!piece.isKing)
                    {
                        //Is the piece white?
                        if (piece.isWhite)
                        {
                            //Is the piece moving backwards?
                            if (desiredCell.y < cell.y)
                            {
                                //Invalid - Check the next one
                                continue;
                            }

                        }
                    }
                    //Is the piece red?
                    else
                    {
                        //Is the piece moving backwards?
                        if (desiredCell.y > cell.y)
                        {
                            //Invalid - Check next one
                            continue;
                        }
                    }
                    #endregion

                    #region Check #02 - Is the adjacent cell out of bounds?
                    //Is desired cell out of bounds?
                    if (IsOutOfBounds(desiredCell))
                    {
                        //Invalid - Check next one
                        continue;
                    }

                    #endregion

                    // Try getting the piece at coordinates
                    Piece detectedPiece = GetPiece(desiredCell);

                    #region Check #03 - Is the desired cell empty?
                    //Is there a detected piece?
                    if (detectedPiece == null)
                    {
                        //Invalid - Check next one
                        continue;
                    }

                    #endregion

                    #region Check #04 - Is the detected piece the same color?
                    //Is the detected piece the same color
                    if (detectedPiece.isWhite == piece.isWhite)
                    {
                        //Invalid - Check the next one
                        continue;
                    }
                    #endregion

                    //Try getting the diagonal cell next to the detected piece
                    Vector2Int jumpCell = cell + (offset * 2);

                    #region Check #05 - Is the jump cell out of bounds?
                    //Is the detination cell out of bounds?
                    if (IsOutOfBounds(jumpCell))
                    {
                        //Invalid - Check the next one
                        continue;
                    }
                    #endregion

                    #region Check #06 - Is there a piece at the jump cell?
                    //Get piece next to the one we want to jump
                    detectedPiece = GetPiece(jumpCell);
                    //Is there a piece there?
                    if (detectedPiece)
                    {
                        //Invalid - Check the next one
                        continue;
                    }

                    #endregion
                    //If you made it here a forced move has been detected!

                    #region Store Foreced Move
                    //Check if forced moves contains the piece we're currently checking
                    if (!forcedMoves.ContainsKey(piece))
                    {
                        //Add it to the list of forced moves
                        forcedMoves.Add(piece, new List<Vector2Int>());
                    }
                    //Add the jump cell to the piece's forced moves
                    forcedMoves[piece].Add(jumpCell);
                    #endregion

                }

            }
        }
        //Scans the board for forced moves
        void DetectForcedMoves()
        {
            //Refresh forced moves
            forcedMoves = new ForcedMoves();
            //Loop through entire board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    //Get piece at index
                    Piece pieceToCheck = pieces[x, y];
                    //If the piece exists
                    if(pieceToCheck)
                    {
                        //Check piece for forced moves
                        CheckForcedMove(pieceToCheck);
                    }
                }
            }
        }
        //Checks if a piece has forced pieces based on color
        bool HasForcedMoves(Piece selected)
        {
            //Loop through all forced moves
            foreach (var move in forcedMoves)
            {
                //Get piece for forced move
                Piece piece = move.Key;
                //Is the piece being foreced to move the same color as selected piece?
                if(piece.isWhite == selected.isWhite)
                {
                    //Our selected piece has forced moves
                    return true;
                }
            }
            //Does not have any forced moves
            return false;
        }
        //Check if the selected piece has forced moves
        bool IsforcedMove(Piece selected, Vector2Int desiredCell)
        {
            //Does the selected piece have a forced move?
            if(forcedMoves.ContainsKey(selected))
            {
                //Is there any forced moves for this piece?
                if(forcedMoves[selected].Contains(desiredCell))
                {
                    //It is a forced move
                    return true;
                }
            }
            //It is not a forced move
            return false;
        }

        //Remove a piece from the board
        void RemovePiece(Piece pieceToRemove)
        {
            Vector2Int cell = pieceToRemove.cell;
            //Clear cell in 2D array
            pieces[cell.x, cell.y] = null;
            //Destroy the gameobject of the piece immediately
            DestroyImmediate(pieceToRemove.gameObject);
        }

        //Calculates & returns the piece between start and end locations
        Piece GetPieceBetween(Vector2Int start, Vector2Int end)
        {
            Vector2Int cell = Vector2Int.zero;
            cell.x = (start.x + end.x / 2);
            cell.y = (start.y + end.y / 2);
            return GetPiece(cell);
        }

        //Check if piece was taken
        bool IsPieceTaken(Piece selected)
        {
            //Get the piece in between move
            Piece pieceBetween = GetPieceBetween(selected.oldCell, selected.cell);
            //If there is a piece between and the piece isn't the same color
            if(pieceBetween != null && pieceBetween.isWhite != selected.isWhite)
            {
                //Destroy the piece between
                RemovePiece(pieceBetween);
                //Piece taken
                return true;
            }
            //Piece not taken
            return false;
        }
    }
}