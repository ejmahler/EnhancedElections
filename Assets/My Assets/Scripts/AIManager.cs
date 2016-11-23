using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MoveManager))]
[RequireComponent(typeof(TurnManager))]
[RequireComponent(typeof(CityGenerator))]
public class AIManager : MonoBehaviour {

    private TurnManager turnManager;
    private MoveManager moveManager;
    private CityGenerator cityGenerator;

	// Use this for initialization
	void Start () {
        turnManager = GetComponent<TurnManager>();
        moveManager = GetComponent<MoveManager>();
        cityGenerator = GetComponent<CityGenerator>();
	}

    //when this coroutine ends, the ai's turn is over
    public IEnumerator AITurn()
    {
        yield return null;

        yield return new WaitForSeconds(0.5f);
        while(turnManager.MovesThisTurn < turnManager.MovesPerTurn)
        {
            Constituent.Party currentParty = turnManager.GetPartyforPlayer(turnManager.CurrentPlayer);
            Constituent.Party opponentParty = turnManager.GetPartyforPlayer(turnManager.NextPlayer);


            MoveManager.Move move;

            //first, try to convert neutral districts
            if (!TacticConvertNeutral(out move))
            {
                //if we're here, there are no neutral districts. if we're losing, convert enemy districts, otherwise try to consolidate the opponent
                int ourScore = cityGenerator.Districts.Count((c) => c.CurrentMajority == currentParty);
                int opponentScore = cityGenerator.Districts.Count((c) => c.CurrentMajority == opponentParty);

                if(ourScore <= opponentScore + 1)
                {
                    TacticConvertWeakOpponents(out move);
                }
                else
                {
                    if (Utils.Chance(0.5f))
                    {
                        if (TacticConsolidateOpponent(out move)) { }
                        else if (TacticDeconsolidateUs(out move)) { }
                    }
                    else
                    {
                        if (TacticDeconsolidateUs(out move)) { }
                        else if (TacticConsolidateOpponent(out move)) { }
                    }
                }
            }

            //if we actually found a beneficial move
            if(move.constituent != null)
            {
                moveManager.MoveConstituent(move.constituent, move.newDistrict, undo:false);
            }
            else
            {
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    //tactic: convert neutral districts to our side
    //return true and populate "move" with the relevant move details if this can be done, return false if it cannot
    private bool TacticConvertNeutral(out MoveManager.Move move)
    {
        Constituent.Party currentParty = turnManager.GetPartyforPlayer(turnManager.CurrentPlayer);
        Constituent.Party opponentParty = turnManager.GetPartyforPlayer(turnManager.NextPlayer);

        var neutralDistricts = cityGenerator.Districts.Where((d) => d.CurrentMajority == Constituent.Party.None).ToList();

        //loop through every neutral district, attempting to flip this district by consolidating an opponent's district
        foreach (District d in neutralDistricts)
        {
            //go through all the neighbor districts that are owned by the opponent. prioritize ones with a large majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, opponentParty).OrderByDescending(n => n.CurrentMajorityPercent))
            {
                //try to move one of our suppporters into this neutral district
                if (FindConstituentToMove(neighborDistrict, d, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of the opponent's supporters out of the neutral district
                if (FindConstituentToMove(d, neighborDistrict, opponentParty, out move))
                    return true;
            }
        }

        //we didn't find any neutrals that we could flip by consolidation, so instead find any that we can flip by weakening our own
        foreach (District d in neutralDistricts)
        {
            //go through all the neighbor districts that are owned by us. prioritize ones with a large majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, currentParty).OrderByDescending(n => n.CurrentMajorityPercent))
            {
                //try to move one of our suppporters into this neutral district
                if (FindConstituentToMove(neighborDistrict, d, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of the opponent's supporters out of the neutral district
                if (FindConstituentToMove(d, neighborDistrict, opponentParty, out move))
                    return true;
            }
        }

        //we didn't find anything, so just populate it with defaults
        move = new MoveManager.Move(null, null, null);
        return false;
    }





    //tactic: Weaken the opponent by consolidating their constituents
    //return true and populate "move" with the relevant move details if this can be done, return false if it cannot
    private bool TacticConsolidateOpponent(out MoveManager.Move move)
    {
        Constituent.Party currentParty = turnManager.GetPartyforPlayer(turnManager.CurrentPlayer);
        Constituent.Party opponentParty = turnManager.GetPartyforPlayer(turnManager.NextPlayer);

        //loop through every opponent district, prioritizing the ones with the largest majority
        foreach (District d in cityGenerator.Districts.Where((d) => d.CurrentMajority == opponentParty).OrderByDescending(n => n.CurrentMajorityPercent))
        {
            //go through all the neighbor districts that are owned by us. prioritize ones with a large majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, currentParty).OrderByDescending(n => n.CurrentMajorityPercent))
            {
                //try to move one of our opponents into the district we're consolidating
                if (FindConstituentToMove(d, neighborDistrict, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of our supporters out of the district we're consolidating
                if (FindConstituentToMove(neighborDistrict, d, opponentParty, out move))
                    return true;
            }

            //go through all the neighbor districts that are owned by the opponent. prioritize ones with a large majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, opponentParty).OrderByDescending(n => n.CurrentMajorityPercent))
            {
                //try to move one of our opponents into the district we're consolidating
                if (FindConstituentToMove(d, neighborDistrict, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of our supporters out of the district we're consolidating
                if (FindConstituentToMove(neighborDistrict, d, opponentParty, out move))
                    return true;
            }
        }

        //we didn't find anything, so just populate it with defaults
        move = new MoveManager.Move(null, null, null);
        return false;
    }



    //tactic: Convert opponent districts that have narrow margins
    //return true and populate "move" with the relevant move details if this can be done, return false if it cannot
    private bool TacticConvertWeakOpponents(out MoveManager.Move move)
    {
        Constituent.Party currentParty = turnManager.GetPartyforPlayer(turnManager.CurrentPlayer);
        Constituent.Party opponentParty = turnManager.GetPartyforPlayer(turnManager.NextPlayer);

        //loop through every opponent district, prioritizing the ones with the smallest majority
        foreach (District d in cityGenerator.Districts.Where((d) => d.CurrentMajority == opponentParty).OrderBy(n => n.CurrentMajorityPercent))
        {
            //go through all the neighbor districts that are owned by the opponent. prioritize ones with a large majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, opponentParty).OrderByDescending(n => n.CurrentMajorityPercent))
            {
                //try to move one of our suppporters into this district
                if (FindConstituentToMove(neighborDistrict, d, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of the opponent's supporters out of this district
                if (FindConstituentToMove(d, neighborDistrict, opponentParty, out move))
                    return true;
            }

            //go through all the neighbor districts that are owned by us. prioritize ones with a large majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, currentParty).OrderByDescending(n => n.CurrentMajorityPercent))
            {
                //try to move one of our suppporters into this district
                if (FindConstituentToMove(neighborDistrict, d, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of the opponent's supporters out of this district
                if (FindConstituentToMove(d, neighborDistrict, opponentParty, out move))
                    return true;
            }
        }

        //we didn't find anything, so just populate it with defaults
        move = new MoveManager.Move(null, null, null);
        return false;
    }

    //tactic: De-consolidate our own districts.
    //return true and populate "move" with the relevant move details if this can be done, return false if it cannot
    private bool TacticDeconsolidateUs(out MoveManager.Move move)
    {
        Constituent.Party currentParty = turnManager.GetPartyforPlayer(turnManager.CurrentPlayer);
        Constituent.Party opponentParty = turnManager.GetPartyforPlayer(turnManager.NextPlayer);

        //loop through every friendly district, prioritizing the ones with the largest majority
        foreach (District d in cityGenerator.Districts.Where((d) => d.CurrentMajority == currentParty).OrderByDescending(n => n.CurrentMajorityPercent))
        {
            //go through all the neighbor districts that are owned by us. prioritize ones with a low majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, currentParty).OrderBy(n => n.CurrentMajorityPercent))
            {
                //try to move one of our opponents into the district we're deconsolidating
                if (FindConstituentToMove(d, neighborDistrict, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of our supporters out of the district we're deconsolidating
                if (FindConstituentToMove(neighborDistrict, d, opponentParty, out move))
                    return true;
            }

            //go through all the neighbor districts that are owned by the opponent. prioritize ones with a low majority percent.
            foreach (District neighborDistrict in GetNeighborsWithParty(d, opponentParty).OrderBy(n => n.CurrentMajorityPercent))
            {
                //try to move one of our opponents into the district we're deconsolidating
                if (FindConstituentToMove(d, neighborDistrict, currentParty, out move))
                    return true;

                //if we're here, that means it didn't work. instead, try to move one of our supporters out of the district we're deconsolidating
                if (FindConstituentToMove(neighborDistrict, d, opponentParty, out move))
                    return true;
            }
        }

        //we didn't find anything, so just populate it with defaults
        move = new MoveManager.Move(null, null, null);
        return false;
    }




    private bool FindConstituentToMove(District fromDistrict, District toDistrict, Constituent.Party party, out MoveManager.Move move)
    {
        //candidates for moving are any district in the "from" district, with the requested party, that hasn't already been moved this turn
        var moveCandidates = fromDistrict.ConstituentsQuery.Where(c => c.party == party && !moveManager.OriginalDistricts.ContainsKey(c));

        //intersect the move candidates with the moves the rules actually allow us to make
        HashSet<Constituent> validMoves = moveManager.GetValidMovesForDistrict(toDistrict);
        validMoves.IntersectWith(moveCandidates);

        //if there are any moves available, choose one at random
        if (validMoves.Count > 0)
        {
            Constituent chosenMove = Utils.ChooseRandom(moveCandidates.ToArray());
            move = new MoveManager.Move(chosenMove, chosenMove.District, toDistrict);
            return true;
        }
        else
        {
            //we didn't find anything, so just populate it with defaults
            move = new MoveManager.Move(null, null, null);
            return false;
        }
    }





    private IEnumerable<District> GetNeighborsWithParty(District d, Constituent.Party majorityParty)
    {
        return d.NeighborConstituents
                .GroupBy(n => n.District)
                .Select(grp => grp.First().District)
                .Where(n => n.CurrentMajority == majorityParty);
    }
}
