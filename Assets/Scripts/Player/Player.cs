using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Entity
{
    public static Player instance;
    public static event Action OnPlayerDeath;

    public UI ui { get; private set; }
    public PlayerInputSet input { get; private set; }
    public Player_SkillManager skillManager { get; private set; }
    public Player_VFX vfx { get; private set; }
    public Entity_Health health { get; private set; }
    public Entity_StatusHandler statusHandler { get; private set; }
    public Player_Combat combat { get; private set; }
    public Inventory_Player inventory { get; private set; }
    public Player_Stats stats { get; private set; }
    public Player_QuestManager questManager { get; private set; }
    public Player_DragonForm dragonForm { get; private set; }

    [Header("Ground Position")]
    [SerializeField] private Transform groundPos;
    public Transform GroundPos => groundPos;


    #region State Variables

    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttackState jumpAttackState { get; private set; }
    public Player_DeadState deadState { get; private set; }
    public Player_CounterAttackState counterAttackState { get; private set; }
    public Player_SwordThrowState swordThrowState { get; private set; }
    public Player_DomainExpansionState domainExpansionState { get; private set; }

    #endregion

    [Header("Attack details")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = .1f;
    public float comboResetTime = 1;
    private Coroutine queuedAttackCo;

    [Header("Ultimate ability details")]
    public float riseSpeed = 25;
    public float riseMaxDistance = 3;

    [Header("Movement details")]
    public float moveSpeed;
    public float jumpForce = 5;
    public Vector2 wallJumpForce;
    [Range(0, 1)]
    public float inAirMoveMultiplier = .7f; // Should be from 0 to 1;
    [Range(0, 1)]
    public float wallSlideSlowMultiplier = .7f;
    [Space]
    public float dashDuration = .25f;
    public float dashSpeed = 20;
    public Vector2 moveInput { get; private set; }
    public Vector2 mousePosition { get; private set; }

    private float defaultMoveSpeed;
    private float defaultJumpForce;
    private float defaultAnimSpeed;
    private Vector2 defaultWallJump;
    private Vector2 defaultJumpAttack;
    private Vector2[] defaultAttackVelocity;

    protected override void Awake()
    {
        base.Awake();
        instance = this;

        ui = FindAnyObjectByType<UI>();
        vfx = GetComponent<Player_VFX>();
        health = GetComponent<Entity_Health>();
        skillManager = GetComponent<Player_SkillManager>();
        statusHandler = GetComponent<Entity_StatusHandler>();
        combat = GetComponent<Player_Combat>();
        inventory = GetComponent<Inventory_Player>();
        stats = GetComponent<Player_Stats>();
        questManager = GetComponent<Player_QuestManager>();

        if (groundPos == null)
            groundPos = transform.Find("GroundPos");

        if (groundPos == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "GroundPos")
                {
                    groundPos = child;
                    break;
                }
            }
        }

        input = new PlayerInputSet();
        if (ui != null)
            ui.SetupControlsUI(input);

        dragonForm = GetComponent<Player_DragonForm>() ?? gameObject.AddComponent<Player_DragonForm>();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttackState(this, stateMachine, "jumpAttack");
        deadState = new Player_DeadState(this, stateMachine, "dead");
        counterAttackState = new Player_CounterAttackState(this, stateMachine, "counterAttack");
        swordThrowState = new Player_SwordThrowState(this, stateMachine, "swordThrow");
        domainExpansionState = new Player_DomainExpansionState(this, stateMachine, "jumpFall");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultAnimSpeed = anim != null ? anim.speed : 1f;
        defaultWallJump = wallJumpForce;
        defaultJumpAttack = jumpAttackVelocity;
        if (attackVelocity != null)
            defaultAttackVelocity = (Vector2[])attackVelocity.Clone();
    }

    public void TeleportPlayer(Vector3 position) => transform.position = position;

    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        float speedMultiplier = 1 - slowMultiplier;

        moveSpeed = defaultMoveSpeed * speedMultiplier;
        jumpForce = defaultJumpForce * speedMultiplier;
        if (anim != null) anim.speed = defaultAnimSpeed * speedMultiplier;
        wallJumpForce = defaultWallJump * speedMultiplier;
        jumpAttackVelocity = defaultJumpAttack * speedMultiplier;

        if (defaultAttackVelocity != null && attackVelocity != null)
        {
            for (int i = 0; i < attackVelocity.Length; i++)
            {
                attackVelocity[i] = defaultAttackVelocity[i] * speedMultiplier;
            }
        }

        yield return new WaitForSeconds(duration);

        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        if (anim != null) anim.speed = defaultAnimSpeed;
        wallJumpForce = defaultWallJump;
        jumpAttackVelocity = defaultJumpAttack;

        if (defaultAttackVelocity != null && attackVelocity != null)
        {
            for (int i = 0; i < attackVelocity.Length; i++)
            {
                attackVelocity[i] = defaultAttackVelocity[i];
            }
        }
    }

    public override void EntityDeath()
    {
        base.EntityDeath();

        OnPlayerDeath?.Invoke();
        stateMachine.ChangeState(deadState);
    }

    public void EnterAttackStateWithDelay()
    {
        if (queuedAttackCo != null)
            StopCoroutine(queuedAttackCo);

        queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }

    private IEnumerator EnterAttackStateWithDelayCo()
    {
        yield return new WaitForEndOfFrame();

        if (health != null && health.isDead)
            yield break;

        if (stateMachine.currentState == deadState)
            yield break;

        stateMachine.ChangeState(basicAttackState);
    }

    private void TryInteract()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Collider2D[] objectsAround = Physics2D.OverlapCircleAll(transform.position, 1f);

        foreach (var target in objectsAround)
        {
            IInteractable interactable = target.GetComponent<IInteractable>() ?? target.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target.transform;
            }
        }

        if (closest == null)
            return;

        IInteractable finalInteractable = closest.GetComponent<IInteractable>() ?? closest.GetComponentInParent<IInteractable>();
        finalInteractable?.Interact();
    }

    private void OnEnable()
    {
        if (input == null)
            input = new PlayerInputSet();

        input.Enable();

        input.Player.Mouse.performed += HandleMousePosition;

        input.Player.Movement.performed += HandleMovement;
        input.Player.Movement.canceled += HandleMovementCanceled;

        input.Player.Spell.performed += HandleSpell;

        input.Player.Interact.performed += HandleInteract;

        input.Player.QuickItemSlot_1.performed += HandleQuickItem1;
        input.Player.QuickItemSlot_2.performed += HandleQuickItem2;
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Player.Mouse.performed -= HandleMousePosition;

            input.Player.Movement.performed -= HandleMovement;
            input.Player.Movement.canceled -= HandleMovementCanceled;

            input.Player.Spell.performed -= HandleSpell;

            input.Player.Interact.performed -= HandleInteract;

            input.Player.QuickItemSlot_1.performed -= HandleQuickItem1;
            input.Player.QuickItemSlot_2.performed -= HandleQuickItem2;

            input.Disable();
        }
    }

    private void HandleMousePosition(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => mousePosition = ctx.ReadValue<Vector2>();
    private void HandleMovement(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void HandleMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
    private void HandleInteract(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => TryInteract();
    private void HandleQuickItem1(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => inventory?.TryUseQuickItemInSlot(1);
    private void HandleQuickItem2(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => inventory?.TryUseQuickItemInSlot(2);

    private void HandleSpell(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (Player_DragonForm.instance != null && Player_DragonForm.instance.IsDragonForm)
            return;

        if (skillManager == null)
            return;

        if (skillManager.timeEcho != null && skillManager.timeEcho.CanUseSkill())
            skillManager.timeEcho.TryUseSkill();
        else if (skillManager.shard != null && skillManager.shard.CanUseSkill())
            skillManager.shard.TryUseSkill();
    }

    private void OnDestroy()
    {
        OnPlayerDeath = null;
        if (instance == this)
            instance = null;
    }
}
