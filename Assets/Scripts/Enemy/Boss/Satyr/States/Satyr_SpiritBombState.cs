using UnityEngine;

public class Satyr_SpiritBombState : EnemyState
{
    private Enemy_Satyr satyr;
    private Vector3 targetAirPos;
    private float stateTime = 0f;
    private bool bombLaunched = false;
    private float originalGravity;

    public Satyr_SpiritBombState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public override void Enter()
    {
        base.Enter();

        if (satyr != null)
            satyr.hasHyperArmor = true;

        originalGravity = enemy.rb.gravityScale;
        enemy.rb.gravityScale = 0f;
        enemy.SetVelocity(0f, 0f);

        stateTime = 0f;
        bombLaunched = false;

        // Turn to player
        Transform player = satyr.GetPlayerTarget();
        if (player != null)
        {
            int dir = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(dir);
        }

        // Float up into air
        targetAirPos = enemy.transform.position + Vector3.up * satyr.SpiritBombFlyHeight;

        // Start charging Spirit Bomb on head
        satyr.StartSpiritBombCharge();
    }

    public override void Update()
    {
        base.Update();

        stateTime += Time.deltaTime;

        // Ascend to hover position
        if (stateTime < 0.35f)
        {
            enemy.transform.position = Vector3.Lerp(enemy.transform.position, targetAirPos, Time.deltaTime * 6f);
            enemy.SetVelocity(0f, 0f);
        }
        else if (!bombLaunched)
        {
            enemy.SetVelocity(0f, 0f);

            // Once 1s charge duration completes -> Throw bomb!
            if (stateTime >= satyr.SpiritBombChargeDuration + 0.35f)
            {
                bombLaunched = true;
                Transform player = satyr.GetPlayerTarget();
                Vector3 targetFloor = enemy.transform.position;
                if (player != null)
                {
                    // Ngắm vào giữa thân (tâm collider) thay vì gốc transform/đầu
                    targetFloor = player.TryGetComponent<Collider2D>(out var playerCol)
                        ? (Vector3)playerCol.bounds.center
                        : player.position;
                }
                satyr.LaunchSpiritBomb(targetFloor);
            }
        }
        else
        {
            // Descend back to ground
            enemy.rb.gravityScale = originalGravity;

            if ((enemy.groundDetected && stateTime >= satyr.SpiritBombChargeDuration + 0.7f) || stateTime >= satyr.SpiritBombChargeDuration + 2.5f)
            {
                satyr.SetSpiritBombOnCooldown();
                stateMachine.ChangeState(satyr.satyrBattleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (satyr != null)
            satyr.hasHyperArmor = false;

        enemy.rb.gravityScale = originalGravity;
        satyr.SetSpiritBombOnCooldown();
    }
}
