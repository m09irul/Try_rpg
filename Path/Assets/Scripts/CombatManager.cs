﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    ObjectPooler objectPooler;
    Animator animator;
    Movement characterMovement;
    DamageHandler damageHandler;
    Coroutine attackCoroutine;
    Rigidbody2D rb;


    [Header("Hitbox section")]
    [Header("Insert in UP, RIGHT, DOWN, LEFT sequence in hitbox array")]
    [SerializeField][Tooltip("Last hitbox is for charged attack")] Transform[] hitboxes;

    [SerializeField] float leftHitboxRange, downHitboxRange, rightHitboxRange, upHitboxRange, midHitboxRange;
    [SerializeField] LayerMask hitLayer;

    [Header("Health functionality")]
    [HideInInspector]public float maxHealth, currentHealth;
    [SerializeField] int chargedDamage;
    [SerializeField] Slider healthBar;
    
    [HideInInspector] public bool isDead, isAttacking = false, isBlocked;

    [Header("Armor Attack functionality")]
    [SerializeField]  bool closedRangedArmor;
    [SerializeField]  bool longRangedArmor;
    [SerializeField]  bool magicalArmor;
    int armorType;
    [SerializeField]  bool closedRangedWeapon;
    [SerializeField]  bool longRangedWeapon;
    [SerializeField]  bool magicalWeapon;
    int weaponType;

    bool isHitCritical = false;

    Vector2 playerPos;
    Vector2 arrowPos;

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        animator = GetComponent<Animator>();
        characterMovement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        damageHandler = GetComponent<DamageHandler>();

        ResetSession();
    }

    //only for debug mode.
    private void Update()
    {
        //only for debug mode.
        //armor section
        if (closedRangedArmor && !longRangedArmor && !magicalArmor)
            armorType = 1;
        else if (longRangedArmor && !closedRangedArmor && !magicalArmor)
            armorType = 2;
        else if (magicalArmor && !closedRangedArmor && !longRangedArmor)
            armorType = 3;
        else
            armorType = 0;

        //weapon section
        if (closedRangedWeapon && !longRangedWeapon && !magicalWeapon)
            weaponType = 1;
        else if (longRangedWeapon && !closedRangedWeapon && !magicalWeapon)
            weaponType = 2;
        else if (magicalWeapon && !closedRangedWeapon && !longRangedWeapon)
            weaponType = 3;
        else
            weaponType = 0;
    }
    private void FixedUpdate()
    {
        if (!isDead)
        {
            if (characterMovement.player != null)
                playerPos = characterMovement.player.transform.position;
            arrowPos = transform.position;
        }
        //handle health bar ui
        healthBar.value = currentHealth;
    }



    #region Combat
    public void performAttack(float attackIndex, float nextAttackTime)
    {
        if(!isAttacking)
            attackCoroutine = StartCoroutine(StartAttack(attackIndex,nextAttackTime));
    }

    /// <summary>
    /// This method sets the attack animation according to the attackIndex
    /// attackIndex are responsible for playing diferent kinds of attack animation within character Attack blend tree..
    /// </summary>
    IEnumerator StartAttack(float attackIndex, float nextAttackTime)
    {

        isAttacking = true;
        rb.velocity = Vector2.zero;

        //play attack animation..
        animator.SetBool("Attack", isAttacking);
        animator.SetFloat("AttackIndex", attackIndex);

        yield return new WaitForSeconds(nextAttackTime);
        StopAttack();

    }
    /// <summary>
    /// This method is set to stop the attack
    /// </summary>
    public void StopAttack()
    {
       
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            isAttacking = false;
            animator.SetBool("Attack", isAttacking);

            if (characterMovement.MovementControl != Movement.MovementControls.chargedAttack)
            {
                characterMovement.MovementControl = Movement.MovementControls.walk;
                characterMovement.OnFreezeInputDisable();
            }
            
        }
    }

    /// <summary>
    /// This is called when character performs block operation.
    /// Movement is paused
    /// </summary>
    public void OnBlockEnable()
    {
        isBlocked = true;
        animator.SetBool("IsBlocked", isBlocked);

        rb.velocity = Vector2.zero;
        if (characterMovement.path != null)
            characterMovement.path.enabled = false;
        animator.SetFloat("Speed", 0f);
    }
    public void OnBlockDisable()
    {
        isBlocked = false;
        animator.SetBool("IsBlocked", isBlocked);
        characterMovement.MovementControl = Movement.MovementControls.walk;
        if (characterMovement.path != null)
            characterMovement.path.enabled = false;

    }

    #endregion

    #region Hitbox

    /// <summary>
    /// This methods deals the damage to characters when hit..
    /// </summary>
    public void AttackUp()
    {
        //Debug.Log("up");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[0].position, upHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(weaponType,this.transform, characterMovement.MovementControl);

        }

    }
    public void AttackRight()
    {
        //Debug.Log("right");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[1].position, rightHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(weaponType, this.transform, characterMovement.MovementControl);

        }

    }
    public void AttackDown()
    {
       // Debug.Log("down");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[2].position, downHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(weaponType, this.transform, characterMovement.MovementControl);

        }

    }
    public void AttackLeft()
    {
       // Debug.Log("left");
        Collider2D[] hitArea = Physics2D.OverlapCircleAll(hitboxes[3].position, leftHitboxRange, hitLayer);

        foreach (Collider2D hitObject in hitArea)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(weaponType, this.transform, characterMovement.MovementControl);

        }

    }
    public void ChargeAttack()
    {
        Debug.Log("mid");
        Collider2D hitObject = Physics2D.OverlapCircle(hitboxes[4].position, midHitboxRange, hitLayer);

        if (hitObject != null)
        {
            hitObject.GetComponent<CombatManager>().TakeDamage(chargedDamage, this.transform, characterMovement.MovementControl);
            characterMovement.characterHit = true;
            //characterMovement.chargedAttackTime = -1f;
            
            //StopAttack();

        }


    }

    //Get visuals of the hitboxes area..
    private void OnDrawGizmos()
    {
        if (hitboxes.Length != 0 ) {
            Gizmos.DrawWireSphere(hitboxes[0].position, upHitboxRange);
            Gizmos.DrawWireSphere(hitboxes[1].position, rightHitboxRange);
            Gizmos.DrawWireSphere(hitboxes[2].position, downHitboxRange);
            Gizmos.DrawWireSphere(hitboxes[3].position, leftHitboxRange);
            if(hitboxes.Length == 5)
                Gizmos.DrawWireSphere(hitboxes[4].position, midHitboxRange);
        }
    }
    #endregion

    #region Health system



    /// <summary>
    /// This method takes the damage by characters and handles damage afterward animations..
    /// characters dont get damage when blocking is enable
    /// </summary>
    /// <param name="damageType"> other character weapon type </param>
    /// <param name="otherCharacter"></param>
    /// <param name="AttackState"></param>
    public void TakeDamage(int damageType, Transform otherCharacter, Movement.MovementControls AttackState)
    {
        // if player hits enemy then this section of codes will activate..
        if(characterMovement.character != Movement.characters.player)
        {
            if (!isDead)
            {
                isHitCritical = characterMovement.GetHitProb();

                characterMovement.otherCharacterFacingDirection =
                            otherCharacter.GetComponent<Movement>().myFacingDirection;

                if (!isBlocked)
                {
                    //decrease health..
                    currentHealth -= damageHandler.GetDamageInfo(damageType,armorType,isHitCritical);// get damage info (damage type, armor type)

                    // DEMO...(knock off)
                    if (AttackState == Movement.MovementControls.chargedAttack && currentHealth > 0)
                    {
                        //Debug.Log(otherCharacter.GetComponent<Movement>().myFacingDirection); 

                        characterMovement.MovementControl = Movement.MovementControls.knockedOff;
                        characterMovement.OnFreezeInputEnable();
                    }
                    else
                    {
                        animator.SetTrigger("GetHit");
                        characterMovement.OnHitPush(isHitCritical);
                    }
                }
                else
                {
                    //decrease half health..
                    currentHealth -= damageHandler.GetDamageInfo(damageType, armorType,isHitCritical) * 0.5f; // get damage info (damage type, armor type)

                    OnBlockDisable();
                    characterMovement.OnHitPush(isHitCritical);
                }
                
            }
        }
        // if enemy hits player then this section of codes will activate..
        else
        {
            if(!isDead)
            { 
                currentHealth -= damageHandler.GetDamageInfo(damageType, armorType, false);// get damage info (damage type, armor type, critical hit)

                if (AttackState == Movement.MovementControls.chargedAttack && currentHealth>0)
                {
                    //Debug.Log(otherCharacter.GetComponent<Movement>().facingDirection); 
                    characterMovement.otherCharacterFacingDirection =
                            otherCharacter.GetComponent<Movement>().myFacingDirection;
                    characterMovement.MovementControl = Movement.MovementControls.knockedOff;
                    characterMovement.OnFreezeInputEnable();
                }
                else if (AttackState != Movement.MovementControls.none)
                {
                    characterMovement.otherCharacterFacingDirection =
                             otherCharacter.GetComponent<Movement>().myFacingDirection;
                    animator.SetTrigger("GetHit");
                    characterMovement.OnHitPush(false);
                }
            }
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            StopAllAvailableCoroutines();
            StartCoroutine(Die()); //Die()
        }

    }

    /// <summary>
    /// Handles death animation of character..
    /// </summary>
    IEnumerator  Die()
    {
        //play die animation..
        animator.SetBool("IsDead", isDead);
        characterMovement.OnFreezeInputEnable();
        yield return new WaitForSeconds(2f);
        //destroy enemy..

        if (gameObject.tag != "Player")
        {
            characterMovement.RestSession();
            gameObject.SetActive(false); //Destroy(gameObject, 2f);
        }
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    #endregion

    #region Archer

    /// <summary>
    /// this method is called during archer attack animation via trigger..
    /// </summary>
    public void LaunchArrow()
    {
        GameObject arrow = objectPooler.SpawnFromPool("Arrow", arrowPos, Quaternion.identity);

        Vector2 Vo = calculateVelocity(arrowPos, playerPos, 1);

        arrow.GetComponent<Rigidbody2D>().velocity = Vo;

        arrow.GetComponent<Projectile>().setRotation(Mathf.Acos(Vo.x / Vo.magnitude) * Mathf.Rad2Deg, playerPos);
    }

    /// <summary>
    /// Used to calculate the velocity vector and the trajectory..
    /// </summary>
    private Vector2 calculateVelocity(Vector2 arrowPos, Vector2 playerPos, float fightTime)
    {
        Vector2 distance = playerPos - arrowPos;
        Vector2 distanceX = distance;
        distanceX.y = 0f;

        float Sy = distance.y;
        float Sx = distanceX.magnitude;

        float Vx = Sx / fightTime;
        float Vy = Sy / fightTime + 0.5f * Mathf.Abs(Physics.gravity.y) * fightTime;

        Vector2 result = distanceX.normalized;
        result *= Vx;
        result.y = Vy;


        return result;
    }

    #endregion

    /// <summary>
    /// resets everythig of that script
    /// </summary>
    public void ResetSession()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = currentHealth;
        isDead = false;
        isBlocked = false;
    }

    void StopAllAvailableCoroutines()
    {
        StopAllCoroutines();
        characterMovement.StopAllCoroutines();
        damageHandler.StopAllCoroutines();

    }

}
