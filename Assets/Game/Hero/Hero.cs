using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RandomParams = GameRules.RandomParams;

public class Hero : MonoBehaviour {

    // Data Structures.
    [System.Serializable]
    public class Stats {

        // Parameters
        [Space(5), Header("Properties")]
        public int strength; // basic attack damage
        public int power; // special attack damage

        public int agility; // basic attack speed
        public int speed; // movement speed

        public int vitality; // total hp / regen rate
        public int toughness; // basic attack defense
        public int resistance; // special defense

        public int energy; // energy capacity / charge speed

        // Evaluate stats
        [Space(5), Header("Evaluation")]
        [HideInInspector] public static RandomParams StatRange = new RandomParams(0f, 10f);

        public static Stats Random() {
            Stats instance = new Stats(0, 0, 0, 0, 0, 0, 0, 0);
            instance.strength = StatRange.iValue;
            instance.power = StatRange.iValue;
            instance.agility = StatRange.iValue;
            instance.speed = StatRange.iValue;
            instance.vitality = StatRange.iValue;
            instance.toughness = StatRange.iValue;
            instance.resistance = StatRange.iValue;
            instance.energy = StatRange.iValue;
            return instance;
        }

        public Stats(int strength, int power, int agility, int speed, int vitality, int toughness, int resistance, int energy) {
            this.strength = strength;
            this.power = power;
            this.agility = agility;
            this.speed = speed;
            this.vitality = vitality;
            this.toughness = toughness;
            this.resistance = resistance;
            this.energy = energy;
        }

    }

    public float maxHealth;
    public float currHealth;

    // Components.
    BoxCollider2D hitbox;
    Vector2 hitboxOrigin;

    // Properties.
    [Space(5), Header("Switches")]
    public bool randomize;
    public bool standard;
    public bool reevaluate;
    public float movement;
    public bool hurt;

    [Space(5), Header("Jumping")]
    public float height;
    public float jumpForce;
    public float gravity;
    public int jumpTicks;

    [Space(5), Header("Knockback")]
    public float stunTicks;
    public int knockbackTicks;
    public float vKnockbackForce;
    public float hKnockbackForce;
    public float knockbackGravity;

    [Space(5), Header("Combos")]
    public Transform hitNode;
    private Vector3 hitNodeOrigin;
    public bool attack;
    public bool attackHit;
    public float comboTicks;
    public float comboChainCooldown;
    public int comboCount; // The current combo.
    public int comboChain; // The number of hits for a successful combo.
    public float attackTicks;
    public float attackCooldown;
    public float comboAttackCooldown;

    #region STATS
    [Space(5), Header("Stats")]
    // Actual Stats.
    public Stats stats;

    // Attack Damage
    public float attackDamage;
    public float m_AttackDamage => AttackDamageRange.Evaluate(Stats.StatRange.Ratio(stats.strength));
    public static RandomParams AttackDamageRange = new RandomParams(1f, 7f);

    // Attack Speed
    public float attackSpeed;
    [HideInInspector] public float m_AttackSpeed => AttackSpeedRange.Evaluate(Stats.StatRange.Ratio(stats.agility));
    public static RandomParams AttackSpeedRange = new RandomParams(1f, 2f);

    // Movement Speed
    public float movementSpeed;
    [HideInInspector] public float m_MovementSpeed => MovementSpeedRange.Evaluate(Stats.StatRange.Ratio(stats.speed));
    public static RandomParams MovementSpeedRange = new RandomParams(3f, 7.5f);

    // Health

    [HideInInspector] public float m_Health => HealthRange.Evaluate(Stats.StatRange.Ratio(stats.vitality));
    public static RandomParams HealthRange = new RandomParams(5f, 50f);

    // Health Regen
    public float healthRegen;
    [HideInInspector] public float m_HealthRegen => HealthRegenRange.Evaluate(Stats.StatRange.Ratio(stats.vitality));
    public static RandomParams HealthRegenRange = new RandomParams(0.05f, 2f);

