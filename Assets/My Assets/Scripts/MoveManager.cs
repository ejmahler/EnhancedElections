using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CityGenerator))]
public class MoveManager : MonoBehaviour {

    public District CurrentlySelectedDistrict { get; private set; }
    public HashSet<Constituent> CurrentValidMoves { get; private set; }

    public Stack<Move> UndoStack { get; private set; }

    //unordered history of movies every key is a constituent that has been changed, and every value is that constituent's original district
    //if the player moves a constituent back into its original district, it is removed from this map
    public Dictionary<Constituent, District> MoveHistory { get; private set; }

    private CityGenerator cityGenerator;

    private bool _AllowMoves;
    public bool AllowMoves
    {
        get { return _AllowMoves; }
        set
        {
            _AllowMoves = value;

            UpdateValidMoves();

            //update the borders of the currently selected district
            foreach (var member in CurrentlySelectedDistrict.Constituents)
            {
                member.UpdateBorders();
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        _AllowMoves = true;

        cityGenerator = GetComponent<CityGenerator>();
        SelectDistrict(cityGenerator.Districts[0]);

        UndoStack = new Stack<Move>();
        MoveHistory = new Dictionary<Constituent, District>();
    }

    public void ConstituentClicked(Constituent c)
    {
        SelectDistrict(c.district);
    }

    public void ConstituentDragged(Constituent c)
    {
        if (CurrentlySelectedDistrict != null && cityGenerator.IsValidMove(c, CurrentlySelectedDistrict) && AllowMoves)
        {
            UndoStack.Push(new Move(c, c.district, CurrentlySelectedDistrict));
            MoveConstituent(c, CurrentlySelectedDistrict);
        }
    }

    public void Undo()
    {
        if(UndoStack.Count > 0)
        {
            //undo the most recent move
            var lastMove = UndoStack.Pop();
            MoveConstituent(lastMove.constituent, lastMove.oldDistrict);

            //also undo as many 'none' moves as possible, if they're adjacent to the one we just removed
            while (UndoStack.Count > 0 && UndoStack.Peek().constituent.party == Constituent.Party.None && UndoStack.Peek().constituent.Neighbors.Contains(lastMove.constituent))
            {
                lastMove = UndoStack.Pop();
                MoveConstituent(lastMove.constituent, lastMove.oldDistrict);
            }
        }
    }

    private void MoveConstituent(Constituent c, District newDistrict)
    {
        var oldDistrict = c.district;
        c.district = newDistrict;

        oldDistrict.UpdateMemberData();
        newDistrict.UpdateMemberData();

        UpdateValidMoves();

        //update the borders of the new district
        foreach (var member in oldDistrict.Constituents)
        {
            member.UpdateBorders();
        }
        foreach (var member in newDistrict.Constituents)
        {
            member.UpdateBorders();
        }

        //if the player has moved this constituent back to its original district, remove it from the move history
        District originalDistrict;
        if(MoveHistory.TryGetValue(c, out originalDistrict))
        {
            if(newDistrict == originalDistrict)
                MoveHistory.Remove(c);
        }
        else
        {
            //since this constituent is not in the move history, oldDistrict is this constituent's original district (for this turn at least)
            MoveHistory.Add(c, oldDistrict);
        }
    }

    private void SelectDistrict(District newDistrict)
    {
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

            foreach (var member in newDistrict.Constituents)
            {
                member.UpdateBorders();
            }

            if (oldDistrict != null)
            {
                foreach (var member in oldDistrict.Constituents)
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
                if (cityGenerator.IsValidMove(districtNeighbor, CurrentlySelectedDistrict))
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
