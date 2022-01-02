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

    // Properties.
    [Space(5), Header("Switches")]
    public bool randomize;
    public bool standard;
    public bool reevaluate;
    public float movement;

    [Space(5), Header("Jumping")]
    public float height;
    public float jumpForce;
    public float gravity;
    public int jumpTicks;

    [Space(5), Header("Combos")]
    public Transform hitNode;
    public bool attack;
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
    public float m_AttackDamage => attackDamageRange.Evaluate(Stats.StatRange.Ratio(stats.strength));
    public RandomParams attackDamageRange;

    // Attack Speed
    public float attackSpeed;
    [HideInInspector] public float m_AttackSpeed => attackSpeedRange.Evaluate(Stats.StatRange.Ratio(stats.agility));
    public RandomParams attackSpeedRange;

    // Movement Speed
    public float movementSpeed;
    [HideInInspector] public float m_MovementSpeed => movementSpeedRange.Evaluate(Stats.StatRange.Ratio(stats.speed));
    public RandomParams movementSpeedRange;

    // Health
    public float health;
    [HideInInspector] public float m_Health => healthRange.Evaluate(Stats.StatRange.Ratio(stats.vitality));
    public RandomParams healthRange;

    // Health Regen
    public float healthRegen;
    [HideInInspector] public float m_HealthRegen => healthRegenRange.Evaluate(Stats.StatRange.Ratio(stats.vitality));
    public RandomParams healthRegenRange;

    // Attack Defense
    public float attackDefense;
    [HideInInspector] public float m_AttackDefense => attackDefenseRange.Evaluate(Stats.StatRange.Ratio(stats.toughness));
    public RandomParams attackDefenseRange;

    // Special Defense
    public float specialDefense;
    [HideInInspector] public float m_SpecialDefense => specialDefenseRange.Evaluate(Stats.StatRange.Ratio(stats.vitality));
    public RandomParams specialDefenseRange;

    // Energy Capacity
    public float energyCapacity;
    [HideInInspector] public float m_EnergyCapacity => energyCapacityRange.Evaluate(Stats.StatRange.Ratio(stats.energy));
    public RandomParams energyCapacityRange;

    // Energy Regen
    public float energyRegen;
    [HideInInspector] public float m_EnergyRegen => energyRegenRange.Evaluate(Stats.StatRange.Ratio(stats.energy));
    public RandomParams energyRegenRange;
    #endregion STATS
    
    // Runs once every frame.
    void Update() {

        if (randomize) {
            stats = Stats.Random();
            reevaluate = true;
            randomize = false;
        }
        if (standard) {
            stats = new Stats(5, 5, 5, 5, 5, 5, 5, 5);
            reevaluate = true;
            standard = false;
        }
        if (reevaluate) {
            EvaluateStats();
            reevaluate = false;
        }

        Input();
        Move();
        Attack();
        Jump();
    }

    // Updates the hero based on its stats.
    void EvaluateStats() {
        attackDamage = m_AttackDamage;

        attackSpeed = m_AttackSpeed;
        attackCooldown = 1f / (2f * m_AttackSpeed);
        comboAttackCooldown = 1f / (.5f * m_AttackSpeed);
        comboChainCooldown = 1f / (.35f * m_AttackSpeed);   

        movementSpeed = m_MovementSpeed;
        health = m_Health;
        healthRegen = m_HealthRegen;
        attackDefense = m_AttackDefense;
        specialDefense = m_SpecialDefense;
        energyCapacity = m_EnergyCapacity;
        energyRegen = m_EnergyRegen;
    }

    // Get the input.
    void Input() {
        movement = UnityEngine.Input.GetAxisRaw("Horizontal");
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) {
            attack = true;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.J) && jumpTicks == -1) {
            jumpTicks = 0;
        }
    }

    // Move the hero.
    void Move() {
        Vector3 deltaPosition = Vector3.right * movement * Time.deltaTime * movementSpeed;
        transform.position += deltaPosition;
    }

    // Attack.
    void Attack() {

        comboTicks -= Time.deltaTime;
        if (comboTicks < 0) {
            comboCount = 0;
            comboTicks = 0;
        }

        attackTicks -= Time.deltaTime;
        if (attackTicks > 0f) { return; }

        attackTicks = 0f;
        if (attack) {

            Hit();
            attack = false;

            attackTicks = attackCooldown;
            if (comboCount > 0 && comboCount % comboChain == 0) {
                attackTicks = comboAttackCooldown;
            }
        }

    }

    void Jump() {

        if (jumpTicks == -1) {
            return;
        }

        float currForce = jumpForce * Mathf.Pow(0.995f, jumpTicks);
        if (currForce < 0.001f) {
            currForce = 0f;
        }
        print(currForce);
        height += currForce * Time.deltaTime;
        if (height > 0f) {
            jumpTicks += 1;
            height -= gravity * Time.deltaTime;
        }
        else if (height < 0f) {
            height = 0f;
            jumpTicks = -1;
        }
    }

    void Hit() {

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitNode.transform.position, 1f);
        bool hitAnEnemy = false;
        print(hits.Length);
        for (int i = 0; i < hits.Length; i++) {
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null) {
                hitAnEnemy = true;
                enemy.Hurt();
            }
        }
        hitAnEnemy = true;
        if (hitAnEnemy) {
            comboCount += 1;
            comboTicks = comboChainCooldown;
        }

    }

    void OnDrawGizmos() {

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(hitNode.transform.position, 1f);

    }

}
