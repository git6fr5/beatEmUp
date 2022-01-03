// Libraries.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMass {

    // Mass.
    public float inverseMass;
    public float damping;

    // Motion.
    public Vector3 position;
    public Vector3 velocity;
    private Vector3 acceleration;
    public static float SquareThreshold = 0.001f * 0.001f;

    public PointMass(float mass, float damping, Vector3 position) {
        this.inverseMass = mass > 0 ? 1f / mass : 0f;
        this.damping = damping;
        this.position = position;
        this.velocity = Vector3.zero;
        this.acceleration = Vector3.zero;
    }

    public void ApplyForce(Vector3 force) {
        acceleration += force * inverseMass;
    }

    public void Update(float deltaTime) {
        velocity += acceleration * deltaTime;
        position += velocity * deltaTime;
        acceleration = Vector3.zero;
        if (velocity.sqrMagnitude < SquareThreshold) {
            velocity = Vector3.zero;
        }

        velocity *= damping;
    }

}
