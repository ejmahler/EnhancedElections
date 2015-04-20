using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CityGenerator))]
public class MoveManager : MonoBehaviour
{

    private AudioManager audioManager;

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
        audioManager = GetComponent<AudioManager>();
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
            UndoStack.Push(new Move(c, c.district, CurrentlySelectedDistrict));
            MoveConstituents(new Dictionary<Constituent, District> { { c, CurrentlySelectedDistrict } });

            CurrentlySelectedConstituent = c;
        }
        else if (c.district == CurrentlySelectedDistrict)
        {
            CurrentlySelectedConstituent = c;
        }
    }

    public void Undo()
    {
        if (UndoStack.Count > 0)
        {
            Dictionary<Constituent, District> undoMoves = new Dictionary<Constituent, District>();

            //undo the most recent move
            var lastMove = UndoStack.Pop();
            undoMoves.Add(lastMove.constituent, lastMove.oldDistrict);

            //also undo as many 'none' moves as possible, if they're adjacent to the one we just removed
            while (UndoStack.Count > 0 && UndoStack.Peek().constituent.party == Constituent.Party.None && UndoStack.Peek().constituent.Neighbors.Contains(lastMove.constituent))
            {
                lastMove = UndoStack.Pop();
                undoMoves.Add(lastMove.constituent, lastMove.oldDistrict);
            }

            MoveConstituents(undoMoves);
        }
    }

    public void UndoAll()
    {
        //move every modified constituent back into its original district
        MoveConstituents(new Dictionary<Constituent, District>(OriginalDistricts));

        //clear the undo stack since it isn't of much use anymore
        UndoStack.Clear();
    }

    private void MoveConstituents(Dictionary<Constituent, District> moves)
    {
        HashSet<District> modifiedDistricts = new HashSet<District>();

        foreach (var item in moves)
        {
            modifiedDistricts.Add(item.Value);//new district for this constituent
            modifiedDistricts.Add(item.Key.district);//previous district for this constituent

            //if the player has moved this constituent back to its original district, remove it from the move history
            District originalDistrict;
            if (OriginalDistricts.TryGetValue(item.Key, out originalDistrict))
            {
                if (item.Value == originalDistrict)
                    OriginalDistricts.Remove(item.Key);
            }
            else
            {
                //since this constituent is not in the move history, oldDistrict is this constituent's original district (for this turn at least)
                OriginalDistricts.Add(item.Key, item.Key.district);
            }

            //move this constituent to its new district
            item.Key.district = item.Value;
        }

        //update the member data of all the districts we've modified
        foreach (District d in modifiedDistricts)
        {
            d.UpdateMemberData();
        }

        //update our set of valid moves
        UpdateValidMoves();

        //tell every constituent of a modified district to update their borders
        foreach (Constituent c in cityGenerator.Constituents.Where(c => modifiedDistricts.Contains(c.district)))
        {
            c.UpdateBorders();
        }
    }

    public void SelectConstituent(Constituent newConstituent)
    {
        CurrentlySelectedConstituent = newConstituent;

        var newDistrict = newConstituent.district;
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
        CurrentValidMoves = new HashSet<Constituent>();

        //if moves aren't allowed, leave it as an empty set
        if (AllowMoves)
        {
            foreach (Constituent districtNeighbor in CurrentlySelectedDistrict.NeighborConstituents)
            {
                if (!LockedConstituents.Contains(districtNeighbor) && cityGenerator.IsValidMove(districtNeighbor, CurrentlySelectedDistrict))
                {
                    CurrentValidMoves.Add(districtNeighbor);
                }
            }
        }
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
