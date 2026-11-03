using UnityEngine;

public enum CharacterGender
{
    Male,
    Female
}

public class Player_Visual : MonoBehaviour
{
    [Header("Controllers")]
    [Tooltip("Controller cho nhân vật Nam (Base Player.controller)")]
    [SerializeField] private RuntimeAnimatorController maleController;

    [Tooltip("Controller cho nhân vật Nữ (Player_Huntress.overrideController)")]
    [SerializeField] private RuntimeAnimatorController femaleController;

    [Header("Gender Selection")]
    [SerializeField] private CharacterGender currentGender = CharacterGender.Male;

    private Player player;
    private Animator anim;

    public CharacterGender CurrentGender => currentGender;
    public bool IsFemale => currentGender == CharacterGender.Female;

    private void Awake()
    {
        player = GetComponent<Player>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        bool isMale = PlayerPrefs.GetInt("Player_IsMale", 1) == 1;
        currentGender = isMale ? CharacterGender.Male : CharacterGender.Female;
        ApplyVisual();
    }

    public void ApplyVisual()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (anim == null) return;

        RuntimeAnimatorController target = (currentGender == CharacterGender.Female) ? femaleController : maleController;

        if (target != null && anim.runtimeAnimatorController != target)
        {
            anim.runtimeAnimatorController = target;
        }
    }

    public void SetGender(CharacterGender gender)
    {
        if (currentGender == gender) return;
        currentGender = gender;
        ApplyVisual();
    }

    public void ToggleGender()
    {
        SetGender(currentGender == CharacterGender.Male ? CharacterGender.Female : CharacterGender.Male);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyVisual();
    }

    private void Reset()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (maleController == null)
        {
            maleController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/AnimatorControllers/Characters/Player.controller");
        }

        if (femaleController == null)
        {
            femaleController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/AnimatorControllers/Characters/Player_Huntress.overrideController");
        }
    }

    [ContextMenu("Set Male")]
    public void ContextSetMale() => SetGender(CharacterGender.Male);

    [ContextMenu("Set Female")]
    public void ContextSetFemale() => SetGender(CharacterGender.Female);

    [ContextMenu("Toggle Gender")]
    public void ContextToggle() => ToggleGender();
#endif
}

