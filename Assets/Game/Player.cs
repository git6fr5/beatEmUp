using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hero))]
public class Player : MonoBehaviour {

    // Components.
    Hero hero;
    Camera mainCamera;

    // Runs once before the first frame.
    void Start() {
        hero = GetComponent<Hero>();
        mainCamera = Camera.main;
    }

    // Runs once every frame.
    void Update() {
        Input();
        Follow();
    }


    // Get the input.
    void Input() {
        hero.movement = UnityEngine.Input.GetAxisRaw("Horizontal");
        if (UnityEngine.Input.GetKeyDown(KeyCode.J)) {
            hero.attack = true;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && hero.jumpTicks == -1) {
            hero.jumpTicks = 0;
        }
    }

    void Follow() {
        Vector3 followPosition = transform.position;
        followPosition.z = -10f;
        followPosition.y = 3f;
        mainCamera.transform.position = followPosition;
    }
}
