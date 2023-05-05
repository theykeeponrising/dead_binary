using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static AudioSource AudioSource;
    public static GameObject IndicatorAOE;

    static StartMenuUI _startMenuUI;
    static StatusMenuUI _statusMenuUI;
    static GameOverUI _gameOverUI;
    static GameWinUI _gameWinUI;
    static ActionPanelScript _actionPanel;
    static InCombatPlayerActionUI _inCombatPlayerActionUI;
    static InventoryPanelScript _inventoryPanel;
    static InfoPanelScript _infoPanel;
    static TurnIndicatorPanel _turnIndicator;
    static DialogPanel _dialogPanel;
    static TextMeshProUGUI _stateDebugText;

    [SerializeField] private GameObject _indicatorAOE;

    private void Awake()
    {
        Instance = this;
        AudioSource = GetComponent<AudioSource>();
        IndicatorAOE = _indicatorAOE;
    }

    public static StartMenuUI GetStartMenu()
    {
        if (!_startMenuUI) _startMenuUI = Instance.GetComponentInChildren<StartMenuUI>();
        return _startMenuUI;
    }

    public static InfoPanelScript GetInfoPanel()
    {
        if (!_infoPanel) _infoPanel = Instance.GetComponentInChildren<InfoPanelScript>();
        return _infoPanel;
    }

    public static StatusMenuUI GetStatusMenu()
    {
        if (!_statusMenuUI) _statusMenuUI = Instance.GetComponentInChildren<StatusMenuUI>();
        return _statusMenuUI;
    }

    public static GameOverUI GetGameOverMenu()
    {
        if (!_gameOverUI) _gameOverUI = Instance.GetComponentInChildren<GameOverUI>();
        return _gameOverUI;
    }

    public static GameWinUI GetGameWinMenu()
    {
        if (!_gameWinUI) _gameWinUI = Instance.GetComponentInChildren<GameWinUI>();
        return _gameWinUI;
    }

    public static ActionPanelScript GetActionPanel()
    {
        if (!_actionPanel) _actionPanel = Instance.GetComponentInChildren<ActionPanelScript>();
        return _actionPanel;
    }

    public static InCombatPlayerActionUI GetPlayerAction()
    {
        if (!_inCombatPlayerActionUI) _inCombatPlayerActionUI = Instance.GetComponentInChildren<InCombatPlayerActionUI>();
        return _inCombatPlayerActionUI;
    }

    public static InventoryPanelScript GetInventoryPanel()
    {
        if (!_inventoryPanel) _inventoryPanel = Instance.GetComponentInChildren<InventoryPanelScript>();
        return _inventoryPanel;
    }

    public static TurnIndicatorPanel GetTurnIndicator()
    {
        if (!_turnIndicator) _turnIndicator = Instance.GetComponentInChildren<TurnIndicatorPanel>();
        return _turnIndicator;
    }

    public static DialogPanel GetDialogPanel()
    {
        if (!_dialogPanel) _dialogPanel = Instance.GetComponentInChildren<DialogPanel>();
        return _dialogPanel;
    }

    public static TextMeshProUGUI GetStateDebug()
    {
        if (!_stateDebugText) _stateDebugText = Instance.transform.Find("StateDebugText").GetComponent<TextMeshProUGUI>();
        return _stateDebugText;
    }

    public static GameObject InstantiateIndicatorAOE(Vector3 position)
    {
        return Map.MapEffects.CreateEffect(IndicatorAOE, position, Quaternion.identity);
    }

    public static void DestroyIndicatorAOE(GameObject indicatorAOE)
    {
        Map.MapEffects.DestroyEffect(indicatorAOE);
    }
}
