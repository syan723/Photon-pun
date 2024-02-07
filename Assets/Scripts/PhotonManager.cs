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
    private Dictionary<string, RoomInfo> roomListData;
    public GameObject roomListPrefab;
    public GameObject roomListParent;
    private Dictionary<string, GameObject> roomListGameObject;
    public GameObject playerListItemPrefab;
    public GameObject playerListItemParent;
    private Dictionary<int , GameObject> playerListGameObject;
    public GameObject startBtn;




    #region UnityMethods
    void Start()
    {
        ActivePanel(uiPanels[0].name);
        roomListData = new Dictionary<string, RoomInfo>();
        roomListGameObject = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update()
    {
        Debug.Log(PhotonNetwork.NetworkClientState);
    }
    #endregion

    #region UiMethods

    public void BackFromRoomList()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivePanel(uiPanels[1].name);
    }
    public void BackFromPlayerList()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        ActivePanel(uiPanels[1].name);
    }
    public void OnLoginClick()
    {
        string name = userNameText.text;
        if (!string.IsNullOrEmpty(name))
        {
            
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.LocalPlayer.NickName = name;
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

    public void OnRoomListBtnClicked()
    {
        if(!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivePanel(uiPanels[4].name);
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
   /* public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
    }*/
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + ": Joined The Room.");
        ActivePanel(uiPanels[5].name);
        if(playerListGameObject == null)
        {
            playerListGameObject = new Dictionary<int, GameObject>();
        }

        if(PhotonNetwork.IsMasterClient)
        {
            startBtn.SetActive(true);
        }
        else
        {
            startBtn.SetActive(false);
        }
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject playerListItem = Instantiate(playerListItemPrefab);
            playerListItem.transform.SetParent(playerListItemParent.transform);
            playerListItem.transform.localScale = Vector3.one;

            playerListItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = p.NickName;
            if(p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            { 
            playerListItem.transform.GetChild(1).gameObject.SetActive(true);

            }
            playerListGameObject.Add(p.ActorNumber, playerListItem);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject playerListItem = Instantiate(playerListItemPrefab);
        playerListItem.transform.SetParent(playerListItemParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        playerListItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text =newPlayer.NickName;
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListItem.transform.GetChild(1).gameObject.SetActive(true);

        }
        playerListGameObject.Add(newPlayer.ActorNumber, playerListItem);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListGameObject[otherPlayer.ActorNumber]);
        playerListGameObject.Remove(otherPlayer.ActorNumber);
        if(PhotonNetwork.IsMasterClient)
        {
            startBtn.SetActive(true);
        }
        else
        {
            startBtn.SetActive(false);
        }
    }
    public void OnClickStartBtn()
    {
        PhotonNetwork.LoadLevel("Game");
    }
    public override void OnLeftRoom()
    {
        ActivePanel(uiPanels[1].name);
        foreach (GameObject obj in playerListGameObject.Values){
            Destroy(obj);
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear List

        if (roomListGameObject.Count > 0)
        {
            ClearRoomList();
        }

        foreach (RoomInfo rooms in roomList)
        {
            Debug.LogError("Room Name : " + rooms.Name);
            if (!rooms.IsOpen || !rooms.IsVisible || rooms.RemovedFromList)
            {
                if (roomListData.ContainsKey(rooms.Name))
                {
                    roomListData.Remove(rooms.Name);
                }
            }
            else
            {
                if (roomListData.ContainsKey(rooms.Name))
                {
                    roomListData[rooms.Name] = rooms;
                }
                else
                {
                    roomListData.Add(rooms.Name, rooms);

                }
            }
        }

        // GEnerate List Items
        foreach(RoomInfo roomItem in roomListData.Values)
        {
            GameObject roomListItemObject = Instantiate(roomListPrefab);
            roomListItemObject.transform.SetParent(roomListParent.transform);
            roomListItemObject.transform.localScale = Vector3.one;
            // room Namem, Player Number , button Join 
            roomListItemObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = roomItem.Name;
            roomListItemObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = roomItem.PlayerCount + "/" + roomItem.MaxPlayers;
            roomListItemObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(()=>RoomJoinFromList(roomItem.Name));

            roomListGameObject.Add(roomItem.Name,roomListItemObject);
        }
    }

    public override void OnLeftLobby()
    {
       ClearRoomList();
        roomListData.Clear();
    }
    #endregion

    #region Public_Methods


    public void RoomJoinFromList(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }
    public void ClearRoomList()
    {
        foreach(var v in roomListGameObject.Values)
        {
            Destroy(v);
        }
        roomListGameObject.Clear();
    }
    public void ActivePanel(string panelName)
    {
        uiPanels[0].SetActive(panelName.Equals(uiPanels[0].name));
        uiPanels[1].SetActive(panelName.Equals(uiPanels[1].name));
        uiPanels[2].SetActive(panelName.Equals(uiPanels[2].name));
        uiPanels[3].SetActive(panelName.Equals(uiPanels[3].name));
        uiPanels[4].SetActive(panelName.Equals(uiPanels[4].name));
        uiPanels[5].SetActive(panelName.Equals(uiPanels[5].name));

    } 


    #endregion
}
