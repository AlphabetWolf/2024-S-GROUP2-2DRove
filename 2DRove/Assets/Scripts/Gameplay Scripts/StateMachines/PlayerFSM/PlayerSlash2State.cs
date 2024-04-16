using System.Collections;
using UnityEngine;

public class PlayerSlash2State : PlayerBaseState
{
    private float attackTime;
    private bool combo;
    public override void EnterState(PlayerStateManager Player)
    {
        Debug.Log("Entering Slash2 State");
        attackTime = Player.slash2Time;
        combo = false;
        Player.Coroutine(DashDelay(Player));
        Player.animator.SetTrigger("slash2");
    }

    public override void UpdateState(PlayerStateManager Player)
    {
        if(attackTime <= 0 && combo == false)
        {
            Player.animator.SetTrigger("neutral");
            Player.SwitchState(Player.NeutralState);
        }
        else if(attackTime <= (Player.slash2Time * .2f) && combo == true)
        {
            Player.SwitchState(Player.Slash3State);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            combo = true;
        }

        attackTime -= Time.deltaTime;
    }

    public override void OnCollisionEnter2D(PlayerStateManager Player, Collision2D other)
    {
        
    }
    
    public override void OnTriggerStay2D(PlayerStateManager Player, Collider2D other) 
    {

    }

    //done in animation events
    public override void EventTrigger(PlayerStateManager Player)
    {
        Vector2 knockbackDirection = (Vector2)(Player.transform.position - Player.attackPoint.position).normalized;
        LayerMask mask = LayerMask.GetMask("Enemy");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Player.attackPoint.position, Player.attackRange, mask);

        foreach (Collider2D enemy in colliders)
        {
            if (enemy is not BoxCollider2D)
                continue;
            NewEnemy enemyScript = enemy.GetComponent<NewEnemy>();
            
            if (enemyScript != null) {
                enemyScript.TakeDamage(1);
                if (enemy.CompareTag("Enemy")){
                    Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                    if (enemyRb != null) {
                        // Apply knockback
                        enemyRb.AddForce(-knockbackDirection * 5, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    public override void TakeDamage(PlayerStateManager Player)
    {
        Player.SwitchState(Player.HitState);
    }

    IEnumerator DashDelay(PlayerStateManager Player)
    {
        yield return new WaitForSeconds(Player.slash2Time * (3/4));
        Vector2 inputDirection = new Vector2(Player.findDirectionFromInputs("Left", "Right"), Player.findDirectionFromInputs("Down", "Up")).normalized;
        if (inputDirection == Vector2.zero)
        {
            //already normalized
            Player.rb.AddForce(100 * Player.slash2Lurch * Player.lastInput);
        }
        else
        {
            inputDirection.Normalize();
            Player.rb.AddForce(100 * Player.slash2Lurch * inputDirection);
            if (inputDirection.x != 0){ //If the player is moving horizontally
                Player.flipped = inputDirection.x < 0; //If the player is moving left, flipped is true, if the player is moving right, flipped is false
            }

            Player.transform.rotation = Quaternion.Euler(new Vector3(0f, Player.flipped ? 180f: 0f, 0f));
        }
    }
}
