using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hero))]
public class Enemy : MonoBehaviour {

    Hero hero;
    Player player;

    // Runs once before the first frame.
    void Start() {
        hero = GetComponent<Hero>();
        player = (Player)GameObject.FindObjectOfType<Player>();
        StartCoroutine(IEThink());
    }

    // Get the input.
    private IEnumerator IEThink() {
        while (true) {
            hero.movement = 0f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(hero.hitNode.transform.position, 1f);
            bool hitAnEnemy = false;
            for (int i = 0; i < hits.Length; i++) {
                Player player = hits[i].GetComponent<Player>();
                if (player != null) {
                    hitAnEnemy = true;
                }
            }
            if (hitAnEnemy) {
                hero.attack = true;
            }
            else {
                hero.movement = Mathf.Sign(player.transform.position.x - transform.position.x);
            }
            yield return new WaitForSeconds(0.1f);
        }
        
    }

}
