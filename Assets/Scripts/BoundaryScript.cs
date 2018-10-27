using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryScript : MonoBehaviour {
    public bool debugRender = true;
    public Material mainMaterial;
    public Material outlineMaterial;
    public Color color;
    private PolyPieceGenerator ppGen = new PolyPieceGenerator();

    // Use this for initialization
    void Start () {
        ppGen.numberOfSides = 4;
        ppGen.debugRender = this.debugRender;
        ppGen.mainRadius = 0.5f;
        ppGen.mainMaterial = new Material(this.mainMaterial)
        {
            color = color
        };
        ppGen.outlineMaterial = this.outlineMaterial;
        ppGen.generateCollider = true;
        ppGen.colliderIsTrigger = false;
        ppGen.Generate(gameObject);
    }
}
