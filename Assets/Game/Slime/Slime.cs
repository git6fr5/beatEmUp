using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour {

    public class SlimeShape {

        public PointMass centerPoint;
        public PointMass[] points;
        public PointMass[] fixedPoints;

        public Rod[] rods;
        public Spring[] springs;
        public QuantumSpring[] quantumSprings;

        [System.Serializable]
        public struct SlimeShapeData {
            public float radius;
            public int precision;
            public float totalMass;
            [Range(0f, 1f)] public float damping;
            public float stiffness;
            [Range(0f, 1f)] public float taughtness;
        }

        public SlimeShape(SlimeShapeData shapeData) {
            Init(shapeData.radius, shapeData.precision, shapeData.totalMass, shapeData.damping, shapeData.stiffness, shapeData.taughtness);
        }

        public void Init(float radius, int precision, float totalMass, float damping, float stiffness, float taughtness) {

            centerPoint = new PointMass(totalMass / precision, damping, Vector3.zero);
            points = new PointMass[precision];
            fixedPoints = new PointMass[precision];  // these fixed points will be used to anchor the grid to fixed positions on the screen

            for (int i = 0; i < precision; i++) {
                Vector3 position = Quaternion.Euler(0f, 0f, 360f * (float)i / precision) * Vector3.right;
                points[i] = new PointMass(totalMass / precision, damping, position);
                fixedPoints[i] = new PointMass(totalMass / precision, damping, position);
            }

            List<Rod> L_Rods = new List<Rod>();
            List<Spring> L_Springs = new List<Spring>();
            List<QuantumSpring> L_QuantumSprings = new List<QuantumSpring>();

            // 0th case.
            Rod fixedRod = new Rod(centerPoint, fixedPoints[0]);
            Spring anchorSpring = new Spring(points[0], fixedPoints[0], stiffness, taughtness);
            Spring outlineSpring = new Spring(points[precision-1], points[0], stiffness, taughtness);
            QuantumSpring centerSpring = new QuantumSpring(points[0], centerPoint, stiffness, taughtness);

            L_Rods.Add(fixedRod);
            L_Springs.Add(anchorSpring);
            L_Springs.Add(outlineSpring);
            L_QuantumSprings.Add(centerSpring);

            // link the point masses with springs
            for (int i = 1; i < precision; i++) {

                fixedRod = new Rod(centerPoint, fixedPoints[i]);
                anchorSpring = new Spring(points[i], fixedPoints[i], stiffness, taughtness);
                outlineSpring = new Spring(points[i-1], points[i], stiffness, taughtness);
                centerSpring = new QuantumSpring(points[i], centerPoint, stiffness, taughtness);

                L_Rods.Add(fixedRod);
                L_Springs.Add(anchorSpring);
                L_Springs.Add(outlineSpring);
                L_QuantumSprings.Add(centerSpring);

            }

            rods = L_Rods.ToArray();
            springs = L_Springs.ToArray();
            quantumSprings = L_QuantumSprings.ToArray();

        }

        public void Gravity(float gravity) {

            if (centerPoint.position.y > 0f) {
                centerPoint.ApplyForce(gravity * Vector3.down);
            }

            for (int i = 0; i < fixedPoints.Length; i++) {
                if (fixedPoints[i].position.y > 0f) {
                    fixedPoints[i].ApplyForce(gravity * Vector3.down);
                }
            }

        }

        public void Update(float deltaTime) {

            for (int i = 0; i < rods.Length; i++) {
                rods[i].Update();
            }
            for (int i = 0; i < springs.Length; i++) {
                springs[i].Update(deltaTime);
            }
            for (int i = 0; i < points.Length; i++) {
                points[i].Update(deltaTime);
            }

        }

        public void ApplyExplosiveForce(float force, float factor = 10f, float dampingFactor = 0.6f) {
            for (int i = 0; i < points.Length; i+=2) {
                float sqrDistance = (centerPoint.position - points[i].position).sqrMagnitude;
                points[i].ApplyForce(factor * force * (points[i].position - centerPoint.position) / (factor * factor + sqrDistance));
            }
        }
    }

    private SlimeShape shape;
    public SlimeShape.SlimeShapeData slimeShapeData;

    public float speed;

    public bool reset;
    public bool debugCircle;
    public bool debugRods;
    public bool debugSprings;
    public bool debugQSprings;
    public bool debugCenter;


    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {

        if (reset) {
            shape = new SlimeShape(slimeShapeData);
            reset = false;
        }

        if (shape == null) {
            return;
        }

        Vector3 movementVector = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
        shape.centerPoint.position += movementVector * Time.deltaTime * speed;

        // shape.Gravity(1000000f);
        shape.Update(Time.deltaTime);

        if (Input.GetMouseButtonDown(1)) {
            shape.ApplyExplosiveForce(10000f);
        }

    }

    void OnDrawGizmos() {

        if (shape == null) {
            return;
        }

        if (debugCircle) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(shape.points[shape.points.Length - 1].position, shape.points[0].position);
            for (int i = 1; i < shape.points.Length; i++) {
                Gizmos.DrawLine(shape.points[i - 1].position, shape.points[i].position);
            }
        }
        
        if (debugRods) {
            Gizmos.color = Color.red;
            for (int i = 0; i < shape.rods.Length; i++) {
                Gizmos.DrawLine(shape.rods[i].nodeA.position, shape.rods[i].nodeB.position);
            }
        }

        if (debugSprings) {
            Gizmos.color = Color.blue;
            for (int i = 0; i < shape.springs.Length; i++) {
                Gizmos.DrawLine(shape.springs[i].nodeA.position, shape.springs[i].nodeB.position);
            }
        }

        if (debugQSprings) {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < shape.quantumSprings.Length; i++) {
                Gizmos.DrawLine(shape.quantumSprings[i].nodeA.position, shape.quantumSprings[i].nodeB.position);
            }
        }

        if (debugCenter) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(shape.centerPoint.position, 0.1f);

        }

    }

}
