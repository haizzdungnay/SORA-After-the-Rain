using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    public enum InteractKind { Register, Shelf, Delivery, StockCount, Customer, ShiftClock, ExitDoor }

    public sealed class SoraInteractable : MonoBehaviour
    {
        public InteractKind kind;
        public string title;
        public int customerIndex;
    }

    [Serializable]
    public sealed class SoraSaveData
    {
        public int schema = 1;
        public SoraWeatherState weather;
        public bool hasWeatherSnapshot;
        public int activeIndex;
        public string toshiharuModel;
        public string akikoModel;
        public float[] characterPositions = new float[6];
        public int storeCash = 0;
        public int sales = 0;
        public int shelfStock = 8;
        public int backStock = 0;
        public bool deliveryReceived;
        public int[] familiarity = new int[6];
        public int[] trust = new int[6];
        public int restockCount;
        public int deliveries;
    }

    public sealed class SoraDemoCore : MonoBehaviour
    {
        public static SoraDemoCore Instance { get; private set; }
        public readonly List<SoraInteractable> Interactables = new List<SoraInteractable>();
        public readonly List<SoraDemoCharacter> Characters = new List<SoraDemoCharacter>();
        public readonly string[] CustomerNames = { "Rei", "Miyu", "Kazuki" };
        public readonly string[] CustomerDetails = { "Khach quen thich ca phe", "Sinh vien cung truong", "Khach mua sam buoi toi" };
        public SoraSaveData State = new SoraSaveData();
        public SoraInteractable CurrentTarget;
        public bool IsDialogue { get; private set; }
        public int DialogueCustomer { get; private set; }
        public bool ShiftSummary { get; private set; }
        public bool HasPendingSale { get; private set; }
        public bool IsChoosingModels { get; private set; }
        public string Notice { get; private set; } = "Chao mung den SORA: After the Rain - prototype 0.1";
        private float noticeUntil;
        private SoraThirdPersonCamera followCam;
        private GUIStyle titleStyle, hudStyle, infoStyle, buttonStyle, overlayStyle;
        private bool initialized;
        private float begunAt;
        private const int SalePrice = 380;
        private Vector2 modelMenuScroll;

        public SoraDemoCharacter ActiveCharacter { get { return Characters.Count == 2 ? Characters[State.activeIndex] : null; } }
        public string SavePath { get { return Path.Combine(Application.persistentDataPath, "sora_demo_v01.json"); } }

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            SoraDemoWorld.Build(this);
            if (Characters.Count != 2)
            {
                Debug.LogError("SORA: failed to create both twins.");
                return;
            }
            State.activeIndex = 0;
            SelectCharacterModel(0, "Zhongli");
            SelectCharacterModel(1, "Chisa");
            ConfigureActive();
            followCam = FindFirstObjectByType<SoraThirdPersonCamera>();
            if (followCam != null) followCam.SetTarget(ActiveCharacter.transform);
            begunAt = Time.time;
            initialized = true;
            SetModelSelectionOpen(true);
            ShowNotice("Ca dau tien tai Tokyo! WASD di chuyen / E tuong tac / TAB doi anh em", 8f);
        }

        private void Update()
        {
            if (!initialized || ActiveCharacter == null) return;
            if (Input.GetKeyDown(KeyCode.M)) SetModelSelectionOpen(!IsChoosingModels);
            if (Input.GetKeyDown(KeyCode.F5)) Save();
            if (Input.GetKeyDown(KeyCode.F9)) Load();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                IsChoosingModels = false;
                IsDialogue = false;
                ShiftSummary = false;
                HasPendingSale = false;
                ShowNotice("Da dong menu / huy hoa don dang cho.");
                return;
            }
            if (IsChoosingModels)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) SetModelSelectionOpen(false);
                int key = 0;
                for (int twin = 0; twin < 2; twin++)
                    foreach (var model in SoraCharacterModels.All)
                        if (model.TwinIndex == twin && Input.GetKeyDown(KeyCode.Alpha1 + key++)) SelectCharacterModel(twin, model.Id);
                return;
            }
            if (ShiftSummary) return;
            if (IsDialogue)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseDialogue(1);
                if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseDialogue(2);
                if (Input.GetKeyDown(KeyCode.Alpha3)) IsDialogue = false;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Tab)) SwitchCharacter();
            CurrentTarget = FindNearest(ActiveCharacter.transform.position, 2.45f);
            if (CurrentTarget != null && Input.GetKeyDown(KeyCode.E)) Interact(CurrentTarget);
        }

        private SoraInteractable FindNearest(Vector3 from, float distance)
        {
            SoraInteractable nearest = null;
            float min = distance * distance;
            foreach (SoraInteractable target in Interactables)
            {
                if (target == null || !target.gameObject.activeInHierarchy) continue;
                Vector3 offset = target.transform.position - from;
                offset.y = 0f; // Interaction range is horizontal, even for floating NPC markers.
                float d = offset.sqrMagnitude;
                if (d < min) { min = d; nearest = target; }
            }
            return nearest;
        }

        private void SwitchCharacter()
        {
            if (HasPendingSale)
            {
                HasPendingSale = false;  // Draft-only: no inventory/cash mutation occurred.
                ShowNotice("Hoa don nhap da huy an toan khi doi nhan vat.");
            }
            State.activeIndex = 1 - State.activeIndex;
            ConfigureActive();
            if (followCam != null) followCam.SetTarget(ActiveCharacter.transform);
            ShowNotice("Dang dieu khien: " + ActiveCharacter.DisplayName);
        }

        private void ConfigureActive()
        {
            for (int i = 0; i < Characters.Count; i++) Characters[i].SetPlayerControlled(i == State.activeIndex);
        }

        public bool SelectCharacterModel(int twinIndex, string id)
        {
            if (Characters.Count != 2 || !SoraCharacterModels.IsAllowed(twinIndex, id) || !Characters[twinIndex].TrySetModel(id)) return false;
            if (twinIndex == 0) State.toshiharuModel = id; else State.akikoModel = id;
            HasPendingSale = false;
            ShowNotice(Characters[twinIndex].DisplayName + ": " + SoraCharacterModels.Find(id).Label);
            return true;
        }

        public void SetModelSelectionOpen(bool open)
        {
            IsChoosingModels = open;
            if (!open) return;
            HasPendingSale = false;
            IsDialogue = false;
            ShiftSummary = false;
            CurrentTarget = null;
        }

        private void Interact(SoraInteractable t)
        {
            switch (t.kind)
            {
                case InteractKind.Register:
                    if (!HasPendingSale)
                    {
                        if (State.shelfStock <= 0) { ShowNotice("Ke da het hang. Can sap them hang truoc."); break; }
                        HasPendingSale = true;
                        ShowNotice("POS: Da quet 1 mon (380 yen). Nhan E lan nua de thanh toan; TAB/ESC de huy.", 6f);
                    }
                    else
                    {
                        // Single synchronous commit. E press cannot charge twice for the same draft.
                        HasPendingSale = false;
                        State.shelfStock--;
                        State.storeCash += SalePrice;
                        State.sales++;
                        ShowNotice("Thanh toan thanh cong: +380 yen. Ton ke -1. So don: " + State.sales);
                    }
                    break;
                case InteractKind.Shelf:
                    if (State.backStock <= 0) { ShowNotice("Kho dang trong. Hay nhan hang o thung phia sau!"); break; }
                    int moved = Mathf.Min(3, State.backStock);
                    State.backStock -= moved;
                    State.shelfStock += moved;
                    State.restockCount++;
                    ShowNotice("Da xep " + moved + " mon len ke. Ke: " + State.shelfStock + ", kho: " + State.backStock);
                    break;
                case InteractKind.Delivery:
                    if (State.deliveryReceived) { ShowNotice("Lo hang nay da nhan. Khong duoc nhan trung!"); break; }
                    State.deliveryReceived = true;
                    State.deliveries++;
                    State.backStock += 6;
                    ShowNotice("Da nhan 6 mon vao kho. Nhiem vu tiep: sap hang len ke.");
                    break;
                case InteractKind.StockCount:
                    ShowNotice("Kiem ke: " + State.shelfStock + " tren ke / " + State.backStock + " trong kho. Tong: " + (State.shelfStock + State.backStock));
                    break;
                case InteractKind.Customer:
                    DialogueCustomer = Mathf.Clamp(t.customerIndex, 0, 2);
                    IsDialogue = true;
                    break;
                case InteractKind.ShiftClock:
                    ShiftSummary = true;
                    ShowNotice("Tong ket ca lam da mo. Nhan ESC de tiep tuc.");
                    break;
                case InteractKind.ExitDoor:
                    ShowNotice("Tokyo dang mua. Hay hoan thanh ca truoc khi ve nha!");
                    break;
            }
        }

        private void ChooseDialogue(int option)
        {
            int ix = State.activeIndex * 3 + DialogueCustomer;
            if (option == 1)
            {
                State.familiarity[ix]++;
                State.trust[ix]++;
                ShowNotice(CustomerNames[DialogueCustomer] + " nho ten " + ActiveCharacter.DisplayName + ". +1 quen biet, +1 tin cay.");
            }
            else
            {
                State.familiarity[ix] += 2;
                if (State.sales == 0) State.trust[ix] = Mathf.Max(0, State.trust[ix] - 1);
                else State.trust[ix]++;
                ShowNotice("Ban da co gang gay an tuong. +2 quen biet; tin cay tuy thuoc hieu suat ca.");
            }
            IsDialogue = false;
        }

        public void Register(SoraInteractable target) { Interactables.Add(target); }
        public void RegisterCharacter(SoraDemoCharacter character) { Characters.Add(character); }
        public void ShowNotice(string msg, float seconds = 4f) { Notice = msg; noticeUntil = Time.time + seconds; }

        public void Save()
        {
            try
            {
                if (HasPendingSale) { HasPendingSale = false; }
                var weather = FindFirstObjectByType<SoraWeather>();
                if (weather != null) { State.weather = weather.State.Copy(); State.hasWeatherSnapshot = true; }
                for (int i = 0; i < 2; i++)
                {
                    Vector3 p = Characters[i].transform.position;
                    State.characterPositions[i * 3] = p.x;
                    State.characterPositions[i * 3 + 1] = p.y;
                    State.characterPositions[i * 3 + 2] = p.z;
                }
                File.WriteAllText(SavePath, JsonUtility.ToJson(State, true));
                ShowNotice("Da luu progress: " + SavePath, 6f);
            }
            catch (Exception ex) { Debug.LogException(ex); ShowNotice("Khong the luu - xem Console."); }
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) { ShowNotice("Chua co save. Nhan F5 de luu truoc."); return; }
                var loaded = JsonUtility.FromJson<SoraSaveData>(File.ReadAllText(SavePath));
                if (!RestoreSave(loaded)) ShowNotice("Save schema / model khong hop le.");
            }
            catch (Exception ex) { Debug.LogException(ex); ShowNotice("Khong the load - xem Console."); }
        }

        private bool RestoreSave(SoraSaveData loaded)
        {
            if (loaded == null || loaded.schema != 1 || Characters.Count != 2 ||
                loaded.characterPositions == null || loaded.characterPositions.Length != 6 ||
                loaded.familiarity == null || loaded.familiarity.Length != 6 || loaded.trust == null || loaded.trust.Length != 6 ||
                (!string.IsNullOrEmpty(loaded.toshiharuModel) && !SoraCharacterModels.IsAllowed(0, loaded.toshiharuModel)) ||
                (!string.IsNullOrEmpty(loaded.akikoModel) && !SoraCharacterModels.IsAllowed(1, loaded.akikoModel))) return false;
            foreach (float coordinate in loaded.characterPositions) if (float.IsNaN(coordinate) || float.IsInfinity(coordinate)) return false;
            // JsonUtility creates inline objects for absent fields; presence needs an explicit flag.
            if (loaded.hasWeatherSnapshot && (loaded.weather == null || !loaded.weather.IsValid())) return false;
            State = loaded;
            if (loaded.hasWeatherSnapshot) FindFirstObjectByType<SoraWeather>()?.Restore(loaded.weather);
            State.activeIndex = Mathf.Clamp(State.activeIndex, 0, 1);
            bool missingModel = false;
            for (int i = 0; i < 2; i++)
            {
                string id = i == 0 ? State.toshiharuModel : State.akikoModel;
                if (string.IsNullOrEmpty(id) || !Characters[i].TrySetModel(id))
                {
                    Characters[i].RestoreDefaultAppearance();
                    missingModel |= !string.IsNullOrEmpty(id);
                }
                CharacterController cc = Characters[i].GetComponent<CharacterController>();
                bool wasEnabled = cc != null && cc.enabled;
                if (cc != null) cc.enabled = false;
                Characters[i].transform.position = new Vector3(State.characterPositions[i * 3], State.characterPositions[i * 3 + 1], State.characterPositions[i * 3 + 2]);
                if (cc != null) cc.enabled = wasEnabled;
            }
            IsDialogue = false; ShiftSummary = false; HasPendingSale = false; IsChoosingModels = false;
            ConfigureActive();
            if (followCam != null) followCam.SetTarget(ActiveCharacter.transform);
            ShowNotice(missingModel ? "Da load; thieu model local, dang dung ngoai hinh prototype." : "Da load progress va ngoai hinh tu JSON.");
            return true;
        }

        private void InitializeStyles()
        {
            if (hudStyle != null) return;
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            hudStyle.normal.textColor = new Color(0.92f, 0.97f, 1f);
            infoStyle = new GUIStyle(hudStyle) { fontSize = 13 };
            titleStyle = new GUIStyle(hudStyle) { fontSize = 20, fontStyle = FontStyle.Bold };
            buttonStyle = new GUIStyle(GUI.skin.box) { fontSize = 15, wordWrap = true };
            buttonStyle.normal.textColor = Color.white;
            overlayStyle = new GUIStyle(GUI.skin.box) { fontSize = 17, wordWrap = true };
            overlayStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            if (!initialized || ActiveCharacter == null) return;
            InitializeStyles();
            if (IsChoosingModels) { DrawModelSelection(); return; }
            GUI.Box(new Rect(12, 12, 440, 177), GUIContent.none);
            GUI.Label(new Rect(24, 17, 425, 29), "SORA: AFTER THE RAIN | PROTOTYPE 0.1", titleStyle);
            GUI.Label(new Rect(24, 54, 425, 24), "Dieu khien: " + ActiveCharacter.DisplayName + " | Tokyo / 20:" + Mathf.FloorToInt((Time.time - begunAt) / 12f).ToString("00"), hudStyle);
            GUI.Label(new Rect(24, 80, 415, 24), "Quy cua hang: " + State.storeCash + " yen  |  Don POS: " + State.sales, hudStyle);
            GUI.Label(new Rect(24, 106, 415, 24), "Ton ke: " + State.shelfStock + "  |  Ton kho: " + State.backStock + "  |  Hang nhan: " + (State.deliveryReceived ? "Co" : "Chua"), hudStyle);
            GUI.Label(new Rect(24, 136, 415, 45), "WASD: di chuyen | Shift: chay | TAB: doi nguoi | E: tuong tac\nM: chon model | RMB: camera | F5/F9: save/load | ESC: dong", infoStyle);
            if (CurrentTarget != null && !IsDialogue && !ShiftSummary)
            {
                GUI.Box(new Rect(Screen.width / 2f - 170f, Screen.height - 106, 340, 46), "[E] " + CurrentTarget.title, buttonStyle);
            }
            if (Time.time <= noticeUntil)
                GUI.Box(new Rect(Mathf.Max(12, Screen.width / 2f - 310), Screen.height - 53, Mathf.Min(620, Screen.width - 24), 45), Notice, infoStyle);
            if (IsDialogue)
            {
                GUI.Box(new Rect(Screen.width / 2f - 250, Screen.height / 2f - 105, 500, 230), GUIContent.none);
                GUI.Label(new Rect(Screen.width / 2f - 230, Screen.height / 2f - 92, 465, 32), CustomerNames[DialogueCustomer] + " - " + CustomerDetails[DialogueCustomer], titleStyle);
                int ix = State.activeIndex * 3 + DialogueCustomer;
                GUI.Label(new Rect(Screen.width / 2f - 230, Screen.height / 2f - 48, 465, 26), "Quen biet: " + State.familiarity[ix] + "  |  Tin cay: " + State.trust[ix], hudStyle);
                GUI.Label(new Rect(Screen.width / 2f - 230, Screen.height / 2f - 8, 465, 26), "[1] Chao hoi lich su (+quen biet, +tin cay)", hudStyle);
                GUI.Label(new Rect(Screen.width / 2f - 230, Screen.height / 2f + 28, 465, 40), "[2] Gay an tuong (co the anh huong uy tin)", hudStyle);
                GUI.Label(new Rect(Screen.width / 2f - 230, Screen.height / 2f + 78, 465, 26), "[3] Ket thuc hoi thoai", hudStyle);
            }
            if (ShiftSummary)
            {
                GUI.Box(new Rect(Screen.width / 2f - 260, Screen.height / 2f - 130, 520, 270), GUIContent.none);
                GUI.Label(new Rect(Screen.width / 2f - 235, Screen.height / 2f - 115, 485, 34), "BAO CAO CUOI CA", titleStyle);
                GUI.Label(new Rect(Screen.width / 2f - 235, Screen.height / 2f - 65, 485, 170), "Tong hoa don: " + State.sales + "\nDoanh thu: " + State.storeCash + " yen\nHang da nhan: " + State.deliveries + " lo\nSo lan xep ke: " + State.restockCount + "\nTon cuoi ca: " + (State.shelfStock + State.backStock) + " mon\n\nNhan F5 de luu / ESC de quay lai", hudStyle);
            }
        }

        private void DrawModelSelection()
        {
            float width = Mathf.Min(680, Screen.width - 24), height = Mathf.Min(440, Screen.height - 24);
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box);
            modelMenuScroll = GUILayout.BeginScrollView(modelMenuScroll);
            GUILayout.Label("CHON NGOAI HINH HAI NHAN VAT", titleStyle);
            GUILayout.Label("Chon bang chuot hoac phim 1-6. F5 luu ca ngoai hinh va progress.", infoStyle);
            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            int key = 0;
            for (int twin = 0; twin < 2; twin++)
            {
                GUILayout.BeginVertical(GUILayout.Width((width - 40) / 2));
                GUILayout.Label(twin == 0 ? "Tei Toshiharu - Nam" : "Tei Akiko - Nu", hudStyle);
                foreach (var model in SoraCharacterModels.All)
                {
                    if (model.TwinIndex != twin) continue;
                    bool available = model.Prefab != null;
                    GUI.enabled = available;
                    string selected = Characters[twin].ModelId == model.Id ? "[x] " : "[ ] ";
                    if (GUILayout.Button("[" + (++key) + "] " + selected + model.Label + (available ? "" : " (chua nap)"), GUILayout.Height(44)))
                        SelectCharacterModel(twin, model.Id);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12);
            GUILayout.Label("M: mo lai | Enter / Esc: tiep tuc | Tab trong game: doi nguoi dieu khien", infoStyle);
            if (GUILayout.Button("Tiep tuc [Enter]", GUILayout.Height(38))) SetModelSelectionOpen(false);
            GUILayout.Label(Notice, infoStyle);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
