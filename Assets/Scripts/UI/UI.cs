using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI instance;


    [SerializeField] private GameObject[] uiElements;
    public bool alternativeInput { get; private set; }
    private PlayerInputSet input;

    #region UI Components
    public UI_SkillToolTip skillToolTip { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }

    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }
    public UI_Storage storageUI { get; private set; }
    public UI_Craft craftUI { get; private set; }
    public UI_Merchant merchantUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    public UI_DeathScreen deathScreenUI { get; private set; }
    public UI_FadeScreen fadeScreenUI { get; private set; }
    public UI_Quest questUI { get; private set; }
    public UI_Dialogue dialogueUI { get; private set; }
    public UI_Teleport teleportUI { get; private set; }
    public UI_BossChallenge bossChallengeUI { get; private set; }
    public UI_Codex codexUI { get; private set; }
    public HintManager hintManager { get; private set; }

    #endregion

    private bool skillTreeEnabled;
    private bool inventoryEnabled;

    private void Awake()
    {
        instance = this;

        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        statToolTip = GetComponentInChildren<UI_StatToolTip>();

        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        craftUI = GetComponentInChildren<UI_Craft>(true);
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        deathScreenUI = GetComponentInChildren<UI_DeathScreen>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true);
        questUI = GetComponentInChildren<UI_Quest>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
        teleportUI = GetComponentInChildren<UI_Teleport>(true);
        bossChallengeUI = GetComponentInChildren<UI_BossChallenge>(true);
        codexUI = GetComponentInChildren<UI_Codex>(true);
        hintManager = GetComponentInChildren<HintManager>(true);
        if (hintManager == null && inGameUI != null)
        {
            hintManager = inGameUI.gameObject.AddComponent<HintManager>();
        }

        if (uiElements != null)
        {
            uiElements = System.Array.FindAll(uiElements, elem => elem != null);
        }

        skillTreeEnabled = skillTreeUI.gameObject.activeSelf;
        inventoryEnabled = inventoryUI.gameObject.activeSelf;
    }

    private void Start()
    {
        skillTreeUI.UnlockDefaultSkills();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleCodexUI();
        }
    }

    public void SetupControlsUI(PlayerInputSet inputSet)
    {
        input = inputSet;

        input.UI.SkillTreeUI.performed += ctx => ToggleSkillTreeUI();
        input.UI.InventoryUI.performed += ctx => ToggleInventoryUI();

        input.UI.AlternativeInput.performed += ctx => alternativeInput = true;
        input.UI.AlternativeInput.canceled += ctx => alternativeInput = false;

        input.UI.OptionsUI.performed += ctx =>
        { 
            if (uiElements != null)
            {
                foreach (var element in uiElements)
                {
                    if (element != null && element.activeSelf)
                    {
                        Time.timeScale = 1;
                        SwitchToInGameUI();
                        return;
                    }
                }
            }

            Time.timeScale = 0;
            OpenOptionsUI();
        };

        input.UI.DialogueInteraction.performed += ctx =>
        {
            if (dialogueUI.gameObject.activeInHierarchy)
                dialogueUI.DialogueInteraction();
        };

        input.UI.DialogueNavigation.performed += ctx =>
        {
            int direction = Mathf.RoundToInt(ctx.ReadValue<float>());

            if (dialogueUI.gameObject.activeInHierarchy)
                dialogueUI.NavigateChoice(direction);
        };
    }

    public void OpenDeathScreenUI()
    {
        SwitchTo(deathScreenUI.gameObject);
        input.Disable(); // pay attention to this if you use gamepad
    }

    public void OpenOptionsUI()
    {
        HideAllTooltips();
        StopPlayerControls(true);
        SwitchTo(optionsUI.gameObject);
    }

    public void SwitchToInGameUI()
    {
        
        HideAllTooltips();
        StopPlayerControls(false);
        SwitchTo(inGameUI.gameObject);

        skillTreeEnabled = false;
        inventoryEnabled = false;
        if (codexUI != null)
            codexUI.gameObject.SetActive(false);
    }
    
    private void SwitchTo(GameObject objectToSwitchOn)
    {
        if (uiElements != null)
        {
            foreach (var element in uiElements)
            {
                if (element != null)
                    element.SetActive(false);
            }
        }

        if (objectToSwitchOn != null)
            objectToSwitchOn.SetActive(true);
    }

    private void StopPlayerControls(bool stopControls)
    {
        if (stopControls)
            input.Player.Disable();
        else
            input.Player.Enable();
    }

    private void StopPlayerControlsIfNeeded()
    {
        foreach (var element in uiElements)
        {
            if (element != null && element.activeSelf)
            {
                StopPlayerControls(true);
                return;
            }
        }

        if (codexUI != null && codexUI.gameObject.activeSelf)
        {
            StopPlayerControls(true);
            return;
        }

        StopPlayerControls(false);
    }

    public void ToggleCodexUI()
    {
        if (codexUI == null)
        {
            codexUI = GetComponentInChildren<UI_Codex>(true);
        }

        if (codexUI == null) return;

        bool willBeActive = !codexUI.gameObject.activeSelf;
        if (willBeActive)
        {
            codexUI.transform.SetAsLastSibling();
            SetTooltipsAsLastSibling();
            if (fadeScreenUI != null)
                fadeScreenUI.transform.SetAsLastSibling();

            codexUI.gameObject.SetActive(true);
            HideAllTooltips();
            StopPlayerControls(true);
        }
        else
        {
            codexUI.gameObject.SetActive(false);
            HideAllTooltips();
            StopPlayerControlsIfNeeded();
        }
    }
    
    public void ToggleSkillTreeUI()
    {
        skillTreeUI.transform.SetAsLastSibling();
        SetTooltipsAsLastSibling();
        fadeScreenUI.transform.SetAsLastSibling();

        skillTreeEnabled = !skillTreeEnabled;
        skillTreeUI.gameObject.SetActive(skillTreeEnabled);
        HideAllTooltips();

        StopPlayerControlsIfNeeded();
    }

    public void ToggleInventoryUI()
    {
        inventoryUI.transform.SetAsLastSibling();
        SetTooltipsAsLastSibling();
        fadeScreenUI.transform.SetAsLastSibling();

        inventoryEnabled = !inventoryEnabled;
        inventoryUI.gameObject.SetActive(inventoryEnabled);
        HideAllTooltips();

        StopPlayerControlsIfNeeded();
    }

    public void OpenDialogueUI(DialogueLineSO firstLine,DialogueNpcData npcData)
    {
        StopPlayerControls(true);
        HideAllTooltips();

        dialogueUI.gameObject.SetActive(true);
        dialogueUI.SetupNpcData(npcData);
        dialogueUI.PlayDialogueLine(firstLine);
    }

    public void OpenQuestUI(QuestDataSO[] questsToShow)
    {
        StopPlayerControls(true);
        HideAllTooltips();

        questUI.gameObject.SetActive(true);
        questUI.SetupQuestUI(questsToShow);
    }

    public void OpenStorageUI(bool openStorageUI)
    {
        storageUI.gameObject.SetActive(openStorageUI);
        StopPlayerControls(openStorageUI);

        if (openStorageUI == false)
        {
            craftUI.gameObject.SetActive(false);
            HideAllTooltips();
        }
    }

    public void OpenCraftUI(bool openStorageUI)
    {
        craftUI.gameObject.SetActive(openStorageUI);
        StopPlayerControls(openStorageUI);

        if (openStorageUI == false)
        {
            storageUI.gameObject.SetActive(false);
            HideAllTooltips();
        }
    }

    public void OpenMerchantUI(bool openMerchantUI)
    {
        merchantUI.gameObject.SetActive(openMerchantUI);
        StopPlayerControls(openMerchantUI);

        if (openMerchantUI == false)
            HideAllTooltips();
    }

    public void OpenTeleportUI(bool openTeleportUI)
    {
        if (teleportUI == null && openTeleportUI)
        {
            teleportUI = GetComponentInChildren<UI_Teleport>(true);
            if (teleportUI == null)
            {
                teleportUI = UI_Teleport.CreateDefaultUI(transform);
            }
        }

        if (teleportUI != null)
        {
            teleportUI.gameObject.SetActive(openTeleportUI);
            if (openTeleportUI)
                teleportUI.SetupButtons();

            StopPlayerControls(openTeleportUI);

            if (openTeleportUI == false)
                HideAllTooltips();
        }
    }

    public void OpenBossChallengeUI(bool openBossChallengeUI)
    {
        if (bossChallengeUI == null && openBossChallengeUI)
        {
            bossChallengeUI = GetComponentInChildren<UI_BossChallenge>(true);
            if (bossChallengeUI == null)
            {
                bossChallengeUI = UI_BossChallenge.CreateDefaultUI(transform);
            }
        }

        if (bossChallengeUI != null)
        {
            bossChallengeUI.gameObject.SetActive(openBossChallengeUI);
            if (openBossChallengeUI)
                bossChallengeUI.SetupButtons();

            StopPlayerControls(openBossChallengeUI);

            if (openBossChallengeUI == false)
                HideAllTooltips();
        }
    }

    public void HideAllTooltips()
    {
        if (itemToolTip != null)
            itemToolTip.ShowToolTip(false, null);
        if (skillToolTip != null)
            skillToolTip.ShowToolTip(false, null);
        if (statToolTip != null)
            statToolTip.ShowToolTip(false, null);
    }

    private void SetTooltipsAsLastSibling()
    {
        itemToolTip.transform.SetAsLastSibling();
        skillToolTip.transform.SetAsLastSibling();
        statToolTip.transform.SetAsLastSibling();
    }
}
