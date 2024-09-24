using System;
using System.Collections;
using System.Collections.Generic;
using Aptos.HdWallet;
using NBitcoin;
using UnityEngine;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using UnityEngine.UI;
using Aptos.Accounts;
using Newtonsoft.Json;

namespace Aptos.Unity.Sample.UI
{
    public class AptosUILink : MonoBehaviour
    {
        static public AptosUILink Instance { get; set; }

        [HideInInspector]
        public string mnemonicsKey = "MnemonicsKey";
        [HideInInspector]
        public string privateKey = "PrivateKey";
        [HideInInspector]
        public string currentAddressIndexKey = "CurrentAddressIndexKey";

        [SerializeField] private int accountNumLimit = 10;
        public List<string> addressList;

        public event Action<float> onGetBalance;

        private Wallet wallet;
        private string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {

        }

        void Update()
        {

        }

        public void InitWalletFromCache()
        {
            wallet = new Wallet(PlayerPrefs.GetString(mnemonicsKey));
            GetWalletAddress();
            LoadCurrentWalletBalance();
        }

        public bool CreateNewWallet()
        {
            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
            wallet = new Wallet(mnemo);

            PlayerPrefs.SetString(mnemonicsKey, mnemo.ToString());
            PlayerPrefs.SetInt(currentAddressIndexKey, 0);

            GetWalletAddress();
            LoadCurrentWalletBalance();

            if (mnemo.ToString() != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RestoreWallet(string _mnemo)
        {
            try
            {
                wallet = new Wallet(_mnemo);
                PlayerPrefs.SetString(mnemonicsKey, _mnemo);
                PlayerPrefs.SetInt(currentAddressIndexKey, 0);

                GetWalletAddress();
                LoadCurrentWalletBalance();

                return true;
            }
            catch
            {

            }

            return false;
        }

        public List<string> GetWalletAddress()
        {
            addressList = new List<string>();

            for (int i = 0; i < accountNumLimit; i++)
            {
                var account = wallet.GetAccount(i);
                var addr = account.AccountAddress.ToString();

                addressList.Add(addr);
            }

            return addressList;
        }

        public string GetCurrentWalletAddress()
        {
            return addressList[PlayerPrefs.GetInt(currentAddressIndexKey)];
        }

        public string GetPrivateKey()
        {
            return wallet.Account.PrivateKey;
        }

        public void LoadCurrentWalletBalance()
        {
            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            ResponseInfo responseInfo = new ResponseInfo();

            StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;

                if (responseInfo.status != ResponseInfo.Status.Success)
                {
                    onGetBalance?.Invoke(0.0f);
                }
                else
                {
                    onGetBalance?.Invoke(float.Parse(coin.Value));
                }

            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)).AccountAddress));
        }

        public IEnumerator AirDrop(int _amount)
        {
            Coroutine cor = StartCoroutine(FaucetClient.Instance.FundAccount((success, returnResult) =>
            {
            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)).AccountAddress.ToString()
                , _amount
                , faucetEndpoint));

            yield return cor;

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();
            UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully Get Airdrop of " + AptosTokenToFloat((float)_amount) + " APT");
        }

        public IEnumerator SendToken(string _targetAddress, long _amount)
        {
            Rest.Model.Transaction transferTxn = new Rest.Model.Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transferTxn, _responseInfo) =>
            {
                transferTxn = _transferTxn;
                responseInfo = _responseInfo;
            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)), _targetAddress, _amount));

            yield return transferCor;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                string transactionHash = transferTxn.Hash;
                bool waitForTxnSuccess = false;
                Coroutine waitForTransactionCor = StartCoroutine(
                    RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                    {
                        waitForTxnSuccess = _pending;
                        responseInfo = _responseInfo;
                    }, transactionHash)
                );
                yield return waitForTransactionCor;

                if (waitForTxnSuccess)
                {
                    UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully send " + AptosTokenToFloat((float)_amount) + " APT to " + UIController.Instance.ShortenString(_targetAddress, 4));
                }
                else
                {
                    UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, "Send Token Transaction Failed");
                }

            }
            else
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, responseInfo.message);
            }

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();
        }

        public IEnumerator CreateCollection(string _collectionName, string _collectionDescription, string _collectionUri)
        {
            Rest.Model.Transaction createCollectionTxn = new Rest.Model.Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine createCollectionCor = StartCoroutine(RestClient.Instance.CreateCollection((_createCollectionTxn, _responseInfo) =>
            {
                createCollectionTxn = _createCollectionTxn;
                responseInfo = _responseInfo;
            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)),
            _collectionName, _collectionDescription, _collectionUri));
            yield return createCollectionCor;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully Create Collection: " + _collectionName);
            }
            else
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, responseInfo.message);
            }

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();

            string transactionHash = createCollectionTxn.Hash;
        }

        public IEnumerator CreateNFT(string _collectionName, string _tokenName, string _tokenDescription, int _supply, int _max, string _uri, int _royaltyPointsPerMillion)
        {
            Rest.Model.Transaction createTokenTxn = new Rest.Model.Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine createTokenCor = StartCoroutine(
                RestClient.Instance.CreateToken((_createTokenTxn, _responseInfo) =>
                {
                    createTokenTxn = _createTokenTxn;
                    responseInfo = _responseInfo;
                }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)),
                _collectionName,
                _tokenName,
                _tokenDescription,
                _supply,
                _max,
                _uri,
                _royaltyPointsPerMillion)
            );
            yield return createTokenCor;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully Create NFT: " + _tokenName);
            }
            else
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, responseInfo.message);
            }

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();

            string createTokenTxnHash = createTokenTxn.Hash;
        }

        string mnemo = "voyage chalk social search pair zone husband mix lonely gather cherry beach";
        Mnemonic mnemo1 = new Mnemonic(Wordlist.English, WordCount.Twelve);

        public Button addAccountTab;
        public Button accountTab;
        public Button sendTransactionTab;
        public Button gameShopTab;
        public Button playGameTab;

        public Button buttonPiggy;
        public Button buttonDoggy;
        public Button buttonGoaty;
        public Button buttonTurtle;
        public Button buttonKoala;
        public Button buttonSheep;
        public Button buttonDuck;
        public Button buttonHeart;
        public Button buttonRefresh;

        public TMPro.TextMeshProUGUI buyingStatusText;
        public TMPro.TextMeshProUGUI aptosBalanceNoticeText;

        public Text ButtonPiggyTextPrice;
        public Text ButtonDoggyTextPrice;
        public Text ButtonGoatyTextPrice;
        public Text ButtonTurtleTextPrice;
        public Text ButtonKoalaTextPrice;
        public Text ButtonSheepTextPrice;
        public Text ButtonDuckTextPrice;
        public Text ButtonHeartTextPrice;


        public IEnumerator BuyVIPNFT(int index, bool checking)
        {
            float AccountBalance = UIController.Instance.aptosBalance;
            if (AccountBalance < 0.1f)
            {
                Debug.Log("Not enough Aptos");
                aptosBalanceNoticeText.text = "Get more Aptos to claim";
                aptosBalanceNoticeText.gameObject.SetActive(true);
                yield break;
            }
            if (checking == false)
            {
                buyingStatusText.text = "Buying!";
            }
            else
            {
                buyingStatusText.text = "Checking!";
            }

            buyingStatusText.gameObject.SetActive(true);

            buttonPiggy.interactable = false;
            buttonDoggy.interactable = false;
            buttonGoaty.interactable = false;
            buttonTurtle.interactable = false;
            buttonKoala.interactable = false;
            buttonSheep.interactable = false;
            buttonDuck.interactable = false;
            buttonHeart.interactable = false;
            buttonRefresh.interactable = false;

            addAccountTab.interactable = false;
            accountTab.interactable = false;
            sendTransactionTab.interactable = false;
            playGameTab.interactable = false;
            gameShopTab.interactable = false;

            ResponseInfo responseInfo = new ResponseInfo();

            #region REST Client Setup
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            Debug.Log("<color=cyan>=== Set Up REST Client ===</color>");
            Debug.Log("<color=cyan>=== =========================== ===</color>");

            RestClient restClient = RestClient.Instance.SetEndPoint(Constants.TESTNET_BASE_URL);
            Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp());
            yield return restClientSetupCor;
            #endregion

            Wallet wallet1 = new Wallet(mnemo);

            #region Alice Account
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Account alice = wallet1.GetAccount(0);
            string authKey = alice.AuthKey();
            Debug.Log("Alice Auth Key: " + authKey);

            AccountAddress aliceAddress = alice.AccountAddress;
            Debug.Log("Alice's Account Address: " + aliceAddress.ToString());

            PrivateKey privateKey = alice.PrivateKey;
            Debug.Log("Aice Private Key: " + privateKey);
            #endregion

            Wallet wallet2 = new Wallet(PlayerPrefs.GetString(AptosUILink.Instance.mnemonicsKey));

            #region Bob Account
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Account bob = wallet2.GetAccount(0);
            string authKeyBob = bob.AuthKey();
            Debug.Log("Bob Auth Key: " + authKeyBob);

            AccountAddress bobAddress = bob.AccountAddress;
            Debug.Log("Bob's Account Address: " + bobAddress.ToString());

            PrivateKey privateKeyBob = bob.PrivateKey;
            Debug.Log("Bob Private Key: " + privateKeyBob);
            #endregion

            #region Collection & Token Naming Details
            string collectionName = "";
            string collectionDescription = "NFTs In Hungry Frog Eats APT Coin Game";
            string collectionUri = "https://aptos.dev";

            string tokenName = "";
            string tokenDescription = "";
            string tokenUri = "";
            int propertyVersion = 0;

            if (index == 1)
            {
                collectionName = "Piggy";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Piggy";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmNgeJ9sGysTd8588rTnnjmpupoSrHcJKqxNzW4899gAq4";
            }
            else if (index == 2)
            {
                collectionName = "Doggy";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Doggy";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmcMJiifFwTExJdM73B7THCcuoaYUJEMAp5R7uP5akLvLS";
            }
            else if (index == 3)
            {
                collectionName = "Goaty";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Goaty";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmUUFSDgM6B8utJ3RDmEmPxZPLP2usDwNt4vALaKtBT7bP";
            }
            else if (index == 4)
            {
                collectionName = "Turtle";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Turtle";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmQSwLCfNjwRWEY7cMpx5bKtprUQwSFT44VLwjyGxKiiLX";
            }
            else if (index == 5)
            {
                collectionName = "Koala";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Koala";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmNi9tEdtyarbb2xsu3yQQQ7TY55U2GRejQiCKqQYX9MSw";
            }
            else if (index == 6)
            {
                collectionName = "Sheep";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Sheep";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmW77RAGssXNzPptyvbbeEDrvenm6haLPyMLpYPHcuVrzy";
            }
            else if (index == 7)
            {
                collectionName = "Duck";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Duck";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/Qmcjg2Y5FLbdE5GQRbbK1wCiovgha7awZSDdAdHjwXHzsE";
            }
            else
            {
                collectionName = "Piggy";
                collectionDescription = "Collection In Hungry Frog Eats APT Coin Game";
                collectionUri = "https://aptos.dev";

                tokenName = "Piggy";
                tokenDescription = "Cute NFT In Hungry Frog Eats APT Coin Game";
                tokenUri = "https://gateway.pinata.cloud/ipfs/QmNgeJ9sGysTd8588rTnnjmpupoSrHcJKqxNzW4899gAq4";
            }
            #endregion

            //#region Create Collection
            //Debug.Log("<color=cyan>=== Creating Collection and Token ===</color>");
            //Rest.Model.Transaction createCollectionTxn = new Rest.Model.Transaction();
            //Coroutine createCollectionCor = StartCoroutine(RestClient.Instance.CreateCollection((_createCollectionTxn, _responseInfo) =>
            //{
            //    createCollectionTxn = _createCollectionTxn;
            //    responseInfo = _responseInfo;
            //}, alice, collectionName, collectionDescription, collectionUri));
            //yield return createCollectionCor;

            //if (responseInfo.status != ResponseInfo.Status.Success)
            //{
            //    Debug.LogError("Cannot create collection. " + responseInfo.message);
            //}

            //Debug.Log("Create Collection Response: " + responseInfo.message);
            //string transactionHash1 = createCollectionTxn.Hash;
            //Debug.Log("Create Collection Hash: " + createCollectionTxn.Hash);
            //#endregion

            //#region Wait For Transaction
            //bool waitForTxnSuccess = false;
            //Coroutine waitForTransactionCor = StartCoroutine(
            //    RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
            //    {
            //        waitForTxnSuccess = _pending;
            //        responseInfo = _responseInfo;
            //    }, transactionHash1)
            //);
            //yield return waitForTransactionCor;

            //if (!waitForTxnSuccess)
            //{
            //    Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
            //    yield break;
            //}

            //#endregion

            //#region Create Non-Fungible Token
            //Rest.Model.Transaction createTokenTxn = new Rest.Model.Transaction();
            //Coroutine createTokenCor = StartCoroutine(
            //    RestClient.Instance.CreateToken((_createTokenTxn, _responseInfo) =>
            //    {
            //        createTokenTxn = _createTokenTxn;
            //        responseInfo = _responseInfo;
            //    }, alice, collectionName, tokenName, tokenDescription, 1000000000, 1000000000, tokenUri, 0)
            //);
            //yield return createTokenCor;

            //if (responseInfo.status != ResponseInfo.Status.Success)
            //{
            //    Debug.LogError("Error creating token. " + responseInfo.message);
            //}

            //Debug.Log("Create Token Response: " + responseInfo.message);
            //string createTokenTxnHash = createTokenTxn.Hash;
            //Debug.Log("Create Token Hash: " + createTokenTxn.Hash);
            //#endregion

            //#region Wait For Transaction
            //waitForTransactionCor = StartCoroutine(
            //    RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
            //    {
            //        waitForTxnSuccess = _pending;
            //        responseInfo = _responseInfo;
            //    }, transactionHash1)
            //);
            //yield return waitForTransactionCor;

            //if (!waitForTxnSuccess)
            //{
            //    Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
            //    yield break;
            //}
            //#endregion

            #region Get Collection
            string getCollectionResult = "";
            Coroutine getCollectionCor = StartCoroutine(
                RestClient.Instance.GetCollection((returnResult) =>
                {
                    getCollectionResult = returnResult;
                }, aliceAddress, collectionName)
            );
            yield return getCollectionCor;
            Debug.Log("Alice's Collection: " + getCollectionResult);
            #endregion

            #region Get Token Balance for NFT Bob Before Claim
            Debug.Log("<color=cyan>=== Get Token Balance for Bob NFT ===</color>");
            string getTokenBalanceResultBobBeforeClaim = "";
            Coroutine getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultBobBeforeClaim = returnResult;
                }, bobAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Bob's NFT Token Balance Before Claim: " + getTokenBalanceResultBobBeforeClaim);

            if (float.Parse(getTokenBalanceResultBobBeforeClaim) >= 1f)
            {
                Debug.Log("Bob Already has NFT");
                buyingStatusText.text = "You already have NFT";

                if (index == 1)
                {
                    ResourceBoost.Instance.piggy = 1;
                    ButtonPiggyTextPrice.text = "Owned";
                }
                else if (index == 2)
                {
                    ResourceBoost.Instance.doggy = 1;
                    ButtonDoggyTextPrice.text = "Owned";
                }
                else if (index == 3)
                {
                    ResourceBoost.Instance.goaty = 1;
                    ButtonGoatyTextPrice.text = "Owned";
                }
                else if (index == 4)
                {
                    ResourceBoost.Instance.turtle = 1;
                    ButtonTurtleTextPrice.text = "Owned";
                }
                else if (index == 5)
                {
                    ResourceBoost.Instance.koala = 1;
                    ButtonKoalaTextPrice.text = "Owned";
                }
                else if (index == 6)
                {
                    ResourceBoost.Instance.sheep = 1;
                    ButtonSheepTextPrice.text = "Owned";
                }
                else if (index == 7)
                {
                    ResourceBoost.Instance.duck = 1;
                    ButtonDuckTextPrice.text = "Owned";
                }

                buttonPiggy.interactable = true;
                buttonDoggy.interactable = true;
                buttonGoaty.interactable = true;
                buttonTurtle.interactable = true;
                buttonKoala.interactable = true;
                buttonSheep.interactable = true;
                buttonDuck.interactable = true;
                buttonHeart.interactable = true;
                buttonRefresh.interactable = true;

                addAccountTab.interactable = true;
                accountTab.interactable = true;
                sendTransactionTab.interactable = true;
                playGameTab.interactable = true;
                gameShopTab.interactable = true;

                //Save 12 words for sending Aptos
                //Using Singleton

                yield break;
            }
            #endregion

            if (checking == true)
            {
                yield break;
            }

            #region Get Token Balance
            Debug.Log("<color=cyan>=== Get Token Balance for Alice NFT ===</color>");
            string getTokenBalanceResultAlice = "";
            getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultAlice = returnResult;
                }, aliceAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Alice's NFT Token Balance: " + getTokenBalanceResultAlice);

            TableItemTokenMetadata tableItemToken = new TableItemTokenMetadata();

            Coroutine getTokenDataCor = StartCoroutine(
                RestClient.Instance.GetTokenData((_tableItemToken, _responseInfo) =>
                {
                    //getTokenDataResultAlice = returnResult;
                    tableItemToken = _tableItemToken;
                    responseInfo = _responseInfo;
                }, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenDataCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Could not get toke data.");
                yield break;
            }
            Debug.Log("Alice's Token Data: " + JsonConvert.SerializeObject(tableItemToken));
            #endregion


            #region Get Bob Account Balance
            responseInfo = new ResponseInfo();
            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            responseInfo = new ResponseInfo();
            Coroutine getBobBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;

            }, bobAddress));
            yield return getBobBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Bob's Balance Before Funding: " + coin.Value);
            #endregion

            #region Have Bob give Alice 0.02 Aptos coins - Submit Transfer Transaction

            int buyingCost = 2000000;

            Rest.Model.Transaction transferTxn = new Rest.Model.Transaction();
            Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transaction, _responseInfo) =>
            {
                transferTxn = _transaction;
                responseInfo = _responseInfo;
            }, bob, alice.AccountAddress.ToString(), buyingCost));

            yield return transferCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogWarning("Transfer failed: " + responseInfo.message);
                yield break;
            }

            Debug.Log("Transfer Response: " + responseInfo.message);
            string transactionHash = transferTxn.Hash;
            Debug.Log("Transfer Response Hash: " + transferTxn.Hash);
            #endregion

            #region Wait For Transaction
            bool waitForTxnSuccess = false;
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example", gameObject);
                yield break;
            }

            #endregion

            Debug.Log("<color=cyan>=== ===================== ===</color>");
            Debug.Log("<color=cyan>=== Intermediate Balances ===</color>");
            Debug.Log("<color=cyan>=== ===================== ===</color>");

            #region Get Alice Account Balance After Transfer
            Coroutine getAliceAccountBalance3 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, aliceAddress));
            yield return getAliceAccountBalance3;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice Balance After Transfer: " + coin.Value);
            #endregion

            #region Get Bob Account Balance After Transfer
            Coroutine getBobAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bobAddress));
            yield return getBobAccountBalance2;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Bob Balance After Transfer: " + coin.Value);

            #endregion

            #region Transferring the Token to Bob
            Debug.Log("<color=cyan>=== Alice Offering Token to Bob ===</color>");
            Rest.Model.Transaction offerTokenTxn = new Rest.Model.Transaction();
            Coroutine offerTokenCor = StartCoroutine(RestClient.Instance.OfferToken((_offerTokenTxn, _responseInfo) =>
            {
                offerTokenTxn = _offerTokenTxn;
                responseInfo = _responseInfo;
            }, alice, bob.AccountAddress, alice.AccountAddress, collectionName, tokenName, 1));

            yield return offerTokenCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Error offering token. " + responseInfo.message);
                yield break;
            }

            Debug.Log("Offer Token Response: " + responseInfo.message);
            Debug.Log("Offer Sender: " + offerTokenTxn.Sender);
            string offerTokenTxnHash = offerTokenTxn.Hash;
            Debug.Log("Offer Token Hash: " + offerTokenTxnHash);

            Coroutine waitForTransaction = StartCoroutine(WaitForTransaction(offerTokenTxnHash));
            yield return waitForTransaction;
            #endregion

            #region Bob Claims Token
            Debug.Log("<color=cyan>=== Bob Claims Token ===</color>");
            Rest.Model.Transaction claimTokenTxn = new Rest.Model.Transaction();
            Coroutine claimTokenCor = StartCoroutine(RestClient.Instance.ClaimToken((_claimTokenTxn, _responseInfo) =>
            {
                claimTokenTxn = _claimTokenTxn;
                responseInfo = _responseInfo;
            }, bob, alice.AccountAddress, alice.AccountAddress, collectionName, tokenName, propertyVersion));

            yield return claimTokenCor;

            Debug.Log("Claim Token Response: " + responseInfo.message);
            string claimTokenTxnHash = claimTokenTxn.Hash;
            Debug.Log("Claim Token Hash: " + claimTokenTxnHash);

            waitForTransaction = StartCoroutine(WaitForTransaction(claimTokenTxnHash));
            yield return waitForTransaction;
            #endregion

            #region Get Token Balance for NFT Alice
            Debug.Log("<color=cyan>=== Get Token Balance for Alice NFT ===</color>");
            getTokenBalanceResultAlice = "";
            getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultAlice = returnResult;
                }, aliceAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Alice's NFT Token Balance: " + getTokenBalanceResultAlice);
            #endregion

            #region Get Token Balance for NFT Bob
            Debug.Log("<color=cyan>=== Get Token Balance for Bob NFT ===</color>");
            string getTokenBalanceResultBob = "";
            getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultBob = returnResult;
                }, bobAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Bob's NFT Token Balance: " + getTokenBalanceResultBob);

            LoadCurrentWalletBalance();

            if (float.Parse(getTokenBalanceResultBob) >= 1f)
            {
                Debug.Log("Bob just bought NFT");
                buyingStatusText.text = "You just bought NFT";

                if (index == 1)
                {
                    ResourceBoost.Instance.piggy = 1;
                    ButtonPiggyTextPrice.text = "Owned";
                }
                else if (index == 2)
                {
                    ResourceBoost.Instance.doggy = 1;
                    ButtonDoggyTextPrice.text = "Owned";
                }
                else if (index == 3)
                {
                    ResourceBoost.Instance.goaty = 1;
                    ButtonGoatyTextPrice.text = "Owned";
                }
                else if (index == 4)
                {
                    ResourceBoost.Instance.turtle = 1;
                    ButtonTurtleTextPrice.text = "Owned";
                }
                else if (index == 5)
                {
                    ResourceBoost.Instance.koala = 1;
                    ButtonKoalaTextPrice.text = "Owned";
                }
                else if (index == 6)
                {
                    ResourceBoost.Instance.sheep = 1;
                    ButtonSheepTextPrice.text = "Owned";
                }
                else if (index == 7)
                {
                    ResourceBoost.Instance.duck = 1;
                    ButtonDuckTextPrice.text = "Owned";
                }

                buttonPiggy.interactable = true;
                buttonDoggy.interactable = true;
                buttonGoaty.interactable = true;
                buttonTurtle.interactable = true;
                buttonKoala.interactable = true;
                buttonSheep.interactable = true;
                buttonDuck.interactable = true;
                buttonHeart.interactable = true;
                buttonRefresh.interactable = true;

                addAccountTab.interactable = true;
                accountTab.interactable = true;
                sendTransactionTab.interactable = true;
                playGameTab.interactable = true;
                gameShopTab.interactable = true;

                //Save 12 words for sending Aptos
                //Using Singleton

                yield break;
            }

            #endregion

            yield return null;
        }

        public IEnumerator BuyMoreHearth()
        {
            float AccountBalance = UIController.Instance.aptosBalance;
            if (AccountBalance < 0.1f)
            {
                Debug.Log("Not enough Aptos");
                aptosBalanceNoticeText.text = "Get more Aptos to claim";
                aptosBalanceNoticeText.gameObject.SetActive(true);
                yield break;
            }

            buyingStatusText.text = "Buying!";
            buyingStatusText.gameObject.SetActive(true);

            buttonPiggy.interactable = false;
            buttonDoggy.interactable = false;
            buttonGoaty.interactable = false;
            buttonTurtle.interactable = false;
            buttonKoala.interactable = false;
            buttonSheep.interactable = false;
            buttonDuck.interactable = false;
            buttonHeart.interactable = false;
            buttonRefresh.interactable = false;

            addAccountTab.interactable = false;
            accountTab.interactable = false;
            sendTransactionTab.interactable = false;
            playGameTab.interactable = false;
            gameShopTab.interactable = false;

            #region REST & Faucet Client Setup
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            Debug.Log("<color=cyan>=== Set Up REST Client ===</color>");
            Debug.Log("<color=cyan>=== =========================== ===</color>");

            FaucetClient faucetClient = FaucetClient.Instance;
            RestClient restClient = RestClient.Instance.SetEndPoint(Constants.TESTNET_BASE_URL);
            Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp());
            yield return restClientSetupCor;
            #endregion

            Wallet wallet = new Wallet(PlayerPrefs.GetString(AptosUILink.Instance.mnemonicsKey));

            #region Alice Account
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Account alice = wallet.GetAccount(0);
            string authKey = alice.AuthKey();
            //Debug.Log("Alice Auth Key: " + authKey);

            AccountAddress aliceAddress = alice.AccountAddress;
            Debug.Log("Alice's Account Address: " + aliceAddress.ToString());

            PrivateKey privateKey = alice.PrivateKey;
            //Debug.Log("Aice Private Key: " + privateKey);
            #endregion

            Wallet wallet1 = new Wallet(mnemo);

            #region Bob Account
            Account bob = wallet1.GetAccount(0);
            AccountAddress bobAddress = bob.AccountAddress;
            Debug.Log("Bob's Account Address: " + bobAddress.ToString());

            Debug.Log("Wallet: Account 0: Alice: " + aliceAddress.ToString());
            Debug.Log("Wallet: Account 1: Bob: " + bobAddress.ToString());
            #endregion

            Debug.Log("<color=cyan>=== ================ ===</color>");
            Debug.Log("<color=cyan>=== Initial Balances ===</color>");
            Debug.Log("<color=cyan>=== ================ ===</color>");

            #region Get Alice Account Balance
            ResponseInfo responseInfo = new ResponseInfo();
            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            responseInfo = new ResponseInfo();
            Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;

            }, aliceAddress));
            yield return getAliceBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice's Balance Before Funding: " + coin.Value);
            #endregion

            #region Have Alice give Bob 0.02 APT coins - Submit Transfer Transaction
            Rest.Model.Transaction transferTxn = new Rest.Model.Transaction();
            Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transaction, _responseInfo) =>
            {
                transferTxn = _transaction;
                responseInfo = _responseInfo;
            }, alice, bob.AccountAddress.ToString(), 2000000));

            yield return transferCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogWarning("Transfer failed: " + responseInfo.message);
                yield break;
            }

            Debug.Log("Transfer Response: " + responseInfo.message);
            string transactionHash = transferTxn.Hash;
            Debug.Log("Transfer Response Hash: " + transferTxn.Hash);
            #endregion

            #region Wait For Transaction
            bool waitForTxnSuccess = false;
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example", gameObject);
                yield break;
            }

            #endregion

            Debug.Log("<color=cyan>=== ===================== ===</color>");
            Debug.Log("<color=cyan>=== Intermediate Balances ===</color>");
            Debug.Log("<color=cyan>=== ===================== ===</color>");

            #region Get Alice Account Balance After Transfer
            Coroutine getAliceAccountBalance3 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, aliceAddress));
            yield return getAliceAccountBalance3;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice Balance After Transfer: " + coin.Value);
            #endregion

            #region Get Bob Account Balance After Transfer
            Coroutine getBobAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bobAddress));
            yield return getBobAccountBalance2;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Bob Balance After Transfer: " + coin.Value);

            #endregion

            LoadCurrentWalletBalance();

            buttonPiggy.interactable = true;
            buttonDoggy.interactable = true;
            buttonGoaty.interactable = true;
            buttonTurtle.interactable = true;
            buttonKoala.interactable = true;
            buttonSheep.interactable = true;
            buttonDuck.interactable = true;
            buttonHeart.interactable = true;
            buttonRefresh.interactable = true;

            addAccountTab.interactable = true;
            accountTab.interactable = true;
            sendTransactionTab.interactable = true;
            playGameTab.interactable = true;
            gameShopTab.interactable = true;

            buttonHeart.gameObject.SetActive(false);

            ResourceBoost.Instance.heartBoost = 1;

            buyingStatusText.text = "Bought 1 More Heart!";

            yield return null;

        }



        public float AptosTokenToFloat(float _token)
        {
            return _token / 100000000f;
        }

        public long AptosFloatToToken(float _amount)
        {
            return Convert.ToInt64(_amount * 100000000);
        }

        IEnumerator WaitForTransaction(string txnHash)
        {
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((pending, _responseInfo) =>
                {
                    Debug.Log(_responseInfo.message);
                }, txnHash)
            );
            yield return waitForTransactionCor;
        }
    }
}