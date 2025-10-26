using UnityEngine;
using UnityEngine.InputSystem; // Importante: aggiungi questo!

public class TelloInputMapper : MonoBehaviour
{
    // Riferimento al nostro script del drone
    public CockpitCommandManager cockpitCommManager;

    // --- Riferimenti alle AZIONI di Input ---
    // Collegheremo queste nell'Inspector di Unity
    
    // Pulsanti (Azioni singole)
    [SerializeField] public InputActionReference takeoffAction; // Es. Tasto 'A'
    [SerializeField] public InputActionReference landAction;    // Es. Tasto 'B'

    // Stick analogici (Azioni continue)
    [SerializeField] public InputActionReference moveAction;          // Es. Stick destro per muoversi
    [SerializeField] public InputActionReference altitudeYawAction;   // Es. Stick sinistro per altitudine/rotazione

    void Awake()
    {
        // Trova lo script TelloController che si trova sullo STESSO GameObject
        cockpitCommManager = GetComponent<CockpitCommandManager>();
        if (cockpitCommManager == null)
        {
            Debug.LogError("TelloController non trovato! Assicurati che sia sullo stesso GameObject.");
        }
    }

    // --- Iscrizione agli Eventi ---

    void OnEnable()
    {
        // Ci iscriviamo all'evento 'performed' (quando il pulsante viene premuto)
        takeoffAction.action.performed += OnTakeoffPerformed;
        landAction.action.performed += OnLandPerformed;
    }

    void OnDisable()
    {
        // Ci disiscriviamo per evitare errori quando l'oggetto è disattivato
        takeoffAction.action.performed -= OnTakeoffPerformed;
        landAction.action.performed -= OnLandPerformed;
    }

    // --- Funzioni Chiamate dagli Eventi ---

    private void OnTakeoffPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Input VR: DECOLLO");
        cockpitCommManager.Takeoff();
    }

    private void OnLandPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Input VR: ATTERRAGGIO");
        cockpitCommManager.Land();
    }

    // --- Gestione Stick (in Update) ---

    void Update()
    {
        if (cockpitCommManager == null) return;

        // 1. Leggi i valori degli stick
        // (Valori 2D, con X e Y che vanno da -1 a 1)
        Vector2 moveValue = moveAction.action.ReadValue<Vector2>();
        Vector2 altitudeYawValue = altitudeYawAction.action.ReadValue<Vector2>();

        // 2. Converti i valori per il comando 'rc' del Tello
        // Il Tello vuole 4 valori tra -100 e 100:
        // rc [left/right] [forward/back] [up/down] [yaw]

        // Stick Destro (Move): Assegniamo X a L/R e Y a F/B
        int lr = (int)(moveValue.x * 100);
        int fb = (int)(moveValue.y * 100);

        // Stick Sinistro (Altitude/Yaw): Assegniamo Y a U/D e X a Yaw
        int ud = (int)(altitudeYawValue.y * 100);
        int yaw = (int)(altitudeYawValue.x * 100);

        if (lr == 0 && fb == 0 && ud == 0 && yaw == 0) return;

        // 3. Invia il comando
        // Il comando 'rc' va inviato continuamente per mantenere il controllo.
        // Se tutti i valori sono 0, il drone starà fermo (hovering).
        cockpitCommManager.SetStickCommand($"rc {lr} {fb} {ud} {yaw}");
    }
}