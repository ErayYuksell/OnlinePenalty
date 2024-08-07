using Photon.Pun;
using UnityEngine.SceneManagement;

namespace OnlinePenalty
{
    public class LoadingScreenController : MonoBehaviourPunCallbacks
    {
        void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            SceneManager.LoadScene("CreateRoomScreen");
        }
    }
}
