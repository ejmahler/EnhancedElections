using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CityGenerator))]
public class MoveManager : MonoBehaviour
{
    public event System.Action<Constituent, District, bool> MoveMade;

    public District CurrentlySelectedDistrict { get; private set; }

    private Constituent _currentlySelectedConstituent;
    public Constituent CurrentlySelectedConstituent
    {
        get { return _currentlySelectedConstituent; }
        private set
        {
            if (value != _currentlySelectedConstituent)
            {
                var old = _currentlySelectedConstituent;
                _currentlySelectedConstituent = value;

                if (old != null)
                {
                    old.CurrentlySelected = false;
                    old.UpdateBackground();
                }
                value.CurrentlySelected = true;

                value.UpdateBackground();
            }
        }
    }

    public HashSet<Constituent> CurrentValidMoves { get; private set; }

    private HashSet<Constituent> _lockedConstituents;
    public HashSet<Constituent> LockedConstituents
    {
        get { return _lockedConstituents; }
        set
        {
            var oldLocked = _lockedConstituents;

            _lockedConstituents = value;

            foreach (var c in oldLocked)
            {
                c.UpdateBackground();
            }
            foreach (var c in value)
            {
                c.UpdateBackground();
            }
        }
    }

    public Stack<Move> UndoStack { get; private set; }

    //unordered history of movies every key is a constituent that has been changed, and every value is that constituent's original district
    //if the player moves a constituent back into its original district, it is removed from this map
    public Dictionary<Constituent, District> OriginalDistricts { get; private set; }

    private CityGenerator cityGenerator;

    private bool _AllowMoves;
    public bool AllowMoves
    {
        get { return _AllowMoves; }
        set
        {
            if (_AllowMoves != value)
            {
                _AllowMoves = value;

                UpdateValidMoves();

                //update the borders of the currently selected district
                foreach (var member in CurrentlySelectedDistrict.ConstituentsQuery)
                {
                    member.UpdateBorders();
                }
            }
        }
    }

    void Awake()
    {
        _lockedConstituents = new HashSet<Constituent>();
        cityGenerator = GetComponent<CityGenerator>();
        UndoStack = new Stack<Move>();
        OriginalDistricts = new Dictionary<Constituent, District>();
    }

    // Use this for initialization
    void Start()
    {
        _AllowMoves = true;
        SelectConstituent(cityGenerator.Constituents[0]);
    }

    void Update()
    {
        //performance hack: ideally this would go in constituent.update(), but that method is expensive to run on every single constituent
        //at the moment, the constituent's background color is changed by its district, EXCEPT when it is the currently selected constituent
        //in that case (and only in that case), it has its own background color for a mouseover effect
        //so we save a ton un update calls here by calling it only on the constituent that needs ot
        CurrentlySelectedConstituent.UpdateBackground();
    }

    public void ConstituentDragged(Constituent c)
    {
        if (CurrentlySelectedDistrict != null && CurrentValidMoves.Contains(c))
        {
            UndoStack.Push(new Move(c, c.District, CurrentlySelectedDistrict));
            MoveConstituent(c, CurrentlySelectedDistrict, undo: false);

            CurrentlySelectedConstituent = c;

            AudioManager.instance.PlayScrape();
        }
        else if (c.District == CurrentlySelectedDistrict)
        {
            CurrentlySelectedConstituent = c;
        }
    }

    public void Undo()
    {
        if (UndoStack.Count > 0)
        {
            Dictionary<Constituent, District> undoMoves = new Dictionary<Constituent, District>();

            //undo the most recent move, and also undo as many preceding "empty space" moves as possible
            Move lastMove;
            do
            {
                lastMove = UndoStack.Pop();
                MoveConstituent(lastMove.constituent, lastMove.oldDistrict, undo: true);
            }
            while (UndoStack.Count > 0 && UndoStack.Peek().constituent.party == Constituent.Party.None && UndoStack.Peek().constituent.Neighbors.Contains(lastMove.constituent));
        }
    }

