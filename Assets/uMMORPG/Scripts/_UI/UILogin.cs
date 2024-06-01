// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public partial class UILogin : MonoBehaviour
{
    public UIPopup uiPopup;
    public NetworkManagerMMO manager; // singleton=null in Start/Awake
    public NetworkAuthenticatorMMO auth;
    public GameObject panel;
    public InputField accountInput;
    public Button loginButton;
    public Button hostButton;
    public Button dedicatedButton;
    public Button quitButton;

    public string IOS_Token;
    public string android_Token;

    void Update()
    {
        // only show while offline
        // AND while in handshake since we don't want to show nothing while
        // trying to login and waiting for the server's response
        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            panel.SetActive(true);


            // buttons. interactable while network is not active
            // (using IsConnecting is slightly delayed and would allow multiple clicks)
            loginButton.interactable = !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            loginButton.onClick.SetListener(() => { manager.StartClient(); });
            hostButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            hostButton.onClick.SetListener(() => { manager.StartHost(); });
            dedicatedButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive;
            dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });
            quitButton.onClick.SetListener(() => { NetworkManagerMMO.Quit(); });

            // inputs
            auth.loginAccount = accountInput.text;
            auth.loginPassword = "admin";

            // copy servers to dropdown; copy selected one to networkmanager ip/port.
            //serverDropdown.interactable = !manager.isNetworkActive;
            //serverDropdown.options = manager.serverList.Select(
            //    sv => new Dropdown.OptionData(sv.name)
            //).ToList();
            //manager.networkAddress = manager.serverList[serverDropdown.value].ip;
        }
        else panel.SetActive(false);
    }

    [HideInInspector] public Text logTxt;
    [HideInInspector] public string facebookToken = "....";
    void Start()
    {
        if(!Application.isMobilePlatform)
            accountInput.gameObject.SetActive(true);

        SignIn();
    }


    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            // Continue with Play Games Services

            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

            auth.loginAccount = id;
            accountInput.gameObject.SetActive(false);

        }
        else
        {

            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
        }
    }
}
