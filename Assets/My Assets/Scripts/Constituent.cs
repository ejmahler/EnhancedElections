﻿using UnityEngine;
using System.Collections;

public class Constituent : MonoBehaviour {

    public enum Party { Red, Blue, Yellow, None }

    public Party party;

    private Renderer _backgroundMesh, _sphereMesh;

    private Renderer _borderTop, _borderBottom, _borderLeft, _borderRight;
    private Renderer[] NeighborBorders
    {
        get
        {
            return new Renderer[] { _borderTop, _borderBottom, _borderLeft, _borderRight };
        }
    }

    

    private District _district;
    public District district
    {
        get { return _district; }
        set
        {
            _district = value;
            _backgroundMesh.material = value.BackgroundMaterial;
            foreach (var border in NeighborBorders)
            {
                border.material = value.BorderMaterial;
            }
        }
    }

    public Constituent neighborTop, neighborBottom, neighborLeft, neighborRight;
    public Constituent[] Neighbors
    {
        get
        {
            return new Constituent[] { neighborTop, neighborBottom, neighborLeft, neighborRight };
        }
    }

	void Awake () {
        _backgroundMesh = transform.Find("Background").GetComponent<MeshRenderer>();
        _sphereMesh = transform.Find("Sphere").GetComponent<MeshRenderer>();

        _borderTop = transform.Find("Border Top").GetComponent<MeshRenderer>();
        _borderBottom = transform.Find("Border Bottom").GetComponent<MeshRenderer>();
        _borderLeft = transform.Find("Border Left").GetComponent<MeshRenderer>();
        _borderRight = transform.Find("Border Right").GetComponent<MeshRenderer>();
	}
	
	void Update () {
        if (party == Party.None)
        {
            _sphereMesh.gameObject.SetActive(false);
        }
        else
        {
            _sphereMesh.gameObject.SetActive(true);
            _sphereMesh.material = GetMaterial(party);
        }

        //hide borders if our neighbors are in the same district as us
        var borders = NeighborBorders;
        var neighbors = Neighbors;
        for (int i = 0; i < 4; i++)
        {
            borders[i].gameObject.SetActive(neighbors[i] != null && neighbors[i].district != this.district);
        }

        //if our district is selected, move our borders forward in the z direction so that they appear on top of unselected
        if (district.CurrentlySelected)
        {
            foreach (var b in borders)
            {
                b.transform.position = new Vector3(b.transform.position.x, b.transform.position.y, -2.0f);
            }
        }
        else
        {
            foreach (var b in borders)
            {
                b.transform.position = new Vector3(b.transform.position.x, b.transform.position.y, -1.0f);
            }
        }
	}

    private static Material GetMaterial(Party party)
    {
        if (party == Party.None)
        {
            return null;
        }
        return Resources.Load<Material>("Materials/Constituent " + party.ToString());
    }
}
