using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathLine : MonoBehaviour {

    public Material mainMaterial;
    public Material outlineMaterial;
    public Color color;
    private PolyPieceGenerator ppGen = new PolyPieceGenerator();
	// Use this for initialization
	void Start () {
        ppGen.numberOfSides = 4;
        ppGen.debugRender = true;
        ppGen.mainRadius = 1.0f;
        ppGen.mainMaterial = new Material(this.mainMaterial)
        {
            color = color
        };
        ppGen.outlineMaterial = this.outlineMaterial;
        ppGen.generateCollider = false;
        ppGen.Generate(gameObject);
    }
}
