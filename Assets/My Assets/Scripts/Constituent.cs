using UnityEngine;
using System.Collections;

public class Constituent : MonoBehaviour
{

    public enum Party { Red, Blue, Yellow, None }

    private GameObject _partyRedShape;
    private GameObject _partyBlueShape;
    private GameObject _partyOtherShape;

    private LTDescr activeShapeSizeTween = null;

	private Material validBorder { get { return District.ValidBorderMaterial; } }
	private Material invalidBorder { get { return District.InvalidBorderMaterial; } }
	private Material BackgroundMaterial { get { return District.BackgroundMaterial; } }

    private Material SelectedBackgroundMaterial;

    [SerializeField]
    private Material lockedBackground;

    private MoveManager moveManager;

    private Renderer _backgroundMesh;

    private Renderer _borderTop, _borderBottom, _borderLeft, _borderRight;
    private Renderer[] NeighborBorders
    {
        get
        {
            return new Renderer[] { _borderTop, _borderBottom, _borderLeft, _borderRight };
        }
    }

    private District _district;
    public District District
    {
        get { return _district; }
        set
        {
            _district = value;

            UpdateBorders();
            UpdateBackground();

            //if we have a party shape, pulse its size
            var activeShape = GetShapeForParty();
            if (activeShape != null)
            {
                System.Action tweenSize = () =>
                {
                    var originalScale = activeShape.transform.localScale;
                    activeShape.transform.localScale = originalScale * 1.75f;

                    activeShapeSizeTween = LeanTween.scale(activeShape, originalScale, 0.25f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {
                            activeShapeSizeTween = null;
                        }); ;
                };

                //if we're already tweening the size, wait until it finishes
                if (activeShapeSizeTween != null)
                {
                    activeShapeSizeTween.setOnComplete(tweenSize);
                }
                else
                {
                    tweenSize();
                }
            }
        }
    }

    [System.NonSerialized]
    public Constituent neighborTop, neighborBottom, neighborLeft, neighborRight;
    public Constituent[] Neighbors
    {
        get
        {
            return new Constituent[] { neighborTop, neighborBottom, neighborLeft, neighborRight };
        }
    }

    private Party _party;
    public Party party
    {
        get { return _party; }
        set
        {
            _party = value;
            _partyRedShape.SetActive(value == Party.Red);
            _partyBlueShape.SetActive(value == Party.Blue);
            _partyOtherShape.SetActive(value == Party.Yellow);
        }
    }

    private float selectionEffectPercentage = 1.0f;

    private bool _currentlySelected = false;
    public bool CurrentlySelected
    {
        get
        {
            return _currentlySelected;
        }
        set
        {
            if (value != _currentlySelected)
            {
                _currentlySelected = value;

                if (value) //if we are selected, transition from the glaze color back to the normal color
                {
                    LeanTween.cancel(_backgroundMesh.gameObject);
                    SelectedBackgroundMaterial.SetColor("_Color", District.PartyColor);
                    selectionEffectPercentage = 0.0f;
                }
                else //if we are unselected, transition from the normal color back to the glaze color
                {
                    LeanTween.value(_backgroundMesh.gameObject, 0.0f, 1.0f, 0.5f).setOnComplete(() => UpdateBackground())
                        .setOnUpdate((percent) =>
                    {
                        //if our district is not selected and the district is at the minimum size, cancel any selection fade effect running
                        if (!District.CurrentlySelected && District.BackgroundMaterial == District.MinimumBackgroundMaterial)
                        {
                            LeanTween.cancel(_backgroundMesh.gameObject);
                            selectionEffectPercentage = 1.0f;
                            UpdateBackground();
                        }
                        else
                        {
                            selectionEffectPercentage = percent;
                            Color resultingColor = Color.Lerp(District.PartyColor, District.CurrentColor, selectionEffectPercentage);
                            SelectedBackgroundMaterial.SetColor("_Color", resultingColor);
                        }
                    });
                }
            }
        }
    }

    void Awake()
    {
        _backgroundMesh = transform.Find("Background").GetComponent<MeshRenderer>();

        _partyRedShape = transform.Find("RedShape").gameObject;
        _partyBlueShape = transform.Find("BlueShape").gameObject;
        _partyOtherShape = transform.Find("OtherShape").gameObject;

        _borderTop = transform.Find("Border Top").GetComponent<MeshRenderer>();
        _borderBottom = transform.Find("Border Bottom").GetComponent<MeshRenderer>();
        _borderLeft = transform.Find("Border Left").GetComponent<MeshRenderer>();
        _borderRight = transform.Find("Border Right").GetComponent<MeshRenderer>();

        moveManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();

        SelectedBackgroundMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Background"));
    }

    public void UpdateBorders()
    {
        //hide borders if our neighbors are in the same district as us
        var borders = NeighborBorders;
        var neighbors = Neighbors;
        for (int i = 0; i < 4; i++)
        {
            borders[i].gameObject.SetActive(neighbors[i] != null && neighbors[i].District != this.District);

            if (moveManager.CurrentValidMoves == null || moveManager.CurrentValidMoves.Contains(neighbors[i]))
            {
                borders[i].GetComponent<Renderer>().material = validBorder;
            }
            else
            {
                borders[i].GetComponent<Renderer>().material = invalidBorder;
            }
        }

        //if our district is selected, move our borders forward in the z direction so that they appear on top of unselected
        if (District.CurrentlySelected)
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

    public void UpdateBackground()
    {
        if (moveManager.LockedConstituents.Contains(this))
        {
            _backgroundMesh.material = lockedBackground;
        }
        else if (CurrentlySelected || selectionEffectPercentage < 1.0f)
        {
			SelectedBackgroundMaterial.SetColor("_Color", Color.Lerp(District.PartyColor, District.CurrentColor, selectionEffectPercentage));
            _backgroundMesh.material = SelectedBackgroundMaterial;
        }
        else
        {
            _backgroundMesh.material = BackgroundMaterial;
        }
    }

    private GameObject GetShapeForParty()
    {
        switch (party)
        {
            case Party.Blue:
                return _partyBlueShape;
            case Party.Red:
                return _partyRedShape;
            case Party.Yellow:
                return _partyOtherShape;
            default:
                return null;
        }
    }
}
