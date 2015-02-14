using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CityGenerator))]
public class TurnManager : MonoBehaviour {

    public District CurrentlySelectedDistrict { get; private set; }
    public HashSet<Constituent> CurrentValidMoves { get; private set; }

    private Stack<Move> undoStack;

    private CityGenerator cityGenerator;

    // Use this for initialization
    void Start()
    {
        cityGenerator = GetComponent<CityGenerator>();
        SelectDistrict(cityGenerator.Districts[0]);

        undoStack = new Stack<Move>();
    }

    public void ConstituentClicked(Constituent c)
    {
        SelectDistrict(c.district);
    }

    public void ConstituentDragged(Constituent c)
    {
        if (CurrentlySelectedDistrict != null && cityGenerator.IsValidMove(c, CurrentlySelectedDistrict))
        {
            undoStack.Push(new Move(c, c.district, CurrentlySelectedDistrict));
            MoveConstituent(c, CurrentlySelectedDistrict);
        }
    }

    public void Undo()
    {
        if(undoStack.Count > 0)
        {
            var lastMove = undoStack.Pop();
            MoveConstituent(lastMove.constituent, lastMove.oldDistrict);
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
        foreach (Constituent districtNeighbor in CurrentlySelectedDistrict.NeighborConstituents)
        {
            if (cityGenerator.IsValidMove(districtNeighbor, CurrentlySelectedDistrict))
            {
                CurrentValidMoves.Add(districtNeighbor);
            }
        }
    }




    private struct Move
    {
        public Constituent constituent { get; private set; }
        public District oldDistrict { get; private set; }
        public District newDistrict { get; private set; }

        public Move(Constituent c, District old, District n)
        {
            constituent = c; oldDistrict = old; newDistrict = n;
        }
    }
}
