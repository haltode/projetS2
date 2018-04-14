﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {

    public GameObject leftPannel;
    public GameObject rightPannel;
    public GameObject activePlayer;
    public ControlsManager controls;

    public bool cheatmode = false;
    private float time = 0F;

    void Start()
    {
        controls = FindObjectOfType<ControlsManager>();
    }

    void Update()
    {
        if(Input.GetKey(controls.cheat) && Time.time >= time)
            CheatMode();
    }

    public void CheatMode()
    {
        if(cheatmode)
        {
            Debug.Log("CheatMode deactivated");
            leftPannel.SetActive(false);
            rightPannel.SetActive(false);
            cheatmode = false;
        }
        else
        {
            Debug.Log("CheatMode activated");
            leftPannel.SetActive(true);
            rightPannel.SetActive(true);
            cheatmode = true;
        }
        time = Time.time + 0.5F;
    }

    public void EndTurnButton()
    {
        Client client = FindObjectOfType<Client>();
        client.Send("CEND|" +  client.clientName);
        client.player.canPlay = false;
        EndTurn();
    }

    public void TakeTurn()
    {
        Client client = FindObjectOfType<Client>();
        client.player.canPlay = true; ;
        activePlayer.SetActive(true);
    }

    public void EndTurn()
    {
        activePlayer.SetActive(false);
    }

    public void DeconnectionButton()
    {
        Server server = FindObjectOfType<Server>();
        if(server != null)
        {
            server.server.Stop();
            Destroy(server.gameObject);
        }

        Client client = FindObjectOfType<Client>();
        if(client != null)
        {
            Destroy(client.gameObject);
        }
        SceneManager.LoadScene("Menu");

        Destroy(GameObject.Find("GameManager"));
    }
}
