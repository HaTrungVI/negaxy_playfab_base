using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Login : MonoBehaviour
{
    private string token;
    private const string USERDATA = "UserDataPlayFab";
    public TMP_InputField updateInput;
    public TextMeshProUGUI textData;
    public string dataInput;
    public bool UserHasExistingSave
    {
        get
        {
            return false;
        }
    }
    void Start()
    {
        LoginGooglePlay();
    }
    #region Login GooglePlay
    public async void LoginGooglePlay()
    {
        await LoginGooglePlayGames();
        LoginWithGoogleAccount();
    }
    public void LoginWithGoogleAccount()
    {
        var request = new LoginWithGooglePlayGamesServicesRequest
        {
            TitleId = PlayFabSettings.TitleId,
            ServerAuthCode = token,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithGooglePlayGamesServices(request, result =>
        {

        }, error => Debug.LogError(error.GenerateErrorReport()));
    }
    public Task LoginGooglePlayGames()
    {
        var taskSource = new TaskCompletionSource<object>();
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");
                PlayGamesPlatform.Instance.RequestServerSideAccess(true,
                    code =>
                    {
                        token = code;
                        taskSource.SetResult(null);
                    });
            }
            else
            {
                PlayGamesPlatform.Instance.ManuallyAuthenticate(success =>
                {
                    if (success == SignInStatus.Success)
                    {
                        PlayGamesPlatform.Instance.RequestServerSideAccess(true,
                            code =>
                            {
                                token = code;
                                taskSource.SetResult(null);
                            });
                    }
                    else
                    {
                        LoginAndroidDeviceID();
                    }
                });
            }
        });
        return taskSource.Task;
    }
    #endregion
    #region LoginAndroidDeviceID
    public void LoginAndroidDeviceID()
    {
        if (UserHasExistingSave)
        {
            LoginWithPlayFabRequest loginWithPlayFabRequest = new()
            {
                Username = "",
                Password = ""
            };
            PlayFabClientAPI.LoginWithPlayFab(loginWithPlayFabRequest,
            result =>
            {
                Debug.Log("Login with Google Play games successful.");
                PlayFabSettings.staticPlayer.ClientSessionTicket = result.SessionTicket;
            }, error => Debug.LogError(error.GenerateErrorReport()));
            return;
        }
        PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest()
        {
            CreateAccount = true,
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier
        }, result =>
        {
            Debug.Log("Login with Google Play games successful.");
            //DataRuntimeManager.Instance.Init();

        }, error => Debug.LogError(error.GenerateErrorReport()));
    }
    #endregion
    #region Login IOS
    public void LoginIOSDeviceID()
    {
        if (UserHasExistingSave)
        {
            LoginWithPlayFabRequest loginWithPlayFabRequest = new()
            {
                Username = "",
                Password = ""
            };
            PlayFabClientAPI.LoginWithPlayFab(loginWithPlayFabRequest,
            result =>
            {
                Debug.Log("Login with IOS successful.");
                PlayFabSettings.staticPlayer.ClientSessionTicket = result.SessionTicket;
            }, error => Debug.LogError(error.GenerateErrorReport()));
            return;
        }
        PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest()
        {
            CreateAccount = true,
            DeviceId = SystemInfo.deviceUniqueIdentifier
        }, result =>
        {
            Debug.Log("Login with IOS successful.");
            //DataRuntimeManager.Instance.Init();

        }, error => Debug.LogError(error.GenerateErrorReport()));
    }
    #endregion
    // Start is called before the first frame update
    public void GetUserData()
    {
        var keys = new List<string>
        {
            USERDATA
        };
        var request = new PlayFab.ClientModels.GetUserDataRequest()
        {
            Keys = keys
        };
        PlayFabClientAPI.GetUserData(request, resultCallback =>
        {
            Dictionary<string, PlayFab.ClientModels.UserDataRecord> userData = resultCallback.Data;
            if (userData.Count != 0)
            {
                foreach (var kvp in userData)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    if (value.Value.Length != 0)
                    {
                        if (key == USERDATA)
                        {
                            textData.text = value.Value.ToString();
                        }
                    }
                }
            }
        }, errorCallback =>
        {
            Debug.LogError(errorCallback.GenerateErrorReport());
        });
    }
    public void UpdateUserData()
    {
        var data = new Dictionary<string, string>();
        data.Add(USERDATA, updateInput.text);
        var request = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = data,
            Permission = PlayFab.ClientModels.UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, resultCallback => { Debug.Log("Update thanh cong"); }, errorCallback => { Debug.LogError(errorCallback.GenerateErrorReport()); });
    }
}
