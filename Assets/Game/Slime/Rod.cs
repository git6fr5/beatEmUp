// Libraries.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Rod {

    // Nodes
    public PointMass nodeA;
    public PointMass nodeB;

    // Properties
    public Vector3 displacement;

    public Rod(PointMass nodeA, PointMass nodeB) {
        this.nodeA = nodeA;
        this.nodeB = nodeB;

        // Slightly less than the actual distance in order to keep it taught.
        this.displacement = nodeA.position - nodeB.position;
    }

    public void Update() {
        nodeB.position = nodeA.position + displacement;
    }
}
