using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealSoftGames.Network;
using System;

public class TestStream : MonoBehaviour, IObservable
{
    private float xPos, YPos;

    public Action OnRecieve;

    private void Awake()
    {
        OnRecieve += UpdatePos;
    }

    private void OnDestroy()
    {
        OnRecieve -= UpdatePos;
    }

    private void UpdatePos()
    {
        Debug.Log($"Pos Updated: [{xPos},{YPos}]");
    }

    public void OnSerializeView(Stream stream)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Writing");
            stream.SendNext(xPos);
            stream.SendNext(YPos);
        }
        else
        {
            Debug.Log("Recieving");
            xPos = stream.RecieveNext<float>();
            YPos = stream.RecieveNext<float>();
            OnRecieve();
        }
    }
}