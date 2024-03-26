using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class NetWorkUtils : MonoBehaviour
{
    public static string GetLocalIPAddress()
    {
        string ipAddress = null;
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip.ToString();
                break;
            }
        }
        return ipAddress;
    }
}