    public void UndoAll()
    {
        //move every modified constituent back into its original district
        while (UndoStack.Count > 0)
        {
            Move move = UndoStack.Pop();
            MoveConstituent(move.constituent, move.oldDistrict, undo: true);
        }
    }

    public bool TryMove(Constituent c, District newDistrict, bool undo)
    {
        if (IsValidMove(c, newDistrict))
        {
            MoveConstituent(c, newDistrict, undo);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void MoveConstituent(Constituent constituent, District newDistrict, bool undo)
    {
        District previousDistrict = constituent.District;

        //if the player has moved this constituent back to its original district, remove it from the move history
        District originalDistrict;
        if (OriginalDistricts.TryGetValue(constituent, out originalDistrict))
        {
            if (newDistrict == originalDistrict)
                OriginalDistricts.Remove(constituent);
        }
        else
        {
            //since this constituent is not in the move history, oldDistrict is this constituent's original district (for this turn at least)
            OriginalDistricts.Add(constituent, previousDistrict);
        }

        //move this constituent to its new district
        constituent.District = newDistrict;

        //update the member data of all the districts we've modified
        previousDistrict.UpdateMemberData();
        newDistrict.UpdateMemberData();

        //update our set of valid moves
        UpdateValidMoves();

        //tell every neighbor of this consituent to update its borders
        foreach (Constituent n in constituent.Neighbors)
        {
            if (n != null)
            {
                n.UpdateBorders();
            }
        }

        //tell every constituent of the new district to update their borders
        foreach (Constituent c in newDistrict.ConstituentsQuery)
        {
            c.UpdateBorders();
        }

        if (MoveMade != null) { MoveMade(constituent, newDistrict, undo); }
    }

    public void SelectConstituent(Constituent newConstituent)
    {
        CurrentlySelectedConstituent = newConstituent;

        var newDistrict = newConstituent.District;
        if (CurrentlySelectedDistrict != newDistrict)
        {
            var oldDistrict = CurrentlySelectedDistrict;
            CurrentlySelectedDistrict = newDistrict;

            foreach (District d in cityGenerator.Districts)
            {
                if (d == newDistrict)
                {
                    d.CurrentlySelected = true;
                }
                else
                {
                    d.CurrentlySelected = false;
                }
            }
            UpdateValidMoves();

            foreach (var member in newDistrict.ConstituentsQuery)
            {
                member.UpdateBorders();
            }

            if (oldDistrict != null)
            {
                foreach (var member in oldDistrict.ConstituentsQuery)
                {
                    member.UpdateBorders();
                }
            }
        }
    }

    private void UpdateValidMoves()
    {
        if (AllowMoves)
        {
            CurrentValidMoves = GetValidMovesForDistrict(CurrentlySelectedDistrict);
        }
        else
        {
            CurrentValidMoves = new HashSet<Constituent>();
        }
    }

    public HashSet<Constituent> GetValidMovesForDistrict(District d)
    {
        return new HashSet<Constituent>(d.NeighborConstituents.Where((neighbor) => IsValidMove(neighbor, d)).ToArray());
    }

    private bool IsValidMove(Constituent constituent, District newDistrict)
    {
        //make sure this constituent isn't locked
        if (LockedConstituents.Contains(constituent))
        {
            return false;
        }

        if (constituent.party != Constituent.Party.None)
        {
            //make sure the size of the old district will be within size constraints
            if (constituent.District.VotingMemberCount - 1 < cityGenerator.MinDistrictSize)
            {
                return false;
            }
        }

        //check if this constituent is a cut vertex for the old district. if it is, return false
        if (constituent.District.ArticulationPoints.Contains(constituent))
        {
            return false;
        }

        //verify that this constituent is actually adjacent to the new district
        if (!newDistrict.NeighborConstituents.Contains(constituent))
        {
            return false;
        }

        //return true
        return true;
    }

    public struct Move
    {
        public readonly Constituent constituent;
        public readonly District oldDistrict;
        public readonly District newDistrict;

        public Move(Constituent c, District old, District n)
        {
            constituent = c; oldDistrict = old; newDistrict = n;
        }
    }
}
