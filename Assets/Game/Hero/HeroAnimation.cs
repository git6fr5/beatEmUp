using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hero))]
public class HeroAnimation : MonoBehaviour {

    public struct AnimationComponent {

        public SpriteRenderer spriteRenderer;
        public Sprite[] animation;
        public Sprite original;
        public float frameInterval;

        public AnimationComponent(SpriteRenderer spriteRenderer, Sprite[] animation, Sprite original, float interval) {
            this.spriteRenderer = spriteRenderer;
            this.animation = animation;
            this.original = original;
            this.frameInterval = interval / (float)animation.Length;
        }

    }

    // Components.
    public SpriteRenderer spriteRenderer;
    Hero hero;

    // Properties.
    [Space(5), Header("Settings")]
    public Vector3 origin;

    [Space(5), Header("Animations")]
    public bool isAnimating;
    public bool isJumping;
    private bool animationHasBeenReset;

    public Sprite idle;
    public Sprite[] attackA;
    public Sprite[] attackB;
    public Sprite[] comboAttack;
    public Sprite[] jump;


    [HideInInspector] public Sprite[][] animationLoop;

    // Runs once before the first frame.
    void Start() {
        // Caching.
        hero = GetComponent<Hero>();
        origin = spriteRenderer.transform.localPosition;

        // Set up.
        animationLoop = new Sprite[2][];
        animationLoop[0] = attackA; animationLoop[1] = attackB;
        animationHasBeenReset = true;
    }

    void Update() {

        if (isJumping) {

            if (!isAnimating) {
                spriteRenderer.sprite = jump[1];
            }

        }
        else {

            if (!isAnimating) {
                spriteRenderer.sprite = idle;
            }

            if (hero.attackTicks > 0f && !isAnimating && animationHasBeenReset) {
                Sprite[] animation = animationLoop[(hero.comboCount + 1) % animationLoop.Length];
                if (hero.comboCount % hero.comboChain == 0) {
                    animation = comboAttack;
                }
                Play(spriteRenderer, animation, idle, hero.attackCooldown, hero.attackTicks);
                animationHasBeenReset = false;
            }
        }

        if (hero.height != 0) {
            isJumping = true;
            spriteRenderer.transform.localPosition = origin + Vector3.up * hero.height;
        }
        else {
            isJumping = false;
            spriteRenderer.transform.localPosition = origin;
        }
    }

    public void Play(SpriteRenderer spriteRenderer, Sprite[] animation, Sprite original, float interval, float resetDelay) {
        AnimationComponent newAnimation = new AnimationComponent(spriteRenderer, animation, original, interval);
        StartCoroutine(IEAnimator(newAnimation));
        StartCoroutine(IEReset(resetDelay));
        isAnimating = true;
    }

    private IEnumerator IEAnimator(AnimationComponent animComp) {
        for (int i = 0; i < animComp.animation.Length; i++) {
            animComp.spriteRenderer.sprite = animComp.animation[i];
            yield return new WaitForSeconds(animComp.frameInterval);
        }
        isAnimating = false;
    }

    private IEnumerator IEReset(float delay) {
        yield return new WaitForSeconds(delay);
        animationHasBeenReset = true;
        yield return null;
    }

}