    // Attack Defense
    public float attackDefense;
    [HideInInspector] public float m_AttackDefense => AttackDefenseRange.Evaluate(Stats.StatRange.Ratio(stats.toughness));
    public static RandomParams AttackDefenseRange = new RandomParams(0f, 0.75f);

    // Special Defense
    public float specialDefense;
    [HideInInspector] public float m_SpecialDefense => SpecialDefenseRange.Evaluate(Stats.StatRange.Ratio(stats.vitality));
    public static RandomParams SpecialDefenseRange = new RandomParams(0f, 0.75f);

    // Energy Capacity
    public float energyCapacity;
    [HideInInspector] public float m_EnergyCapacity => EnergyCapacityRange.Evaluate(Stats.StatRange.Ratio(stats.energy));
    public static RandomParams EnergyCapacityRange = new RandomParams(1f, 1f);

    // Energy Regen
    public float energyRegen;
    [HideInInspector] public float m_EnergyRegen => EnergyRegenRange.Evaluate(Stats.StatRange.Ratio(stats.energy));
    public RandomParams EnergyRegenRange = new RandomParams(0f, 0f);
    #endregion STATS

    // Runs once before the first frame.
    void Start() {

        hitbox = GetComponent<BoxCollider2D>();
        hitboxOrigin = hitbox.offset;
        hitNodeOrigin = hitNode.transform.localPosition;

        jumpTicks = -1;
        knockbackTicks = -1;

        comboCount = 1;
        knockbackGravity = gravity;
    }
    
    // Runs once every frame.
    void Update() {

        if (randomize) {
            stats = Stats.Random();
            reevaluate = true;
            randomize = false;
        }
        if (standard) {
            stats = new Stats(5, 5, 5, 5, 5, 5, 5, 5);
            if (GetComponent<Enemy>() != null) {
                stats = new Stats(2, 2, 2, 2, 2, 2, 2, 2);
            }
            reevaluate = true;
            standard = false;
        }
        if (reevaluate) {
            EvaluateStats();
            reevaluate = false;
        }

        Stun();
        Knockback();
        Regen();

        if (stunTicks > 0f) {
            return;
        }

        Move();
        Attack();
        Jump();
        // Point();
        Bounds();
    }

    // Updates the hero based on its stats.
    void EvaluateStats() {

        attackDamage = m_AttackDamage;

        attackSpeed = m_AttackSpeed;
        attackCooldown = 1f / (2f * m_AttackSpeed);
        comboAttackCooldown = 1f / (m_AttackSpeed);
        comboChainCooldown = 1f / (.5f * m_AttackSpeed);   

        movementSpeed = m_MovementSpeed;

        maxHealth = m_Health;
        currHealth = m_Health;

        healthRegen = m_HealthRegen;
        attackDefense = m_AttackDefense;
        specialDefense = m_SpecialDefense;
        energyCapacity = m_EnergyCapacity;
        energyRegen = m_EnergyRegen;
    }

    // Move the hero.
    void Move() {
        if (jumpTicks == -1 && attackTicks > 0f) {
            movement = 0f;
            if (attackTicks < attackCooldown / 2f) {
                movement = .25f * transform.localScale.x;
            }
        }
        Vector3 deltaPosition = Vector3.right * movement * Time.deltaTime * movementSpeed;
        transform.position += deltaPosition;
    }

    // Attack.
    void Attack() {

        comboTicks -= Time.deltaTime;
        if (comboTicks < 0) {
            comboCount = 1;
            comboTicks = 0;
        }

        attackTicks -= Time.deltaTime;
        if (attackTicks > 0f) {
            if (attackHit && attackTicks < attackCooldown / 2f) {
                Hit();
                attackHit = false;
            }
            attack = false;
            return; 
        }

        attackTicks = 0f;
        if (attack) {

            attackHit = true;

            attackTicks = attackCooldown;
            if (comboCount != 0 && comboCount % comboChain == 0) {
                attackTicks = comboAttackCooldown;
            }
        }

        attack = false;

    }

