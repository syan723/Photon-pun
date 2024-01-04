using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class PhotonManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField userNameText, RoomNameText,maxPlayersText;
    public GameObject[] uiPanels;

    #region UnityMethods
    void Start()
    {
        ActivePanel(uiPanels[0].name);
    }
 
    void Update()
    {
        Debug.Log(PhotonNetwork.NetworkClientState);
    }
    #endregion

    #region UiMethods
    public void OnLoginClick()
    {
        string name = userNameText.text;
        if (!string.IsNullOrEmpty(name))
        {
            PhotonNetwork.LocalPlayer.NickName = name;
            PhotonNetwork.ConnectUsingSettings();
            ActivePanel(uiPanels[3].name);


        }
        else
        {
            Debug.Log("Name Is Null !");
        }
    }

    public void onClickRoomCreate()
    {
        string roomName = RoomNameText.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = roomName+Random.Range(0, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte) int.Parse(maxPlayersText.text);

        PhotonNetwork.CreateRoom(roomName,roomOptions);
    }

    public void OnCancelClick()
    {
        ActivePanel(uiPanels[1].name);
    }
    #endregion

    #region PhotonCallBacks
    public override void OnConnected()
    {
        Debug.Log("Connected To Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " : Is Connected To photon");
        ActivePanel(uiPanels[1].name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " : is Created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + ": Joined The Room.");
    }

    #endregion

    #region Public_Methods

    public void ActivePanel(string panelName)
    {
        uiPanels[0].SetActive(panelName.Equals(uiPanels[0].name));
        uiPanels[1].SetActive(panelName.Equals(uiPanels[1].name));
        uiPanels[2].SetActive(panelName.Equals(uiPanels[2].name));
        uiPanels[3].SetActive(panelName.Equals(uiPanels[3].name));
        uiPanels[4].SetActive(panelName.Equals(uiPanels[4].name));

    } 

    #endregion
}
