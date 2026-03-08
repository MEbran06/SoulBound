using UnityEngine;
using System.IO;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    private class GameConfig
    {
        public float mouseSensitivity = 100f;
    }

    [SerializeField] float xRot = 0f;
    [SerializeField] float mouseSensibility = 100f;
    [SerializeField] Transform player;

    private string configPath;

    void Start()
    {
        configPath = Path.Combine(Application.persistentDataPath, "config.json");
        LoadConfig();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensibility * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensibility * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -20f, 70f);

        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        player.Rotate(Vector3.up * mouseX);
    }

    private void LoadConfig()
    {
        if (!File.Exists(configPath))
        {
            CreateDefaultConfig();
            return;
        }

        try
        {
            string json = File.ReadAllText(configPath);
            GameConfig config = JsonUtility.FromJson<GameConfig>(json);

            if (config != null)
                mouseSensibility = config.mouseSensitivity;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to load config.json: " + e.Message);
        }
    }

    private void CreateDefaultConfig()
    {
        GameConfig config = new GameConfig();
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(configPath, json);

        mouseSensibility = config.mouseSensitivity;
    }
}