    void Jump() {

        if (jumpTicks == -1 || attackTicks > 0f) {
            return;
        }

        float currForce = jumpForce * Mathf.Pow(0.995f, jumpTicks);
        if (currForce < 0.001f) {
            currForce = 0f;
        }
        
        height += currForce * Time.deltaTime;
        if (height > 0f) {
            jumpTicks += 1;
            height -= gravity * Time.deltaTime;
        }
        else if (height < 0f) {
            height = 0f;
            jumpTicks = -1;
        }

        hitbox.offset = hitboxOrigin + Vector2.up * height;
        hitNode.transform.localPosition = hitNodeOrigin + Vector3.up * height;
    }

    void Bounds() {

    }

    void Hit() {

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitNode.transform.position, 1f);
        bool hitAnEnemy = false;
        print(hits.Length);
        for (int i = 0; i < hits.Length; i++) {
            Hero enemy = hits[i].GetComponent<Hero>();
            if (enemy != this && enemy != null) {
                hitAnEnemy = true;
                enemy.Hurt(attackDamage);
                float verticalForce = 70f;
                float horizontalForce = 25f * -Mathf.Sign(enemy.transform.position.x - transform.position.x);
                float gravityFactor = 1f;
                if (comboCount != 0 && (comboCount) % comboChain == 0) {
                    gravityFactor = .2f;
                }
                print(gravityFactor);
                enemy.Knockback(verticalForce, horizontalForce, gravityFactor);
                enemy.Stun(0.2f);
            }
        }
        
        if (hitAnEnemy) {
            comboCount += 1;
            comboTicks = comboChainCooldown;
        }
        else {
            comboCount = 1;
            comboTicks = 0f;
        }

    }

    void Regen() {
        currHealth += healthRegen * Time.deltaTime;
        if (currHealth > maxHealth) {
            currHealth = maxHealth;
        }
    }

    public void Hurt(float damage, bool special = false) {
        float defense = special ? specialDefense : attackDefense;
        damage *= (1 - defense);
        currHealth -= damage;
        comboCount = 1;
        StartCoroutine(IEHurt());
    }

    private IEnumerator IEHurt() {
        hurt = true;
        yield return new WaitForSeconds(0.05f);
        hurt = false;
        if (currHealth <= 0f) {
            Destroy(gameObject);
        }
        yield return null;
    }

    public void Knockback(float vForce = 0f, float hForce = 25f, float gravityFactor = 1f) {

        if (knockbackTicks == -1 && vForce == 0f) {
            return;
        }
        else if (vForce != 0f) {
            knockbackTicks = 0;
            vKnockbackForce = vForce;
            hKnockbackForce = hForce;
            knockbackGravity = gravity * gravityFactor;
        }

        vKnockbackForce *= Mathf.Pow(.995f, knockbackTicks);
        hKnockbackForce *= Mathf.Pow(.995f, knockbackTicks);

        if (vKnockbackForce < .001f) {
            vKnockbackForce = 0f;
        }
        height += vKnockbackForce * Time.deltaTime;
        transform.position -= transform.right * hKnockbackForce * Time.deltaTime;

        if (height > 0f) {
            knockbackTicks += 1;
            height -= knockbackGravity * Time.deltaTime;
        }
        else if (height < 0f) {
            height = 0f;
            knockbackTicks = -1;
            vKnockbackForce = 0f;
            hKnockbackForce = .3f;
            knockbackGravity = gravity;
        }

    }

    public void Stun(float duration = 0f) {

        if (duration > stunTicks) {
            stunTicks = duration;
            attackHit = false;
            comboCount = 0;
        }

        stunTicks -= Time.deltaTime;
        if (stunTicks < 0f) {
            stunTicks = 0f;
        }

    }

    void OnDrawGizmos() {

        Gizmos.color = Color.white;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitNode.transform.position, 1f);
        bool hitAnEnemy = false;
        for (int i = 0; i < hits.Length; i++) {
            Hero enemy = hits[i].GetComponent<Hero>();
            if (enemy != this && enemy != null) {
                hitAnEnemy = true;
            }
        }

        if (hitAnEnemy) {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireSphere(hitNode.transform.position, 1f);

    }

}
