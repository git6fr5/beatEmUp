// Libraries.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Spring {

    // Nodes
    public PointMass nodeA;
    public PointMass nodeB;

    // Properties
    public float restLength;
    public float stiffness; // How much change from rest increases force

    public Spring(PointMass nodeA, PointMass nodeB, float stiffness, float taughtness) {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.stiffness = stiffness;

        // Slightly less than the actual distance in order to keep it taught.
        this.restLength = Vector3.Distance(nodeA.position, nodeB.position) * taughtness;
    }

    public void Update(float deltaTime) {

        Vector3 deltaPosition = (nodeA.position - nodeB.position);
        float currLength = deltaPosition.magnitude;

        // The springs can only pull, not push
        if (currLength <= restLength) {
            return;
        }

        Vector3 displacement = (deltaPosition / currLength) * (currLength - restLength); // Hooke's Law
        Vector3 deltaVelocity = nodeA.velocity - nodeB.velocity;
        Vector3 force = stiffness * displacement;

        nodeA.ApplyForce(-force);
        nodeB.ApplyForce(force);
    }
}

public struct QuantumSpring {

    // Nodes
    public PointMass nodeA;
    public PointMass nodeB;

    // Properties
    public float restLength;
    public float stiffness; // How much change from rest increases force

    public QuantumSpring(PointMass nodeA, PointMass nodeB, float stiffness, float taughtness) {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.stiffness = stiffness;

        // Slightly less than the actual distance in order to keep it taught.
        this.restLength = Vector3.Distance(nodeA.position, nodeB.position) * taughtness;
    }

    public void Update(float deltaTime) {

        Vector3 deltaPosition = (nodeA.position - nodeB.position);
        float currLength = deltaPosition.magnitude;

        // The springs can only pull, not push
        if (currLength <= restLength) {
            return;
        }

        Vector3 displacement = (deltaPosition / currLength) * (currLength - restLength); // Hooke's Law
        Vector3 deltaVelocity = nodeA.velocity - nodeB.velocity;
        Vector3 force =  1f / (stiffness * displacement.magnitude) * displacement.normalized;

        nodeA.ApplyForce(-force);
        nodeB.ApplyForce(force);
    }
}
