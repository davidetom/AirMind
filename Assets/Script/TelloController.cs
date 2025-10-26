using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections; // Per i thread

public class TelloController : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint remoteEP;

    // Indirizzo IP e porta del Tello per i comandi
    private string telloIP = "192.168.10.1";
    private int telloPort = 8889;

    // --- Inizializzazione ---
    void Start()
    {
        StartCoroutine(WaitForLoad());
    }

    IEnumerator WaitForLoad()
    {
        yield return new WaitForSeconds(5f);

        try
        {
            // Crea il client UDP
            udpClient = new UdpClient();

            // Definisce l'indirizzo del drone a cui inviare i comandi
            remoteEP = new IPEndPoint(IPAddress.Parse(telloIP), telloPort);

            Debug.Log("Client UDP creato. Invio del comando 'command'...");

            // Il Tello richiede questo comando iniziale per accettare altri comandi
            SendCommand("command");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Errore durante l'inizializzazione del client UDP: {e.Message}");
        }
    }

    // --- Funzione Pubblica per Inviare Comandi ---
    public void SendCommand(string command)
    {
        if (udpClient == null)
        {
            Debug.LogError("Il client UDP non è inizializzato!");
            return;
        }

        try
        {
            // Converte il comando da stringa a un array di byte
            byte[] data = Encoding.UTF8.GetBytes(command);

            // Invia i dati al drone
            udpClient.Send(data, data.Length, remoteEP);

            Debug.Log($"Comando inviato al Tello: {command}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Errore nell'invio del comando '{command}': {e.Message}");
        }
    }

    // --- Pulizia alla Chiusura ---
    void OnDestroy()
    {
        // È FONDAMENTALE chiudere la connessione e far atterrare il drone
        // per sicurezza quando l'app si chiude o si ferma la simulazione.

        Debug.Log("Chiusura dell'applicazione...");

        // Fai atterrare il drone per sicurezza
        SendCommand("land");

        if (udpClient != null)
        {
            udpClient.Close();
            Debug.Log("Client UDP chiuso.");
        }
    }

    [ContextMenu("Take off")]
    public void TakeOff()
    {
        SendCommand("takeoff");
    }

    [ContextMenu("Land")]
    public void Land()
    {
        SendCommand("land");
    }
}
