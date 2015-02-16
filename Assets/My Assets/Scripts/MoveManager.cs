﻿using UnityEngine;
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
            MoveConstituents(new Dictionary<Constituent, District> {{c, CurrentlySelectedDistrict}});
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
        MoveConstituents(new Dictionary<Constituent, District>(MoveHistory));

        //clear the undo stack since it isn't of much use anymore. the move history will be cleared by the move constituents method
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
            if (MoveHistory.TryGetValue(item.Key, out originalDistrict))
            {
                if (item.Value == originalDistrict)
                    MoveHistory.Remove(item.Key);
            }
            else
            {
                //since this constituent is not in the move history, oldDistrict is this constituent's original district (for this turn at least)
                MoveHistory.Add(item.Key, item.Key.district);
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
        foreach (Constituent c in cityGenerator.Constituents.Where((c) => { return modifiedDistricts.Contains(c.district); }))
        {
            c.UpdateBorders();
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
