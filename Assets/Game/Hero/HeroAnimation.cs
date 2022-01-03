using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Hero))]
public class HeroAnimation : MonoBehaviour {

    public struct AnimationComponent {

        public Sprite[] animation;
        public Sprite original;
        public float frameInterval;

        public AnimationComponent(Sprite[] animation, Sprite original, float interval) {
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

    public LineRenderer healthLine;
    public float healthWidth;

    [Space(5), Header("Animations")]
    public bool isAnimating;
    public bool isJumping;

    public Sprite[] idle;
    private int idleIndex = 0;
    public float idleFrameInterval = 1f / 12f;

    public Sprite[] attackA;
    public Sprite[] attackB;
    public Sprite[] comboAttack;
    public Sprite[] jump;
    public Sprite[] jumpAttack;

    [HideInInspector] public Sprite[][] animationLoop;

    // Runs once before the first frame.
    void Start() {
        // Caching.
        hero = GetComponent<Hero>();
        origin = spriteRenderer.transform.localPosition;

        // Set up.
        animationLoop = new Sprite[2][];
        animationLoop[0] = attackA; animationLoop[1] = attackB;

        StartCoroutine(IEIdleAnimator());
    }

    void Update() {

        if (hero.attackTicks > 0f && !isAnimating) {
            Sprite[] animation = animationLoop[(hero.comboCount + 1) % animationLoop.Length];
            if (hero.comboCount != 0 && hero.comboCount % hero.comboChain == 0) {
                animation = comboAttack;
            }
            if (isJumping) {
                animation = jumpAttack;
            }
            Play(animation, idle[0], hero.attackTicks);
        }

        if (hero.height != 0) {
            isJumping = true;
            spriteRenderer.transform.localPosition = origin + Vector3.up * hero.height;
        }
        else {
            isJumping = false;
            spriteRenderer.transform.localPosition = origin;
        }

        if (hero.hurt) {
            spriteRenderer.material.SetColor("_OverrideColor", Color.red);
            spriteRenderer.material.SetFloat("_Override", 1f);
        }
        else if (hero.stunTicks > 0f) {
            spriteRenderer.material.SetColor("_OverrideColor", Color.yellow);
            spriteRenderer.material.SetFloat("_Override", 1f);
        }
        else {
            spriteRenderer.material.SetFloat("_Override", 0f);
        }

        if ((float)hero.currHealth < (float)hero.maxHealth / 4f) {
            if (idleIndex % 2 == 0) {
                spriteRenderer.material.SetColor("_Color", Color.red);
            }
            else {
                spriteRenderer.material.SetColor("_Color", Color.white);
            }
        }
        else {
            spriteRenderer.material.SetColor("_Color", Color.white);
        }

        if (hero.movement > 0f) {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (hero.movement < 0f) {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Displays.
        HealthDisplay();
        DamageDisplay();
        ComboDisplay();

    }

    public void Play(Sprite[] animation, Sprite original, float interval) {
        AnimationComponent newAnimation = new AnimationComponent(animation, original, interval);
        StartCoroutine(IEAnimator(newAnimation));
        isAnimating = true;
    }

    private IEnumerator IEAnimator(AnimationComponent animComp) {
        for (int i = 0; i < animComp.animation.Length; i++) {
            if (hero.stunTicks == 0f) {
                spriteRenderer.sprite = animComp.animation[i];
            }
            yield return new WaitForSeconds(animComp.frameInterval);
        }
        idleIndex = 0;
        isAnimating = false;
    }

    private IEnumerator IEIdleAnimator() {
        while (true) {

            float frameInterval = 0f;

            if (hero.stunTicks > 0f) {
                spriteRenderer.sprite = jump[0];
            }
            else {

                idleIndex += 1;
                idleIndex = idleIndex % idle.Length;

                if (!isAnimating) {
                    spriteRenderer.sprite = idle[idleIndex];
                    if (isJumping) {
                        spriteRenderer.sprite = jump[2];
                    }
                }
                frameInterval = idleFrameInterval;
                if (hero.maxHealth > 0) {
                    frameInterval = idleFrameInterval + idleFrameInterval * (1f - hero.currHealth / hero.maxHealth);
                }
            }

            yield return new WaitForSeconds(frameInterval);

        }
    }

    public void HealthDisplay() {

        if (hero.maxHealth == 0f) {
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        Vector3 healthOrigin = healthLine.transform.position;
        positions.Add(healthOrigin);
        float health = healthWidth * hero.currHealth / hero.maxHealth;
        positions.Add(healthOrigin + Vector3.right * health);

        healthLine.startWidth = 2f / 16f;
        healthLine.endWidth = 2f / 16f;
        healthLine.positionCount = 2;
        healthLine.SetPositions(positions.ToArray());

    }

    private float prevHealth;
    private float damageTicks = 0.5f;
    public Text damageText;
    public void DamageDisplay() {

        if (prevHealth > hero.currHealth) {
            float value = Mathf.Round(100 * (hero.currHealth - prevHealth)) / 100;
            damageText.text = value.ToString();

            Text newDamageText = Instantiate(damageText).GetComponent<Text>();
            newDamageText.GetComponent<RectTransform>().SetParent(damageText.transform.parent);
            newDamageText.GetComponent<RectTransform>().localPosition = damageText.GetComponent<RectTransform>().localPosition + 0.25f * (Vector3)Random.insideUnitCircle;
            newDamageText.gameObject.SetActive(true);
            Destroy(newDamageText.gameObject, damageTicks);

        }

        prevHealth = hero.currHealth;

    }

    public Text comboText;
    public void ComboDisplay() {

        if (hero.comboCount > 1) {
            comboText.text = (hero.comboCount - 1).ToString();
            comboText.gameObject.SetActive(true);
        }
        else {
            comboText.gameObject.SetActive(false);
        }

    }

}
