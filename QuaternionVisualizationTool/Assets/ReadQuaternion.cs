using System;
using System.IO.Ports;
using System.Globalization;
using UnityEngine;

public class ReadQuaternion : MonoBehaviour
{
    [Header("Serial Port Settings")]
    public string portName = "COM5";
    public int baudRate = 115200;

    private SerialPort serialPort;
    private Quaternion receivedRotation = Quaternion.identity;
    private readonly object quaternionLock = new object();

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 100;
            serialPort.Open();
            Debug.Log($"Serial port {portName} opened at {baudRate} baud.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open serial port: {e.Message}");
        }
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string line = serialPort.ReadLine();
                string[] parts = line.Split(',');

                if (parts.Length == 4 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    lock (quaternionLock)
                    {
                        //receivedRotation = new Quaternion(x, y, z, w);
                        receivedRotation = new Quaternion(-x, z, y, w); // remap NED → Unity (Y-up)
                    }
                }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                Debug.LogWarning($"Serial read error: {e.Message}");
            }
        }

        lock (quaternionLock)
        {
            transform.rotation = receivedRotation;
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}
